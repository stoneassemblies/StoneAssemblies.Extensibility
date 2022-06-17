namespace StoneAssemblies.Extensibility
{
    using System.Collections.Generic;

    using NuGet.Protocol.Core.Types;

    /// <summary>
    ///     The extension package.
    /// </summary>
    public class ExtensionPackage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionPackage" /> class.
        /// </summary>
        /// <param name="id">
        ///     The package id.
        /// </param>
        /// <param name="versions">
        ///     The version infos.
        /// </param>
        /// <param name="installedVersion">
        ///     The installed version.
        /// </param>
        public ExtensionPackage(string id, List<VersionInfo> versions, VersionInfo installedVersion)
        {
            this.Id = id;
            this.Versions = versions;
            this.InstalledVersion = installedVersion;
        }

        /// <summary>
        ///     Gets the id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     Gets the installed version.
        /// </summary>
        public VersionInfo InstalledVersion { get; }

        /// <summary>
        ///     Gets the version infos.
        /// </summary>
        public List<VersionInfo> Versions { get; }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return "${Id} (Installed version: {InstalledVersion}";
        }
    }
}