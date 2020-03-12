# sensenet Project templates
This repo contains project templates for [sensenet](https://github.com/SenseNet/sensenet) web application projects.

All templates are based on one of the default **ASP.NET Web Application** templates. All projects are extended with **NuGet packages** of one or more sensenet [components](http://community.sensenet.com/docs/sensenet-components). 

As a developer you can make use of these templates in several ways:

- *copy* one of these projects manually as a starting point, when you want to kickstart a **new sensenet solution**. This will save you the process of creating a new web application and importing all the necessary sensenet packages.
- use these projects in your *builds* as a skeleton of a complete website.

> Optional: you may want to **update NuGet packages** before using the templates, because there may be newer versions available of those components than the installed ones in these templates.

> As sensenet consists of many optional components, it is not feasible to provide templates for all combinations. If you think we should add a new project template with a particular package combination, please contact us - or better yet, add and [contribute it](CONTRIBUTING.md) yourself :).

## .Net Core templates
The .Net Core templates are the ones that we are actively developing and updating. This is a list of currently available templates and their contents. 

> You'll find these in the `src\netcore` subfolder. All projects are collected in the `SenseNet.Web.All` solution. We try to give appropriate names to these projects so you can see their purpose and environment immediately.

#### SnConsoleInstaller
This project is a helper console application that is able to **install sensenet** into the defined **database** (and optionally import additional custom content items - see the app configuration file for details).

##### Installing a new content repository

1. create a new empty database manually
2. set the connection string in the configuration file of the tool
3. execute the tool

The steps above will create the content repository tables in the db and add default content items. The index will be created in the file system. You can use these in the following way:

- set the connection string for this new db in the configuration file of a sensenet web app you have
- copy the index to the `App_Data` folder of the web app (in case of local index) or the search service (in case of centralized index)

#### SnWebApplication.Api.InMem.TokenAuth
This is the most simple template that contains in-memory storage and can be started immediately without installation and dependencies. The drawback? It will start with the same minimal content repository every time you start the app, no persistence storage is available.

- web: only REST api, no UI
- db: in memory
- index: in memory
- authentication: JWT bearer token (requires external IdentityServer)

#### SnWebApplication.Api.Sql.SearchService.TokenAuth
- web: only REST api, no UI
- db: SQL db (requires installation)
- index: external search service
- authentication: JWT bearer token (requires external IdentityServer)

#### SnWebApplication.Api.Sql.TokenAuth
- web: only REST api, no UI
- db: SQL db (requires installation)
- index: file system
- authentication: JWT bearer token (requires external IdentityServer)

#### SnWebApplication.Client.JavaScript
This is a test client application for validating the services represented by the other templates. It is a static Html/JavaScript app with no sensenet references, only containing authenticated sample REST calls.

#### SnWebApplication.Mvc.Sql.Oidc
This project is different from the others because it uses a server-side **OIDC authentication** (as opposed to token-based authentication used by the others). This means that an SPA (that works with JWT tokens) will *not* be able to connect to this app and use it as a service.

- web: MVC sample controllers and views
- db: SQL db (requires installation)
- index: file system
- authentication: OpenId Conntect (requires external IdentityServer)

#### SnWebApplication.Mvc.Sql.SearchService.LocalUserStore
- web: MVC sample controllers and views
- db: SQL db (requires installation)
- index: external search service
- authentication: default authentication and default Asp.Net views for registration and login. The *Asp.Net Identity* user storage however is the content repository itself. This means that the persistence storage layer for users created on the UI is the sensenet repository and users will become content items themselves.

#### SnWebApplication.Mvc.Sql.SearchService.TokenAuth
- web: MVC sample controllers and views
- db: SQL db (requires installation)
- index: external search service
- authentication: JWT bearer token (requires external IdentityServer)