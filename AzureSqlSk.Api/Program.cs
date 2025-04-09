using AzureSqlSk.Api.Config;
using AzureSqlSk.Api.Services;
using AzureSqlSk.Shared.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.SqlServer;
using Microsoft.SemanticKernel.Memory;

#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0020

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpClient
builder.Services.AddHttpClient();

// Add ChatHistoryService
builder.Services.AddSingleton<ChatHistoryService>();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "[HH:mm:ss] ";
});
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Configure Semantic Kernel services
var azureOpenAIConfig = builder.Configuration.GetSection("AzureOpenAI").Get<AzureOpenAIConfig>();
var sqlConfig = builder.Configuration.GetSection("SqlServer").Get<SqlServerConfig>();

if (azureOpenAIConfig == null || sqlConfig == null)
{
    throw new InvalidOperationException("Required configuration sections are missing");
}

// Add Azure OpenAI services
builder.Services.AddAzureOpenAIChatCompletion(
    azureOpenAIConfig.ChatDeploymentName,
    azureOpenAIConfig.Endpoint,
    azureOpenAIConfig.ApiKey
);

// Configure memory store
builder.Services.AddSingleton<ISemanticTextMemory>(sp =>
{
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var httpClient = sp.GetRequiredService<HttpClient>();

    return new MemoryBuilder()
        .WithSqlServerMemoryStore(sqlConfig.ConnectionString)
        .WithTextEmbeddingGeneration(
            (loggerFactory, httpClient) => new AzureOpenAITextEmbeddingGenerationService(
                azureOpenAIConfig.EmbeddingDeploymentName,
                azureOpenAIConfig.Endpoint,
                azureOpenAIConfig.ApiKey,
                modelId: null,
                httpClient: httpClient,
                loggerFactory: loggerFactory
                //dimensions: 1536 // If you are using a text-embedding-3-small or large model, uncomment this line.
            ))
        .Build();
});

// Add Kernel with SearchSessionPlugin
builder.Services.AddSingleton<Kernel>(sp =>
{
    var kernel = Kernel.CreateBuilder()
        .AddAzureOpenAIChatCompletion(
            azureOpenAIConfig.ChatDeploymentName,
            azureOpenAIConfig.Endpoint,
            azureOpenAIConfig.ApiKey)
        .Build();

    var memory = sp.GetRequiredService<ISemanticTextMemory>();
    var logger = sp.GetRequiredService<ILogger<Program>>();

    // Register the SearchSessionPlugin
    kernel.Plugins.AddFromObject(new SearchSessionPlugin(kernel, memory, logger, sqlConfig.ConnectionString));

    return kernel;
});

// Initialize the application
var app = builder.Build();

// Initialize memory with insurance-related information
using (var scope = app.Services.CreateScope())
{
    var memory = scope.ServiceProvider.GetRequiredService<ISemanticTextMemory>();

    // Initialize memory with insurance-related information
    await memory.SaveInformationAsync(sqlConfig.TableName,
        "Premium for car insurance have been increased by 15% starting from September 2024",
        "policy-price-increase");

    await memory.SaveInformationAsync(sqlConfig.TableName, """
        Customers can reduce their premium by subscribing to the "Safety Score" program which will 
        monitor their driving habits and provide discounts based on their driving score.
    """, "memory-1");
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Azure SQL SK API V1");
        c.RoutePrefix = string.Empty; // Makes Swagger the default page
    });
}

// Remove HTTP redirection since we're using HTTPS only
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
