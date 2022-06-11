namespace StoneAssemblies.Extensibility
{
    using System.Collections.Generic;

    public interface ISchedule
    {
        IReadOnlyCollection<string> Install { get; }

        IReadOnlyCollection<string> UnInstall { get; }

        /// <summary>
        /// Determines whether the package scheduled to install.
        /// </summary>
        /// <param name="packageId">
        /// The package id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool IsPackageScheduledToInstall(string packageId);

        /// <summary>
        /// Determines whether the package scheduled to uninstall.
        /// </summary>
        /// <param name="packageId">
        /// The package id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool IsPackageScheduledToUnInstall(string packageId);
    }
}