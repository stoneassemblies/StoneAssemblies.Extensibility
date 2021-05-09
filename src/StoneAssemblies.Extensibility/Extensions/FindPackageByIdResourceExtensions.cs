// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FindPackageByIdResourceExtensions.cs" company="Stone Assemblies">
//     Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility.Extensions
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using NuGet.Common;
    using NuGet.Packaging.Core;
    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;

    using Serilog;

    /// <summary>
    ///     The FindPackageByIdResourceExtensions.
    /// </summary>
    public static class FindPackageByIdResourceExtensions
    {
        /// <summary>
        ///     Download package.
        /// </summary>
        /// <param name="resource">
        ///     The resource.
        /// </param>
        /// <param name="packageId">
        ///     The package id.
        /// </param>
        /// <param name="packageVersion">
        ///     The package version.
        /// </param>
        /// <param name="packageFileName">
        ///     The package file name.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public static async Task DownloadPackageAsync(
            this FindPackageByIdResource resource,
            string packageId,
            NuGetVersion packageVersion,
            string packageFileName)
        {
            if (!File.Exists(packageFileName))
            {
                await using (var packageStream = new FileStream(packageFileName, FileMode.Create, FileAccess.Write))
                {
                    Log.Information(
                        "Downloading dependency package {PackageId} {PackageVersion}",
                        packageId,
                        packageVersion.OriginalVersion);

                    await resource.CopyNupkgToStreamAsync(
                        packageId,
                        packageVersion,
                        packageStream,
                        new NullSourceCacheContext(),
                        NullLogger.Instance,
                        CancellationToken.None);
                }
            }
        }

        /// <summary>
        ///     Download package.
        /// </summary>
        /// <param name="resource">
        ///     The resource.
        /// </param>
        /// <param name="package">
        ///     The package.
        /// </param>
        /// <param name="packageVersion">
        ///     The package version.
        /// </param>
        /// <param name="packageFileName">
        ///     The package file name.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public static async Task DownloadPackageAsync(
            this FindPackageByIdResource resource,
            PackageDependency package,
            NuGetVersion packageVersion,
            string packageFileName)
        {
            await resource.DownloadPackageAsync(package.Id, packageVersion, packageFileName);
        }
    }
}