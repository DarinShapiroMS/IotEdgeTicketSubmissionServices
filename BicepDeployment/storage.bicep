
@description('Provide an Azure Region for the Storage Account to be created in.')
param location string = resourceGroup().location

param projectName string

var storageAccountName = '${toLower(projectName)}${uniqueString(resourceGroup().id)}'



resource storageAcct 'Microsoft.Storage/storageAccounts@2022-09-01'=	{	
	name: toLower(storageAccountName)
	location: location
	sku: {
		name: 'Standard_LRS'
	}
	kind: 'StorageV2'
}
