// Bicep template to create an Azure SQL Database with a firewall rule.
// This template creates an Azure SQL Server and a SQL Database, and configures a firewall rule to allow access from a specified IP address.
// It also assigns the Azure Principal ID of the user deploying the template as an administrator of the SQL Server.

@description('The Azure region into which the SQL Server will be deployed.')
param location string = resourceGroup().location

@description('The name of the SQL Server.')
param serverName string

@description('The name of the SQL Server administrator.')
param administratorLogin string = 'sqlAdmin'

@description('The password for the SQL Server administrator.')
@secure()
param administratorLoginPassword string

@description('Name of the SQL database')
param databaseName string = 'sessiondb'

@description('The client IP address for the SQL Server firewall.')
param clientIpAddress string

@description('The principal ID of the user deploying the template.')
param principalId string = deployer().objectId

@description('The Azure Principal Name of the user deploying the template.')
param userPrincipalName string // Set to the name of the user deploying the template

@description('Tags to assign to the Azure OpenAI resource.')
param tags object = {}

@description('Creates an Azure SQL Server and a SQL Database with a firewall rule.')
resource server 'Microsoft.Sql/servers@2024-05-01-preview' = {
  name: serverName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: true
      login: userPrincipalName
      principalType: 'User'
      sid: principalId
      tenantId: subscription().tenantId
    }
    publicNetworkAccess: 'Enabled'
  }
  tags: tags

  resource database 'databases@2024-05-01-preview' = {
    name: databaseName
    location: location
    sku: {
      name: 'Basic'
      tier: 'Basic'
      capacity: 5
    }
    properties: {}
  }

  resource firewallRule 'firewallRules@2024-05-01-preview' = {
    name: 'AllowAllAzureServices'
    properties: {
      startIpAddress: '0.0.0.0'
      endIpAddress: '0.0.0.0'
    }
  }
}

resource firewallRule 'Microsoft.Sql/servers/firewallRules@2024-05-01-preview' = {
  name: 'AllowClientIP'
  parent: server
  properties: {
    startIpAddress: clientIpAddress
    endIpAddress: clientIpAddress
  }
}
