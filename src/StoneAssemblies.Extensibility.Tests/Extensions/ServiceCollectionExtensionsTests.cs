// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensionsTests.cs" company="Stone Assemblies">
// Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility.Tests.Extensions
{
    using System.Collections.Generic;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using StoneAssemblies.Extensibility.Extensions;

    using Xunit;

    /// <summary>
    ///     The extension manager tests.
    /// </summary>
    public class ServiceCollectionExtensionsTests
    {
        /// <summary>
        ///     The creates the extension manager.
        /// </summary>
        [Fact]
        public void CreatesTheExtensionManager()
        {
            var configurationMock = new Mock<IConfiguration>();
            var serviceCollection = new ServiceCollection();

            var extensionManager = serviceCollection.AddExtensions(
                configurationMock.Object,
                new List<string>
                    {
                        "StoneAssemblies.Extensibility.DemoPlugin",
                    },
                new List<string>
                    {
                        "../../../../../output/nuget-local/",
                        "https://api.nuget.org/v3/index.json",
                    });

            Assert.NotNull(extensionManager);
        }

        /// <summary>
        ///     The load extensions async.
        /// </summary>
        [Fact]
        public void LoadPluginsProperly()
        {
            var configurationMock = new Mock<IConfiguration>();
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddExtensions(
                configurationMock.Object,
                new List<string>
                    {
                        "StoneAssemblies.Extensibility.DemoPlugin",
                    },
                new List<string>
                    {
                        "../../../../../output/nuget-local/",
                        "https://api.nuget.org/v3/index.json",
                    });

            Assert.NotEqual(0, serviceCollection.Count);
        }
    }
}