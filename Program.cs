using System.CommandLine;
using azure_sql_sk;

var envFileOption = new Option<string>(
    name: "--env-file", 
    description: "The .env file to load environment variables from.",
    getDefaultValue: () => ".env");
envFileOption.AddAlias("-e"); 

var rootCommand = new RootCommand();

var deployDbCommand = new Command("deploy", "Deploy the database");
deployDbCommand.AddOption(envFileOption); 
deployDbCommand.SetHandler((envFileOptionValue) =>
    {
        DatabaseUtils.Deploy(envFileOptionValue);
    },
    envFileOption);

rootCommand.Add(deployDbCommand);

var chatCommand = new Command("chat", "Run the chatbot");
chatCommand.AddOption(envFileOption); 
chatCommand.SetHandler(async (envFileOptionValue) => 
    {
        var chatBot = new ChatBot(envFileOptionValue);
        await chatBot.RunAsync();
    },
    envFileOption);
rootCommand.Add(chatCommand);


await rootCommand.InvokeAsync(args);
