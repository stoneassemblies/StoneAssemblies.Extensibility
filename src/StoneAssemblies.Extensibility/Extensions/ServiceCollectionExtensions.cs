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
        /// <returns>
        ///     The <see cref="IExtensionManager" />.
        ///     The extension manager.
        /// </returns>
        public static IExtensionManager AddExtensions(
            this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            var extensionManager = new ExtensionManager(configuration, serviceCollection);
            serviceCollection.AddSingleton<IExtensionManager>(extensionManager);
            extensionManager.LoadExtensionsAsync().GetAwaiter().GetResult();
            return extensionManager;
        }
    }
}