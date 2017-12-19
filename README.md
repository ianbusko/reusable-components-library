# reusable-components-library
Steps to create a reusable View Component Library in .NET Core 2.0.

## Requirements
This class library should accomplish a few goals. It should:
* Be resuable and self-contained.
* Be Testable. Components should be strongly-typed.
* Be Simple. I want to find an easy way to load components into a new site. 
* Provide Support for scripts and styles.
## Process
### Setting up View Components in an External Library

Get started by creating a new solution with two projects. The first project should be an empty .NET Core Class Library, and the second project should be an ASP.NET MVC Web Application.
