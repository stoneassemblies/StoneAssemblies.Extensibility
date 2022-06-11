// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExtensionManager.cs" company="Stone Assemblies">
// Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    ///     The ExtensionManager interface.
    /// </summary>
    public interface IExtensionManager
    {
        /// <summary>
        ///     Gets the settings.
        /// </summary>
        IExtensionManagerSettings Settings { get; }

        /// <summary>
        ///     Gets  schedule async.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        Task<ISchedule> GetScheduleAsync();

        /// <summary>
        ///     Call configure method of all start up objects that match with give arguments types.
        /// </summary>
        /// <param name="parameters">
        ///     The parameters.
        /// </param>
        void Configure(params object[] parameters);

        /// <summary>
        ///     Gets the available extension packages async.
        /// </summary>
        /// <param name="skip">
        ///     The skip.
        /// </param>
        /// <param name="take">
        ///     The take.
        /// </param>
        /// <returns>
        ///     The <see cref="IAsyncEnumerable{ExtensionPackage}" />.
        /// </returns>
        IAsyncEnumerable<ExtensionPackage> GetAvailableExtensionPackagesAsync(int skip, int take);

        /// <summary>
        ///     Gets the extension assemblies.
        /// </summary>
        /// <returns>
        ///     The <see cref="IEnumerable{Assembly}" />.
        /// </returns>
        IEnumerable<Assembly> GetExtensionAssemblies();

        /// <summary>
        ///     Loads the extensions from package ids.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        Task LoadExtensionsAsync();

        /// <summary>
        ///     Schedule install package.
        /// </summary>
        /// <param name="packageId">
        ///     The package id.
        /// </param>
        /// <param name="version">
        ///     The version.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        Task ScheduleInstallPackageAsync(string packageId, string version);

        /// <summary>
        ///     Schedule uninstall package.
        /// </summary>
        /// <param name="packageId">
        ///     The package id.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        Task ScheduleUnInstallPackageAsync(string packageId);

        /// <summary>
        /// Clears the schedule.
        /// </summary>
        Task RemoveScheduleAsync();
    }
}