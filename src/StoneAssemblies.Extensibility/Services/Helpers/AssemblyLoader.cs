// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyLoader.cs" company="Stone Assemblies">
// Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;

    using Serilog;

    /// <summary>
    /// The assembly loader.
    /// </summary>
    public static class AssemblyLoader
    {
        /// <summary>
        ///     The target frameworks.
        /// </summary>
        public static readonly string[] TargetFrameworks =
            {
#if NET5_0_OR_GREATER
                "net6.0",
                "net5.0",
#endif
                "netcoreapp3.1",
                "netcoreapp2.1",
                "netstandard2.1",
                "netstandard2.0",
                "net461",
                "net46",
                "net45",
            };

        /// <summary>
        ///     The package library directory names.
        /// </summary>
        private static readonly string[] PackageLibraryDirectoryNames =
            {
                Path.Combine("runtimes", "$(Platform)"),
                "lib",
            };

        /// <summary>
        ///     Load assembly from.
        /// </summary>
        /// <param name="directoryPath">
        ///     The directory path.
        /// </param>
        /// <param name="fileName">
        ///     The file name.
        /// </param>
        /// <returns>
        ///     The <see cref="Assembly" />.
        /// </returns>
        public static Assembly LoadAssemblyFrom(string directoryPath, string fileName)
        {
            var packageDirectories = Directory
                .EnumerateFiles(directoryPath, fileName + ".dll", SearchOption.AllDirectories)
                .GroupBy(f => f.Substring(0, f.IndexOf(Path.DirectorySeparatorChar, directoryPath.Length + 1)))
                .ToList();

            foreach (var grouping in packageDirectories)
            {
                var packageDirectory = grouping.Key;
                foreach (var packageLibraryDirectoryName in PackageLibraryDirectoryNames)
                {
                    var directoryName = packageLibraryDirectoryName.Replace("$(Platform)", GetRuntimeId());
                    if (TryLoadAnyAssemblyFrom(packageDirectory, directoryName, fileName, out var assembly))
                    {
                        return assembly;
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Try load assembly from package directory.
        /// </summary>
        /// <param name="packageDirectory">
        ///     The package directory.
        /// </param>
        /// <param name="directoryName">
        ///     The directory name.
        /// </param>
        /// <param name="fileName">
        ///     The file name.
        /// </param>
        /// <param name="assembly">
        ///     The assembly.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the assembly was loaded correctly, otherwise <c>false</c>.
        /// </returns>
        private static bool TryLoadAnyAssemblyFrom(
            string packageDirectory,
            string directoryName,
            string fileName,
            out Assembly assembly)
        {
            assembly = null;
            var packageLibraryDirectoryPath = Path.Combine(packageDirectory, directoryName);
            if (Directory.Exists(packageLibraryDirectoryPath))
            {
                var assemblyFiles = Directory
                    .EnumerateFiles(packageLibraryDirectoryPath, fileName + ".dll", SearchOption.AllDirectories)
                    .ToList();

                foreach (var targetFramework in TargetFrameworks)
                {
                    var assemblyFilesFilteredByTargetFramework = assemblyFiles
                        .Where(s => s.Contains($"{Path.DirectorySeparatorChar}lib{Path.DirectorySeparatorChar}{targetFramework}{Path.DirectorySeparatorChar}"))
                        .ToList();

                    if (TryLoadAnyAssemblyFrom(assemblyFilesFilteredByTargetFramework, out assembly))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Try to load assembly from a list of candidate files.
        /// </summary>
        /// <param name="assemblyFiles">
        ///     The assembly files.
        /// </param>
        /// <param name="assembly">
        ///     The assembly.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the assembly was loaded correctly, otherwise <c>false</c>.
        /// </returns>
        private static bool TryLoadAnyAssemblyFrom(List<string> assemblyFiles, out Assembly assembly)
        {
            assembly = null;
            foreach (var assemblyFile in assemblyFiles)
            {
                try
                {
                    Log.Information("Loading assembly file '{AssemblyName}'", assemblyFile);
#pragma warning disable S3885 // "Assembly.Load" should be used
                    assembly = Assembly.LoadFrom(assemblyFile);
#pragma warning restore S3885 // "Assembly.Load" should be used
                    Log.Information("Loaded assembly file '{AssemblyName}' successfully.", assemblyFile);

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Error loading assembly file '{AssemblyName}'", assemblyFile);
                }
            }

            return false;
        }

        /// <summary>
        ///     Gets the runtime id.
        /// </summary>
        /// <returns>
        ///     The runtime id.
        /// </returns>
        private static string GetRuntimeId()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" : "unix";
        }
    }
}