// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Stone Assemblies">
// Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

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
        /// <returns>
        ///     The <see cref="IExtensionManager" />.
        /// </returns>
        public static IExtensionManager AddExtensions(
            this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            IExtensionManager extensionManager = new ExtensionManager(serviceCollection, configuration);
            serviceCollection.AddSingleton(extensionManager);
            extensionManager.LoadExtensionsAsync().GetAwaiter().GetResult();
            return extensionManager;
        }

        /// <summary>
        ///     Add the extension manager.
        /// </summary>
        /// <param name="serviceCollection">
        ///     The service collection.
        /// </param>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        /// <param name="settings">
        ///     The ExtensionManagerSettings.
        /// </param>
        /// <returns>
        ///     The <see cref="IExtensionManager" />.
        /// </returns>
        public static IExtensionManager AddExtensions(
            this IServiceCollection serviceCollection,
            IConfiguration configuration, ExtensionManagerSettings settings)
        {
            IExtensionManager extensionManager = new ExtensionManager(serviceCollection, configuration, settings);
            serviceCollection.AddSingleton(extensionManager);
            extensionManager.LoadExtensionsAsync().GetAwaiter().GetResult();
            return extensionManager;
        }
    }
}