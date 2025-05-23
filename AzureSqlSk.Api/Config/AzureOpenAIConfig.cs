namespace AzureSqlSk.Api.Config;

public class AzureOpenAIConfig
{
    public required string Endpoint { get; set; }
    public required string ApiKey { get; set; }
    public required string EmbeddingDeploymentName { get; set; }
    public required string ChatDeploymentName { get; set; }
} 