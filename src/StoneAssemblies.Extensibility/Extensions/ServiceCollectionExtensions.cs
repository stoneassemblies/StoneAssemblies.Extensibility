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
        /// <returns>
        ///     The <see cref="IExtensionManager" />.
        ///     The extension manager.
        /// </returns>
        public static IExtensionManager AddExtensions(
            this IServiceCollection serviceCollection,
            IConfiguration configuration,
            List<string> packages = null,
            List<string> packageSources = null)
        {
            var extensionManager = new ExtensionManager(serviceCollection, configuration, packageSources);
            serviceCollection.AddSingleton<IExtensionManager>(extensionManager);
            extensionManager.LoadExtensionsAsync(packages).GetAwaiter().GetResult();
            return extensionManager;
        }
    }
}