// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISchedule.cs" company="Stone Assemblies">
// Copyright © 2021 - 2022 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility
{
    using System.Collections.Generic;

    /// <summary>
    /// The Schedule interface.
    /// </summary>
    public interface ISchedule
    {
        /// <summary>
        ///     Gets the install.
        /// </summary>
        IReadOnlyCollection<string> Install { get; }

        /// <summary>
        ///     Gets the uninstall.
        /// </summary>
        IReadOnlyCollection<string> Uninstall { get; }

        /// <summary>
        ///     Determines whether the package scheduled to install.
        /// </summary>
        /// <param name="packageId">
        ///     The package id.
        /// </param>
        /// <param name="version">
        ///     The package version.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        bool IsExtensionPackageScheduledToInstall(string packageId, out string version);

        /// <summary>
        ///     Determines whether the package scheduled to uninstall.
        /// </summary>
        /// <param name="packageId">
        ///     The package id.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        bool IsExtensionPackageScheduledToUninstall(string packageId);
    }
}