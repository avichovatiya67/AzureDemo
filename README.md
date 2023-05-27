# SmartMedic Azure Functions - [v4.5.0](#latest-version)


The SmartMedic Azure Functions are used to communicate the Telemetry and Non-Telemetary events with the Iot Hub Twin properties and 
interact with the cosmos db for data manipulations. 
These functions are handling all the events from the tablets/devices and perform the functionalities based on the flow.
The project is based on c# azure function and asp.net core. You must have a good understanding of c# and Azure functions.


![Stryker Logo](https://www.stryker.com/etc/designs/stryker/images/header/logo.png)

# Topic covered

- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Jira and Version Control Workflow](#jira-and-version-control-workflow)
- [Pre Requisites](#pre-requisites)
- [Development](#development)
- [Deployment](#deployment)
- [Release Version History](#release-version-history)
- [Authors](#authors)
- [License](#license)


# Architecture

The SmartMedic Azure Functions uses `Serverless architecture`.Serverless architecture or computes service. Using Azure Function, we can run the event-based code without managing any infrastructure. Since the Azure function is a trigger-based service, so it always executes a script of a block of code in response to a variety of events. It alllows achieving the decoupling, high throughput or response, reusability of code, etc.

This project is divided in to 4 sub-projects:

a) Smartmedic Models- This includes all the c# models for each events and cosmos db containers.

b) Non-Telemetry Functions-  It includes the EventHub Trigger Azure Function which trigger based on the Non-Telemetary Event type sent from the tablet to MessageProcessor 
azure function, based on which appropriate non-telemetary function will get executed to update the data in the database (Cosmos db).

c) BedSide API  Functions - These are the Http based Trigger Azure function used for fetching Bedlists and iculist based on icu and hospital.

d) EventHub Insert Telemetary - It includes the EventHub Trigger Azure Function which trigger based on the Telemetary Event type sent from the tablet to Iot-Hub,
   Based on which the telemetary properties or twin properties of the Iot devices will get updated.

<a id="tech-stack"></a>
# Tech Stack

- Azure Function (v3)
- Microsoft .NET Framework Version 4.8.04084
- Tools: [Visual Studio](https://visualstudio.microsoft.com/vs/older-downloads/) (Professional), [SourceTree](https://www.sourcetreeapp.com/)

<a id="jira-and-version-control-workflow"></a>
# Jira and Version Control Workflow

We have used BitBucket as a version control system. Along with that, SourceTree is used for managing the repository and perform actions such as push, pull, etc.

The workflow for developing a feature/ resolving a bug is as follows:

1. A Jira ticket needs to be created with details and tagged with Sprint or Release version. No development/ documentation will be taken up without a Jira ticket.
2. Any communication regarding the feature/ bug should be done in the ticket history itself.
3. While developing, a feature branch should be created prior to committing the code.
4. To merge the changes in `master`, a `PR (Pull Request)` should be created and linked to the Jira ticket. The reviewer should add comments, either improvements or appreciation.
5. After merging the branch with the master, it should be deleted to reduce the clutter.
6. The status of the respective Jira ticket should be updated with a proper closure comment describing what change has been done.
7. Make sure that DevOps build done not fail.
8. Update necessary documents.
9. Appropriately tag the commit (if applicable).

<a id="pre-requisites"></a>
# Pre Requisites

- C# (Minimum: 10.x, Recommended: 12.x or above) C# Tools   3.11.0-4.21403.6+ae1fff344d46976624e68ae17164e0607ab68b10
- SourceTree (For managing the repo)
- Target Framework : netcoreapp3.1
- Microsoft Visual Studio Professional 2019 Version 16.11.8
- VisualStudio.16.Release/16.11.8+32002.261
- Microsoft .NET Framework Version 4.8.04084
- ASP.NET and Web Tools 2019   16.11.94.52318
- ASP.NET and Web Tools 2019
- ASP.NET Web Frameworks and Tools 2019   16.11.94.52318
- Azure App Service Tools v3.0.0   16.11.94.52318
- Azure App Service Tools v3.0.0
- Azure Functions and Web Jobs Tools 16.11.94.52318
- Azure Functions and Web Jobs Tools
- Common Azure Tools 1.10
- GitHub.VisualStudio 2.11.102.28613
- Microsoft Azure HDInsight Azure Node   2.6.1000.0
- Microsoft Azure Tools for Visual Studio 2.9
- Microsoft Visual Studio VC Package   1.0
- NuGet Package Manager   5.11.0
- Razor (ASP.NET Core)   16.1.0.2122504+13c05c96ea6bdbe550bd88b0bf6cdddf8cde1725

<a id="development"></a>
# Development

- Clone the repository and open it in your choice of Editor/ IDE
- After this, Right click on Solution Explorer and click on Clean Solution and then click on Build or Rebuild the Solution. 
- If there is some issue in the package related dependency, use the Nudget package Manager to install the dependency.
- Then, again Rebuild the Project.
- Just Run the Azure Function by clicking on the Azure Function or using F5

<a id="deployment"></a>
# Deployment

For deploying the application into production:

1. Tag the commit to be released.
2. Increment the release version while releasing.
3. Attach Release Notes with the release.
4. Release the application from SmartMedic Azure Function [App Release Portal]

> <br/>
> NOTE: Contact your Admin for login credentials of the App Release Portal <br/>
> <br/>

<a id="release-version-history"></a>
# Release Version History

<a id="latest-version"></a>
- `v4.5.0` (12-04-2022)

<a id="authors"></a>
# Authors

- [Stryker](https://www.stryker.com/us/en/index.html)

<a id="license"></a>
# License

```
@Stryker 1998-2022

Copyright (c) 2022 Stryker

This software is copyrighted by and is the sole property of Stryker.
All rights, title, ownership, or other interests in the software remain the property
of Stryker. This software may only be used in accordance with
the corresponding license agreement. Any unauthorized use, duplication, transmission,
distribution, or disclosure of this software is expressly forbidden.

This Copyright notice may not be removed or modified without written consent of Stryker.
Stryker reserves the right to modify this software.

