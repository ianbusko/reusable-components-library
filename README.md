# Building a reusable ViewComponent in ASP.NET Core 2.0
Steps to create a reusable View Component Library in .NET Core 2.0.

## Requirements
The goal is to create a library of View Components, scripts and styles that can be packaged and used quickly and easily. Ideally, it could be packaged for use in NuGet.
There are a few requirements. The library should:
* Be resuable and self-contained.
* Provide strong typing.
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

Next, you'll need to create the actual View returned by the ViewComponent. By default, .NET Core will look for a file with this folder structure: `Views\Shared\Components\[ViewComponentName]\Default.cshtml`. You can change this by editing the path of the returned View.
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

At this point, we have a library with a single class and a view in it. In the next section, we're going to see how to load use it.

### Using the Library in a Project
Start by opening up your Web Application project (`ComponentLibrary.UI`). On the `_Layout` view, replace the default navigation markup with this:
``` C#
@await Component.InvokeAsync(nameof(ComponentLibrary.ViewComponents.NavComponent))
```

At this point, Intellisense should give you an error because we haven't actually loaded the library yet. 

Add a reference to the `ComponentLibrary` project from `ComponentLibrary.UI`. 

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

Build your project again and run it to make sure everything still works. Now you only have a single line of code in your Startup Configuration that loads up all the embedded components from the Component Library.

### Including Scripts
For the next phase of the project, we're going to add some JavaScript into our class library. The goal here would be that any scripts necessary to make your ViewComponents work should be rolled into the project. 

First, set create a folder in the root of your library called `Scripts`. In that folder, add a JavaScript file called `Script.js` with a `console.log` in it. Then, open up your `.csproj`. Add this line to pack your scripts into your assembly:
```XML
<EmbeddedResource Include="Scripts/**/*.js" Pack="true" />
```

Add a reference to the script in the `NavComponent` view:
```HTML
<script src="~/Scripts/Script.js"></script>
```

If you run the site now, you'll notice that the script resource cannot be found. Since it's embedded in the assembly, we have to tell MVC to provide those files if they are requested.

Back in our Extensions class, we need to set up a function that will serve our embedded Static Resources to the web application. In `Extensions.cs`, add this method:
```C#
public static void UseComponentLibraryScripts(this IApplicationBuilder builder)
{
    var embeddedProvider = new EmbeddedFileProvider(typeof(ComponentLibrary.ViewComponents.NavComponent)
        .GetTypeInfo().Assembly, "ComponentLibrary.Scripts");

    builder.UseStaticFiles(new StaticFileOptions()
    {
        FileProvider = embeddedProvider,
        RequestPath = new PathString("/Scripts")
    });
}
```

There are a couple of things going on in this function. First we're using the class library's assembly to be the basis of our `EmbeddedFileProvider`. Notice that this time around, we specified the default namespace `ComponentLibrary.Scripts`, which is the folder path to the Scripts folder. 
Next, we called the `useStaticFiles` method on the builder. We're supplying our embeddedProvider to specify where the files are coming from, and we supply a RequestPath that will tell us what path the resource is located in. For example, we could set that value to `/Scripts/Lib/` and the directory of `Script.js` would be `http://website/Scripts/Lib/Script.js`.
Back in our `Startup.cs` class, add a call to our new extension method in `Configure`:
```C#
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
	...

    app.UseStaticFiles();
    app.UseComponentLibraryScripts();

	...
}
```

Now build the solution and run your site. In the console, you should see the message that was added into `Script.js`.
If you want to include other static resources (like styles or images), you would use this same steps. You just need to embed them in the assembly and then load them into the application using a Static File Provider. 

### Summary
At this point, we have a library with a ViewComponent and some JavaScript in it. To use it, you need to include a reference to the Extensions class in your `Startup.cs` class and include on line in `Configure` and another line in `Startup`.

### Future Plans
I have a few more things I want to add to my ViewComponents library:
* Develop a class-based system of strongly-typed ViewComponents
* Develop TagHelpers to wrap common markup structures
* Add Bundling/Minification and package scripts/styles with their ViewComponents
* Embed scripts/styles into the ViewComponents themselves
* Restructure the ViewComponent locations to make the folder structures less verbose.
