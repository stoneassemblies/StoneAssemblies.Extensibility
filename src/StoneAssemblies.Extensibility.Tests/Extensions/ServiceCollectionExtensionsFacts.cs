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
    using Xunit;

    /// <summary>
    /// The service collection extensions facts.
    /// </summary>
    public class ServiceCollectionExtensionsFacts
    {
        /// <summary>
        /// The add extensions method.
        /// </summary>
        public class The_AddExtensions_Method
        {
            /// <summary>
            ///     Initializes the plugin registering services in service collection.
            /// </summary>
            [Fact]
            public void Initializes_The_Plugin_Registering_Services_In_ServiceCollection()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddExtensions(configurationMock.Object, settings =>
                {
                    settings.Packages.Add("StoneAssemblies.Extensibility.DemoPlugin");
                    settings.Sources.Add(new ExtensionSource
                    {
                        Uri = "../../../../../output/nuget-local/",
                    });
                    settings.Sources.Add(new ExtensionSource
                    {
                        Uri = "https://api.nuget.org/v3/index.json",
                    });

                });

                Assert.Equal(2, serviceCollection.Count);
            }

            /// <summary>
            ///     Does not initialize the plugin without registering services in service collection.
            /// </summary>
            [Fact]
            public void Does_Not_Initialize_The_Plugin_Without_Registering_Services_In_ServiceCollection()
            {
                var configurationMock = new Mock<IConfiguration>();
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddExtensions(configurationMock.Object, settings =>
                {
                    settings.Packages.Add("StoneAssemblies.Extensibility.DemoPlugin");
                    settings.Sources.Add(new ExtensionSource
                    {
                        Uri = "../../../../../output/nuget-local/",
                    });
                    settings.Sources.Add(new ExtensionSource
                    {
                        Uri = "https://api.nuget.org/v3/index.json",
                    });

                    settings.Initialize = false;
                });

                Assert.Single(serviceCollection);
            }
        }
    }
}
