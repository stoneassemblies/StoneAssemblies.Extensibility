// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionManager.cs" company="Stone Assemblies">
// Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Threading;
    using System.Threading.Tasks;

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
            IConfiguration configuration,
            IServiceCollection serviceCollection,
            List<string> packageSources = null)
        {
            this.configuration = configuration;
            this.serviceCollection = serviceCollection;

            AssemblyLoadContext.Default.ResolvingUnmanagedDll += this.OnAssemblyLoadContextResolvingUnmanagedDll;
            AppDomain.CurrentDomain.AssemblyResolve += this.OnCurrentAppDomainAssemblyResolve;

            var sources = new List<string>();
            this.configuration.GetSection("Extensions")?.GetSection("Sources")?.Bind(sources);

            if (packageSources != null)
            {
                sources.AddRange(packageSources);
            }

            foreach (var source in sources)
            {
                var s = source;
                if (!Uri.TryCreate(s, UriKind.Absolute, out _) && Directory.Exists(s))
                {
                    s = Path.GetFullPath(s);
                }

                try
                {
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
        public IEnumerable<Assembly> GetExtensionAssemblies()
        {
            foreach (var extension in this.extensions)
            {
                yield return extension;
            }
        }

        /// <summary>
        ///     Loads the extensions.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public async Task LoadExtensionsAsync()
        {
            var packageIds = new List<string>();
            this.configuration.GetSection("Extensions").GetSection("Packages").Bind(packageIds);
            await this.LoadExtensionsAsync(packageIds);
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
        public async Task LoadExtensionsAsync(List<string> packageIds)
        {
            var pendingPackageIds = new List<string>(packageIds);
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

                    var packageIdParts = packageId.Split(':');
                    if (packageIdParts.Length == 2)
                    {
                        packageId = packageIdParts[0];
                        NuGetVersion.TryParse(packageIdParts[1], out packageVersion);
                    }

                    if (packageVersion == null)
                    {
                        var versions = await resource.GetAllVersionsAsync(
                                           packageId,
                                           new NullSourceCacheContext(),
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
            var packageDependency = new PackageDependency(packageId, new VersionRange(packageVersion));
            await this.DownloadPackageAsync(packageDependency, pluginsDirectoryPath);
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
                        try
                        {
                            await this.DownloadPackageAsync(packageDependency, DependenciesDirectoryFolderName);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, "Error downloading package {PackageId}", packageDependency.Id);
                        }
                    }

                    break;
                }
            }
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
            foreach (var sourceRepository in this.sourceRepositories)
            {
                var resource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>();

                if (!Directory.Exists(CacheDirectoryFolderName))
                {
                    Directory.CreateDirectory(CacheDirectoryFolderName);
                }

                var packageId = package.Id;
                var packageDependencyVersions = await resource.GetAllVersionsAsync(
                                                    packageId,
                                                    new NullSourceCacheContext(),
                                                    NullLogger.Instance,
                                                    CancellationToken.None);

                var packageVersion = package.VersionRange.FindBestMatch(packageDependencyVersions);
                if (packageVersion != null)
                {
                    var packageFileName = Path.Combine(CacheDirectoryFolderName, $"{packageId}.{packageVersion.OriginalVersion}.nupkg");
                    await resource.DownloadPackageAsync(package, packageVersion, packageFileName);
                    await this.DownloadDependenciesAsync(packageFileName);
                    PackageFile.ExtractToDirectory(packageFileName, destination);

                    break;
                }
            }
        }


        /// <summary>
        ///     The initialize extension.
        /// </summary>
        /// <param name="assembly">
        ///     The assembly.
        /// </param>
        private void InitializeExtension(Assembly assembly)
        {
            var startupType = assembly.GetTypes().FirstOrDefault(type => type.Name == "Startup");
            if (startupType != null)
            {
                var constructorInfo = startupType.GetConstructor(new[] { typeof(IConfiguration) });
                var startup = constructorInfo != null ? constructorInfo.Invoke(new object[] { this.configuration }) : Activator.CreateInstance(startupType);

                var configureServiceMethod = startupType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(info => info.Name == "ConfigureServices");

                if (configureServiceMethod != null && configureServiceMethod.GetParameters().Length == 1
                                                   && typeof(IServiceCollection).IsAssignableFrom(
                                                       configureServiceMethod.GetParameters()[0].ParameterType))
                {
                    try
                    {
                        configureServiceMethod.Invoke(startup, new object[] { this.serviceCollection });
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error configuring plugins services");
                    }
                }
            }
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
                        "Native dependency '{LibraryFileName}' was loaded successfully from '{LibraryPath} ",
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
                                var assembly = Assembly.LoadFrom(assemblyFile);
                                this.extensions.Add(assembly);
                                this.InitializeExtension(assembly);
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