namespace StoneAssemblies.Extensibility.Tests.Extensions
{
    using System.Collections.Generic;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using StoneAssemblies.Extensibility.Extensions;

    /// <summary>
    ///     The extension manager tests.
    /// </summary>
    [TestClass]
    public class ServiceCollectionExtensionsTests
    {
        /// <summary>
        ///     The load extensions async.
        /// </summary>
        [TestMethod]
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

            Assert.AreNotEqual(0, serviceCollection.Count);
        }

        /// <summary>
        /// The creates the extension manager.
        /// </summary>
        [TestMethod]
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

            Assert.IsNotNull(extensionManager);
        }
    }
}