// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionManagerFacts.cs" company="Stone Assemblies">
// Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility.Tests.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using StoneAssemblies.Extensibility.Extensions;
    using StoneAssemblies.Extensibility.Services;
    using StoneAssemblies.Extensibility.Services.Interfaces;

    using Xunit;

    /// <summary>
    ///     The extension manager tests.
    /// </summary>
    [CollectionDefinition(nameof(ExtensionManagerFacts), DisableParallelization = true)]
    public class ExtensionManagerFacts
    {
        /// <summary>
        ///     The creates the extension manager.
        /// </summary>
        [Fact]
        public void Creates_The_ExtensionManager()
        {
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
                        "https://api.nuget.org/v3/index.json"
                    });

            Assert.NotNull(extensionManager);
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
            var configurationMock = new Mock<IConfiguration>();
            var serviceCollection = new ServiceCollection();

            IExtensionManager extensionManager = new ExtensionManager(
                serviceCollection,
                configurationMock.Object,
                new List<string>
                    {
                        "../../../../../output/nuget-local/",
                        "https://api.nuget.org/v3/index.json"
                    });

            await extensionManager.LoadExtensionsAsync(
                new List<string>
                    {
                        "StoneAssemblies.Extensibility.DemoPlugin"
                    });

            Assert.NotEmpty(serviceCollection);
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
            var dictionary = new Dictionary<string, string>
                                 {
                                     { "Extensions:Sources:0", "../../../../../output/nuget-local/" },
                                     { "Extensions:Sources:1", "https://api.nuget.org/v3/index.json" },
                                     { "Extensions:Packages:0", "StoneAssemblies.Extensibility.DemoPlugin" }
                                 };

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(dictionary).Build();
            var serviceCollection = new ServiceCollection();
            IExtensionManager extensionManager = new ExtensionManager(serviceCollection, configuration);

            await extensionManager.LoadExtensionsAsync();
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
            var configurationMock = new Mock<IConfiguration>();
            var serviceCollection = new ServiceCollection();

            IExtensionManager extensionManager = new ExtensionManager(
                serviceCollection,
                configurationMock.Object,
                new List<string>
                    {
                        "../../../../../output/nuget-local/",
                        "https://api.nuget.org/v3/index.json"
                    });

            await extensionManager.LoadExtensionsAsync(
                new List<string>
                    {
                        "StoneAssemblies.Extensibility.DemoPlugin"
                    });

            Assert.Single(extensionManager.GetExtensionAssemblies());
        }

        /// <summary>
        ///     Registers services in service collection.
        /// </summary>
        [Fact]
        public void Registers_Services_In_ServiceCollection()
        {
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
                        "https://api.nuget.org/v3/index.json"
                    });

            Assert.NotEmpty(serviceCollection);
        }

        /// <summary>
        ///     Succeeds even if extension list is null.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Fact]
        public async Task Succeeds_Without_Exception_For_Empty_Load()
        {
            var configurationMock = new Mock<IConfiguration>();
            var serviceCollection = new ServiceCollection();

            IExtensionManager extensionManager = new ExtensionManager(
                serviceCollection,
                configurationMock.Object,
                new List<string>
                    {
                        "../../../../../output/nuget-local/",
                        "https://api.nuget.org/v3/index.json"
                    });

            await extensionManager.LoadExtensionsAsync();
        }
    }
}