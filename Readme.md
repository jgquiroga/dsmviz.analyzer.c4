# Dsmviz.Analyzer.C4

This is a command line tool to generate DSI files from C4 diagrams.

GitHub Source Code: [https://github.com/jgquiroga/dsmviz.analyzer.c4](https://github.com/jgquiroga/dsmviz.analyzer.c4)

DSI files can be visualized using the [DSM Visualizer](https://github.com/dsmviz/dsmviz.github.io).

## C4 (Structurizr) Files

The [C4 model](https://c4model.com/) files are used to define the architecture of the system. They are written in the DSL [structurizr](https://structurizr.com/) format.

Before running this tool, you need to convert the DSL files to json files using the following docker command:

```bash
docker run -it --rm -v ${PWD}:/usr/local/structurizr structurizr/cli export -workspace workspace.dsl -format json
```

## Installation

Run the following command to install the tool:

```bash
dotnet tool install --global dsmviz-analyzer-c4 --version 0.1.0-alpha.1
```

**Notes**:

The tool requires Net core 8.0 or later.

## Usage

Execute the following command to generate the AnalyzerSettings.xml file:

```bash
dsmviz-analyzer-c4 AnalyzerSettings.xml
```

The AnalyzerSettings.xml file is used to configure the analyzer.

The following is an example of the AnalyzerSettings.xml file:

```xml
<?xml version="1.0" encoding="utf-8"?>
<AnalyzerSettings xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <LogLevel>Error</LogLevel>
  <Input>
    <Workspace>workspace.json</Workspace>
  </Input>
  <Transformation />
  <Output>
    <Filename>workspace.dsi</Filename>
    <Compress>true</Compress>
  </Output>
</AnalyzerSettings>
```

### Parameters:

- LogLevel: The log level. Possible values are Error, Warning, Info, and Debug.
- Input.Workspace: The path to the workspace file.
- Output.Filename: The path to the output file.
- Output.Compress: A flag to compress the output file.
- Transformation: The transformation settings.


## Uninstallation

Run the following command to uninstall the tool:

```bash
dotnet tool uninstall --global dsmviz-analyzer-c4
```