using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add our API project
var api = builder.AddProject<Projects.AzureSqlSk_Api>("api");

// Add our Vue.js frontend project as a Node.js application
var web = builder.AddNpmApp("web", "../AzureSqlSk.Web")
    .WithHttpEndpoint(env: "PORT")
    .WaitFor(api)
    .WithReference(api);

builder.Build().Run();
