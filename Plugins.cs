using System.ComponentModel;
using Microsoft.SemanticKernel;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace azure_sql_sk;

public class Session {
    public required string Id { get; set; }
    public required string Title { get; set; }
    public string? Abstract { get; set; }
    public string? ExternalId { get; set; }
    public string? Speakers { get; set; }
    public decimal Distance { get; set; }
    public decimal CosineSimilarity { get; set; }  
}

public class SearchSessionPlugin(Kernel kernel, ILogger logger, string connectionString)
{   
    private readonly ILogger logger = logger;
    private readonly Kernel kernel = kernel;    
    private readonly string connectionString = connectionString;

    [KernelFunction("QueryDatabase")]
    [Description("""
        Query the database to return data for the given query, if there are no other plugins that can be used to answer the query. This function only return data from the SQL Konferenz 2024 conference.
        The high-level schema of the database is the following:
        
        TABLE: [web].[sessions]
        COLUMNS:
        [id]: internal id of the sessions,
        [title]: session's title
        [abstract]: session's abstract
        [external_id]: session id provided by conference organizers
        [details]: various details about the session (speakers, track, language, level) stored as JSON data            
        """)]
    public async Task<IEnumerable<dynamic>> QueryDatabase(string query)
    {        
        logger.LogInformation($"Querying the database for '{query}'");

        var ai = kernel.GetRequiredService<IChatCompletionService>();
        var chat = new ChatHistory(@"You create T-SQL queries based on the given user request and the provided schema. Just return T-SQL query and no other text or explanation. Don't use markdown or any wrappers.
        The database schema is the following:

        // this table contains the sessions at the SQL Konferenz 2024 conference
        CREATE TABLE [web].[sessions]
        (
            [id] INT DEFAULT (NEXT VALUE FOR [web].[global_id]) NOT NULL,
            [title] NVARCHAR (200) NOT NULL,
            [abstract] NVARCHAR (MAX) NOT NULL,
            [external_id] VARCHAR (100) NOT NULL,
            [details] JSON NULL
        );

        the [details] column contains JSON data with the following structure:
        {
            speakers: string[] // array with the speakers of the session
            track: string // the track of the session
            language: string // in which language the session is held
            level: int // values can be 100, 200, 300, 400, 500. 500 is the most advanced, 100 is the most basic
        }

        when you need to extract a value from a JSON column, you can use the following functions to get an array or a JSON object:

        CAST(JSON_QUERY(json_column, '$.key') AS NVARCHAR(MAX)) // to extract a JSON object
        CAST(JSON_QUERY(json_column, '$.key') AS NVARCHAR(MAX)) // to extract a JSON array

        when you need to extract a scalar value, you can use the following functions:
        
        JSON_VALUE(json_column, '$.key') // to extract a string value
        JSON_VALUE(json_column, '$.key') // to extract a number value
        JSON_VALUE(json_column, '$.key') // to extract a boolean value        

        for example, to extract the speakers from the [details] column, you can use the following query:
        CAST(JSON_QUERY(details, '$.speakers') AS NVARCHAR(MAX)) 
        
        ");

        chat.AddUserMessage(query);
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

    [KernelFunction("GetSessionSimilarToTopic")]
    [Description("Returns list of sessions at SQL Konferenz 2024 at that are similar to a specific topic specified in the provided parameter. If no results are found, an empty list is returned. This function only return data from the SQL Konferenz 2024 conference.")]
    public async Task<IEnumerable<Session>> GetSessionSimilarToTopic(string topic)
    {        
        logger.LogInformation($"Searching for sessions related to '{topic}'");

        DefaultTypeMap.MatchNamesWithUnderscores = true;

        await using var connection = new SqlConnection(connectionString);
        var sessions = await connection.QueryAsync<Session>("web.find_similar_sessions", 
            new { 
                topic                
            }, 
            commandType: CommandType.StoredProcedure
        );
                     
        return sessions;    
    }
}