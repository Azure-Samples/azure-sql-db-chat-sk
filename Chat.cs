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
using Microsoft.Extensions.VectorData;
using System.Text.Json;
using OpenAI;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Primitives;

#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0020

namespace azure_sql_sk;

public class Memory
{
    [VectorStoreKey]
    public int Id { get; set; }

    [VectorStoreData]
    public string? Content { get; set; }

    [VectorStoreVector(Dimensions: 1536, DistanceFunction = DistanceFunction.CosineDistance)]
    public ReadOnlyMemory<float>? Embedding { get; set; }
}
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
        var embeddingGenerator = new AzureOpenAIClient(
            new Uri(azureOpenAIEndpoint),
            new System.ClientModel.ApiKeyCredential(azureOpenAIApiKey))
            .GetEmbeddingClient(embeddingModelDeploymentName)
            .AsIEmbeddingGenerator();
        
        var vectorStore =  new SqlServerVectorStore(sqlConnectionString, new SqlServerVectorStoreOptions() { EmbeddingGenerator = embeddingGenerator });        
        var memoriesCollection = vectorStore.GetCollection<int, Memory>(sqlTableName);        
        await memoriesCollection.EnsureCollectionExistsAsync();

        string[] memories = [
            "With the new connector Microsoft.SemanticKernel.Connectors.SqlServer it is possible to efficiently store and retrieve memories thanks to the newly added vector support",
            "Microsoft.SemanticKernel.Connectors.SqlServer works with the new Vector Type in SQL Server 2025, Azure SQL and Fabric SQL",
            "Azure SQL support for vectors Generally Available in Azure!"            
        ];

        var records = memories.Select(async (input, index) => new Memory { Id = index+1, Content = input, Embedding = await embeddingGenerator.GenerateVectorAsync(input) }).ToList();        
        await memoriesCollection.UpsertAsync(records.Select(t => t.Result));

        Console.WriteLine("Ready to chat! Hit 'ctrl-c' to quit.");
        var chat = new ChatHistory("You are an AI assistant that helps developers find information on Microsoft technologies. If users ask about topics you don't know, answer that you don't know. Be concise when answering.");
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
            var questionVector = await embeddingGenerator.GenerateVectorAsync(question);
            await foreach (var result in memoriesCollection.SearchAsync(questionVector, 3))
            {
                builder.AppendLine(result.Record.Content);
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

