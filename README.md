# IoT Edge Web API Sample Project

## Overview

This sample project contains the Azure and Edge components necessary to host a Web API on the edge that accepts valid JSON and forwards it to IoT Hub in Azure. IoT Hub in turns sends the data to Cosmos Database. The components that run on the edge are hosted on a Windows device configured with EFLOW to host the IoT Edge runtime and custom modules to host a web api for json submission, and a module for sending additional metrics to Log Anatlycs for observability. 

Clone this repo and follow the steps below to get the solution running in your own Azure subscription.

## Install Azure Infrastructure

Using a bicep file and the Azure CLI, you can deploy the Azure services as visualized below.  This includes...
1. [Azure IoT Hub](https://azure.microsoft.com/en-us/products/iot-hub/)
2. [Azure Cosmos Database](https://azure.microsoft.com/en-us/products/cosmos-db/)
3. [Azure Log Analytics Workspace - part of Azure Monitor](https://azure.microsoft.com/en-us/products/monitor/)


![Azure Infra Overview](./Docs/azure-architecture.png)



After cloning this repo locally, navigate to the [BicepDeployment Directory](..BicepDeployment/) and execute the following commands.  
```C#	
// You must first authenticate with Azure.
az login

// use az account set if you need to set the active subscription
az account set -s <subscription id here>

// Create a resource group for all Azure resources related to this project
// use az account list-locations for list of Azure regions
az group create -n <new resource group name> -l <desired Azure region>

// deploy main.bicep file to the resource group you just created
az deployment group create --resource-group <resource-group-name> --template-file main.bicep
```

## Install  Edge Components
