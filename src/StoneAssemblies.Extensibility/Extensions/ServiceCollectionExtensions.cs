// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Stone Assemblies">
// Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility.Extensions
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using StoneAssemblies.Extensibility.Services;

    /// <summary>
    ///     The service collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Add the extension manager.
        /// </summary>
        /// <param name="serviceCollection">
        ///     The service collection.
        /// </param>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        /// <param name="load">
        ///     The load.
        /// </param>
        public static void AddExtensionManager(
            this IServiceCollection serviceCollection,
            IConfiguration configuration,
            bool load = false)
        {
            var extensionManager = new ExtensionManager(configuration, serviceCollection);
            serviceCollection.AddSingleton(extensionManager);
            if (load)
            {
                extensionManager.LoadExtensionsAsync().GetAwaiter().GetResult();
            }
        }
    }
}