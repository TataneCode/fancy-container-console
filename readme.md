# Fancy container controller
The global aim of this application is to offer some basics container management function (docker for the MVP)
The project will be done using last .net10 release.

# Architecture of the solution
## Overall Architecture
This is a domain centric architecture project, for simplicity, all layers will be handled in the same project. The implementation of netarchtest nuget package will control that UI / Infrastructure / Application / domain layers are not imported wherever. A simple UT project with xunit / fluent assertion / moq will cover around 70%

## Dtos / Models / entities
Each layer has its own object model, mappers allows to pass the expected format between them.

## System compatibility
The application must be compatible with linux and windows systems. The application might need to be run with sudo in linux.

## UI
The UI will be provided with spectre.console.

# Functionnalities
## MVP
### Docker Functionnalities
- display all docker containers
- select a container among the list
- display logs of the container
- start / stop / delete container
