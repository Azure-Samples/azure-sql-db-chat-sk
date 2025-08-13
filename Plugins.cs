using System.ComponentModel;
using Microsoft.SemanticKernel;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using DotNetEnv;

#pragma warning disable SKEXP0001

namespace azure_sql_sk;

public class CommunicationHistory {
    public required string Id { get; set; }
    public required string CommunicationType { get; set; }
    public required string Date { get; set; }
    public required string Details { get; set; }
}

public class SearchDatabasePlugin(Kernel kernel, ISemanticTextMemory memory, ILogger logger, string connectionString)
{   
    private readonly ILogger logger = logger;
    private readonly Kernel kernel = kernel;    
    private readonly ISemanticTextMemory memory = memory;
    private readonly string connectionString = connectionString;

    [KernelFunction("query_customers_table")]
    [Description("""
        Query the database to find customer's data
        The high-level schema of the database is the following:
        
        TABLE: [dbo].[customers]
        COLUMNS:
        [id]: internal customer id
        [first_name]: customer first name
        [last_name]: customer last name
        [address]: customer home address
        [city]: customer home city
        [state]: customer home state
        [zip]: customer home zip code
        [country]: customer home country
        [active-policies]: other type of policies the customer has (life, health, car, homeowners, etc)
        """)]
    public async Task<IEnumerable<dynamic>> QueryCustomersTable(string logical_sql_query)
    {        
        logger.LogInformation($"Querying the database for '{logical_sql_query}'");

        var ai = kernel.GetRequiredService<IChatCompletionService>();
        var chat = new ChatHistory(@"You create T-SQL queries based on the given user request and the provided schema. Just return T-SQL query to be executed. Do not return other text or explanation. Don't use markdown or any wrappers.
        The database schema is the following:

        // this table contains customer information
        CREATE TABLE [dbo].[customers]
        (
            [id] INT DEFAULT (NEXT VALUE FOR [dbo].[global_id]) NOT NULL,
            [first_name] NVARCHAR(100) NOT NULL,
            [last_name] NVARCHAR(100) NOT NULL,
            [address] NVARCHAR(100) NOT NULL,
            [city] NVARCHAR(100) NOT NULL,
            [state] NVARCHAR(100) NOT NULL,
            [zip] NVARCHAR(100) NOT NULL,
            [country] NVARCHAR(100) NOT NULL,
            [details] JSON NULL, -- make sure to cast to NVARCHAR(MAX) before using it in a query with OPENJSON or JSON_QUERY
            PRIMARY KEY NONCLUSTERED ([id] ASC)
        )

        the [details] column contains JSON data with the following structure:
        
        active-policies: [string...string] other type of policies the customer has (life, health, car, homeowners, etc)

        make sure to use JSON_QUERY when querying or filtering a JSON array or a JSON object.
        make sure to use double quotes when querying active-policies. For example use ""active-policies"" when trying to access that property in the json document.
        details column must be cast to NVARCHAR(MAX) before using it in a query.
        ");

        chat.AddUserMessage(logical_sql_query);
        var response = await ai.GetChatMessageContentAsync(chat);
        if (response.Content == null)
        {
            logger.LogWarning("AI was not able to generate a SQL query.");
            return [];
        }

        string sqlQuery = response.Content.Replace("```sql", "").Replace("```", "");

        logger.LogInformation($"Executing the following query: {sqlQuery}");
        
        await using var connection = new SqlConnection(connectionString);
        var result = await connection.QueryAsync(sqlQuery);

        return result;
    }

    [KernelFunction("query_claims_table")]
    [Description("""
        Query the database to find claims data for customers
        The high-level schema of the database is the following:
        
        TABLE: [dbo].[claims]
        COLUMNS:
        [id]: internal claim id
        [customer_id]: customer id
        [claim_type]: claim type (auto, home, life, health, etc)
        [claim_date]: claim date
        [details]: details and notes about the claim added by the agent 
        """)]
    public async Task<IEnumerable<dynamic>> QueryClaimsTable(string logical_sql_query)
    {        
        logger.LogInformation($"Querying the database for '{logical_sql_query}'");

        var ai = kernel.GetRequiredService<IChatCompletionService>();
        var chat = new ChatHistory(@"You create T-SQL queries based on the given user request and the provided schema. Just return T-SQL query to be executed. Do not return other text or explanation. Don't use markdown or any wrappers.     
        The database schema is the following:

        // this table contains customer's claims information
        CREATE TABLE [dbo].[claims]
        (
            [id] INT NOT NULL,
            [customer_id] int NOT NULL,
            [claim_type] VARCHAR (100) NOT NULL,
            [claim_date] DATETIME2(0) NOT NULL,
            [details] NVARCHAR(MAX) NOT NULL,

            PRIMARY KEY NONCLUSTERED ([id] ASC)
        );
        ");

        chat.AddUserMessage(logical_sql_query);
        var response = await ai.GetChatMessageContentAsync(chat);
        if (response.Content == null)
        {
            logger.LogWarning("AI was not able to generate a SQL query.");
            return [];
        }

        string sqlQuery = response.Content.Replace("```sql", "").Replace("```", "");

        logger.LogInformation($"Executing the following query: {sqlQuery}");
        
        await using var connection = new SqlConnection(connectionString);
        var result = await connection.QueryAsync(sqlQuery);

        return result;
    }

    [KernelFunction("query_policies_table")]
    [Description("""
        Query the database to find policies data for customers
        The high-level schema of the database is the following:
        
        TABLE: [dbo].[claims]
        COLUMNS:
        [id]: internal claim id
        [customer_id]: customer id
        [type]: policy type (auto, home, life, health, etc)
        [premium]: premium for the policy duration
        [payment_type]: payment type (monthly, yearly, etc)
        [start_date]: payment start date
        [duration]: policy duration (1 month, 6 months, 1 year, etc)
        [payment_amount]: payment amount
        [additional_notes]: details and notes about the policy and payment status
        """)]
    public async Task<IEnumerable<dynamic>> QueryPoliciesTable(string logical_sql_query)
    {        
        logger.LogInformation($"Querying the database for '{logical_sql_query}'");

        var ai = kernel.GetRequiredService<IChatCompletionService>();
        var chat = new ChatHistory(@"You create T-SQL queries based on the given user request and the provided schema. Just return T-SQL query to be executed. Do not return other text or explanation. Don't use markdown or any wrappers.
        The database schema is the following:

        // this table contains customer's policies information
        CREATE TABLE [dbo].[policies]
        (
            [id] INT NOT NULL,
            [customer_id] int NOT NULL,
            [type] VARCHAR (100) NOT NULL,
            [premium] DECIMAL(9,4) NOT NULL,
            [payment_type] VARCHAR(50) NOT NULL,
            [start_date] DATE NOT NULL, 
            [duration] VARCHAR(50) NOT NULL,
            [payment_amount] DECIMAL(9,4) NOT NULL,
            [additional_notes] NVARCHAR(MAX) NULL,

            PRIMARY KEY NONCLUSTERED ([id] ASC)
        );        

        ");

        chat.AddUserMessage(logical_sql_query);
        var response = await ai.GetChatMessageContentAsync(chat);
        if (response.Content == null)
        {
            logger.LogWarning("AI was not able to generate a SQL query.");
            return [];
        }

        string sqlQuery = response.Content.Replace("```sql", "").Replace("```", "");

        logger.LogInformation($"Executing the following query: {sqlQuery}");
        
        await using var connection = new SqlConnection(connectionString);
        var result = await connection.QueryAsync(sqlQuery);

        return result;
    }

    [KernelFunction("find_communication_history_by_subject")]
    [Description("Return interactions history for a customer based on the customer id on a specified subject. If subject is not provided, return all interactions.")]
    public async Task<IEnumerable<CommunicationHistory>> GetCustomerInteractions(int customerId, string subject)
    {        
        logger.LogInformation($"Searching for in customers history for interactions on '{subject}' subject");

        DefaultTypeMap.MatchNamesWithUnderscores = true;

        await using var connection = new SqlConnection(connectionString);
        var notes = await connection.QueryAsync<CommunicationHistory>("dbo.find_communication_history_by_subject", 
            new { 
                customerId,
                subject = subject ?? ""               
            }, 
            commandType: CommandType.StoredProcedure
        );
                     
        return notes;    
    }

    // [KernelFunction("store_memory_for_customer")]
    // [Description("Store a new memory in the agent memory for a specific customer")]
    // public async void StoreMemory(int customerId, string memoryContent)
    // {
    //     logger.LogInformation($"Storing memory for Customer '{customerId}', Memory: '{memoryContent}'");

    //     var sqlTableName = Env.GetString("MSSQL_TABLE_NAME") ?? "ChatMemories";
    //     await memory.SaveInformationAsync(sqlTableName, $"Customer Id:{customerId} - ", memoryContent, Guid.NewGuid().ToString(), additionalMetadata: $"CustomerId:{customerId}");        
    // }

    // [KernelFunction("retrieve_memory_for_customer")]
    // [Description("Search for memories about a specific customer")]
    // public async Task<IEnumerable<string>> SearchMemory(int customerId, string searchText)
    // {
    //     logger.LogInformation($"Querying memories for Customer '{customerId}', Search Query: '{searchText}'");
        
    //     var sqlTableName = Env.GetString("MSSQL_TABLE_NAME") ?? "ChatMemories";
    //     var results = new List<string>();
    //     await foreach (var result in memory.SearchAsync(sqlTableName, searchText, limit: 3, minRelevanceScore: 0.35))
    //     {
    //         if (result.Metadata.AdditionalMetadata == $"CustomerID:{customerId}")
    //             results.Add(result.Metadata.Text);           
    //     }
    //     return results;
    // }
}