# HotelListing.API
.NET 6 Hotel Listing API


This is an application largely from the "Ultimate ASP.NET Core Web API Development Guide" course on Udemy from Trevoir Williams. There are some small differences between the two such as this repo has more DTO classes for granularity and a cleaner (in my opinion) naming scheme for novelty endpoints that are only there to demonstrate a specific feature. There are also a lot more comments in this repo due to me wanting to refer back to it later.


What this application does is serve as a RESTful API that returns data on countries and the hotels contained within them. In order to run this repo on your machine locally simply download the repo and run the update-database command from the Nuget package manager console to import the database data and schemo into the local SQL server. 

The database was created through code-first development using Entity Framework.

This application includes local logging to text files as well as logging using Seq(https://datalust.co/seq). If you want to use Seq as well, you will need to download and install the Seq program. This application also incorporates authentication and roles using JWT authentication and Microsoft Identity framework. Authentication is also used in the SwaggerUI. Versioning is also configured in this application. You can specify the version as a query string, in the URL, or in the http header.

There are also a couple pieces of middleware configured in the application to aid in functionality. One of these is an exception handling helper that acts as a global try-catch statement for handling Http request errors. The other middleware incorporates caching into the program. 

This application also incorporates DTO's and Automapper to efficiently handle the mapping between Entity Framework models and client models.

Key technologies in this project: .NET Core 6, Entity Framework, JWT Authentication, Roles, Logging via Seq, Automapper, OData, Identity Framework
