param iotHubName            string
param cosmosAccountName     string
param cosmosDbName          string
param cosmosCollectionName  string

resource symbolicname 'Microsoft.Devices/IotHubs@2022-04-30-preview' = {
  name: iotHubName
  location: resourceGroup().location
   sku: {
    capacity: 1
    name: 'S1'
  }
  properties:{   
     routing: {
      endpoints: {
        cosmosDBSqlCollections: [
          {
            authenticationType: 'identityBased'
            collectionName: cosmosCollectionName
            databaseName: cosmosDbName
            endpointUri: 'https://${cosmosAccountName}.documents.azure.com:443/'                  
            name: 'cosmosticketendpoint'         
          }
        ]
        }
         routes: [
        {
          
          endpointNames: [
            'cosmosticketendpoint'
          ]
          isEnabled: true
          name: 'cosmosTelemetryRoute'
          source: 'DeviceMessages'
        }
      ]
     }
}
}
output iotHubIdentity string = symbolicname.identity.principalId