using Microsoft.SemanticKernel.ChatCompletion;

namespace AzureSqlSk.Api.Services;

public class ChatHistoryService(ILogger<ChatHistoryService> logger)
{
    private static readonly string SystemMessage = $"""
                                                     You are an AI assistant that helps insurance agents to find information on customers data and status.
                                                     Use a professional tone when answering and provide a summary of data instead of lists. If users ask about topics you don't know, answer that you don't know.
                                                     Today's date is {DateTime.Now:yyyy-MM-dd}. Query the database at every user request, even if information is available in chat history, to make sure you always have the latest information.
                                                     """;

    private readonly ChatHistory _chatHistory = new ChatHistory(SystemMessage);
    private readonly ILogger<ChatHistoryService> _logger = logger;

    public ChatHistory GetHistory() => _chatHistory;

    public void ClearHistory()
    {
        _logger.LogInformation("Clearing chat history");
        _chatHistory.Clear();
        _chatHistory.AddSystemMessage(SystemMessage);
    }

    public void AddUserMessage(string message)
    {
        _chatHistory.AddUserMessage(message);
    }

    public void AddAssistantMessage(string message)
    {
        _chatHistory.AddAssistantMessage(message);
    }
} 