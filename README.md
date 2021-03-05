# Dynamics 365 Frontline Worker Integrator for ICONICS Smart Buildings

This project provides a reference solution to create Dynamics Field Service IoT Alerts from ICONICS faults.

The solution provides one approach for ICONICS faults to integrate with Dynamics Field Service.  As ICONICS faults are generated, the fault event flows to the solution and from there to Dynamics Field Service as an IoT Alert.  Once the IoT Alert is created, a Dynamics Field Service user may elect to create a work order from the alert.  The creation of a work order will trigger a response, or acknowledgement, event to be sent back to ICONICS.

Please refer to the [Connector setup](./docs/Connector-Setup.md) for more information on how to get started with the solution, including more details on the high-level architecture.

Please refer to the [Dynamics 365 setup](./docs/Dynamics-365-Setup.md) for more information on how to setup the Dynamics Field Service instance for use with the solution.

## Microsoft Open Source Code of Conduct

This project adheres to the [Microsoft Open Source code of conduct](https://opensource.microsoft.com/codeofconduct).

## Trademark notice

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general). Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party's policies.

## Security reporting instructions

Please view security reporting instructions [here](SECURITY.md).
