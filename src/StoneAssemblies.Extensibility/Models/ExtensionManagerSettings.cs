namespace StoneAssemblies.Extensibility
{
    using System.Collections.Generic;

    /// <summary>
    ///     The ExtensionManagerSettings
    /// </summary>
    public class ExtensionManagerSettings : IExtensionManagerSettings
    {
        /// <summary>
        ///     Gets or set the plugins directory.
        /// </summary>
        public string PluginsDirectory { get; set; } = "plugins";

        /// <summary>
        ///     Gets or set the plugins dependencies directory.
        /// </summary>
        public string PluginsDependenciesDirectory { get; set; } = "lib";

        /// <summary>
        ///     Gets or set the cache directory.
        /// </summary>
        public string CacheDirectory { get; set; } = "cache";

        /// <summary>
        ///     Gets The packages.
        /// </summary>
        public List<string> Packages { get; } = new List<string>();

        /// <summary>
        ///     Gets extension sources.
        /// </summary>
        public List<ExtensionSource> Sources { get; } = new List<ExtensionSource>();

        /// <summary>
        ///     Gets or set a value to indicates whether the extension manager will initialize the plugins.
        /// </summary>
        public bool Initialize { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether skip schedule.
        /// </summary>
        public bool IgnoreSchedule { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether clean install.
        /// </summary>
        public bool IgnoreInstalledPackage { get; set; }
    }
}