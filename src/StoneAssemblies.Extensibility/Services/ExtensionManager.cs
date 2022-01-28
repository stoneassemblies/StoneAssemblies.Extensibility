﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionManager.cs" company="Stone Assemblies">
// Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Threading;
    using System.Threading.Tasks;

    using ICSharpCode.SharpZipLib.Zip;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using NuGet.Common;
    using NuGet.Packaging;
    using NuGet.Packaging.Core;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;

    using Serilog;

    using StoneAssemblies.Extensibility.Extensions;
    using StoneAssemblies.Extensibility.Services.Helpers;
    using StoneAssemblies.Extensibility.Services.Interfaces;

    /// <summary>
    ///     The extension manager.
    /// </summary>
    public class ExtensionManager : IExtensionManager
    {
        /// <summary>
        ///     The cache directory folder name.
        /// </summary>
        private const string CacheDirectoryFolderName = "cache";

        /// <summary>
        ///     The dependencies directory folder name.
        /// </summary>
        private const string DependenciesDirectoryFolderName = "lib";

        /// <summary>
        ///     The plugins directory folder name.
        /// </summary>
        private const string PluginsDirectoryFolderName = "plugins";

        /// <summary>
        ///     The target framework dependencies.
        /// </summary>
        private static readonly string[] TargetFrameworkDependencies =
            {
#if NET5_0_OR_GREATER
                ".NETCoreApp,Version=v5.0",
#endif
                ".NETCoreApp,Version=v3.1",
                ".NetStandard,Version=v2.1",
                ".NetStandard,Version=v2.0",
            };

        /// <summary>
        ///     The configuration.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        ///     The extensions.
        /// </summary>
        private readonly List<Assembly> extensions = new List<Assembly>();

        /// <summary>
        /// The startup objects
        /// </summary>
        private readonly ArrayList startupObjects = new ArrayList();

        /// <summary>
        ///     The service collection.
        /// </summary>
        private readonly IServiceCollection serviceCollection;

        /// <summary>
        ///     The repository.
        /// </summary>
        private readonly List<SourceRepository> sourceRepositories = new List<SourceRepository>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionManager" /> class.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        /// <param name="serviceCollection">
        ///     The service collection.
        /// </param>
        /// <param name="packageSources">
        ///     The package sources.
        /// </param>
        public ExtensionManager(
            IServiceCollection serviceCollection,
            IConfiguration configuration,
            List<string> packageSources = null)
        {
            this.configuration = configuration;
            this.serviceCollection = serviceCollection;

            AssemblyLoadContext.Default.ResolvingUnmanagedDll += this.OnAssemblyLoadContextResolvingUnmanagedDll;
            AppDomain.CurrentDomain.AssemblyResolve += this.OnCurrentAppDomainAssemblyResolve;

            var sources = new List<string>();
            this.configuration?.GetSection("Extensions")?.GetSection("Sources")?.Bind(sources);

            if (packageSources != null)
            {
                sources.AddRange(packageSources);
            }

            foreach (var source in sources)
            {
                try
                {
                    var s = source;
                    if (!Uri.TryCreate(s, UriKind.Absolute, out _) && Directory.Exists(s))
                    {
                        s = Path.GetFullPath(s);
                    }

                    var sourceRepository = Repository.Factory.GetCoreV3(s);
                    this.sourceRepositories.Add(sourceRepository);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error creating source repository");
                }
            }
        }

        /// <summary>
        ///     Gets the extension assemblies.
        /// </summary>
        /// <returns>
        ///     The <see cref="IEnumerable{Assembly}" />.
        /// </returns>
        IEnumerable<Assembly> IExtensionManager.GetExtensionAssemblies()
        {
            foreach (var extension in this.extensions)
            {
                yield return extension;
            }
        }

        /// <summary>
        ///     Loads the extensions from package ids.
        /// </summary>
        /// <param name="packageIds">
        ///     The package ids.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        async Task IExtensionManager.LoadExtensionsAsync(List<string> packageIds)
        {
            await this.LoadExtensionsAsync(packageIds);
            this.InitializeExtensions();
        }

        /// <inheritdoc />
        public void Configure(params object[] parameters)
        {
            parameters = parameters.Where(p => p != null).ToArray();
            var types = parameters.Select(o => o.GetType()).ToArray();

            foreach (var startupObject in this.startupObjects)
            {
                var methods = startupObject.GetType().GetMethods()
                    .Where(info => info.Name == "Configure" && info.GetParameters().Length == parameters.Length)
                    .ToList();

                MethodInfo method = null;
                for (var i = 0; i < methods.Count && method == null; i++)
                {
                    method = methods[i];
                    var methodParameters = method.GetParameters();
                    for (var j = 0; j < methodParameters.Length && method != null; j++)
                    {
                        var parameterType = methodParameters[j].ParameterType;
                        var type = types[j];
                        if (!parameterType.IsAssignableFrom(type))
                        {
                            method = null;
                        }
                    }
                }

                if (method != null)
                {
                    try
                    {
                        method.Invoke(startupObject, parameters);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error configuring extension");
                    }
                }
            }
        }

        /// <summary>
        ///     Loads the extensions from package ids.
        /// </summary>
        /// <param name="packageIds">
        ///     The package ids.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        private async Task LoadExtensionsAsync(List<string> packageIds)
        {
            var pendingPackageIds = this.MergeWithPackageIdsFromConfiguration(packageIds);
            foreach (var sourceRepository in this.sourceRepositories)
            {
                if (pendingPackageIds.Count == 0)
                {
                    break;
                }

                var resource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>();
                for (var idx = pendingPackageIds.Count - 1; idx >= 0; idx--)
                {
                    var packageId = pendingPackageIds[idx];
                    NuGetVersion packageVersion = null;

                    bool parsed = false;
                    var packageIdParts = packageId.Split(':');
                    if (packageIdParts.Length == 2)
                    {
                        packageId = packageIdParts[0];
                        parsed = NuGetVersion.TryParse(packageIdParts[1], out packageVersion);
                    }

                    if (!parsed)
                    {
                        var versions = await resource.GetAllVersionsAsync(
                                           packageId,
                                           NullSourceCacheContext.Instance,
                                           NullLogger.Instance,
                                           CancellationToken.None);

                        packageVersion = versions.AsEnumerable().LastOrDefault();
                    }

                    if (await this.TryLoadExtensionsPackageAsync(packageId, packageVersion))
                    {
                        pendingPackageIds.RemoveAt(idx);
                    }
                }
            }
        }

        /// <summary>
        ///     Merges package ids with package ids from configuration.
        /// </summary>
        /// <param name="packageIds">
        ///     The package ids.
        /// </param>
        /// <returns>
        ///     A merged list with package ids and  package ids from configuration.
        /// </returns>
        private List<string> MergeWithPackageIdsFromConfiguration(List<string> packageIds)
        {
            var mergedPackageIds = new List<string>();
            if (packageIds != null)
            {
                mergedPackageIds.AddRange(packageIds);
            }

            var packageIdsFromConfiguration = new List<string>();
            this.configuration.GetSection("Extensions")?.GetSection("Packages")?.Bind(packageIdsFromConfiguration);
            if (packageIdsFromConfiguration.Count > 0)
            {
                mergedPackageIds.AddRange(packageIdsFromConfiguration);
            }

            return mergedPackageIds.Distinct().ToList();
        }

        /// <summary>
        /// Initialize extensions.
        /// </summary>
        private void InitializeExtensions()
        {
            foreach (var extension in this.extensions)
            {
                try
                {
                    var startup = extension.InitializeExtension(this.serviceCollection, this.configuration, this);
                    this.startupObjects.Add(startup);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error initializing extension {Name}", extension.GetName().Name);
                }
            }
        }

        /// <summary>
        ///     Try load package.
        /// </summary>
        /// <param name="packageId">
        ///     The package id.
        /// </param>
        /// <param name="packageVersion">
        ///     The package version.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        private async Task<bool> TryLoadExtensionsPackageAsync(string packageId, NuGetVersion packageVersion)
        {
            if (packageVersion == null)
            {
                return false;
            }

            var pluginsDirectoryPath = Path.GetFullPath(PluginsDirectoryFolderName);
            if (!Directory.Exists(pluginsDirectoryPath))
            {
                Log.Information("Creating {Directory} directory", PluginsDirectoryFolderName);

                Directory.CreateDirectory(pluginsDirectoryPath);

                Log.Information("Created {Directory} directory", PluginsDirectoryFolderName);
            }

            var packageDependency = new PackageDependency(packageId, new VersionRange(packageVersion));
            await this.EnsureDownloadPackageAsync(packageDependency, pluginsDirectoryPath);
            return this.TryLoadPackageAssemblies(packageId, packageVersion, pluginsDirectoryPath);
        }

        /// <summary>
        ///     Downloads the dependencies of the packages.
        /// </summary>
        /// <param name="packageFileName">
        ///     The package file name.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        private async Task DownloadDependenciesAsync(string packageFileName)
        {
            using var archiveReader = new PackageArchiveReader(packageFileName);
            foreach (var dependencyGroup in archiveReader.GetPackageDependencies())
            {
                if (TargetFrameworkDependencies.Contains(dependencyGroup.TargetFramework.DotNetFrameworkName, StringComparer.InvariantCultureIgnoreCase))
                {
                    foreach (var packageDependency in dependencyGroup.Packages)
                    {
                        var packageName = packageDependency.Id;
                        var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(
                            a => packageName.Equals(a.GetName()?.Name, StringComparison.InvariantCultureIgnoreCase));
                        if (assembly == null)
                        {
                            await this.EnsureDownloadPackageAsync(packageDependency, DependenciesDirectoryFolderName);
                        }
                        else
                        {
                            Log.Warning("Skipping download package {PackageName} because its name matches with an already loaded assembly.", packageName);
                        }
                    }

                    break;
                }
            }
        }

        /// <summary>
        ///     Ensures download package.
        /// </summary>
        /// <param name="packageDependency">
        ///     The package dependency
        /// </param>
        /// <param name="destination">
        ///     The destination.
        /// </param>
        /// <returns>The task.</returns>
        private async Task EnsureDownloadPackageAsync(PackageDependency packageDependency, string destination)
        {
            var succeeded = false;
            do
            {
                try
                {
                    Log.Information("Downloading package {PackageId}", packageDependency.Id);

                    await this.DownloadPackageAsync(packageDependency, destination);
                    succeeded = true;

                    Log.Information("Downloaded package {PackageId}", packageDependency.Id);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error downloading package {PackageId}.", packageDependency.Id);
                }
            }
            while (!succeeded);
        }

        /// <summary>
        ///     Downloads the package async.
        /// </summary>
        /// <param name="package">
        ///     The package.
        /// </param>
        /// <param name="destination">
        ///     The destination.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        private async Task DownloadPackageAsync(PackageDependency package, string destination)
        {
            var succeeded = false;
            do
            {
                foreach (var sourceRepository in this.sourceRepositories)
                {
                    Log.Information(
                        "Searching {PackageId} {PackageVersion} in source {PackageSource}",
                        package.Id,
                        package.VersionRange.OriginalString,
                        sourceRepository.PackageSource.SourceUri);

                    var resource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>();

                    var cacheDirectoryFolderName = Path.GetFullPath(CacheDirectoryFolderName);
                    if (!Directory.Exists(cacheDirectoryFolderName))
                    {
                        Log.Information("Creating {Directory} directory", CacheDirectoryFolderName);

                        Directory.CreateDirectory(cacheDirectoryFolderName);

                        Log.Information("Created {Directory} directory", CacheDirectoryFolderName);
                    }

                    var packageId = package.Id;
                    var packageDependencyVersions = await resource.GetAllVersionsAsync(
                                                        packageId,
                                                        NullSourceCacheContext.Instance,
                                                        NullLogger.Instance,
                                                        CancellationToken.None);

                    var packageVersion = package.VersionRange.FindBestMatch(packageDependencyVersions);
                    if (packageVersion != null)
                    {
                        Log.Information(
                            "Found {PackageId} {PackageVersion} in source {PackageSource}",
                            package.Id,
                            package.VersionRange.OriginalString,
                            sourceRepository.PackageSource.SourceUri);

                        var packageFileName = Path.Combine(
                            CacheDirectoryFolderName,
                            $"{packageId}.{packageVersion.OriginalVersion}.nupkg");

                        if (await resource.DownloadPackageAsync(package, packageVersion, packageFileName))
                        {
                            await this.DownloadDependenciesAsync(packageFileName);

                            Log.Information(
                                "Extracting {PackageId} {PackageVersion} to {Destination}",
                                package.Id,
                                package.VersionRange.OriginalString,
                                destination);

                            PackageFile.ExtractToDirectory(packageFileName, destination);

                            Log.Information(
                                "Extracted {PackageId} {PackageVersion} to {Destination}",
                                package.Id,
                                package.VersionRange.OriginalString,
                                destination);

                            succeeded = true;
                            break;
                        }
                    }

                    Log.Warning(
                        "Not found package {PackageId} {PackageVersion} in source {PackageSource} ",
                        packageId,
                        package.VersionRange.OriginalString,
                        sourceRepository.PackageSource.SourceUri);
                }
            }
            while (!succeeded);
        }

        /// <summary>
        ///     Called on assembly load context resolving unmanaged library.
        /// </summary>
        /// <param name="assembly">
        ///     The assembly.
        /// </param>
        /// <param name="libraryFileName">
        ///     The library file name.
        /// </param>
        /// <returns>
        ///     The <see cref="IntPtr" />.
        /// </returns>
        private IntPtr OnAssemblyLoadContextResolvingUnmanagedDll(Assembly assembly, string libraryFileName)
        {
            var dependencyDirectoryFullPath = Path.GetFullPath(DependenciesDirectoryFolderName);
            var libraryPaths = Directory.EnumerateFiles(
                dependencyDirectoryFullPath,
                libraryFileName,
                SearchOption.AllDirectories);

            var unmanagedDll = IntPtr.Zero;
            foreach (var libraryPath in libraryPaths)
            {
                try
                {
                    Log.Information(
                        "Loading native dependency '{LibraryFileName}' from '{LibraryPath}'",
                        libraryFileName,
                        libraryPath);

                    unmanagedDll = NativeLibrary.Load(libraryPath);

                    Log.Information(
                        "Loaded native dependency '{LibraryFileName}' successfully from '{LibraryPath} ",
                        libraryFileName,
                        libraryPath);

                    break;
                }
                catch (Exception ex)
                {
                    Log.Warning(
                        ex,
                        "Error loading '{LibraryFileName}' from '{LibraryPath}'",
                        libraryFileName,
                        libraryPath);
                }
            }

            return unmanagedDll;
        }

        /// <summary>
        ///     The on current app domain assembly resolve.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="args">
        ///     The args.
        /// </param>
        /// <returns>
        ///     The <see cref="Assembly" />.
        /// </returns>
        private Assembly OnCurrentAppDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(args?.Name))
            {
                var fileName = args.Name.Split(',')[0];
                var directoryPath = Path.GetFullPath(DependenciesDirectoryFolderName);
                return AssemblyLoader.LoadAssemblyFrom(directoryPath, fileName);
            }

            return null;
        }

        /// <summary>
        ///     Try load package assemblies.
        /// </summary>
        /// <param name="packageId">
        ///     The package id.
        /// </param>
        /// <param name="packageVersion">
        ///     The package version.
        /// </param>
        /// <param name="pluginsDirectoryPath">
        ///     The plugin directory path.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the package is loaded, otherwise <c>false</c>.
        /// </returns>
        private bool TryLoadPackageAssemblies(
            string packageId,
            NuGetVersion packageVersion,
            string pluginsDirectoryPath)
        {
            var packageDirectoryName = $"{packageId}.{packageVersion.OriginalVersion}";
            var pluginDirectoryPath = Path.Combine(pluginsDirectoryPath, packageDirectoryName);

            foreach (var targetFramework in AssemblyLoader.TargetFrameworks)
            {
                var targetFrameworkDirectoryPath = Path.Combine(pluginDirectoryPath, "lib", targetFramework);
                if (Directory.Exists(targetFrameworkDirectoryPath))
                {
                    var assemblyFiles = Directory.EnumerateFiles(targetFrameworkDirectoryPath, "*.dll").ToList();
                    if (assemblyFiles.Count > 0)
                    {
                        try
                        {
                            foreach (var assemblyFile in assemblyFiles)
                            {
#pragma warning disable S3885 // "Assembly.Load" should be used
                                var assembly = Assembly.LoadFrom(assemblyFile);
#pragma warning restore S3885 // "Assembly.Load" should be used
                                this.extensions.Add(assembly);
                            }

                            return true;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error loading plugin assembly");
                        }
                    }
                }
            }

            return false;
        }
    }
}