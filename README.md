# IoT Edge Web API Sample Project

## Overview

## Install Azure Infrastructure
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
