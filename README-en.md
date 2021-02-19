# Message Receiver (T2.CLS.LoggerExtensions)

[View Russian version of this README here](README.md)

## Introduction
**T2 Logging** is a comprehensive solution for collecting, storing and analysing logs. The system guarantees the integrity of logs, fast search, compressed data size.

**Receiver of messages (target)** - a component integrated into the logged application for collecting logs and sending logs to the transport. NLog framework is supported.

For more information and to familiarize yourself with **T2 Logging**, you can refer to the documentation portal: https://galaktikasoftofficial.github.io/T2.CLS.Docs/

## Project Description

The solution consists of projects:
* **T2.CLS.LoggerExtensions.Core** - general part project that implements log buffer (file), processing and sending message to transport.
* **T2.CLS.LoggerExtensions.NLog** - Target project for the NLog framework.
* **T2.CLS.LoggerExtensions.Serilog** - Target project for Serilog framework. (Still under development).

## Building the project

### Building the project on GitHub.

The build occurs automatically when changes are submitted to the origin branch (master and develop branches are supported). A release will be created based on the build results. 
You can view the releases at https://github.com/GalaktikasoftOfficial/T2.CLS.LoggerExtensions/releases.

The generated nuget packages are tied to the release and published on the internal nuget gallery: https://nuget.pkg.github.com/GalaktikasoftOfficial/index.json

### Build the project locally

To complete the assembly, you need to clone the project.

Then execute the commands:
```
dotnet restore T2.CLS.LoggerExtensions.sln
dotnet build T2.CLS.LoggerExtensions.sln
```

## Participation in development

Development is required to be done in a separate branch for each task, made on the basis of the develop branch. All changes are merged into the develop branch through a pull request.
`
