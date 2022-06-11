namespace StoneAssemblies.Extensibility
{
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