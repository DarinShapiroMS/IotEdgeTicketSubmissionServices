param workspaceName string = 'iothubanalyticsworkspace'

resource symbolicname 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: workspaceName
  location: resourceGroup().location

  
  identity: {
    type: 'SystemAssigned'  
  }
  
  
}

output logAnalyticsWorkspaceId string = symbolicname.id