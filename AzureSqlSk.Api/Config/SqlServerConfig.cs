namespace AzureSqlSk.Api.Config;

public class SqlServerConfig
{
    public required string ConnectionString { get; set; }
    public required string TableName { get; set; }
} 