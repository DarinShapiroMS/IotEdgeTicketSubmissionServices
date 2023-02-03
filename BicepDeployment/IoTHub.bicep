param iotHubName string = 'iottest'



resource symbolicname 'Microsoft.Devices/IotHubs@2022-04-30-preview' = {
  name: iotHubName
  location: resourceGroup().location
 
  sku: {
    capacity: 1
    name: 'S1'
  }
 
  identity: {
    type: 'SystemAssigned'
   
  }
  properties:{
    features: 'DeviceManagement'

    
    }
}
output iotHubIdentity string = symbolicname.identity.principalId