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
        ///     Gets the package search metadata.
        /// </summary>
        private readonly IPackageSearchMetadata packageSearchMetadata;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionPackage" /> class.
        /// </summary>
        /// <param name="packageSearchMetadata">
        ///     The package search metadata.
        /// </param>
        /// <param name="versionInfos">
        ///     The version infos.
        /// </param>
        /// <param name="installedVersion">
        ///     The installed version.
        /// </param>
        public ExtensionPackage(
            IPackageSearchMetadata packageSearchMetadata, List<VersionInfo> versionInfos, VersionInfo installedVersion)
        {
            this.VersionInfos = versionInfos;
            this.packageSearchMetadata = packageSearchMetadata;
            this.InstalledVersion = installedVersion;
        }

        /// <summary>
        ///     Gets the id.
        /// </summary>
        public string Id => this.packageSearchMetadata.Identity.Id;

        /// <summary>
        ///     Gets the installed version.
        /// </summary>
        public VersionInfo InstalledVersion { get; }

        /// <summary>
        ///     Gets or sets the version infos.
        /// </summary>
        public List<VersionInfo> VersionInfos { get; }
    }
}