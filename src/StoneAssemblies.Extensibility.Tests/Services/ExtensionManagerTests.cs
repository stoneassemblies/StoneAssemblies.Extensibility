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
    [TestClass]
    public class ExtensionManagerTests
    {
        /// <summary>
        ///     The load extensions async.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [TestMethod]
        public async Task LoadPluginsProperlyAsync()
        {
            var configurationMock = new Mock<IConfiguration>();
            var serviceCollection = new ServiceCollection();

            var extensionManager = new ExtensionManager(
                configurationMock.Object,
                serviceCollection,
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
    }
}