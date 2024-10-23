using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.SqlServer;
using Microsoft.SemanticKernel.Memory;
using Microsoft.Extensions.Logging.Console;
using DotNetEnv;
using Microsoft.IdentityModel.Protocols;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;

#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0020

namespace azure_sql_sk;

public class ChatBot
{
    private readonly string azureOpenAIEndpoint;
    private readonly string azureOpenAIApiKey;
    private readonly string embeddingModelDeploymentName;
    private readonly string chatModelDeploymentName;
    private readonly string sqlConnectionString;
    private readonly string sqlTableName;

    public ChatBot(string envFile)
    {
        Env.Load(envFile);
        azureOpenAIEndpoint = Env.GetString("OPENAI_URL");
        azureOpenAIApiKey = Env.GetString("OPENAI_KEY");
        embeddingModelDeploymentName = Env.GetString("OPENAI_EMBEDDING_DEPLOYMENT_NAME");
        chatModelDeploymentName = Env.GetString("OPENAI_CHAT_DEPLOYMENT_NAME");
        sqlConnectionString = Env.GetString("MSSQL_CONNECTION_STRING");
        sqlTableName = Env.GetString("MSSQL_TABLE_NAME") ?? "ChatMemories";
    }
    public async Task RunAsync()
    {
        Console.WriteLine("Initializing the kernel...");
        //Console.WriteLine($"azureOpenAIEndpoint: {azureOpenAIEndpoint}, embeddingModelDeploymentName: {embeddingModelDeploymentName}, chatModelDeploymentName: {chatModelDeploymentName}, sqlTableName: {sqlTableName}");
        var sc = new ServiceCollection();
        sc.AddAzureOpenAIChatCompletion(chatModelDeploymentName, azureOpenAIEndpoint, azureOpenAIApiKey);
        sc.AddKernel();
        sc.AddLogging(b => b.AddSimpleConsole(o => { o.ColorBehavior = LoggerColorBehavior.Enabled; }).SetMinimumLevel(LogLevel.Debug));
        var services = sc.BuildServiceProvider();
        var logger = services.GetRequiredService<ILogger<Program>>();
        var openAIPromptExecutionSettings = new AzureOpenAIPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        Console.WriteLine("Initializing plugins...");
        var kernel = services.GetRequiredService<Kernel>();
        kernel.Plugins.AddFromObject(new SearchSessionPlugin(kernel, logger, sqlConnectionString));
        var ai = kernel.GetRequiredService<IChatCompletionService>();
        
        Console.WriteLine("Initializing long-term memory...");
        var memory = new MemoryBuilder()
            .WithSqlServerMemoryStore(sqlConnectionString)
            .WithTextEmbeddingGeneration(
                   (loggerFactory, httpClient) =>
                   {
                       return new AzureOpenAITextEmbeddingGenerationService(
                            embeddingModelDeploymentName,
                            azureOpenAIEndpoint,
                            azureOpenAIApiKey,
                            modelId: null,
                            httpClient: httpClient,
                            loggerFactory: loggerFactory,
                            dimensions: 1536
                       );
                   }
               )
            .Build();
        
        await memory.SaveInformationAsync(sqlTableName, "Premium for car insurance have been increased by 15% starting from Septmber 2024", "policy-price-increase");        
        await memory.SaveInformationAsync(sqlTableName, "Car insurance can be suspended for up to two months consective or non-consecutive to reduce premium", "memory-1");                

        Console.WriteLine("Ready to chat! Hit 'ctrl-c' to quit.");
        var chat = new ChatHistory($"You are an AI assistant that helps insurance agents to find information on customers data and status. Use a professional tone when aswering and provide a summary of data instead of lists. If users ask about topics you don't know, answer that you don't know. Today's date is {DateTime.Now:yyyy-MM-dd}.");    
        var builder = new StringBuilder();
        while (true)
        {            
            Console.Write($"\n(H: {chat.Count}) Question: ");
            var question = Console.ReadLine()!;

            if (string.IsNullOrWhiteSpace(question))
                continue;

            switch (question)
            {
                case "/c":
                    Console.Clear();
                    continue;
                case "/ch":
                    chat.RemoveRange(1, chat.Count - 1);
                    Console.WriteLine("Chat history cleared.");
                    continue;
            
                case "/h":
                    foreach (var message in chat)
                    {
                        Console.WriteLine($"> ---------- {message.Role} ----------");
                        Console.WriteLine($"> MESSAGE  > {message.Content}");
                        Console.WriteLine($"> METADATA > {JsonSerializer.Serialize(message.Metadata)}");
                        Console.WriteLine($"> ------------------------------------");
                    }                        
                    continue;
            }

            logger.LogDebug("Searching information from the memory...");
            builder.Clear();
            await foreach (var result in memory.SearchAsync(sqlTableName, question, limit: 3, minRelevanceScore: 0.3))
            {
                builder.AppendLine(result.Metadata.Text);
            }
            if (builder.Length > 0)
            {
                logger.LogDebug("Found information from the memory:" + Environment.NewLine + builder.ToString());

                builder.Insert(0, "Here's some additional information you can use to answer the question: ");

                chat.AddSystemMessage(builder.ToString());
            }

            builder.Clear();
            chat.AddUserMessage(question);
            var firstLine = true;
            await foreach (var message in ai.GetStreamingChatMessageContentsAsync(chat, openAIPromptExecutionSettings, kernel))
            {
                if (firstLine)
                {
                    Console.WriteLine($"\n[H: {chat.Count}] Answer: ");
                    firstLine = false;
                }
                Console.Write(message);
                builder.Append(message.Content);
            }
            Console.WriteLine();
            chat.AddAssistantMessage(builder.ToString());

            Console.WriteLine();
        }
    }
}

