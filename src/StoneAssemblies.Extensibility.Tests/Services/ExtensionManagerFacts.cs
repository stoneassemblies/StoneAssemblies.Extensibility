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

    /// <summary>
    ///     The extension manager tests.
    /// </summary>
    [TestFixture]
    public class ExtensionManagerFacts
    {
        [TestFixture]
        public class The_LoadExtensionPackagesAsync_Method
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
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();
                settings.Packages.Add("StoneAssemblies.Extensibility.DemoPlugin");

                settings.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local/" });
                settings.Sources.Add(new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json" });
                settings.IgnoreSchedule = true;
                settings.IgnoreInstalledExtensionPackages = true;

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.LoadExtensionPackagesAsync();

                Assert.IsNotEmpty(serviceCollection);
            }

            [Test]
            [Explicit]
            public async Task It_Works_As_Expected()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();
                settings.Sources.Add(new ExtensionSource
                {
                    Uri = "https://f.feedz.io/[private_feed]/nuget/index.json",
                    Username = "-",
                    Password = "[token]",
                    Searchable = true
                });

                settings.Sources.Add(new ExtensionSource
                                         {
                                             Uri = "https://api.nuget.org/v3/index.json",
                                             Searchable = false
                                         });

                settings.IgnoreSchedule = false;
                settings.IgnoreInstalledExtensionPackages = true;

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.RemoveScheduleAsync();

                // var extensionPackages = await extensionManager.GetAvailableExtensionPackagesAsync(0, 10).ToListAsync();

                // TODO: call ScheduleInstallExtensionPackageAsync with the expected names and versions
                await extensionManager.ScheduleInstallExtensionPackageAsync("[ExtensionPackageName]", "1.0.0-alpha0000");
                

                await extensionManager.LoadExtensionPackagesAsync();

                // Assert.IsNotEmpty(serviceCollection);
            }

            [Test]
            public async Task Throws_ExtensionManagerException_When_Credentials_Are_Not_Specified_For_Private_ExtensionSource_Async()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();
                settings.Packages.Add("StoneAssemblies.Extensibility.DemoPlugin:1.1.0");

                settings.Sources.Add(new ExtensionSource { Uri = "https://f.feedz.io/stone-assemblies/fake/nuget/index.json" });
                settings.Sources.Add(new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json" });
                settings.IgnoreSchedule = true;
                settings.IgnoreInstalledExtensionPackages = true;

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                Assert.CatchAsync<ExtensionManagerException>(() => extensionManager.LoadExtensionPackagesAsync());
            }

            [Test]
            public void Throws_ExtensionManagerException_When_ExtensionSource_Local_Path_Is_Does_Not_Exist()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();
                settings.Packages.Add("StoneAssemblies.Extensibility.DemoPlugin:1.1.0");

                settings.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local-2/" });
                settings.Sources.Add(new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json" });
                settings.IgnoreSchedule = true;
                settings.IgnoreInstalledExtensionPackages = true;

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                Assert.CatchAsync<ExtensionManagerException>(() => extensionManager.LoadExtensionPackagesAsync());
            }

            [Test]
            public async Task Initializes_The_Plugin_Registering_Services_In_ServiceCollection_From_Already_Installed_Packages()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();
                settings.Packages.Add("StoneAssemblies.Extensibility.DemoPlugin");

                settings.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local/" });
                settings.Sources.Add(new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json" });
                settings.IgnoreSchedule = true;
                settings.IgnoreInstalledExtensionPackages = true;

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.LoadExtensionPackagesAsync();

                var settings2 = new ExtensionManagerSettings();
                settings2.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local/" });
                settings2.Sources.Add(new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json" });
                settings2.IgnoreSchedule = true;

                var serviceCollection2 = new ServiceCollection();
                IExtensionManager extensionManager2 = new ExtensionManager(
                    serviceCollection2,
                    configurationMock.Object,
                    settings2);

                await extensionManager2.LoadExtensionPackagesAsync();

                Assert.IsNotEmpty(serviceCollection2);
            }

            [Test]
            public async Task Initializes_The_Plugin_Registering_Services_In_ServiceCollection_Loading_Plugins_From_The_Schedule()
            {
                var currentDirectory = Directory.GetCurrentDirectory();

                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();

                settings.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local/" });
                settings.Sources.Add(new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json" });
                settings.IgnoreInstalledExtensionPackages = true;

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.RemoveScheduleAsync();

                await extensionManager.ScheduleInstallExtensionPackageAsync("StoneAssemblies.Extensibility.DemoPlugin", string.Empty);

                await extensionManager.LoadExtensionPackagesAsync();

                Assert.IsNotEmpty(serviceCollection);
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
                var dictionary = new Dictionary<string, string>
                                     {
                                         { "Extensions:Sources:0:Uri", "../../../../../output/nuget-local/" },
                                         { "Extensions:Sources:1:Uri", "https://api.nuget.org/v3/index.json" },
                                         { "Extensions:Packages:0", "StoneAssemblies.Extensibility.DemoPlugin" },
                                         { "Extensions:IgnoreSchedule", "true" },
                                         { "Extensions:IgnoreInstalledPackage", "true" },
                                     };

                var configuration = new ConfigurationBuilder().AddInMemoryCollection(dictionary).Build();
                var serviceCollection = new ServiceCollection();
                var settings = new ExtensionManagerSettings();
                configuration.GetSection("Extensions")?.Bind(settings);
                IExtensionManager extensionManager = new ExtensionManager(serviceCollection, configuration, settings);

                await extensionManager.LoadExtensionPackagesAsync();

                Assert.IsNotEmpty(serviceCollection);
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
                settings.IgnoreSchedule = true;
                settings.IgnoreInstalledExtensionPackages = true;

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.LoadExtensionPackagesAsync();

                Assert.AreEqual(1, extensionManager.GetExtensionPackageAssemblies().Count());
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
                settings.IgnoreInstalledExtensionPackages = true;
                settings.IgnoreSchedule = true;

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.LoadExtensionPackagesAsync();

                Assert.IsEmpty(serviceCollection);
            }
        }

        [TestFixture]
        public class The_Configure_Method
        {
            [Test]
            public async Task Calls_Configure_Methods_On_Starup_Objects_If_Match_With_The_Signature()
            {
                var dictionary = new Dictionary<string, string>
                                     {
                                         { "Extensions:Sources:0:Uri", "../../../../../output/nuget-local/" },
                                         { "Extensions:Sources:1:Uri", "https://api.nuget.org/v3/index.json" },
                                         { "Extensions:Packages:0", "StoneAssemblies.Extensibility.DemoPlugin" },
                                         { "Extensions:IgnoreSchedule", "true" },
                                         { "Extensions:IgnoreInstalledPackage", "true" },
                                     };

                var configuration = new ConfigurationBuilder().AddInMemoryCollection(dictionary).Build();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();
                configuration.GetSection("Extensions").Bind(settings);
                IExtensionManager extensionManager = new ExtensionManager(serviceCollection, configuration, settings);

                await extensionManager.LoadExtensionPackagesAsync();

                var called = false;
                var list = new List<string>();

                extensionManager.Configure(new Action<string>(s => { called = true; }), list);

                Assert.True(called);
                Assert.IsNotEmpty(list);
            }

            [Test]
            public async Task Method_Calls_Configure_Methods_On_Starup_Objects_If_Match_With_The_Signature_Ignoring_Null_Arguments()
            {
                var dictionary = new Dictionary<string, string>
                                     {
                                         { "Extensions:Sources:0:Uri", "../../../../../output/nuget-local/" },
                                         { "Extensions:Sources:1:Uri", "https://api.nuget.org/v3/index.json" },
                                         { "Extensions:Packages:0", "StoneAssemblies.Extensibility.DemoPlugin" },
                                         { "Extensions:IgnoreSchedule", "true" },
                                         { "Extensions:IgnoreInstalledPackage", "true" },
                                     };

                var configuration = new ConfigurationBuilder().AddInMemoryCollection(dictionary).Build();
                var serviceCollection = new ServiceCollection();
                var settings = new ExtensionManagerSettings();
                configuration.GetSection("Extensions").Bind(settings);
                IExtensionManager extensionManager = new ExtensionManager(serviceCollection, configuration, settings);

                await extensionManager.LoadExtensionPackagesAsync();

                var called = false;
                var list = new List<string>();

                extensionManager.Configure(null, new Action<string>(s => { called = true; }), null, list, null);

                Assert.True(called);
                Assert.IsNotEmpty(list);
            }

            [Test]
            public async Task Doesnt_Call_Configure_Methods_On_Starup_Objects_If_Doesnt_Match_With_The_Signature()
            {
                var dictionary = new Dictionary<string, string>
                                     {
                                         { "Extensions:Sources:0:Uri", "../../../../../output/nuget-local/" },
                                         { "Extensions:Sources:1:Uri", "https://api.nuget.org/v3/index.json" },
                                         { "Extensions:Packages:0", "StoneAssemblies.Extensibility.DemoPlugin" },
                                         { "Extensions:IgnoreSchedule", "true" }
                                     };

                var configuration = new ConfigurationBuilder().AddInMemoryCollection(dictionary).Build();
                var serviceCollection = new ServiceCollection();
                var settings = new ExtensionManagerSettings();
                configuration.GetSection("Extensions").Bind(settings);
                IExtensionManager extensionManager = new ExtensionManager(serviceCollection, configuration, settings);

                await extensionManager.LoadExtensionPackagesAsync();

                var called = false;
                extensionManager.Configure(new Action<string>(s => { called = true; }));

                Assert.False(called);
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

                var settings = new ExtensionManagerSettings
                {
                    IgnoreSchedule = true
                };
                settings.Packages.Add("StoneAssemblies.Extensibility.DemoPlugin");

                settings.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local/" });
                settings.Sources.Add(
                    new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json", Searchable = false });

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.LoadExtensionPackagesAsync();

                var extensionPackages = await extensionManager.GetAvailableExtensionPackagesAsync(0, 10).ToListAsync();

                var extensionPackage = extensionPackages.FirstOrDefault(
                    package => package.Id == "StoneAssemblies.Extensibility.DemoPlugin");

                Assert.NotNull(extensionPackage?.InstalledVersion);
                Assert.NotNull(extensionPackage?.Versions);
            }

            [Test]
            public async Task Does_Not_Returns_The_Package_If_The_Id_In_The_Blacklist_Async()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings
                {
                    IgnoreSchedule = true
                };

                settings.Blacklist.Add("StoneAssemblies.Extensibility.DemoPlugin");

                settings.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local/" });
                settings.Sources.Add(
                    new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json", Searchable = false });

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                var extensionPackages = await extensionManager.GetAvailableExtensionPackagesAsync(0, 10).ToListAsync();

                var extensionPackage = extensionPackages.FirstOrDefault(
                    package => package.Id == "StoneAssemblies.Extensibility.DemoPlugin");

                Assert.IsNull(extensionPackage?.Versions);
            }

            [Test]
            public async Task Does_Not_Returns_The_Package_If_The_Id_Matches_With_A_Regex_In_The_Blacklist_Async()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings
                {
                    IgnoreSchedule = true
                };

                settings.Blacklist.Add(".+DemoPlugin");

                settings.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local/" });
                settings.Sources.Add(
                    new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json", Searchable = false });

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                var extensionPackages = await extensionManager.GetAvailableExtensionPackagesAsync(0, 10).ToListAsync();

                var extensionPackage = extensionPackages.FirstOrDefault(
                    package => package.Id == "StoneAssemblies.Extensibility.DemoPlugin");

                Assert.IsNull(extensionPackage?.Versions);
            }

            [Test]
            public async Task Returns_The_Available_Package_With_Not_Null_Value_And_Nul_In_VersionInfos_When_The_Package_Is_Not_Present_In_The_Feed_Async()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();
                settings.Packages.Add("StoneAssemblies.Extensibility.DemoPlugin");

                settings.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local/" });
                settings.Sources.Add(
                    new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json", Searchable = false });
                settings.IgnoreSchedule = true;

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.LoadExtensionPackagesAsync();

                var settings2 = new ExtensionManagerSettings();
                settings2.Sources.Add(
                    new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json", Searchable = false });
                IExtensionManager extensionManager2 = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings2);

                var extensionPackages = await extensionManager2.GetAvailableExtensionPackagesAsync(0, 10).ToListAsync();

                var extensionPackage = extensionPackages.FirstOrDefault(
                    package => package.Id == "StoneAssemblies.Extensibility.DemoPlugin");

                Assert.NotNull(extensionPackage?.InstalledVersion);
                Assert.IsNull(extensionPackage?.Versions);
            }
        }

        [TestFixture]
        public class The_ScheduleInstallExtensionPackageAsync_Method
        {
            [Test]
            public async Task Adds_The_Package_To_Be_Installed()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.RemoveScheduleAsync();

                await extensionManager.ScheduleInstallExtensionPackageAsync(
                    "StoneAssemblies.Extensibility.DemoPlugin",
                    "1.0.0-alpha0104");

                var (scheduled, version) = await extensionManager.IsExtensionPackageScheduledToInstallAsync("StoneAssemblies.Extensibility.DemoPlugin");

                Assert.IsTrue(scheduled);
                Assert.AreEqual("1.0.0-alpha0104", version);
            }

            [Test]
            public async Task Adds_The_Package_To_Be_Installed_Only_Once()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.RemoveScheduleAsync();

                await extensionManager.ScheduleInstallExtensionPackageAsync(
                    "StoneAssemblies.Extensibility.DemoPlugin",
                    "1.0.0-alpha0104");

                await extensionManager.ScheduleInstallExtensionPackageAsync(
                    "StoneAssemblies.Extensibility.DemoPlugin",
                    "1.0.0-alpha0105");

                var schedule = await extensionManager.GetScheduleAsync();

                Assert.AreEqual(1, schedule.Install.Count(s => s.StartsWith("StoneAssemblies.Extensibility.DemoPlugin")));
            }
        }

        [TestFixture]
        public class The_IsExtensionPackageScheduledToInstallAsync_Method
        {
            [Test]
            public async Task Returns_True_When_ScheduleInstallPackageAsync_Is_Called()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.RemoveScheduleAsync();

                await extensionManager.ScheduleInstallExtensionPackageAsync(
                    "StoneAssemblies.Extensibility.DemoPlugin",
                    "1.0.0-alpha0104");

                var result = await extensionManager.IsExtensionPackageScheduledToInstallAsync("StoneAssemblies.Extensibility.DemoPlugin");
                Assert.IsTrue(result.Scheduled);
            }

            [Test]
            public async Task Returns_False_When_ScheduleInstallPackageAsync_Is_Called_But_With_Diferent_PackageId()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.RemoveScheduleAsync();

                await extensionManager.ScheduleUninstallExtensionPackageAsync("StoneAssemblies.Extensibility.DemoPlugin");

                var result = await extensionManager.IsExtensionPackageScheduledToInstallAsync("StoneAssemblies.Extensibility.DemoPlugin2");

                Assert.IsFalse(result.Scheduled);
                Assert.IsEmpty(result.Version);
            }
        }


        [TestFixture]
        public class The_IsExtensionPackageScheduledToUninstallAsync_Method
        {
            [Test]
            public async Task Returns_True_When_ScheduleUninstallPackageAsync_Is_Called()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.RemoveScheduleAsync();

                await extensionManager.ScheduleUninstallExtensionPackageAsync("StoneAssemblies.Extensibility.DemoPlugin");

                Assert.IsTrue(await extensionManager.IsExtensionPackageScheduledToUninstallAsync("StoneAssemblies.Extensibility.DemoPlugin"));
            }

            [Test]
            public async Task Returns_False_When_ScheduleUninstallPackageAsync_Is_Called_But_With_Diferent_PackageId()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.RemoveScheduleAsync();

                await extensionManager.ScheduleUninstallExtensionPackageAsync("StoneAssemblies.Extensibility.DemoPlugin");

                Assert.IsFalse(await extensionManager.IsExtensionPackageScheduledToUninstallAsync("StoneAssemblies.Extensibility.DemoPlugin2"));
            }
        }

        [TestFixture]
        public class The_ScheduleUninstallExtensionPackageAsync_Method
        {
            [Test]
            public async Task Adds_The_Package_To_Be_Uninstalled()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.RemoveScheduleAsync();

                await extensionManager.ScheduleUninstallExtensionPackageAsync("StoneAssemblies.Extensibility.DemoPlugin");

                Assert.IsTrue(await extensionManager.IsExtensionPackageScheduledToUninstallAsync("StoneAssemblies.Extensibility.DemoPlugin"));
            }

            [Test]
            public async Task Adds_The_Package_To_Be_Uninstalled_Only_Once()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.RemoveScheduleAsync();

                await extensionManager.ScheduleUninstallExtensionPackageAsync("StoneAssemblies.Extensibility.DemoPlugin");
                await extensionManager.ScheduleUninstallExtensionPackageAsync("StoneAssemblies.Extensibility.DemoPlugin");

                var schedule = await extensionManager.GetScheduleAsync();

                Assert.AreEqual(1, schedule.Uninstall.Count(s => s.StartsWith("StoneAssemblies.Extensibility.DemoPlugin")));
            }
        }

        [TestFixture]
        public class The_GetExtensionPackageByIdAsync_Method
        {
            [Test]
            public async Task Returns_The_Available_Package_With_Not_Null_Value_Installed_Version_Property_Async()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();
                settings.IgnoreSchedule = true;
                settings.Packages.Add("StoneAssemblies.Extensibility.DemoPlugin");

                settings.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local/" });
                settings.Sources.Add(
                    new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json", Searchable = false });

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.LoadExtensionPackagesAsync();

                var extensionPackages = await extensionManager.GetAvailableExtensionPackagesAsync(0, 10).ToListAsync();

                var extensionPackage = extensionPackages.FirstOrDefault(
                    package => package.Id == "StoneAssemblies.Extensibility.DemoPlugin");

                Assert.NotNull(extensionPackage?.InstalledVersion);
                Assert.NotNull(extensionPackage?.Versions);
            }


            [Test]
            public async Task Returns_The_Available_Package_With_Not_Null_Value_And_Null_In_VersionInfos_When_The_Package_Is_Not_Present_In_The_Feed_Async()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var settings = new ExtensionManagerSettings();
                settings.Packages.Add("StoneAssemblies.Extensibility.DemoPlugin");

                settings.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local/" });
                settings.Sources.Add(
                    new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json", Searchable = false });
                settings.IgnoreSchedule = true;

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings);

                await extensionManager.LoadExtensionPackagesAsync();

                var settings2 = new ExtensionManagerSettings();
                settings2.Sources.Add(
                    new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json", Searchable = false });
                IExtensionManager extensionManager2 = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    settings2);

                var extensionPackage =
                    await extensionManager2.GetExtensionPackageByIdAsync("StoneAssemblies.Extensibility.DemoPlugin");

                Assert.NotNull(extensionPackage?.InstalledVersion);
                Assert.IsNull(extensionPackage?.Versions);
            }
        }
    }
}