// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensionsFacts.cs" company="Stone Assemblies">
// Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility.Tests.Extensions
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;

    using NUnit.Framework;

    /// <summary>
    ///     The service collection extensions facts.
    /// </summary>
    [TestFixture]
    public class ServiceCollectionExtensionsFacts
    {
        /// <summary>
        ///     The add extensions method.
        /// </summary>
        [TestFixture]
        public class The_AddExtensionPackages_Method
        {
            /// <summary>
            ///     Initializes the plugin registering services in service collection.
            /// </summary>
            [Test]
            public void Initializes_The_Plugin_Registering_Services_In_ServiceCollection()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddExtensionPackages(
                    configurationMock.Object,
                    settings =>
                        {
                            settings.Packages.Add("StoneAssemblies.Extensibility.DemoPlugin");
                            settings.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local/" });
                            settings.Sources.Add(new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json" });
                            settings.IgnoreSchedule = true;
                            settings.IgnoreInstalledExtensionPackages = true;
                        });

                Assert.AreEqual(2, serviceCollection.Count);
            }

            /// <summary>
            ///     Does not initialize the plugin without registering services in service collection.
            /// </summary>
            [Test]
            public void Does_Not_Initialize_The_Plugin_Without_Registering_Services_In_ServiceCollection()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddExtensionPackages(
                    configurationMock.Object,
                    settings =>
                        {
                            settings.Packages.Add("StoneAssemblies.Extensibility.DemoPlugin");
                            settings.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local/" });
                            settings.Sources.Add(new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json" });
                            settings.Initialize = false;
                            settings.IgnoreSchedule = true;
                            settings.IgnoreInstalledExtensionPackages = true;
                        });

                Assert.AreEqual(1, serviceCollection.Count);
            }
        }

        /// <summary>
        ///     Registers services in service collection.
        /// </summary>
        [Test]
        public void Registers_Services_In_ServiceCollection()
        {
            var configurationMock = new Mock<IConfiguration>();
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddExtensionPackages(
                configurationMock.Object,
                settings =>
                    {
                        settings.Packages.Add("StoneAssemblies.Extensibility.DemoPlugin");
                        settings.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local/" });
                        settings.Sources.Add(new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json" });
                        settings.IgnoreSchedule = true;
                    });

            Assert.IsNotEmpty(serviceCollection);
        }

        /// <summary>
        ///     The creates the extension manager.
        /// </summary>
        [Test]
        public void Creates_The_ExtensionManager()
        {
            var configurationMock = new Mock<IConfiguration>();
            var serviceCollection = new ServiceCollection();

            var extensionManager = serviceCollection.AddExtensionPackages(
                configurationMock.Object,
                settings =>
                    {
                        settings.Packages.Add("StoneAssemblies.Extensibility.DemoPlugin");
                        settings.Sources.Add(new ExtensionSource { Uri = "../../../../../output/nuget-local/" });
                        settings.Sources.Add(new ExtensionSource { Uri = "https://api.nuget.org/v3/index.json" });
                        settings.IgnoreSchedule = true;
                    });

            Assert.NotNull(extensionManager);
        }
    }
}
