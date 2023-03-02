@description('Define the project name or prefix for all objects.')
@minLength(1)
@maxLength(11)
param projectName string

//@description('The Uri for the container registry for IoT Edge Modules')
//param containerRegistryUri		string 

//@description('Username for the container registry')
//param containerRegistryUsername string

//@description('key to access the container registry')
//param containerRegistryKey		string

var iotHubName = '${projectName}Hub${uniqueString(resourceGroup().id)}'

var cosmosAccountName = '${projectName}${uniqueString(resourceGroup().id)}'
var cosmosDbName = 'IoTHubData'
var cosmosCollectionName = 'tickets'

var laWorkspaceName = '${projectName}LA${uniqueString(resourceGroup().id)}'

// Deploy the IoT Hub with basic configuration first.
// We need the system identity of this IoT Hub to grant
// IoT Hub permissions to use a Cosmos Db as a custom
// endpoint to send telemetry.
module iotHub 'IoTHub.bicep' = {
	name: 'iothubDeployment'
	params:{
		iotHubName:iotHubName
	}
}

// Deploy a storage account. This might be used for
// Archiving of data and any Azure Functions.
module storageModule 'storage.bicep' = {
	name: 'storageAccountDeployment'
	params: {
		projectName: projectName
	}
	
}

// Deploy the Cosmos database that will be the destination
// for device telemetry sent through IoT Hub by IoT Edge
// devices.
module cosmosModule 'cosmos.bicep' = {
	name: 'cosmosAccountDeployment'	
	params:{		
		principalId: iotHub.outputs.iotHubIdentity
		accountName: cosmosAccountName
		databaseName: cosmosDbName
	}
	dependsOn:[iotHub]
}

// Another IoTHub deployment to add custom endpoint
// and route configuration info we only get after
// deploying the Cosmos database. 
module iothub2 'IoTHub2.bicep' = {
	name: 'iothubDeploymentUpdate'
	params:{
		roleAssignmentOutput: cosmosModule.outputs.assignment
		iotHubName:iotHubName
		cosmosAccountName: cosmosAccountName
		cosmosDbName: cosmosDbName
		cosmosCollectionName: cosmosCollectionName
	}
	dependsOn:[cosmosModule]
}

// Deploy a Log Analytics workspace that is used by the
// metrics collection module deployed to edge devices.  
// This is necssary to provide data for the IoT Hub Workbooks
// for device observability and alerting.  This script can
// be further extended to include custom queries and alerts. 
module logAnalytics 'LogAnalytics.bicep' = {
	name: 'logAnalyticsDeployment'
	params:{
		workspaceName: laWorkspaceName
	}
}

// Output variables necessary for configuration of Modules.
// TODO: maybe automate these modules via another 
// bicep script. 
output logAnalyticsWorkspaceId string = logAnalytics.outputs.logAnalyticsWorkspaceId