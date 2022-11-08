param location string = resourceGroup().location
param techStack string = 'DOTNET|6.0'
param envName string

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: '${envName}-service-plan'
  kind: 'functionapp'
  location: location
  properties: {
    reserved: true
  }
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
}

resource consumerApp 'Microsoft.Web/sites@2022-03-01' = {
  name: '${envName}-consumer'
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: techStack
      appSettings: [
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
      ]
    }
  }
}

resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-01-01-preview' = {
  name: '${envName}-service-bus'
  location: location
}

resource customersQueue 'Microsoft.ServiceBus/namespaces/queues@2022-01-01-preview' = {
  name: 'customers'
  parent: serviceBus
  properties: {
    // TBD
  }
}

resource queueReaderRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: resourceGroup()
  name: '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'
}

resource allowConsumeMessages 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid('allow ${consumerApp.name} to read from ${customersQueue.name}')
  scope: customersQueue
  properties: {
    principalId: consumerApp.identity.principalId
    roleDefinitionId: queueReaderRoleDefinition.id
    principalType: 'ServicePrincipal'
  }
}
