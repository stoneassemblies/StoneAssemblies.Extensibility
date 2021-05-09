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
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using StoneAssemblies.Extensibility.Services;

    /// <summary>
    ///     The extension manager tests.
    /// </summary>
    public class ExtensionManagerFacts
    {
        /// <summary>
        /// The the load extensions async tests.
        /// </summary>
        [TestClass]
        public class The_LoadExtensionsAsync_Method
        {
            /// <summary>
            ///     The load extensions async.
            /// </summary>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [TestMethod]
            public async Task Loads_Plugins_Properly_From_Non_Null_List_Of_Extensions_Async()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var extensionManager = new ExtensionManager(
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

                Assert.AreNotEqual(0, serviceCollection.Count);
            }

            /// <summary>
            /// Succeeds even if extension list is null.
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [TestMethod]
            public async Task Succeeds_Even_If_Extension_List_Is_Null()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                var extensionManager = new ExtensionManager(
                    serviceCollection,
                    configurationMock.Object,
                    new List<string>
                        {
                            "../../../../../output/nuget-local/",
                            "https://api.nuget.org/v3/index.json",
                        });

                await extensionManager.LoadExtensionsAsync(null);
            }
        }
    }
}