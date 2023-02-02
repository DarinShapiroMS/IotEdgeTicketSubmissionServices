module iotHub 'IoTHub.bicep' = {
	name: 'iothubDeployment'
}

module storageModule 'storage.bicep' = {
	name: 'storageAccountDeployment'
	params: {
		storageAccountName: 'iothubstoragedarin'
	}
}

module cosmosModule 'cosmos2.bicep' = {
	name: 'cosmosAccountDeployment'	
	params:{		
		principalId: iotHub.outputs.iotHubIdentity
		accountName: 'soriana'
	}
}

module iothub2 'IoTHub2.bicep' = {
	name: 'iothubDeploymentUpdate'
}