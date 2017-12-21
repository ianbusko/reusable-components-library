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

Finally, we need to set the class library to embed the Views into the output. Open up the `.csproj` for the library and set it to embed the views using this line:
```XML
 <ItemGroup>
   <EmbeddedResource Include="Views/**/*.cshtml" Pack="true" />
 </ItemGroup>
```

At this point, we have a library with a single class and a view in it. In the next section, we're going to see how to load it into a View.

### Using the Library
Start by opening up your Web Application project. On the `_Layout` view, replace the default navigation markup with this:
``` C#
@await Component.InvokeAsync(nameof(ComponentLibrary.ViewComponents.NavComponent))
```

At this point, Intellisense should give you an error because we haven't actually loaded the library yet. 

Add a reference to `ComponentLibrary` from `ComponentLibrary.UI`. 

Now we need to tell the Razor Engine to load Views from inside the library. In `Startup.cs` in the `ConfigureServices` method, add this snippet of code:
```C#
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();

    services.Configure<RazorViewEngineOptions>(options =>
    {
        options.FileProviders.Add(new EmbeddedFileProvider(typeof(ComponentLibrary.ViewComponents.NavComponent)
            .GetTypeInfo().Assembly));
    });
}
```

At this point, you should be able to run your web application and you will see the same old nav bar at the top. Real neat, but we can make it better.

### Moving Configuration to an Extension
The first improvement I would make is to remove the verbose code from `ConfigureServices` method and into a method in the library. Right now it's fairly small, but we might want to add more to it later. It would be really nice if the consuming site only had to include one line of code. 

The good news is that we can do that fairly easily. Hop on back to the `ComponentLibrary` project. In the root directory, create a new static class called `Extensions.cs`. Add a single method to it called `AddComponentLibraryViews` that extends `IServiceCollection`. Then, move the logic we wrote to utilize the embedded Razor Views in `Startup.cs` into that method. It should look something like this:
```C#
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace ComponentLibrary
{
    public static class Extensions
    {
        public static void AddComponentLibraryViews(this IServiceCollection services)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(new EmbeddedFileProvider(typeof(ComponentLibrary.ViewComponents.NavComponent)
                    .GetTypeInfo().Assembly));
            });
        }
    }
}
```
Note: At this point, the only new NuGet package you should need to add to the `ComponentLibrary` projects is `Microsoft.Extensions.FileProviders.Embedded`.

Now, jump back over to `startup.cs` in your MVC application and replace the above section with this line:
```C#
services.AddComponentLibraryViews();
```

At this point, build your project again and run it to make sure everything still works. Now you only have a single line of code in your Startup Configuration that loads up all the embedded components from the Component Library.