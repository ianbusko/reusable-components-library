# reusable-components-library
Steps to create a reusable View Component Library in .NET Core 2.0.

## Requirements
This class library should accomplish a few goals. It should:
* Be resuable and self-contained.
* Be Testable. Components should be strongly-typed.
* Be Simple. I want to find an easy way to load components into a new site. 
* Provide Support for scripts and styles.

## Process

### Setting up the project
Get started by creating a new solution with two projects. The first project should be an empty .NET Core Class Library called `ComponentLibrary`, and the second project should be an ASP.NET MVC Web Application called `ComponentLibrary.UI`. The MVC application will be used to test out our library.

### Creating a View Component
First we need to create a View Component. For this exercise, we're going to copy the navbar from the default page layout. Later on we'll create a more complex example the includes a model. 

To get started, create a folder called `ViewComponents` in the `ComponentLibrary` project. Inside that folder, create a class called `NavComponent` that looks like this:
```C#
using Microsoft.AspNetCore.Mvc;

namespace ComponentLibrary.ViewComponents
{
    public class NavComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
```
Note: you'll also need to install the `Microsoft.AspNetCore.Mvc` NuGet package for this to work.

Next, you'll need to create the actual View returned by the ViewComponent. It seems like .NET Core is opinionated about this part: you'll need to put it in the folder `Views\Shared\Components\[ViewComponentName]\Default.cshtml`.
Let's copy the navigation bar out of our default project. Create the file `Views\Shared\Components\NavComponent\Default.cshtml`, then copy these contents into it:
```HTML
<nav class="navbar navbar-inverse navbar-fixed-top">
    <div class="container">
        <div class="navbar-header">
            <button type="button" class="navbar-toggle" data-toggle="collapse" data-target=".navbar-collapse">
                <span class="sr-only">Toggle navigation</span>
                <span class="icon-bar"></span>
                <span class="icon-bar"></span>
                <span class="icon-bar"></span>
            </button>
            <a asp-area="" asp-controller="Home" asp-action="Index" class="navbar-brand">ComponentsLibrary.UI</a>
        </div>
        <div class="navbar-collapse collapse">
            <ul class="nav navbar-nav">
                <li><a asp-area="" asp-controller="Home" asp-action="Index">Home</a></li>
                <li><a asp-area="" asp-controller="Home" asp-action="About">About</a></li>
                <li><a asp-area="" asp-controller="Home" asp-action="Contact">Contact</a></li>
            </ul>
        </div>
    </div>
</nav>
```

Finally, we need to set the class library to embed the Views into the output. 
