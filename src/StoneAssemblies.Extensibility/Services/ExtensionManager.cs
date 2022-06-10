// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionManager.cs" company="Stone Assemblies">
// Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility
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
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using NuGet.Common;
    using NuGet.Configuration;
    using NuGet.Packaging;
    using NuGet.Packaging.Core;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;

    using Serilog;

    /// <summary>
    ///     The extension manager.
    /// </summary>
    public class ExtensionManager : IExtensionManager
    {
        private readonly ExtensionManagerSettings settings;

        /// <summary>
        ///     The target framework dependencies.
        /// </summary>
        private static readonly string[] TargetFrameworkDependencies =
        {
#if NET5_0_OR_GREATER
            ".NETCoreApp,Version=v6.0",
            ".NETCoreApp,Version=v5.0",
#endif
            ".NETCoreApp,Version=v3.1",
            ".NetStandard,Version=v2.1",
            ".NetStandard,Version=v2.0",
        };

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
        ///     The configuration.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        ///     The source repositories.
        /// </summary>
        private readonly List<SourceRepository> sourceRepositories = new List<SourceRepository>();

        /// <summary>
        ///     The searchable repositories.
        /// </summary>
        private readonly List<SourceRepository> searchableRepositories = new List<SourceRepository>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionManager" /> class.
        /// </summary>
        public ExtensionManager(IServiceCollection serviceCollection, IConfiguration configuration, ExtensionManagerSettings settings)
        {
            this.serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.Initialize();
        }

        private void Initialize()
        {
            AssemblyLoadContext.Default.ResolvingUnmanagedDll += this.OnAssemblyLoadContextResolvingUnmanagedDll;
            AppDomain.CurrentDomain.AssemblyResolve += this.OnCurrentAppDomainAssemblyResolve;
            
            var extensionSources = new List<ExtensionSource>();
            if (this.settings.Sources != null)
            {
                extensionSources.AddRange(this.settings.Sources);
            }

            foreach (var extensionSource in extensionSources)
            {
                try
                {
                    var source = extensionSource;
                    if (!Uri.TryCreate(source.Uri, UriKind.Absolute, out _) && Directory.Exists(source.Uri))
                    {
                        source.Uri = Path.GetFullPath(source.Uri);
                    }

                    SourceRepository sourceRepository = null;
                    if (!string.IsNullOrWhiteSpace(source.Username) && !string.IsNullOrWhiteSpace(source.Password))
                    {
                        var packageSource = new PackageSource(source.Uri)
                        {
                            Credentials = new PackageSourceCredential(extensionSource.Uri, extensionSource.Username, extensionSource.Password, true, null),
                        };

                        sourceRepository = Repository.Factory.GetCoreV3(packageSource);
                    }
                    else
                    {
                        sourceRepository = Repository.Factory.GetCoreV3(source.Uri);
                    }

                    this.sourceRepositories.Add(sourceRepository);
                    if (extensionSource.Searchable)
                    {
                        this.searchableRepositories.Add(sourceRepository);
                    }
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
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        async Task IExtensionManager.LoadExtensionsAsync()
        {
            await this.LoadExtensionsAsync();
            if (this.settings.Initialize)
            {
                this.InitializeExtensions();
            }
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

        public async IAsyncEnumerable<ExtensionPackage> GetAvailableExtensionPackagesAsync(int skip, int take)
        {
            var pluginsDirectoryPath = Path.GetFullPath(this.settings.PluginsDirectory);
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("nuget", "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd");
            var installedPackages = Directory.EnumerateFiles(pluginsDirectoryPath, "*.nuspec", SearchOption.AllDirectories).Select(
                filePath =>
                    {
                        var document = XDocument.Load(filePath);
                        var id = document.XPathSelectElement("/nuget:package/nuget:metadata/nuget:id", namespaceManager)?.Value;
                        var version = document.XPathSelectElement("/nuget:package/nuget:metadata/nuget:version", namespaceManager)?.Value;
                        return (Id:id, Version:version);
                    }).ToDictionary(valueTuple => valueTuple.Id);

            foreach (var repository in this.searchableRepositories)
            {
                var packageSearchResource = await repository.GetResourceAsync<PackageSearchResource>();
                var searchFilter = new SearchFilter(true);
                var searchResults = await packageSearchResource.SearchAsync("", searchFilter, skip, take, NullLogger.Instance, CancellationToken.None);
                foreach (var packageSearchMetadata in searchResults)
                {
                    var versionInfos = (await packageSearchMetadata.GetVersionsAsync()).ToList();
                    VersionInfo installedVersion = null;
                    if (installedPackages.TryGetValue(packageSearchMetadata.Identity.Id, out var installedPackage))
                    {
                        installedVersion = versionInfos?.FirstOrDefault(info => info.Version.OriginalVersion == installedPackage.Version);
                        installedPackages.Remove(packageSearchMetadata.Identity.Id);
                    }

                    yield return new ExtensionPackage(packageSearchMetadata.Identity.Id, versionInfos, installedVersion);
                }
            }

            foreach (var installedPackage in installedPackages)
            {
                var installedPackageValue = installedPackage.Value;
                yield return new ExtensionPackage(installedPackage.Key, null, new VersionInfo(new NuGetVersion(installedPackageValue.Version)));
            }
        }

        /// <summary>
        ///     Loads the extensions from package ids.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        private async Task LoadExtensionsAsync()
        {
            var pendingPackageIds = this.settings.Packages;
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

            var pluginsDirectoryPath = Path.GetFullPath(this.settings.PluginsDirectory);
            if (!Directory.Exists(pluginsDirectoryPath))
            {
                Log.Information("Creating {Directory} directory", this.settings.PluginsDirectory);

                Directory.CreateDirectory(pluginsDirectoryPath);

                Log.Information("Created {Directory} directory", this.settings.PluginsDirectory);
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
                            await this.EnsureDownloadPackageAsync(packageDependency, this.settings.PluginsDependenciesDirectory);
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
                        package.VersionRange.ToString(),
                        sourceRepository.PackageSource.SourceUri);

                    var resource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>();

                    var cacheDirectoryFolderName = Path.GetFullPath(this.settings.CacheDirectory);
                    if (!Directory.Exists(cacheDirectoryFolderName))
                    {
                        Log.Information("Creating {Directory} directory", this.settings.CacheDirectory);

                        Directory.CreateDirectory(cacheDirectoryFolderName);

                        Log.Information("Created {Directory} directory", this.settings.CacheDirectory);
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
                            package.VersionRange.ToString(),
                            sourceRepository.PackageSource.SourceUri);

                        var packageFileName = Path.Combine(
                            this.settings.CacheDirectory,
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
            var dependencyDirectoryFullPath = Path.GetFullPath(this.settings.PluginsDependenciesDirectory);
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
            Assembly assembly = null;
            if (!string.IsNullOrWhiteSpace(args?.Name))
            {
                var fileName = args.Name.Split(',')[0];
                var assemblyLocalCopyFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName + ".dll");
                if (File.Exists(assemblyLocalCopyFilePath))
                {
                    try
                    {
                        Log.Information("Loading assembly from local copy {FileName}", assemblyLocalCopyFilePath);

                        assembly = Assembly.LoadFrom(assemblyLocalCopyFilePath);

                        Log.Information("Loaded assembly from local copy {FileName}", assemblyLocalCopyFilePath);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Error loading assembly local copy {FileName}", assemblyLocalCopyFilePath);
                    }
                }

                if (assembly == null)
                {
                    Log.Information("Loading assembly from {FileName} from lib directory", fileName);

                    var directoryPath = Path.GetFullPath(this.settings.PluginsDependenciesDirectory);
                    assembly = AssemblyLoader.LoadAssemblyFrom(directoryPath, fileName);

                    if (assembly != null)
                    {
                        Log.Information("Loaded assembly from {FileName} from lib directory", fileName);
                    }
                }
            }

            return assembly;
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