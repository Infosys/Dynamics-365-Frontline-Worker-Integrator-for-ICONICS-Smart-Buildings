# Deploying ICONICS BridgeWorX64 Fault Event Publisher

This document describes the process to deploy the ICONICS Fault Event Publisher onto an existing ICONICS GENESIS64 system that has FDDWorX set up to detect faults.

## Preparing for deployment

Before deploying the fault event publisher, there are 2 sets of pre-requisites that have to be met.

### ICONICS pre-requisites

1. A server with ICONICS GENESIS64 suite version 10.96.1 and above
1. FDDWorX module configured to detect and trigger fault events
1. FDDWorX configuration database connection information
1. BridgeWorX64 configuration database connection information
1. Login for a user that is allowed to create database tables in FDDWorX configuration database
1. A valid ICONICS license that also contains these additional features:
    - IoT JSON
    - BridgeWorX64
1. Deployment files:
    - Create-FDD_WorkOrderInfo.sql
    - DatabaseConnectorConfig.xlsx
    - TransactionsConfig.xml

### Azure IoT Hub pre-requisites

1. An Azure IoT Hub of S1 tier or higher
1. The IoT Hub hostname
1. Event Hub Connection String
1. Hub Owner Connection String
1. An IoT device defined in the Hub to represent the fault event publisher
1. SAS token of the IoT device

## Azure IoT Hub connection configuration in ICONICS

To publish fault events, ICONICS has to be configured to communicate with the Azure IoT Hub with the following steps:  

### Create an MQTT Broker connection to the fault event publisher device in Azure IoT Hub

The fault event publisher transaction publishes fault events to Azure IoT Hub using the MQTT protocol.  
Create an MQTT Broker connection to the fault event publisher device in Azure IoT Hub with the following steps:

1. Open the ICONICS Workbench configuration tool
1. Expand the Workbench project &rightarrow; **Internet of Things**
1. Right click on **MQTT Brokers**
1. Select **Add MQTT Broker**
1. In the configuration dialog that opens, name the broker with the same name as the fault event publisher device in Azure IoT Hub, i.e. *BXConnector*
1. In the *Broker Settings* section, set the following options:
    - For *Protocol*, select **Simple MQTT ("mqtt:")**
    - For *Server Address*, enter the IoT Hub hostname
    - For *Port*, enter **8883**
    - For *Keep Alive Period*, enter **1 Minute(s)**
    - **Check** *Clean Session*
    - **Uncheck** *Use MQTT Version 1.1*
1. In the *Security Settings* section, set the following fields:
    - For *Security Mode*, select **TLS ver. 1.2 (recommended)**
    - **Uncheck** *Use CA Cetificate...*
    - **Uncheck** *Enable Client Certificate*
    - **Check** *Use security credentials*
    - For *Username*, enter the username in the following format:
        ***[IoT Hub hostname]*****/*****[IoT device name]*****/?api-version=2018-06-30**  
        *Replace ***[IoT Hub hostname]*** with the host name of your IoT Hub  
        *Replace ***[IoT device name]*** with the device name of the fault publisher device in Azure IoT Hub
    - For *Password*, enter the SAS token of the IoT device
1. Leave the *MQTT Birth Message* and *MQTT Will Message* sections as default
1. Click **Apply** to save the configuration

### Define a decoder

To be able to decode incoming messages from Microsoft Dynamics, a decoder definition has to be defined.  
Create a decoder in ICONICS GENESIS64 with the following steps:

1. Open the ICONICS Workbench configuration tool
1. Expand the Workbench project &rightarrow; **Internet of Things**
1. Right click on **Custom Encoders/Decoders**
1. Select **Add Encoder/Decoder**
1. In the configuration dialog that opens, give the decoder a name, i.e. *JSON-Dataset*
1. In the *General Settings* section, set the following options:
    - For *Plugin*, select **CustomJson**
    - For *Message Type*, select **The whole message is a dataset**
