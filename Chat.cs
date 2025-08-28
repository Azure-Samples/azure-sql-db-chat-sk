using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.SqlServer;
using Microsoft.Extensions.Logging.Console;
using DotNetEnv;
using System.Text.Json;
using Spectre.Console;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using System.Diagnostics;

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
        azureOpenAIApiKey = Env.GetString("OPENAI_KEY") ?? string.Empty;
        embeddingModelDeploymentName = Env.GetString("OPENAI_EMBEDDING_DEPLOYMENT_NAME");
        chatModelDeploymentName = Env.GetString("OPENAI_CHAT_DEPLOYMENT_NAME");
        sqlConnectionString = Env.GetString("MSSQL_CONNECTION_STRING");
        sqlTableName = Env.GetString("MSSQL_TABLE_NAME") ?? "ChatMemories";
    }

    public async Task RunAsync(bool enableDebug = false)
    {
        AnsiConsole.Clear();
        AnsiConsole.Foreground = Color.Green;

        var table = new Table();
        table.Expand();
        table.AddColumn(new TableColumn("[bold]Insurance Agent Assistant[/] v2.3").Centered());
        AnsiConsole.Write(table);

        //AnsiConsole.WriteLine($"azureOpenAIEndpoint: {azureOpenAIEndpoint}, embeddingModelDeploymentName: {embeddingModelDeploymentName}, chatModelDeploymentName: {chatModelDeploymentName}, sqlTableName: {sqlTableName}");

        var openAIPromptExecutionSettings = new AzureOpenAIPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        (var logger, var kernel, var ai, var knowledge) = await AnsiConsole.Status().StartAsync("Booting up agent...", async ctx =>
        {
            ctx.Spinner(Spinner.Known.Default);
            ctx.SpinnerStyle(Style.Parse("yellow"));

            AnsiConsole.WriteLine("Initializing kernel...");
            var credentials = new DefaultAzureCredential();
            var sc = new ServiceCollection();

            if (string.IsNullOrEmpty(azureOpenAIApiKey))
            {
                sc.AddAzureOpenAIChatCompletion(chatModelDeploymentName, azureOpenAIEndpoint, credentials);
                sc.AddAzureOpenAIEmbeddingGenerator(embeddingModelDeploymentName, azureOpenAIEndpoint, credentials);
            }
            else
            {
                sc.AddAzureOpenAIChatCompletion(chatModelDeploymentName, azureOpenAIEndpoint, azureOpenAIApiKey);
                sc.AddAzureOpenAIEmbeddingGenerator(embeddingModelDeploymentName, azureOpenAIEndpoint, azureOpenAIApiKey);
            }

            sc.AddKernel();
            sc.AddLogging(b => b.AddSimpleConsole(o => { o.ColorBehavior = LoggerColorBehavior.Enabled; }).SetMinimumLevel(enableDebug ? LogLevel.Debug : LogLevel.None));

            var services = sc.BuildServiceProvider();
            var logger = services.GetRequiredService<ILogger<Program>>();

            AnsiConsole.WriteLine("Initializing plugins...");
            var kernel = services.GetRequiredService<Kernel>();            
            kernel.Plugins.AddFromObject(new SearchDatabasePlugin(kernel, logger, sqlConnectionString));
            foreach (var p in kernel.Plugins)
            {
                foreach (var f in p.GetFunctionsMetadata())
                {
                    AnsiConsole.WriteLine($"Plugin: {p.Name}, Function: {f.Name}");
                }
            }

            // await using var mcpClient = await McpClientFactory.CreateAsync(
            //     new SseClientTransport(new () {
            //         Name = "MyFirstMCP",
            //         Endpoint = "http://localhost:5248"
            //     })
            // );
            // var tools = await mcpClient.ListToolsAsync();
            // kernel.Plugins.AddFromFunctions("MyFirstMCP", tools.Select(x => x.AsKernelFunction()));

            var ai = kernel.GetRequiredService<IChatCompletionService>();       
            var eg = services.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();     

            AnsiConsole.WriteLine("Initializing vector store...");
            var vectorStore =  new SqlServerVectorStore(sqlConnectionString, new SqlServerVectorStoreOptions() { EmbeddingGenerator = eg });        
            var knowledgeCollection = vectorStore.GetCollection<int, Memory>(sqlTableName);                    
            await knowledgeCollection.EnsureCollectionExistsAsync();                       

            AnsiConsole.WriteLine("Adding sample knowledge...");
            string[] knowledge = [
                "Premium for car insurance have been increased by 15% starting from Septmber 2024",
                "Customers can reduce their premium by subscribing to the 'Safety Score' program which will monitor their driving habits and provide discounts based on their driving score.",
            ];
            var records = knowledge.Select(async (input, index) => new Memory { Id = index+1, Content = input, Embedding = await eg.GenerateVectorAsync(input) }).ToList();
            await knowledgeCollection.UpsertAsync(records.Select(t => t.Result)); 

            AnsiConsole.WriteLine("Done!");

            return (logger, kernel, ai, knowledgeCollection);
        });

        AnsiConsole.WriteLine("Ready to chat! Hit 'ctrl-c' to quit.");
                
        var chat = new ChatHistory($"""
            You are an AI assistant that helps insurance agents to find information on customers data and status. 
            Use a professional tone when aswering and provide a summary of data instead of lists. 
            If users ask about topics you don't know, answer that you don't know. Today's date is {DateTime.Now:yyyy-MM-dd}. 
            Query the database at every user request, even if information is available in chat history, to make sure you always have the latest information.
        """);
        var builder = new StringBuilder();
        while (true)
        {
            AnsiConsole.WriteLine();
            var question = AnsiConsole.Prompt(new TextPrompt<string>($"🧑: "));

            if (string.IsNullOrWhiteSpace(question))
                continue;

            switch (question)
            {
                case "/c":
                    AnsiConsole.Clear();
                    continue;
                case "/ch":
                    chat.RemoveRange(1, chat.Count - 1);
                    AnsiConsole.WriteLine("Chat history cleared.");
                    continue;

                case "/h":
                    foreach (var message in chat)
                    {
                        AnsiConsole.WriteLine($"> ---------- {message.Role} ----------");
                        AnsiConsole.WriteLine($"> MESSAGE  > {message.Content}");
                        AnsiConsole.WriteLine($"> METADATA > {JsonSerializer.Serialize(message.Metadata)}");
                        AnsiConsole.WriteLine($"> ------------------------------------");
                    }
                    continue;
            }

            //AnsiConsole.WriteLine();

            await AnsiConsole.Status().StartAsync("Thinking...", async ctx =>
            {
                if (!enableDebug)
                {
                    ctx.Spinner(Spinner.Known.Default);
                    ctx.SpinnerStyle(Style.Parse("yellow"));
                }

                logger.LogDebug("Searching information from the memory...");
                builder.Clear();
                //var questionVector = await embeddingGenerator.(question);
                await foreach (var result in knowledge.SearchAsync(question, 3))
                {
                     if (result.Score < 0.7) builder.AppendLine(result.Record.Content);
                }
                if (builder.Length > 0)
                {
                    logger.LogDebug("Found information from the memory:" + Environment.NewLine + builder.ToString());

                    builder.Insert(0, "Here's some additional information you can use to answer the question: ");

                    chat.AddSystemMessage(builder.ToString());
                }
            });

            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine("🤖: Formulating answer...");
            builder.Clear();
            chat.AddUserMessage(question);
            var firstLine = true;
            await foreach (var message in ai.GetStreamingChatMessageContentsAsync(chat, openAIPromptExecutionSettings, kernel))
            {
                if (!enableDebug)
                    if (firstLine && message.Content != null && message.Content.Length > 0)
                    {
                        AnsiConsole.Cursor.MoveUp();
                        AnsiConsole.WriteLine("                                  ");
                        AnsiConsole.Cursor.MoveUp();
                        AnsiConsole.Write($"🤖: ");
                        firstLine = false;
                    }
                AnsiConsole.Write(message.Content ?? string.Empty);
                builder.Append(message.Content);
            }
            AnsiConsole.WriteLine();


            chat.AddAssistantMessage(builder.ToString());
        }
    }
}

