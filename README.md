---
page_type: sample
languages:
- csharp
products:
- azure
- azure-iot-edge
description: "Simple sample showing how to use local blob storage with Azure IoT Edge."
---
# Azure IoT Edge Blob storage sample

Simple sample showing how to use [local blob storage](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-store-data-blob) with [Azure IoT Edge](https://azure.microsoft.com/en-us/services/iot-edge/) that's currently in Public Preview (October 2018)

The sample will deploy 3 custom modules on the edge device
1) The Blob storage module (to be able to write blobs to local storage of the edge device)
2) A custom developed module called 'BlobWriterModule' that uses the Blob storage module to write messages from the pipe in single blobs on the edge device
3) The Microsoft Temperature simulator module that generates messages that will be forwarded to the custom developed module

After deploying the modules to an edge device, messages from the Temperature simulator module will be forwarded to the BlobWriterModule that will store each message as a blob on the local blob storage with the name 'MessageContents_Timestamp.txt'(E.g. MessageContents_2018-11-05T115112.txt) and with a contents like this
```
{"machine":{"temperature":21.869028476937224,"pressure":1.0990032442080382},"ambient":{"temperature":21.113945195737269,"humidity":25},"timeCreated":"2018-11-05T11:51:12.7848955Z"}
```

You can also access the local Blob storage with the [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/) by using the following Connection string (assumes usage of an Azure VM for the edge device in West Europe data center region):

```
DefaultEndpointsProtocol=http;BlobEndpoint=http://<DNS name>.westeurope.cloudapp.azure.com:11002/metadatastore;AccountName=metadatastore;AccountKey=<Your configured account key>
```

## Configuration

This sample was developed with Visual Studio Code and uses the Azure IoT Edge extensions. Therefore most of the configuration is stored in the 'deployment.template.json' file.

### Local storage for Blobs on the edge device

- File system location: **/srv/blobdata** : Configured in the create Options of the 'blobstoremodule' module
- Storage account name: **metadatastore** : Configured in the create Options of the 'blobstoremodule' module
- Storage account key: **Not shown here** : Base64 string in the create Options of the 'blobstoremodule' module
- Blob storage connection string: **Not shown here** : Env variable defined in the create Options of the 'BlobWriterModule' module
- Container name for message blobs: **samplecontainer** : Env variable defined in the create Options of the 'BlobWriterModule' module

### Private registry for the custom developed module

Create an '.env' file at the first folder level and add the information about your private registry for the image

```
mycontainers_username=<Put in your username for your private registry>
mycontainers_password=<Put in your password for the private registry>
mycontainers_address=<Put in your address for the private registry>
```

## Tips

Since the Blob storage support is currently in Public Preview (October 2018), lot of people are having problems setting it up correctly. Therefore try to follow this simple guidance:
- On each restart of the blob storage module with an existing local folder for the storage items you’ll get the following error message in the log file of the container that you can ignore: [backend] [info] [Tid:1] [MetaStore.cc:95] [Initialize] Initialize MetaStore. Status:Invalid argument: You have to open all column families. Column families not opened: Container_cf ‘’
- Make sure to use the latest image for the blob storage module. DON’T USE mcr.microsoft.com/azure-blob-storage:latest but mcr.microsoft.com/azure-blob-storage:1.0.1-linux-amd64
- Be careful with the naming of the blob storage module. I have used ‘blobstoremodule’. Notice only lowercase letters, that are also used in the URL for blob containers from a client module (see naming conventions for blob storage)
- Make sure you are using valid Base64 strings for the storage account key
- If the message processing of the EdgeHub isn't working or heavily delayed, simply stop IoT Edge, delete the edgeHub container and restart IoT Edge again
