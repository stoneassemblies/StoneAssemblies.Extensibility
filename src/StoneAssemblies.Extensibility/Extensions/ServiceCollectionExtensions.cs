// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Stone Assemblies">
// Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

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
        /// <param name="configure">
        ///     The configure action.
        /// </param>
        /// <returns>
        ///     The <see cref="IExtensionManager" />.
        /// </returns>
        public static IExtensionManager AddExtensions(
            this IServiceCollection serviceCollection,
            IConfiguration configuration, Action<ExtensionManagerSettings> configure = null)
        {
            var settings = new ExtensionManagerSettings();
            configuration?.GetSection("Extensions")?.Bind(settings);
            if (configure != null)
            {
                configure(settings);
            }

            IExtensionManager extensionManager = new ExtensionManager(serviceCollection, configuration, settings);
            serviceCollection.AddSingleton(extensionManager);
            extensionManager.LoadExtensionsAsync().GetAwaiter().GetResult();
            return extensionManager;
        }
    }
}