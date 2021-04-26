StoneAssemblies Extensibility
=============================

NuGet based extensibility system for netcore apps.

Build Status
------------

Branch | Status
------ | :------:
master | [![Build Status](https://dev.azure.com/alexfdezsauco/External%20Repositories%20Builds/_apis/build/status/stoneassemblies.StoneAssemblies.Extensibility?branchName=master)](https://dev.azure.com/alexfdezsauco/External%20Repositories%20Builds/_build/latest?definitionId=7&branchName=master)
develop | [![Build Status](https://dev.azure.com/alexfdezsauco/External%20Repositories%20Builds/_apis/build/status/stoneassemblies.StoneAssemblies.Extensibility?branchName=develop)](https://dev.azure.com/alexfdezsauco/External%20Repositories%20Builds/_build/latest?definitionId=7&branchName=develop)

Usage
------------

1) Install NuGet Package StoneAssemblies.Extensibility in your application.
2) Add a configuration section like this: 

        {
            "Extensions": {
                "Sources": ["https://api.nuget.org/v3/index.json", "..."],
                "Packages" ["StoneAssemblies.MassAuth.Bank.Rules", "..."]
            }
        }


Create plugin
---------------

1) Create a class library.
2) By convention, the extensibility runtime looks for a class named `Startup` to execute the plugin initialization. The `Startup` class should look like this:

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            // TODO: Register plugin service here!!
        }
    }

3) Pack and publish the class library as NuGet package in a public or private registry.