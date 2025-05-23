@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention')
param environmentName string

@description('Primary Azure region for all resources')
param location string

@description('Name of the resource group')
param resourceGroupName string

@description('The name of the SQL Server admin user')
param sqlAdminUser string = 'sqlAdmin'

@description('SQL Admin Password')
@secure()
param sqlAdminPassword string

@description('The name of the SQL database')
param databaseName string = 'sessiondb'

@description('The Azure OpenAI completions model to deploy')
param openAiCompletionsModel string = 'gpt-4o'
@description('The Azure OpenAI embeddings model to deploy')
param openAiEmbeddingsModel string = 'text-embedding-ada-002'

var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location, resourceGroupName))

@description('The Client IP address for the SQL Server firewall.')
param clientIpAddress string = '0.0.0.0' // This is overwritten by the preprovision script.

@description('The Azure User Principal Name of the user deploying the template.')
param principalName string // This is set by the preprovision script.

@description('The Azure Principal ID of the user deploying the template.')
var principalId = deployer().objectId

// Tags that should be applied to all resources.
var tags = {
  'azd-env-name': environmentName
}

targetScope = 'subscription'
resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

@description('Creates a SQL Server and a SQL Database')
module sqlDatabase 'sql-db.bicep' = {
  name: 'sqlDatabase'
  params: {
    serverName: '${abbrs.sqlServers}${resourceToken}'
    location: location
    principalId: principalId
    administratorLogin: sqlAdminUser
    administratorLoginPassword: sqlAdminPassword
    userPrincipalName: principalName
    databaseName: databaseName
    clientIpAddress: clientIpAddress
  }
  scope: rg
}

module openAi 'openai.bicep' = {
  name: 'openai'
  params: {
    deployments: [
      {
        name: openAiCompletionsModel
        sku: {
          name: 'Standard'
          capacity: 10
        }
        model: {
          name: openAiCompletionsModel
          version: '2024-05-13'
        }
      }
      {
        name: openAiEmbeddingsModel
        sku: {
          name: 'Standard'
          capacity: 10
        }
        model: {
          name: openAiEmbeddingsModel
          version: '2'
        }
      }
    ]
    principalId: principalId
    location: location
    name: '${abbrs.openAiAccounts}${resourceToken}'
    sku: 'S0'
    tags: tags
  }
  scope: rg
}
