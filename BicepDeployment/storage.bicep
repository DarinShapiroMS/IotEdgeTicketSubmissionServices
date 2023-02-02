
@description('Provide an Azure Region for the Storage Account to be created in.')
param location string = resourceGroup().location

@description('Provide a name for the storage account.')
@minLength(3)
@maxLength(24)
param storageAccountName string

resource storageAcct 'Microsoft.Storage/storageAccounts@2022-09-01'=	{	
	name: toLower(storageAccountName)
	location: location
	sku: {
		name: 'Standard_LRS'
	}
	kind: 'StorageV2'
}
