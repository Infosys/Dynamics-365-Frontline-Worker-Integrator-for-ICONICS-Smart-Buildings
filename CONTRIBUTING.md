# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to,
and actually do, grant us the rights to use your contribution. For details, visit [https://cla.microsoft.com](https://cla.microsoft.com).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).

For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Guidance

### Branch naming convention

For this project, the primary branch is named **main**, for all repos.

When working on a Story or Task, create a new branch from **main**. include at least your username and story number (something like `<username>/<storynumber>`). It may be helpful to extend this with a short human-readable description as well, such as `<username>/<storynumber>-<short-description>`.

In this repo, we use the following branch naming conventions:

| Branch Type | Pattern | Example |
| - | - | - |
| Feature _or_ bug fix | username/\<work-item-number#>-\<short description> | smith/498-reorganize-scm-section |

> **Note:**
>
> * Mind the capitalization of the branch prefix (feature, fix). Tools that display branches as a hierarchy are typically case sensitive, and will display different hierarchies for the same words with different capitalization.

### Merging strategy

The preferred strategy for this repo is **linear** commit history with a **squash** merge.

### Linting

Markdown linting can be done from either Visual Studio Code, or from a command prompt / command line.

#### Visual Studio Code

If you use VSCode as your preferred editor, please install the [markdownlint extension](https://marketplace.visualstudio.com/items?itemName=DavidAnson.vscode-markdownlint) and ensure that all rules are followed. This will help ensure consistency in the look and feel of the documentation in this repo.

#### Command Line

If you want to run the markdown linter from a command line, please first install the [markdownlint-cli](https://www.npmjs.com/package/markdownlint-cli) package.  You can run the linter by executing the following command from a command prompt:

```bash
npm run lint
```

The project contains a _package.json_ file with a reference to the _markdownlint-cli_ package, as well as a script to run the linter.

### Testing

Ensure the tests pass.

This project uses [xUnit](https://xunit.net/) as the unit testing framework.

### Developer pre-reqs

The following tools are needed for this project:

* [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio 2019](https://visualstudio.microsoft.com/vs/)
* [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli?view=azure-cli-latest) (version 2.11.1 or higher)
* [Azure Functions Core Tools](https://docs.microsoft.com/azure/azure-functions/functions-run-local) (version 3.0.2881 or higher)
* [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)
* [Dynamics Field Service](https://docs.microsoft.com/dynamics365/field-service/overview)
* Service Principal to access Dynamics Field Service
* Service Principal that has contributor rights to at least the resource group being deployed to. In case of conditional creation of the resource group, contributor rights to the subscription for deployment.

### Contributions and Pull requests

All contributions should be done via a pull requests.  Pull requests must be associated with work item / issue.

### Directory hierarchy

The project uses the following directory hierarchy:

| Folder path | Description |
| -- | -- |
|.azdo\pipelines | Azure DevOps pipelines (build and release pipelines). |
|.azdo\pipelines\cd | Azure DevOps continuous deployment (CD) pipelines,. |
|.azdo\pipelines\ci | Azure DevOps continuous integration (CI) pipelines. |
|.azdo\pipelines\templates | Azure DevOps deployment templates. |
| \deployment | All templates, scripts, etc. needed to deploy the assets. |
| \deployment\azure | Deployment of Azure-related assets. |
| \deployment\azure\function-apps | Templates and/or scripts to deploy the Azure Function app(s). |
| \deployment\azure\iot-hub | Templates and/or scripts to deploy Azure IoT Hub. |
| \deployment\azure\key-vault-secrets | Templates and/or scripts to deploy Azure Key Vault secrets. |
| \deployment\azure\logic-apps | Templates and/or scripts to deploy Azure Logic Apps. |
| \deployment\azure\scripts | Shell scripts to aide in deployment. |
| \deployment\azure\stream-analytics | Templates or scripts to deploy Azure Stream Analytics and related jobs. |
| \docs | Documentation related to the project. |
| \src | All compiled source code. |
| \src\azure | Source code related to Azure. |
| \src\azure\device-simulator | Source code related to an Azure IoT device simulator. |
| \src\azure\dynamics-connector | Source code related to the Connector. |

Below is an example of the directory structure:

```plaintext
\.azdo
    \pipelines
        \cd
            - dynamics-connector-cd.yml
        \ci
            - dynamics-connector-ci.yml
        \templates
            - deploy-azure.yml
\deployment
    \azure
        \function-apps
            - azure-deploy.json
            - azure-deploy.parameters-d.json
        \logic-apps
            \work-order-ack
                - azure-deploy.parameters-d.json
                - azure-deploy.json
        - azure-deploy.json
        - azure-deploy-parameters-d.json
\docs
    \assets
    - Connector-Setup.md
\src
    \azure
        \dynamics-connector
            - dynamics-connector.sln
    - README.md
- CONTRIBUTING.md
- README.md
```