1. Click **Apply** to save the configuration

### Create an MQTT Subscriber Connection for the fault event publisher device

To publish fault events to Azure IoT Hub, an MQTT subscriber connection is required  
Create an MQTT subscriber connection in with the following steps:

1. Open the ICONICS Workbench configuration tool
1. Expand the Workbench project &rightarrow; **Internet of Things**
1. Right click on **Subscriber Connections**
1. Select **Add Subscriber Connection**
1. In the configuration dialog that opens, name the connection with the same name as the fault event publisher device in Azure IoT Hub, i.e. *BXConnector*  
    \***Note this name as you will need it later**
1. In the *General Settings* section, set the following options:
    - **Check** *The connection is enabled*
    - **Uncheck** *Enable compatibility with ICONICS clients*
    - For *Connection Type*, set it to **MQTT**
    - For *Early Start*, set it to **0 Minute(s)**
    - For *Default Decoder*, set it to the decoder defined before in the *Define a decoder* section
    - For *Dynamic Subscription Life Time*, set it to **5 Minute(s)**
    - For *Keep Alive Timeout*, set it to **1 Minute(s)**
    - For *Browse Timeout*, set it to **1 Day(s)**
    - For *Pending Command Timeout*, set it to **30 Second(s)**
    - **Uncheck** *Enable Dynamic Publish Lists*
1. In the *Datasets Support*, set the following options:
    - For *Dataset Update Mode*, select **Overwrite values**
1. In the *MQTT Basic Settings*, set the following options:
    - For *ClientID*, enter the IoT device name
    - For *MQTT Broker*, select the MQTT Broker created before in the *Create an MQTT Broker connection...* section
    - For *Base Topic*, enter the topic in the following format:
        **devices/*****[IoT device name]*****/messages/events/#**  
        *Replace ***[IoT device name]*** with the name of the fault publisher device in Azure IoT Hub
    - For *Quality of Service*, select **At Most Once**
1. In the *MQTT Security Settings* section, leave all options unchecked
1. In the *MQTT Topic Management* section, set the following options:
    - For *DeviceID Location*, select **DeviceID is specified in the topic**
    - For *DeviceID*, enter the IoT device name
    - **Uncheck** *Each Value is sent with its own MQTT message*
1. In the *MQTT Back Channel* section, leave all options unchecked
1. Click **Apply** to save the configuration

### Create an Azure IoT Hub Subscriber Connection to receive work order updates

To receive work order updates from Dynamics, an Azure IoT Hub subscriber connection is required.  
Configure an Azure IoT Hub subscriber connection with the following steps:

1. Open the ICONICS Workbench configuration tool
1. Expand the Workbench project &rightarrow; **Internet of Things**
1. Right click on **Subscriber Connections**
1. Select **Add Subscriber Connection**
1. In the configuration dialog that opens, give the connection a name to represent the IoT Hub, i.e. *SmartBuildingsIoTHub-All*
1. In the *General Settings* section, set the following options:
    - **Check** *The connection is enabled*
    - **Uncheck** *Enable compatibility with ICONICS clients*
    - For *Connection Type*, set it to **Azure IoT Hub**
    - For *Early Start*, set it to **0 Minute(s)**
    - For *Default Decoder*, set it to the decoder defined before in the *Define a decoder* section.
    - For *Dynamic Subscription Life Time*, set it to **5 Minute(s)**
    - For *Keep Alive Timeout*, set it to **1 Minute(s)**
    - For *Browse Timeout*, set it to **1 Day(s)**
    - For *Pending Command Timeout*, set it to **30 Second(s)**
    - **Uncheck** *Enable Dynamic Publish Lists*
1. In the *Datasets Support*, set the following options:
    - For *Dataset Update Mode*, select **Overwrite values**
