# Introduction

Device Simulator will simulate an Azure IoT Device. It supports following:

- Send Device-to-Cloud messages to IoT Hub
- Receive Cloud-to-Device messages from IoT Hub
- Passable Parameters (optional) to the Simulator with following options:
  - Number of messages to be sent
  - Time interval between the messages
  - Input file whose contents to be used while sending message
  - Output file to store the messages received
  - Wait time waiting for receiving messages

## Prerequisites

We will need the following prerequisites:

- Setup your IoT hub (We will be using primary connection string in our code)
- Azure IOT device (We will be using primary connection string in our code)

## Build and Test

- Modify the Properties\launchSettings.json file with appropriate `IoTHubDeviceConnString` string
- Build and run the program

## Usage

DeviceSimulator [options]

Options:

| Name              | Description                                                        | How to use                   | Default value   |
|-------------------|--------------------------------------------------------------------|------------------------------|-----------------|
| Count             | Number of messages to be sent to IoT Hub                           | `--count <count>`            | 1               |
| Interval          | Interval in which the messages to be sent                          | `--interval <interval>`      | 30s             |
| Input File        | Name of input file whose content to be used to send message        | `--inputfile <inputfile>`    |                 |
| Output File       | Name of the output file to save the messages recieved from Iot Hub | `--outputfile <outputfile>`  | OutputData.json |
| Wait Time         | Wait time to receive messages from IoT Hub                         | `--waittime <waittime>`      | 60s             |

- Notes:

  - Input File: By default, we are not providing the content through any input file. However, we can use an input file to provide a sample fault.
  - Wait Time:  If the time required to send the messages exceeds the wait time, then wait time will automatically be increased.

- Examples:

  - `dotnet run DeviceSimulator -- --count 5`
  - `dotnet run DeviceSimulator -- --count 10 --interval 20`
  - `dotnet run DeviceSimulator -- --count 10 --interval 20 --inputfile "SampleInputData.json"`
  - `dotnet run DeviceSimulator -- --inputfile "SampleInputData.json" --outputfile "OutputData.json"`
  - `dotnet run DeviceSimulator -- --outputfile "OutputData.json" --waittime 60`

## References

[Azure IOT Hub and device](https://docs.microsoft.com/azure/iot-hub/iot-hub-create-through-portal)

[Received Cloud-to-Device Messages](https://docs.microsoft.com/azure/iot-hub/iot-hub-csharp-csharp-c2d)
