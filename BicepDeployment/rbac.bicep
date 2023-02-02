param location string = resourceGroup().location
param roleDefinitionResourceId string = '00000000-0000-0000-0000-000000000002'

var managedIdentityName = 'miDarinSorianaPOC'

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: managedIdentityName
  location: location
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
 
  name: guid(resourceGroup().id, managedIdentity.id, roleDefinitionResourceId)
  properties: {
    roleDefinitionId: roleDefinitionResourceId
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}