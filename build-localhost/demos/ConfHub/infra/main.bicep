targetScope = 'subscription'

// ─────────────────────────────────────────────────────────────────────────────
//  ConfHub — main infrastructure (azd entrypoint)
//  Provisions a resource group and all resources for the "Ship It" demo:
//  Cosmos DB, Azure AI Foundry (account + project + model), Container Apps.
// ─────────────────────────────────────────────────────────────────────────────

@minLength(1)
@maxLength(64)
@description('Name of the azd environment — used to derive resource names.')
param environmentName string

@minLength(1)
@description('Primary location for all resources.')
param location string

@description('Id of the signed-in user or service principal to grant data-plane roles for local development. Provided automatically by azd as AZURE_PRINCIPAL_ID.')
param principalId string = ''

@description('Model deployment name exposed to the application.')
param modelDeploymentName string = 'gpt-4o-mini'

@description('Model version to deploy.')
param modelVersion string = '2024-07-18'

@description('Model deployment capacity (thousands of tokens per minute).')
param modelCapacity int = 10

var abbrs = {
  resourceGroup: 'rg'
}
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var tags = {
  'azd-env-name': environmentName
}

resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: '${abbrs.resourceGroup}-${environmentName}'
  location: location
  tags: tags
}

module resources 'resources.bicep' = {
  name: 'resources'
  scope: rg
  params: {
    location: location
    tags: tags
    resourceToken: resourceToken
    principalId: principalId
    modelDeploymentName: modelDeploymentName
    modelVersion: modelVersion
    modelCapacity: modelCapacity
  }
}

// Outputs consumed by azd and the application
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_RESOURCE_GROUP string = rg.name

output AZURE_CONTAINER_REGISTRY_ENDPOINT string = resources.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT
output AZURE_CONTAINER_APP_ENDPOINT string = resources.outputs.AZURE_CONTAINER_APP_ENDPOINT

output COSMOSDB__ACCOUNTENDPOINT string = resources.outputs.COSMOSDB_ACCOUNT_ENDPOINT
output COSMOSDB__DATABASENAME string = resources.outputs.COSMOSDB_DATABASE_NAME
output COSMOSDB__CONTAINERNAME string = resources.outputs.COSMOSDB_CONTAINER_NAME

output AZUREAIFOUNDRY__PROJECTENDPOINT string = resources.outputs.FOUNDRY_PROJECT_ENDPOINT
output AZUREAIFOUNDRY__MODELDEPLOYMENTNAME string = modelDeploymentName
