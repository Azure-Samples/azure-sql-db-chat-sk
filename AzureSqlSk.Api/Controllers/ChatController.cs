using AzureSqlSk.Api.Config;
using AzureSqlSk.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using System;

#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0020

namespace AzureSqlSk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    private readonly Kernel _kernel;
    private readonly ISemanticTextMemory _memory;
    private readonly IChatCompletionService _chatCompletion;
    private readonly ChatHistoryService _chatHistory;
    private readonly string _sqlTableName;
    private readonly AzureOpenAIPromptExecutionSettings _openAiPromptExecutionSettings;

    public ChatController(
        ILogger<ChatController> logger, 
        Kernel kernel, 
        ISemanticTextMemory memory, 
        IChatCompletionService chatCompletion,
        IConfiguration configuration,
        ChatHistoryService chatHistory)
    {
        _logger = logger;
        _kernel = kernel;
        _memory = memory;
        //_chatCompletion = chatCompletion;
        _chatHistory = chatHistory;
        _openAiPromptExecutionSettings = new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var sqlConfig = configuration.GetSection("SqlServer").Get<SqlServerConfig>();
        if (sqlConfig == null)
        {
            throw new InvalidOperationException("SqlServer configuration section is missing");
        }
        _sqlTableName = sqlConfig.TableName;
        _chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        try
        {
            _logger.LogDebug("Processing chat request: {Message}", request.Message);

            // Search memory for relevant information
            _logger.LogDebug("Searching information from the memory...");
            var builder = new System.Text.StringBuilder();
            await foreach (var result in _memory.SearchAsync(_sqlTableName, request.Message, limit: 3, minRelevanceScore: 0.35))
            {
                builder.AppendLine(result.Metadata.Text);
            }
            if (builder.Length > 0)
            {
                _logger.LogDebug("Found information from the memory:" + Environment.NewLine + builder.ToString());
                builder.Insert(0, "Here's some additional information you can use to answer the question: ");
                _chatHistory.GetHistory().AddSystemMessage(builder.ToString());
            }

            // Add the user's message to chat history
            _chatHistory.AddUserMessage(request.Message);
            builder.Clear();
            // Get the AI's response
            var response = await _chatCompletion.GetChatMessageContentAsync(_chatHistory.GetHistory(), _openAiPromptExecutionSettings, _kernel);

            if (response.Content == null)
            {
                return StatusCode(500, "Failed to generate a response");
            }

            // Add the assistant's response to chat history
            _chatHistory.AddAssistantMessage(response.Content);

            return Ok(new ChatResponse 
            { 
                Message = response.Content,
                History = _chatHistory.GetHistory().Select(m => new ChatMessage 
                { 
                    Role = m.Role.ToString().ToLower(),
                    Content = m.Content ?? string.Empty
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("clear")]
    public IActionResult ClearHistory()
    {
        _chatHistory.ClearHistory();
        return Ok(new { message = "Chat history cleared" });
    }

    [HttpGet("history")]
    public IActionResult GetHistory()
    {
        return Ok(_chatHistory.GetHistory().Select(m => new ChatMessage 
        { 
            Role = m.Role.ToString().ToLower(),
            Content = m.Content != null ? m.Content : string.Empty
        }).ToList());
    }
}

public class ChatRequest
{
    public required string Message { get; set; }
}

public class ChatResponse
{
    public required string Message { get; set; }
    public required List<ChatMessage> History { get; set; }
}

public class ChatMessage
{
    public required string Role { get; set; }
    public required string Content { get; set; }
} 