1. In the *IoT Hub Settings* section, set the following options:
    - For *Event Hub Connection String*, enter the Event Hub Connection String of the Azure IoT Hub
    - For *Hub Owner Connection String*, enter the Hub Owner Connection String of the Azure IoT Hub
    - **Uncheck** *Custom Consumer Group*
    - **Uncheck** *Use Epoch*
    - Leave the *Partition Filter* field blank
1. In the *Early Start Settings* section, leave all options as default
1. Click **Apply** to save the configuration

### Starting the Subscriber service

The Internet of Things Subscriber service has to be started for the publishing process to work.  
Start the Subscriber service from Workbench with the following steps:

1. Open the ICONICS Workbench configuration tool
1. Expand the Workbench project
1. Select **Internet of Things**
1. In the Home ribbon, click on the traffic light icon for *Subscriber*
1. The traffic light should turn green when the service is started

For production use, the Subscriber service should be set to start automatically by Windows.  
Set the Subscriber service to start automatically with the following steps:

1. From the Start menu, search for **Services**
1. In the Services dialog, look for **ICONICS Internet of Things Subscriber Service**
1. Right click on the service
1. Select **Properties**
1. Change the *Startup type* to **Automatic**
1. Click **Apply** to save the changes
1. Click **OK** to close the dialog

## Database table creation and connection configuration

To store and display work order updates, a database table must be created and neccessary connections configured.
Create the database table and configure the connection with the following steps:

### Create Work Order Info table

To store work order updates from Dynamics, a new database table, named *FDD_WorkOrderInfo*, needs to be created in the FDDWorX configuration database.  
Create the new database table with the following steps:

1. Double click on **Create-FDD_WorkOrderInfo.sql**
1. The SQL Management Studio should be launched
1. Login to the SQL server with credentials that allow you to create database tables
1. At the top of the SQL query, type in **Use** ***[FDDWorX configuration database name]***  
*Replace ***[FDDWorX configuration database name]*** with the name of your FDDWorX configuration database.
1. Execute the query using the Execute button at the top toolbar
1. The query should be executed successfully

### Deploy the database connector

For the transaction engine to store the work order updates, a database connection to the Work Order Info table is required.  
Deploy the database connector with the following steps:

1. Open the ICONICS Workbench configuration tool
1. Expand the Workbench project &rightarrow; **Data Connectivity** &rightarrow; **Databases**
1. Right click on **SQL Connections**
1. Select **Import**
1. In the *Import Options* dialog, set the following options:
    - For *Import Mode*, select **Create and Update**
    - For *File*, click on the folder icon next to the field and select the import file, **DatabaseConnectorConfig.xlsx**
    - Click on **OK**
1. The import process shows up in the *Recent Tasks* pane on the right as a task
1. Once the import process is completed successfully, right click on **SQL Connections** and select **Refresh**
1. Right click on **FDD**
1. Select **Edit**
1. Click on the **Configure Connection** link
1. Update the connection information with your FDDWorX configuration database connection information
1. Click **OK** to save your changes
1. Click **Apply** to save your configuration
1. Right click on **BWXConfig**
1. Select **Edit**
1. Click on the **Configure Connection** link
1. Update the connection information with your BridgeWorX64 configuration database connection information
1. Click **OK** to save your changes
1. Click **Apply** to save your configuration

### Starting the ICONICS database connector service

The ICONICS database connector service has to be running to read and write to the configured database.  
Start the ICONICS database connector service from Workbench with the following steps:

1. Open the ICONICS Workbench configuration tool
1. Expand the Workbench project &rightarrow; **Data Connectivity**
1. Select **Databases**
1. In the **Home** ribbon, click on the traffic light icon for *Point Manager*
1. The traffic light should turn green when the service is started

For production use, the ICONICS database connector service should be set to start automatically by Windows.  
Set the ICONICS database connector service to start automatically with the following steps:

1. From the Start menu, search for **Services**
1. In the Services dialog, look for **ICONICS GridWorX Point Manager**
1. Right click on the service
1. Select **Properties**
1. Change the *Startup type* to **Automatic**
1. Click **Apply** to save the changes
1. Click **OK** to close the dialog

