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

    [KernelFunction("query_database")]
    [Description("""
        Query the database to return data for the given query, if there are no other plugins that can be used to answer the query. This function only return data from the PASS Data Community Summit 2024.
        The high-level schema of the database is the following:
        
        TABLE: [dbo].[pass_sessions]
        COLUMNS:
        [session_id]: internal id of the sessions,
        [title]: session's title
        [abstract]: session's abstract
        [speakers]: list of speakers of the session
        [track]: track of the session
        [level]: values can be 100, 200, 300, 400, 500. 500 is the most advanced, 100 is the most basic       
        """)]
    public async Task<IEnumerable<dynamic>> QueryDatabase(string query)
    {        
        logger.LogInformation($"Querying the database for '{query}'");

        var ai = kernel.GetRequiredService<IChatCompletionService>();
        var chat = new ChatHistory(@"You create T-SQL queries based on the given user request and the provided schema. Just return T-SQL query to be executed. Do not return other text or explanation. Don't use markdown or any wrappers.
        The database schema is the following:

        // this table contains the sessions at the PASS Data Community Summit 2024 conference
        CREATE TABLE [dbo].[pass_sessions]
        (
            [session_id] INT NOT NULL,
            [title] NVARCHAR(200) NOT NULL,
            [abstract] NVARCHAR(MAX) NOT NULL,
            [speakers] NVARCHAR(MAX) NOT NULL, -- JSON Array of speakers, for example ['speaker1', 'speaker2']
            [properties] NVARCHAR(MAX) NOT NULL -- JSON 
        );

        the [properties] column contains JSON data with the following structure:
        
        Track: string // the track of the session
        Level: integer // session level        

        you must use OPENJSON to get the speakers of a session. 
        For example, to get the speakers of a session, you can use the following query:

        SELECT session_id, [value] AS SessionSpeaker
        FROM [dbo].[pass_sessions]
        CROSS APPLY OPENJSON(speakers)]
        WHERE session_id = 1

        you cannot GROUP BY on the speakers column. You must use OPENJSON to get the speakers of a session.
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

    [KernelFunction("find_sessions_similar_to_topic")]
    [Description("Return a list of sessions at PASS Data Community Summit 2024 at that are similar to a specific topic or by a specific speaker name specified in the provided topic parameter. If no results are found, an empty list is returned. This function only return data from the PASS Data Community Summit 2024 conference.")]
    public async Task<IEnumerable<Session>> GetSessionSimilarToTopic(string topic)
    {        
        logger.LogInformation($"Searching for sessions related to '{topic}'");

        DefaultTypeMap.MatchNamesWithUnderscores = true;

        await using var connection = new SqlConnection(connectionString);
        var sessions = await connection.QueryAsync<Session>("dbo.find_pass_sessions", 
            new { 
                inputText = topic                
            }, 
            commandType: CommandType.StoredProcedure
        );
                     
        return sessions;    
    }
}