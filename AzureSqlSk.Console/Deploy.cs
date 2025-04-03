using System;
using System.Collections.Generic;
using System.Text;
using DbUp;
using DbUp.ScriptProviders;
using Microsoft.Data.SqlClient;
using DotNetEnv;

namespace azure_sql_sk;

class DatabaseUtils
{
    static public void Deploy(string envFile)  
    {
        Env.Load(envFile);

        string azureOpenAIEndpoint = Env.GetString("OPENAI_URL");
        string azureOpenAIApiKey = Env.GetString("OPENAI_KEY");
        string embeddingModelDeploymentName = Env.GetString("OPENAI_EMBEDDING_DEPLOYMENT_NAME");
        string sqlConnectionString = Env.GetString("MSSQL_CONNECTION_STRING");        

        if (string.IsNullOrEmpty(sqlConnectionString)) {
            throw new ApplicationException("MSSQL environment variable not set or empty.");
        }
        
        var csb = new SqlConnectionStringBuilder(sqlConnectionString);
        Console.WriteLine($"Deploying database: {csb.InitialCatalog}");

        Console.WriteLine("Testing connection...");
        var conn = new SqlConnection(csb.ToString());
        conn.Open();
        conn.Close();

        FileSystemScriptOptions options = new() {
            IncludeSubDirectories = false,
            Extensions = ["*.sql"],
            Filter = (file) => !file.EndsWith(".local.sql"),
            Encoding = Encoding.UTF8
        };

        Dictionary<string, string> variables = new() {
            {"OPENAI_URL", azureOpenAIEndpoint},
            {"OPENAI_KEY", azureOpenAIApiKey},
            {"OPENAI_EMBEDDING_DEPLOYMENT_NAME", embeddingModelDeploymentName}
        };

        Console.WriteLine("Starting deployment...");
        var dbup = DeployChanges.To
            .SqlDatabase(csb.ConnectionString)
            .WithVariables(variables)
            .WithScriptsFromFileSystem("sql", options)
            .JournalToSqlTable("dbo", "$__dbup_journal")                                               
            .LogToConsole()
            .Build();
        
        var result = dbup.PerformUpgrade();

        if (!result.Successful)
        {
            throw result.Error;            
        }
    }
}