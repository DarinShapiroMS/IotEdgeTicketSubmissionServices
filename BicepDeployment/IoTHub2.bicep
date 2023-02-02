resource symbolicname 'Microsoft.Devices/IotHubs@2022-04-30-preview' = {
  name: 'sorianatesthub'
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
            collectionName: 'Tickets'
            databaseName: 'IoTTelemetryDb'
            endpointUri: 'https://soriana.documents.azure.com:443/'
         
         
            name: 'cosmosticketendpoint'
          
           
            resourceGroup: 'string'
  
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