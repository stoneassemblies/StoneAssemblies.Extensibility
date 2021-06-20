// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionManagerFacts.cs" company="Stone Assemblies">
// Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using StoneAssemblies.Extensibility.Services;
    using StoneAssemblies.Extensibility.Services.Interfaces;

    using Xunit;

    /// <summary>
    ///     The extension manager tests.
    /// </summary>
    public class ExtensionManagerFacts
    {
        /// <summary>
        /// The get extension assembly method.
        /// </summary>
        public class The_GetExtensionAssemblies_Method
        {
            /// <summary>
            ///     Enum The loaded extensions.
            /// </summary>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Fact]
            public async Task Enum_The_Loaded_Extensions()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    new List<string>
                        {
                            "../../../../../output/nuget-local/",
                            "https://api.nuget.org/v3/index.json",
                        });

                await extensionManager.LoadExtensionsAsync(
                    new List<string>
                        {
                            "StoneAssemblies.Extensibility.DemoPlugin",
                        });

                Assert.Single(extensionManager.GetExtensionAssemblies());
            }
        }

        /// <summary>
        /// The the load extensions async tests.
        /// </summary>
        public class The_LoadExtensionsAsync_Method
        {
            /// <summary>
            ///     The load extensions async.
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
                            "https://api.nuget.org/v3/index.json",
                        });

                await extensionManager.LoadExtensionsAsync(
                    new List<string>
                        {
                            "StoneAssemblies.Extensibility.DemoPlugin",
                        });

                Assert.NotEmpty(serviceCollection);
            }

            /// <summary>
            /// Succeeds even if extension list is null.
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Fact]
            public async Task Empty_Load_Succeeds_Without_Exception()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                IExtensionManager extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    new List<string>
                        {
                            "../../../../../output/nuget-local/",
                            "https://api.nuget.org/v3/index.json",
                        });

                await extensionManager.LoadExtensionsAsync();
            }

            /// <summary>
            /// Loads extensions from configuration.
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Fact]
            public async Task Initializes_The_Plugin_Registering_Services_In_ServiceCollection_From_The_Configuration()
            {
                var dictionary = new Dictionary<string, string>
                                          {
                                              { "Extensions:Sources:0", "../../../../../output/nuget-local/" },
                                              { "Extensions:Sources:1", "https://api.nuget.org/v3/index.json" },
                                              { "Extensions:Packages:0", "StoneAssemblies.Extensibility.DemoPlugin" },
                                          };

                var configuration = new ConfigurationBuilder().AddInMemoryCollection(dictionary).Build();
                var serviceCollection = new ServiceCollection();
                IExtensionManager extensionManager = new ExtensionManager(serviceCollection, configuration);

                await extensionManager.LoadExtensionsAsync();
            }
        }
    }
}