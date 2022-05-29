// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Stone Assemblies">
// Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility.Extensions
{
    using System.Collections.Generic;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using StoneAssemblies.Extensibility.Services;
    using StoneAssemblies.Extensibility.Services.Interfaces;

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
        /// <param name="packages">
        ///     The packages.
        /// </param>
        /// <param name="packageSources">
        ///     The package sources.
        /// </param>
        /// <param name="extensionsDirectoryName">
        ///     The extension directory name.
        /// </param>
        /// <param name="cacheDirectoryName">
        ///     The cache directory name for extensions.
        /// </param>
        /// <param name="dependenciesDirectoryName">
        ///     The extensions dependencies directory name.
        /// </param>
        /// <param name="initialize">
        ///     Indicates whether the extension will be initialized.
        /// </param>
        /// <returns>
        ///     The <see cref="IExtensionManager" />.
        /// </returns>
        public static IExtensionManager AddExtensions(
            this IServiceCollection serviceCollection,
            IConfiguration configuration,
            List<string> packages = null,
            List<string> packageSources = null,
            string extensionsDirectoryName = "plugins",
            string cacheDirectoryName = "cache",
            string dependenciesDirectoryName = "lib",
            bool initialize = true)
        {
            IExtensionManager extensionManager = new ExtensionManager(serviceCollection, configuration, packageSources, extensionsDirectoryName, cacheDirectoryName, dependenciesDirectoryName);
            serviceCollection.AddSingleton(extensionManager);
            extensionManager.LoadExtensionsAsync(packages, initialize).GetAwaiter().GetResult();
            return extensionManager;
        }
    }
}