## Deploy fault event publisher transaction

The fault event publisher is an ICONICS BridgeWorX transaction that receive fault events from FDDWorX and publishes each event to IoT Hub.  
The events published to IoT Hub then get created as IoT Alerts in Microsoft Dynamics 365 by the cloud portion of the connector.

### Import the fault event publisher transaction package

Deploy the fault event publisher transaction with the following steps:

1. Open the ICONICS Workbench configuration tool
1. Right click on **Bridging**
1. Select **Import**
1. In the *Import Options* dialog, set the following options:
    - For *Import Mode*, select **Create and Update**
    - For *File*, click on the folder icon next to the field and select the import file, **TransactionsConfig.xml**
    - Click on **OK**
1. The import process shows up in the *Recent Tasks* pane on the right as a task
1. Once the import process is completed successfully, you should see *DynamicsConnector* under TransactionsConfig
1. Right click on **DynamicsConnector**
1. Select **Edit**
1. In *Generic Properties*, **Check** *Active Configuration*
1. Click **Apply** to save the configuration

### Update transaction configuration

The imported transaction package needs to be modified to match your defined IoT Hub connection.  
Update the transaction with your IoT Hub connection with the following steps:

#### Update Work Order Update transaction

1. Open the ICONICS Workbench configuration tool
1. Expand the Workbench project &rightarrow; **Bridging** &rightarrow; **Transactions** &rightarrow;  **DynamicsConnector**
1. Right click on **Process-Work-Order-Update**
1. Select **Edit**
1. From the transaction details dialog on the right, select **Alarm Subscriptions**
1. In the *Alarm Subscription* section, click on **iot:SmartBuildingsIoTHub-All/**
1. Click on the tag browser icon to the right of the highlighted address
1. From the Data Browser that pops up, browse to **My Computer** &rightarrow; **Internet of Things**
1. Select the subscriber connection name that was created before in the *Create an Azure IoT Hub subscriber connection...* section
1. Click **OK** to confirm selection
1. Click **Apply** to save the configuration

#### Update Fault Publisher transaction diagram

1. Open the ICONICS Workbench configuration tool
1. Expand the Workbench project &rightarrow; **Bridging** &rightarrow; **Templates** &rightarrow; **FDD**
1. Right click on **Publish-Faults-to-IoTHub**
1. Select **Edit**
1. Select the **Create JSON payload for publishing** block
1. In the *Information Broker Activity* section, update the following options:
    - For *Publish Point*, replace ***BXConnector*** with the subscriber connection name created before in the *Create an MQTT Subscriber Connection...* section
    - For *MQTT Topic*, replace ***BXConnector*** with the IoT device name
1. Click **Apply** to save the configuration

## Starting the BridgeWorX64 service

The fault event publisher uses ICONICS BridgeWorX64 to publish the fault events. The BridgeWorX64 service has to be running to publish faults to IoT Hub.  
Start the BridgeWorX64 service from Workbench with the following steps:

1. Open the ICONICS Workbench configuration tool
1. Expand the Workbench project
1. Select **Bridging**
1. In the **Home** ribbon, click on the traffic light icon for *Point Manager and Scheduler*
1. The traffic light should turn green when the service is started

For production use, the ICONICS BridgeWorX64 services should be set to start automatically by Windows.  
Set the ICONICS BridgeWorX64 service to start automatically with the following steps:

1. From the Start menu, search for **Services**
1. In the Services dialog, look for **ICONICS BridgeWorX64 Point Manager**
1. Right click on the service
1. Select **Properties**
1. Change the *Startup type* to **Automatic**
1. Click **Apply** to save the changes
1. Click **OK** to close the dialog
1. In the Services dialog, look for **ICONICS BridgeWorX64 Scheduling Service**
1. Right click on the service
1. Select **Properties**
1. Change the *Startup type* to **Automatic**
1. Click **Apply** to save the changes
1. Click **OK** to close the dialog
