# IoT Edge Sql Sync to Azure Sample Project

## Overview

This sample project demostrates syncing records from an Azure SQL Edge database with Azure Cosmos Db.  The Sql Edge database is hosted as a custom module in IoT Edge running in EFLOW (Azure IoT Edge fpor Linux on Windows).  In addition to the Sql Database running on the edge device, there is also a FakeDataGenerator custom module that continually populates that database with fake data, as well as the SqlSyncModule that reads records from the database and sends them to Azure via IoT Hub. Azure Iot Hub in turn pushes these records to Cosmos DB in near real time.  For this demo to be complete, Azure SQL Edge was used, but you can adapt this to read from your own local SQL Server by chaning the connection string in the code. 

Additionally, a Microsoft provided [Metrics Collection Module](https://learn.microsoft.com/en-us/azure/iot-edge/how-to-collect-and-transport-metrics?view=iotedge-1.4&tabs=iothub#enable-in-restricted-network-access-scenarios) is included to scrape local metrics and upload to Azure Monitor. 

** _**Note, for use with Azure Monitor, public network access must be allowed from the medge device to public endpoints for Azure Monitor.  Remove this module or change the Metrics Collection Module to send via IoT Hub if this is a problem.**_ 

Clone this repo and follow the steps below to get the solution running in your own Azure subscription.

## Deploy Azure Infrastructure

Using a bicep file and the Azure CLI, you can deploy the Azure services as visualized below.  This includes...
1. [Azure IoT Hub](https://azure.microsoft.com/en-us/products/iot-hub/)
2. [Azure Cosmos Database](https://azure.microsoft.com/en-us/products/cosmos-db/)
3. [Azure Log Analytics Workspace - part of Azure Monitor](https://azure.microsoft.com/en-us/products/monitor/)


![Azure Infra Overview](./Docs/azure-architecture.png)

The Edge components that run outside of Azure include..
1. A Windows Server or Client.
2. [EFLOW](https://learn.microsoft.com/en-us/azure/iot-edge/iot-edge-for-linux-on-windows?view=iotedge-1.4) (Azure IoT Edge for Linux on Windows).
3. Custom C# modules to host a local web API and collect metrics for observability.
### To Deploy Azure Infrasructure
After cloning this repo locally, navigate to the [BicepDeployment Directory](..BicepDeployment/) and execute the following commands in an elevated PowerShell terminal (Azure CLI required).   
```PowerShell	
-- You must first authenticate with Azure.
az login

-- use az account set if you need to set the active subscription
az account set -s <subscription id here>

-- Create a resource group for all Azure resources related to this project
-- use az account list-locations for list of Azure regions
az group create -n <new resource group name> -l <desired Azure region>

-- deploy main.bicep file to the resource group you just created
az deployment group create --resource-group <resource-group-name> --template-file main.bicep
```

## Install  Edge Components

1. [Install and configure EFLOW VM on Windows device](https://learn.microsoft.com/en-us/azure/iot-edge/how-to-provision-single-device-linux-on-windows-symmetric?view=iotedge-1.4&tabs=azure-portal)

## Build custom modules and push to a container registry

1. Use Visual Studio or Visual Studio Code to build [the FakeDataGeneratorModule](./FakeTicketGeneratorModule/) and [the SQLSyncModule](./SQLSyncModule2/) docker containers.

2. Push docker containers for those modules to a container registry. [Azure Container Registry](https://learn.microsoft.com/en-us/azure/container-registry/) can be used for this.

3. Deploy [SqlSyncDemo IoT Hub deployment manifest](./DeplymentManifests/SqlSyncDemo.json) using the Azure CLI.



```PowerShell
-- submit the deployment manifest json file to IoT Hub, pusing device configuration down to the named device.
-- Device Id is the name of your edge device.  
-- Hub Name is the name of your Azure IoT Hub
-- Content is the path to the DeploymentManifests/SqlSyncDemo.json file
az iot edge set-modules --device-id [device id] --hub-name [hub name] --content [file path]

```
Additional detail on deploying modules to IoT Edge devices can be found at  [How to deploy IoT Edge Modules](https://learn.microsoft.com/en-us/azure/iot-edge/how-to-deploy-modules-portal?view=iotedge-1.4)


** Note - The database will be seeded with necessary schema, but this requires it to pull a file from Azure Storage at startup. Network access to Azure Storage is required.  If this isn't possible, the bacpac file must be manually imported into the database.  See [this page](https://learn.microsoft.com/en-us/azure/azure-sql-edge/deploy-dacpac) for info on bootstrapping Azure SQL with a dacpac or bacpac. 




## Verify data is flowing from edge to cloud

1. Connect to local Azure SQL Edge Database

    First get the IP address of the EFLOW VM. Run this command in an elevated PowerShell instance. 

```PowerShell
    Get-EflowVmAddr
```

Then use any SQL tool to connect to the Tickets database on the Azure SQL Edge server running within EFLOW.  See [this article for more info](https://learn.microsoft.com/en-us/azure/azure-sql-edge/connect).  You should see fake data being generated and inserted into Tickets table.




2. Query Azure Cosmos Database for new data
After records are inserted into local Sql Edge database, they are sent through to IoT Hub, and then ingested into CosmosDB.  You can query the CosmosDb Tickets collection to see the fake tickets. 



