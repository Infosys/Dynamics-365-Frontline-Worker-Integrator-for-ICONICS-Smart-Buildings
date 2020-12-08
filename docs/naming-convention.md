# Naming conventions for Azure resources

This topic describes the naming convention to use when creating Azure Resources for continuous integration (CI) and continuous deployment (CD) pipelines. This convention follows this format:

`<Group ID><Environment><Region><Resource type>-<Optional info>`

Based on each identifier that's described in this topic, here are some examples:

| Group ID | Environment | Region | Resource type  | Optional info | Resource name |
|----------|-------------|---------------|--------|---------------|---------------|
| abc01 | Development | West US | Storage account V2  | Ingest | abc01dwestussa2ingest |
| abc01 | Test | Central US | Logic app | Sample Service Bus connection | abc01tcentralusls-sample-sb-conn |
| abc01 | Development | Central US | Resource group  | Shared resources | abc01dcentralusrg-shared |
|||||||

## Group ID

This label describes the higher-level functionality for a resource group because you can define multiple resource groups for a specific business solution.

## Environment

This character represents the environment where the resource is used, for example:

| Environment | Character |
|-------------|-----------|
| Development | d |
| Test | t |
| Integration | i |
| Production | p |
|||

## Region

The region where to deploy the resource as defined by the resource definition's `.location` property. For regions with long names, such as North Central US, you can use the designated abbreviation, for example, "West US" is `westus` and "Central US" is `centralus`.

## Resource type

These characters are an acronym for the resource type, for example:

| Resource type | Acronym |
|---------------|---------|
| Storage account | sa |
| Storage account V2 | sa2 |
| Function app | fa |
| Logic app | la |
| Integration account | iact |
| Service Bus | sb |
| Network security group | nsg |
| Resource group | rg |
| Event hub | eh |
| Event Grid subscription | egs |
| Log Analytics workspace | law |
| Log Analytics solution | las |
|||

## Optional information or identification

If a solution uses more than one resource that has the same type, such as multiple storage accounts or function apps, those resources might need more information to identify its purpose. If the resource name permits, add a hyphen and a suitable label for this information.
