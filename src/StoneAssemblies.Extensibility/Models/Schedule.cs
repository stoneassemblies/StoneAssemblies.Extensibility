namespace StoneAssemblies.Extensibility
{
    using System;
    using System.Collections.Generic;

    public sealed class Schedule : ISchedule
    {
        /// <summary>
        /// Gets the install.
        /// </summary>
        public List<string> Install { get; } = new List<string>();

        /// <summary>
        /// Gets the un install.
        /// </summary>
        public List<string> UnInstall { get; } = new List<string>();

        /// <summary>
        /// The install.
        /// </summary>
        IReadOnlyCollection<string> ISchedule.Install => Install?.AsReadOnly();

        /// <summary>
        /// The un install.
        /// </summary>
        IReadOnlyCollection<string> ISchedule.UnInstall => UnInstall?.AsReadOnly();

        /// <summary>
        /// Determines whether the package scheduled to uninstall.
        /// </summary>
        /// <param name="packageId">
        /// The package id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsPackageScheduledToUnInstall(string packageId)
        {
            var idx = this.UnInstall.FindIndex(s => s == packageId);
            return idx >= 0;
        }

        /// <summary>
        /// Determines whether the package scheduled to install.
        /// </summary>
        /// <param name="packageId">
        /// The package id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsPackageScheduledToInstall(string packageId)
        {
            var idx = this.Install.FindIndex(InstallMatch(packageId));
            return idx >= 0;
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
        public void ScheduleInstallPackage(string packageId, string version)
        {
            var idx = this.UnInstall.FindIndex(UnInstallMatch(packageId));
            if (idx > -1)
            {
                this.UnInstall.RemoveAt(idx);
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
        public void ScheduleUnInstallPackage(string packageId)
        {
            var idx = this.Install.FindIndex(InstallMatch(packageId));
            if (idx > -1)
            {
                this.Install.RemoveAt(idx);
            }

            idx = this.UnInstall.FindIndex(s => s == packageId);
            if (idx == -1)
            {
                this.UnInstall.Add(packageId);
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