// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageFile.cs" company="Stone Assemblies">
//     Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility.Services.Helpers
{
    using System.IO;
    using System.IO.Compression;

    /// <summary>
    ///     The package helper.
    /// </summary>
    public static class PackageFile
    {
        /// <summary>
        ///     Extract package file.
        /// </summary>
        /// <param name="packageFileName">
        ///     The package file name.
        /// </param>
        /// <param name="directory">
        ///     The destination directory.
        /// </param>
        public static void ExtractToDirectory(string packageFileName, string directory)
        {
            var packageDirectoryName = Path.GetFileNameWithoutExtension(packageFileName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var packagesDirectoryPath = Path.Combine(directory, packageDirectoryName);
            if (File.Exists(packageFileName) && !Directory.Exists(packagesDirectoryPath))
            {
                ZipFile.ExtractToDirectory(packageFileName, packagesDirectoryPath, true);
            }
        }
    }
}