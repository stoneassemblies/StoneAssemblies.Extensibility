namespace StoneAssemblies.Extensibility
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The schedule.
    /// </summary>
    public sealed class Schedule : ISchedule
    {
        /// <summary>
        /// Gets the install.
        /// </summary>
        public List<string> Install { get; } = new List<string>();

        /// <summary>
        /// Gets the un install.
        /// </summary>
        public List<string> Uninstall { get; } = new List<string>();

        /// <summary>
        /// The install.
        /// </summary>
        IReadOnlyCollection<string> ISchedule.Install => this.Install?.AsReadOnly();

        /// <summary>
        /// The un install.
        /// </summary>
        IReadOnlyCollection<string> ISchedule.Uninstall => this.Uninstall?.AsReadOnly();

        /// <summary>
        /// Determines whether the package scheduled to uninstall.
        /// </summary>
        /// <param name="packageId">
        /// The package id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsExtensionPackageScheduledToUninstall(string packageId)
        {
            var idx = this.Uninstall.FindIndex(s => s == packageId);
            return idx >= 0;
        }

        /// <summary>
        /// Determines whether the package scheduled to install.
        /// </summary>
        /// <param name="packageId">
        ///     The package id.
        /// </param>
        /// <param name="version">
        ///     The package version.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsExtensionPackageScheduledToInstall(string packageId, out string version)
        {
            version = string.Empty;
            var idx = this.Install.FindIndex(InstallMatch(packageId));
            if (idx >= 0)
            {
                 var packageIdParts = this.Install[idx].Split(":");
                 if (packageIdParts.Length == 2)
                 {
                     version = packageIdParts[1];
                 }

                 return true;
            }

            return false;
        }

        /// <summary>
        /// The schedule install package.
        /// </summary>
        /// <param name="packageId">
        /// The package id.
        /// </param>
        /// <param name="version">
        /// The version.
        /// </param>
        public void ScheduleInstallExtensionPackage(string packageId, string version)
        {
            var idx = this.Uninstall.FindIndex(UnInstallMatch(packageId));
            if (idx > -1)
            {
                this.Uninstall.RemoveAt(idx);
            }

            idx = this.Install.FindIndex(InstallMatch(packageId));
            if (idx > -1)
            {
                this.Install.RemoveAt(idx);
            }

            this.Install.Add(!string.IsNullOrEmpty(version) ? $"{packageId}:{version}" : packageId);
        }

        /// <summary>
        /// The uninstall match predicate.
        /// </summary>
        /// <param name="packageId">
        /// The package id.
        /// </param>
        /// <returns>
        /// The <see cref="Predicate{String}"/>.
        /// </returns>
        private static Predicate<string> UnInstallMatch(string packageId)
        {
            return s => s == packageId;
        }

        /// <summary>
        /// The schedule un install package.
        /// </summary>
        /// <param name="packageId">
        /// The package id.
        /// </param>
        public void ScheduleUninstallExtensionPackage(string packageId)
        {
            var idx = this.Install.FindIndex(InstallMatch(packageId));
            if (idx > -1)
            {
                this.Install.RemoveAt(idx);
            }

            idx = this.Uninstall.FindIndex(s => s == packageId);
            if (idx == -1)
            {
                this.Uninstall.Add(packageId);
            }
        }

        /// <summary>
        /// The install match predicate.
        /// </summary>
        /// <param name="packageId">
        /// The package id.
        /// </param>
        /// <returns>
        /// The <see cref="Predicate{String}"/>.
        /// </returns>
        private static Predicate<string> InstallMatch(string packageId)
        {
            return s => s == packageId || s.StartsWith($"{packageId}:");
        }
    }
}