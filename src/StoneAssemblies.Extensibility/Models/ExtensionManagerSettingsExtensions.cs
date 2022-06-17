// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionManagerSettingsExtensions.cs" company="Stone Assemblies">
// Copyright © 2021 - 2022 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The extension manager settings extensions.
    /// </summary>
    public static class ExtensionManagerSettingsExtensions
    {
        /// <summary>
        /// The is in blacklist.
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <param name="packageId">
        /// The package id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsInBlacklist(this ExtensionManagerSettings settings, string packageId)
        {
            return settings.Blacklist.Any(blacklist => Match(blacklist, packageId));
        }

        /// <summary>
        /// The match predicate.
        /// </summary>
        /// <param name="pattern">
        /// The pattern.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool Match(string pattern, string value)
        {
            if (string.Equals(pattern, value, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            try
            {
                var match = Regex.Match(value, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return true;
                }
            }
            catch
            {
                // ignore
            }

            return false;
        }
    }
}