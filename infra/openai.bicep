// Description: Bicep template to deploy Azure OpenAI Service with specified deployments and role assignments.
// This template creates an Azure OpenAI account, configures deployments, and assigns the Cognitive Services OpenAI Contributor role to a specified principal.
// It also outputs the endpoint of the OpenAI account.

@description('The name of the Azure OpenAI resource.')
param name string

@description('The Azure region into Azure OpenAI will be deployed.')
param location string = resourceGroup().location

@description('The SKU of the Azure OpenAI resource.')
param sku string = 'S0'

@description('Tags to assign to the Azure OpenAI resource.')
param tags object = {}

@description('Array of model deployments to be created.')
param deployments array

@description('The Azure Principal ID of the user deploying the template.')
param principalId string

@description('The Azure Principal Type of the user deploying the template.')
param principalType string = 'User'

@description('Creates an Azure OpenAI resource.')
resource openAi 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: name
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  kind: 'OpenAI'
  sku: {
    name: sku
  }
  
  properties: {
    customSubDomainName: name
    publicNetworkAccess: 'Enabled'
  }
  tags: tags
}

@description('Creates model deployments for the Azure OpenAI resource.')
@batchSize(1)
resource openAiDeployments 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = [
  for deployment in deployments: {
    parent: openAi
    name: deployment.name
    sku: {
      capacity: deployment.sku.capacity
      name: deployment.sku.name
    }
    properties: {
      model: {
        format: 'OpenAI'
        name: deployment.model.name
        version: deployment.model.version
      }
    }
  }
]

@description('Assigns the Cognitive Services OpenAI Contributor role to the user deploying the template.')
resource openAIContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  scope: openAi
  name: guid(subscription().id, resourceGroup().id, principalId, 'Cognitive Services OpenAI Contributor')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'a001fd3d-188f-4b5d-821b-7da978bf7442') // Cognitive Services OpenAI Contributor role ID
    principalId: principalId
    principalType: principalType
  }
}

output endpoint string = openAi.properties.endpoint
