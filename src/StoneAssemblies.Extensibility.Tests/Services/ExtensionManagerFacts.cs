// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionManagerFacts.cs" company="Stone Assemblies">
// Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using Serilog;

    using StoneAssemblies.Extensibility.Extensions;
    using StoneAssemblies.Extensibility.Services;
    using StoneAssemblies.Extensibility.Services.Interfaces;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    ///     The extension manager tests.
    /// </summary>
    [CollectionDefinition(nameof(ExtensionManagerFacts), DisableParallelization = true)]
    public class ExtensionManagerFacts
    {

        public ExtensionManagerFacts(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                // add the xunit test output sink to the serilog logger
                // https://github.com/trbenning/serilog-sinks-xunit#serilog-sinks-xunit
                .WriteTo.TestOutput(output)
                .WriteTo.File(@"c:\tmp\x-log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        /// <summary>
        ///     The creates the extension manager.
        /// </summary>
        [Fact]
        public void Creates_The_ExtensionManager()
        {
            Log.Information("Starting {MethodName}", nameof(Creates_The_ExtensionManager));

            var configurationMock = new Mock<IConfiguration>();
            var serviceCollection = new ServiceCollection();

            var extensionManager = serviceCollection.AddExtensions(
                configurationMock.Object,
                new List<string>
                    {
                        "StoneAssemblies.Extensibility.DemoPlugin"
                    },
                new List<string>
                    {
                        "../../../../../output/nuget-local/",
                        "http://localhost:8081/repository/nuget-all/"
                    });

            Assert.NotNull(extensionManager);

            Log.Information("Finished {MethodName}", nameof(Creates_The_ExtensionManager));
        }

        /// <summary>
        ///     Initializes the plugin registering services in service collection.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Fact]
        public async Task Initializes_The_Plugin_Registering_Services_In_ServiceCollection()
        {
            Log.Information("Starting {MethodName}", nameof(Initializes_The_Plugin_Registering_Services_In_ServiceCollection));
            var currentDirectory = Directory.GetCurrentDirectory();

            var configurationMock = new Mock<IConfiguration>();
            var serviceCollection = new ServiceCollection();

            IExtensionManager extensionManager = new ExtensionManager(
                serviceCollection,
                configurationMock.Object,
                new List<string>
                    {
                        "../../../../../output/nuget-local/",
                        "http://localhost:8081/repository/nuget-proxy-internet/"
                    });

            await extensionManager.LoadExtensionsAsync(
                new List<string>
                    {
                        "StoneAssemblies.Extensibility.DemoPlugin"
                    });

            Assert.NotEmpty(serviceCollection);

            Log.Information("Finished {MethodName}", nameof(Initializes_The_Plugin_Registering_Services_In_ServiceCollection));
        }

        /// <summary>
        ///     Initializes the plugin registering services in service collection from the configuration.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Fact]
        public async Task Initializes_The_Plugin_Registering_Services_In_ServiceCollection_From_The_Configuration()
        {
            Log.Information("Starting {MethodName}", nameof(Initializes_The_Plugin_Registering_Services_In_ServiceCollection_From_The_Configuration));

            var dictionary = new Dictionary<string, string>
                                 {
                                     { "Extensions:Sources:0", "../../../../../output/nuget-local/" },
                                     { "Extensions:Sources:1", "http://localhost:8081/repository/nuget-proxy-internet/" },
                                     { "Extensions:Packages:0", "StoneAssemblies.Extensibility.DemoPlugin" }
                                 };

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(dictionary).Build();
            var serviceCollection = new ServiceCollection();
            IExtensionManager extensionManager = new ExtensionManager(serviceCollection, configuration);

            await extensionManager.LoadExtensionsAsync();

            Assert.NotEmpty(serviceCollection);

            Log.Information("Finished {MethodName}", nameof(Initializes_The_Plugin_Registering_Services_In_ServiceCollection_From_The_Configuration));
        }

        /// <summary>
        ///     Loads extensions assemblies available via get extension assemblies method.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Fact]
        public async Task Loads_Extensions_Assemblies_Available_Via_GetExtensionAssemblies_Method()
        {
            Log.Information("Starting {MethodName}", nameof(Loads_Extensions_Assemblies_Available_Via_GetExtensionAssemblies_Method));
            var configurationMock = new Mock<IConfiguration>();
            var serviceCollection = new ServiceCollection();

            IExtensionManager extensionManager = new ExtensionManager(
                serviceCollection,
                configurationMock.Object,
                new List<string>
                    {
                        "../../../../../output/nuget-local/",
                        "http://localhost:8081/repository/nuget-proxy-internet/"
                    });

            await extensionManager.LoadExtensionsAsync(
                new List<string>
                    {
                        "StoneAssemblies.Extensibility.DemoPlugin"
                    });

            Assert.Single(extensionManager.GetExtensionAssemblies());
            Log.Information("Finished {MethodName}", nameof(Loads_Extensions_Assemblies_Available_Via_GetExtensionAssemblies_Method));
        }

        /// <summary>
        ///     Registers services in service collection.
        /// </summary>
        [Fact]
        public void Registers_Services_In_ServiceCollection()
        {
            Log.Information("Starting {MethodName}", nameof(Registers_Services_In_ServiceCollection));
            var configurationMock = new Mock<IConfiguration>();
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddExtensions(
                configurationMock.Object,
                new List<string>
                    {
                        "StoneAssemblies.Extensibility.DemoPlugin"
                    },
                new List<string>
                    {
                        "../../../../../output/nuget-local/",
                        "http://localhost:8081/repository/nuget-proxy-internet/"
                    });

            Assert.NotEmpty(serviceCollection);
            Log.Information("Finished {MethodName}", nameof(Registers_Services_In_ServiceCollection));
        }

        /// <summary>
        ///     Succeeds even if extension list is null.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Fact]
        public async Task Succeeds_Without_Exception_With_Empty_Configuration()
        {
            Log.Information("Starting {MethodName}", nameof(Succeeds_Without_Exception_With_Empty_Configuration));
            var configurationMock = new Mock<IConfiguration>();
            var serviceCollection = new ServiceCollection();

            IExtensionManager extensionManager = new ExtensionManager(
                serviceCollection,
                configurationMock.Object,
                new List<string>
                    {
                        "../../../../../output/nuget-local/",
                        "http://localhost:8081/repository/nuget-proxy-internet/"
                    });

            await extensionManager.LoadExtensionsAsync();

            Assert.Empty(serviceCollection);
            Log.Information("Finished {MethodName}", nameof(Succeeds_Without_Exception_With_Empty_Configuration));
        }

        [Fact]
        public async Task The_Configure_Method_Calls_Configure_Methods_On_Starup_Objects_If_Match_With_The_Signature()
        {
            Log.Information("Starting {MethodName}", nameof(The_Configure_Method_Calls_Configure_Methods_On_Starup_Objects_If_Match_With_The_Signature));
            var dictionary = new Dictionary<string, string>
                                 {
                                     { "Extensions:Sources:0", "../../../../../output/nuget-local/" },
                                     { "Extensions:Sources:1", "http://localhost:8081/repository/nuget-proxy-internet/" },
                                     { "Extensions:Packages:0", "StoneAssemblies.Extensibility.DemoPlugin" }
                                 };

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(dictionary).Build();
            var serviceCollection = new ServiceCollection();
            IExtensionManager extensionManager = new ExtensionManager(serviceCollection, configuration);

            await extensionManager.LoadExtensionsAsync();

            bool called = false;
            var list = new List<string>();

            extensionManager.Configure(
                new Action<string>(
                    s =>
                        {
                        called = true;
                    }),
                list);

            Assert.True(called);
            Assert.NotEmpty(list);
            Log.Information("Finished {MethodName}", nameof(The_Configure_Method_Calls_Configure_Methods_On_Starup_Objects_If_Match_With_The_Signature));
        }

        [Fact]
        public async Task The_Configure_Method_Calls_Configure_Methods_On_Starup_Objects_If_Match_With_The_Signature_Ignoring_Null_Arguments()
        {
            Log.Information("Starting {MethodName}", nameof(The_Configure_Method_Calls_Configure_Methods_On_Starup_Objects_If_Match_With_The_Signature_Ignoring_Null_Arguments));

            var dictionary = new Dictionary<string, string>
                                 {
                                     {
                                         "Extensions:Sources:0", "../../../../../output/nuget-local/"
                                     },
                                     {
                                         "Extensions:Sources:1", "http://localhost:8081/repository/nuget-proxy-internet/"
                                     },
                                     {
                                         "Extensions:Packages:0", "StoneAssemblies.Extensibility.DemoPlugin"
                                     }
                                 };

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(dictionary).Build();
            var serviceCollection = new ServiceCollection();
            IExtensionManager extensionManager = new ExtensionManager(serviceCollection, configuration);

            await extensionManager.LoadExtensionsAsync();

            var called = false;
            var list = new List<string>();

            extensionManager.Configure(null, new Action<string>(s => { called = true; }), null, list, null);

            Assert.True(called);
            Assert.NotEmpty(list);

            Log.Information("Finished {MethodName}", nameof(The_Configure_Method_Calls_Configure_Methods_On_Starup_Objects_If_Match_With_The_Signature_Ignoring_Null_Arguments));

        }

        [Fact]
        public async Task The_Configure_Method_Doesnt_Call_Configure_Methods_On_Starup_Objects_If_Doesnt_Match_With_The_Signature()
        {
            Log.Information("Starting {MethodName}", nameof(The_Configure_Method_Doesnt_Call_Configure_Methods_On_Starup_Objects_If_Doesnt_Match_With_The_Signature));
            var dictionary = new Dictionary<string, string>
                                 {
                                     { "Extensions:Sources:0", "../../../../../output/nuget-local/" },
                                     { "Extensions:Sources:1", "http://localhost:8081/repository/nuget-proxy-internet/" },
                                     { "Extensions:Packages:0", "StoneAssemblies.Extensibility.DemoPlugin" }
                                 };

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(dictionary).Build();
            var serviceCollection = new ServiceCollection();
            IExtensionManager extensionManager = new ExtensionManager(serviceCollection, configuration);

            await extensionManager.LoadExtensionsAsync();

            bool called = false;
            extensionManager.Configure(
                new Action<string>(
                    s =>
                        {
                        called = true;
                    }));

            Assert.False(called);
            Log.Information("Finished {MethodName}", nameof(The_Configure_Method_Doesnt_Call_Configure_Methods_On_Starup_Objects_If_Doesnt_Match_With_The_Signature));
        }
    }
}