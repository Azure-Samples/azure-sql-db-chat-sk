﻿using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.SqlServer;
using Microsoft.SemanticKernel.Memory;
using Microsoft.Extensions.Logging.Console;
using DotNetEnv;
using azure_sql_sk;

#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0020

if (args.Length > 0)
{
    Console.WriteLine($"Using {args[0]} environment file...");
    Env.Load(args[0]);
} else {
    Console.WriteLine($"Using .env environment file...");
    Env.Load();
}

string azureOpenAIEndpoint = Env.GetString("OPENAI_URL");
string azureOpenAIApiKey = Env.GetString("OPENAI_KEY");
string embeddingModelDeploymentName = Env.GetString("OPENAI_EMBEDDING_DEPLOYMENT_NAME");
string chatModelDeploymentName = Env.GetString("OPENAI_CHAT_DEPLOYMENT_NAME");
string sqlConnectionString = Env.GetString("MSSQL_CONNECTION_STRING");
string sqlTableName = Env.GetString("MSSQL_TABLE_NAME") ?? "ChatMemories";

Console.WriteLine("Initializing the kernel...");
var sc = new ServiceCollection();
sc.AddAzureOpenAIChatCompletion(chatModelDeploymentName, azureOpenAIEndpoint, azureOpenAIApiKey);
sc.AddKernel();
sc.AddLogging(b => b.AddSimpleConsole(o => { o.ColorBehavior = LoggerColorBehavior.Enabled; }).SetMinimumLevel(LogLevel.Debug));
var services = sc.BuildServiceProvider();
var logger = services.GetRequiredService<ILogger<Program>>();
var openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings() 
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

Console.WriteLine("Initializing plugins...");
var kernel = services.GetRequiredService<Kernel>();
kernel.ImportPluginFromObject(new SearchSessionPlugin(kernel, logger, sqlConnectionString));
var ai = kernel.GetRequiredService<IChatCompletionService>();

Console.WriteLine("Initializing memory...");
var memory = new MemoryBuilder()
    .WithSqlServerMemoryStore(sqlConnectionString)
    .WithAzureOpenAITextEmbeddingGeneration(embeddingModelDeploymentName, azureOpenAIEndpoint, azureOpenAIApiKey)
    .Build();

await memory.SaveInformationAsync(sqlTableName, "With the new connector Microsoft.SemanticKernel.Connectors.SqlServer it is possible to efficiently store and retrieve memories thanks to the newly added vector support", "semantic-kernel-mssql");
await memory.SaveInformationAsync(sqlTableName, "At the moment Microsoft.SemanticKernel.Connectors.SqlServer can be used only with Azure SQL", "semantic-kernel-azuresql");
await memory.SaveInformationAsync(sqlTableName, "Azure SQL support for vectors is in Early Adopter Preview.", "azuresql-vector-eap");
await memory.SaveInformationAsync(sqlTableName, "Pizza is one of the favourite food in the world.", "pizza-favourite-food");

Console.WriteLine("Ready to chat! Hit 'ctrl-c' to quit.");   
var chat = new ChatHistory("You are an AI assistant that helps developers find information on the SQL Konferenz 2024 conference which is about Microsoft technologies. If users ask about topics you don't know, answer that you don't know. You cannot generate T-SQL code. A plugin will do that for you.");
var builder = new StringBuilder();
while (true)
{
    Console.Write("\nQuestion: ");
    var question = Console.ReadLine()!;
    
    if (string.IsNullOrWhiteSpace(question))
        continue;
    
    Console.WriteLine();

    logger.LogDebug("Searching information from the memory...");
    builder.Clear();
    await foreach (var result in memory.SearchAsync(sqlTableName, question, limit: 3))
    {
        builder.AppendLine(result.Metadata.Text);
    }
    var contextToRemove = -1;
    if (builder.Length > 0)
    {
        logger.LogDebug("Found information from the memory:");
        logger.LogDebug(builder.ToString());

        builder.Insert(0, "Here's some additional information: ");
        contextToRemove = chat.Count;
        chat.AddSystemMessage(builder.ToString());
    }    
    
    builder.Clear();
    chat.AddUserMessage(question);
    var firstLine = true;
    await foreach (var message in ai.GetStreamingChatMessageContentsAsync(chat, openAIPromptExecutionSettings, kernel))    
    {
        if (firstLine) { 
            Console.WriteLine("\nAnswer: ");
            firstLine = false; 
        }
        Console.Write(message);
        builder.Append(message.Content);
    }
    Console.WriteLine();
    chat.AddAssistantMessage(builder.ToString());

    if (contextToRemove >= 0)
        chat.RemoveAt(contextToRemove);

    Console.WriteLine();
}