// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExtensionManagerSettings.cs" company="Stone Assemblies">
// Copyright © 2021 - 2022 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility
{
    /// <summary>
    /// The ExtensionManagerSettings interface.
    /// </summary>
    public interface IExtensionManagerSettings
    {
        /// <summary>
        ///     Gets or set the plugins directory.
        /// </summary>
        string PluginsDirectory { get; }

        /// <summary>
        ///     Gets or set the plugins dependencies directory.
        /// </summary>
        string PluginsDependenciesDirectory { get; }

        /// <summary>
        ///     Gets or set the cache directory.
        /// </summary>
        string CacheDirectory { get; }
    }
}