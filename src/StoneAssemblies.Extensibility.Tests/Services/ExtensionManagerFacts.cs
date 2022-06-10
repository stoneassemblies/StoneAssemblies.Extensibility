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
    using System.Linq;
    using System.Threading.Tasks;

    using Dasync.Collections;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using Serilog;


    /// <summary>
    ///     The extension manager tests.
    /// </summary>
    [TestFixture]
    public class ExtensionManagerFacts
    {
        ///// <summary>
        /////     Initializes the ExtensionManagerFacts
        ///// </summary>
        ///// <param name="output">
        /////     The output.
        ///// </param>
        //public ExtensionManagerFacts(ITestOutputHelper output)
        //{
        //    Log.Logger = new LoggerConfiguration()
        //        // add the xunit test output sink to the serilog logger
        //        // https://github.com/trbenning/serilog-sinks-xunit#serilog-sinks-xunit
        //        .WriteTo.TestOutput(output)
        //        .WriteTo.File(@"c:\tmp\x-log.txt", rollingInterval: RollingInterval.Day)
        //        .CreateLogger();
        //}

        [TestFixture]
        public class The_LoadExtensionsAsync_Method
        {
            /// <summary>
            ///     Initializes the plugin registering services in service collection.
            /// </summary>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Test]
            public async Task Initializes_The_Plugin_Registering_Services_In_ServiceCollection()
            {
                Log.Information(
                    "Starting {MethodName}",
                    nameof(this.Initializes_The_Plugin_Registering_Services_In_ServiceCollection));
                var currentDirectory = Directory.GetCurrentDirectory();

                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();
                settings.Packages.Add("StoneAssemblies.Extensibility.DemoPlugin");

                settings.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local/" });
                settings.Sources.Add(new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json" });

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.LoadExtensionsAsync();

                Assert.IsNotEmpty(serviceCollection);

                Log.Information(
                    "Finished {MethodName}",
                    nameof(this.Initializes_The_Plugin_Registering_Services_In_ServiceCollection));
            }

            /// <summary>
            ///     Initializes the plugin registering services in service collection from the configuration.
            /// </summary>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Test]
            public async Task Initializes_The_Plugin_Registering_Services_In_ServiceCollection_From_The_Configuration()
            {
                Log.Information(
                    "Starting {MethodName}",
                    nameof(this
                        .Initializes_The_Plugin_Registering_Services_In_ServiceCollection_From_The_Configuration));

                var dictionary = new Dictionary<string, string>
                                     {
                                         { "Extensions:Sources:0:Uri", "../../../../../output/nuget-local/" },
                                         { "Extensions:Sources:1:Uri", "https://api.nuget.org/v3/index.json" },
                                         { "Extensions:Packages:0", "StoneAssemblies.Extensibility.DemoPlugin" }
                                     };

                var configuration = new ConfigurationBuilder().AddInMemoryCollection(dictionary).Build();
                var serviceCollection = new ServiceCollection();
                var settings = new ExtensionManagerSettings();
                configuration.GetSection("Extensions")?.Bind(settings);
                IExtensionManager extensionManager = new ExtensionManager(serviceCollection, configuration, settings);

                await extensionManager.LoadExtensionsAsync();

                Assert.IsNotEmpty(serviceCollection);

                Log.Information(
                    "Finished {MethodName}",
                    nameof(this
                        .Initializes_The_Plugin_Registering_Services_In_ServiceCollection_From_The_Configuration));
            }

            /// <summary>
            ///     Loads extensions assemblies available via get extension assemblies method.
            /// </summary>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Test]
            public async Task Loads_Extensions_Assemblies_Available_Via_GetExtensionAssemblies_Method()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();
                settings.Packages.Add("StoneAssemblies.Extensibility.DemoPlugin");

                settings.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local/" });
                settings.Sources.Add(new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json" });

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.LoadExtensionsAsync();

                Assert.AreEqual(1, extensionManager.GetExtensionAssemblies().Count());
            }

            /// <summary>
            ///     Succeeds even if extension list is null.
            /// </summary>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Test]
            public async Task Succeeds_Without_Exception_With_Empty_Configuration()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();
                settings.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local/" });
                settings.Sources.Add(new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json" });

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.LoadExtensionsAsync();

                Assert.IsEmpty(serviceCollection);
            }
        }

        [TestFixture]
        public class The_Configure_Method
        {
            [Test]
            public async Task Calls_Configure_Methods_On_Starup_Objects_If_Match_With_The_Signature()
            {
                Log.Information("Starting {MethodName}", nameof(Calls_Configure_Methods_On_Starup_Objects_If_Match_With_The_Signature));
                var dictionary = new Dictionary<string, string>
            {
                { "Extensions:Sources:0:Uri", "../../../../../output/nuget-local/" },
                { "Extensions:Sources:1:Uri", "https://api.nuget.org/v3/index.json" },
                { "Extensions:Packages:0", "StoneAssemblies.Extensibility.DemoPlugin" },
            };

                var configuration = new ConfigurationBuilder().AddInMemoryCollection(dictionary).Build();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();
                configuration.GetSection("Extensions").Bind(settings);
                IExtensionManager extensionManager = new ExtensionManager(serviceCollection, configuration, settings);

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
                Assert.IsNotEmpty(list);
                Log.Information("Finished {MethodName}", nameof(Calls_Configure_Methods_On_Starup_Objects_If_Match_With_The_Signature));
            }

            [Test]
            public async Task Method_Calls_Configure_Methods_On_Starup_Objects_If_Match_With_The_Signature_Ignoring_Null_Arguments()
            {
                Log.Information("Starting {MethodName}", nameof(this.Method_Calls_Configure_Methods_On_Starup_Objects_If_Match_With_The_Signature_Ignoring_Null_Arguments));

                var dictionary = new Dictionary<string, string>
            {
                { "Extensions:Sources:0:Uri", "../../../../../output/nuget-local/" },
                { "Extensions:Sources:1:Uri", "https://api.nuget.org/v3/index.json" },
                { "Extensions:Packages:0", "StoneAssemblies.Extensibility.DemoPlugin" },
            };

                var configuration = new ConfigurationBuilder().AddInMemoryCollection(dictionary).Build();
                var serviceCollection = new ServiceCollection();
                var settings = new ExtensionManagerSettings();
                configuration.GetSection("Extensions").Bind(settings);
                IExtensionManager extensionManager = new ExtensionManager(serviceCollection, configuration, settings);

                await extensionManager.LoadExtensionsAsync();

                var called = false;
                var list = new List<string>();

                extensionManager.Configure(null, new Action<string>(s => { called = true; }), null, list, null);

                Assert.True(called);
                Assert.IsNotEmpty(list);

                Log.Information("Finished {MethodName}", nameof(this.Method_Calls_Configure_Methods_On_Starup_Objects_If_Match_With_The_Signature_Ignoring_Null_Arguments));

            }

            [Test]
            public async Task Doesnt_Call_Configure_Methods_On_Starup_Objects_If_Doesnt_Match_With_The_Signature()
            {
                Log.Information("Starting {MethodName}", nameof(this.Doesnt_Call_Configure_Methods_On_Starup_Objects_If_Doesnt_Match_With_The_Signature));
                var dictionary = new Dictionary<string, string>
                                 {
                                     { "Extensions:Sources:0:Uri", "../../../../../output/nuget-local/" },
                                     { "Extensions:Sources:1:Uri", "https://api.nuget.org/v3/index.json" },
                                     { "Extensions:Packages:0", "StoneAssemblies.Extensibility.DemoPlugin" }
                                 };

                var configuration = new ConfigurationBuilder().AddInMemoryCollection(dictionary).Build();
                var serviceCollection = new ServiceCollection();
                var settings = new ExtensionManagerSettings();
                configuration.GetSection("Extensions").Bind(settings);
                IExtensionManager extensionManager = new ExtensionManager(serviceCollection, configuration, settings);

                await extensionManager.LoadExtensionsAsync();

                bool called = false;
                extensionManager.Configure(
                    new Action<string>(
                        s =>
                        {
                            called = true;
                        }));

                Assert.False(called);
                Log.Information("Finished {MethodName}", nameof(this.Doesnt_Call_Configure_Methods_On_Starup_Objects_If_Doesnt_Match_With_The_Signature));
            }
        }

        [TestFixture]
        public class The_GetAvailableExtensionPackagesAsync_Method
        {
            [Test]
            public async Task Returns_The_Available_Package_With_Not_Null_Value_Installed_Version_Property_Async()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();
                settings.Packages.Add("StoneAssemblies.Extensibility.DemoPlugin");

                settings.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local/" });
                settings.Sources.Add(
                    new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json", Searchable = false });

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.LoadExtensionsAsync();

                var extensionPackages = await extensionManager.GetAvailableExtensionPackagesAsync(0, 10).ToListAsync();

                var extensionPackage = extensionPackages.FirstOrDefault(package => package.Id == "StoneAssemblies.Extensibility.DemoPlugin");

                Assert.NotNull(extensionPackage?.InstalledVersion);
            }
        }
    }
}