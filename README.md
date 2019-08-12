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

Simple sample showing how to use [local blob storage](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-store-data-blob) with [Azure IoT Edge](https://azure.microsoft.com/en-us/services/iot-edge/) that's GA since August 2019.

The sample will deploy 3 custom modules on the edge device

1) The Blob storage module (to be able to write blobs to local storage of the edge device)
2) A custom developed module called 'BlobWriterModule' that uses the Blob storage module to write messages from the pipe in single blobs on the edge device
3) The Microsoft Temperature simulator module that generates messages that will be forwarded to the custom developed module

After deploying the modules to an edge device, messages from the Temperature simulator module will be forwarded to the BlobWriterModule that will store each message as a blob on the local blob storage with the name 'MessageContents_Timestamp.txt'(E.g. MessageContents_2018-11-05T115112.txt) and with a contents like this

```json
{"machine":{"temperature":21.869028476937224,"pressure":1.0990032442080382},"ambient":{"temperature":21.113945195737269,"humidity":25},"timeCreated":"2018-11-05T11:51:12.7848955Z"}
```

You can also access the local Blob storage with the [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/) by using the following Connection string (assumes usage of an Azure VM for the edge device in West Europe data center region):

```yaml
DefaultEndpointsProtocol=http;BlobEndpoint=http://<DNS name>.westeurope.cloudapp.azure.com:11002/metadatastore;AccountName=metadatastore;AccountKey=<Your configured account key>
```

## Configuration

This sample was developed with Visual Studio Code and uses the Azure IoT Edge extensions. Therefore most of the configuration is stored in the 'deployment.template.json' file.

### Local storage for Blobs on the edge device

- Docker volume for Blobs: **blobvolume** : Configured in the create Options of the 'blobstoremodule' module
- Storage account name: **metadatastore** : Configured in the create Options of the 'blobstoremodule' module
- Storage account key: **Not shown here** : Base64 string in the create Options of the 'blobstoremodule' module
- Blob storage connection string: **Not shown here** : Env variable defined in the create Options of the 'BlobWriterModule' module
- Container name for message blobs: **samplecontainer** : Env variable defined in the create Options of the 'BlobWriterModule' module

### Private registry for the custom developed module

Create an '.env' file at the first folder level and add the information about your private registry for the image

```yaml
mycontainers_username=<Put in your username for your private registry>
mycontainers_password=<Put in your password for the private registry>
mycontainers_address=<Put in your address for the private registry>
```

## Tips

For further tips see the [official documentation](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-store-data-blob).
