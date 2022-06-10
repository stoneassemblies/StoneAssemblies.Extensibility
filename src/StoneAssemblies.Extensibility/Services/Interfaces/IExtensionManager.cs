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
        ///     Gets the extension assemblies.
        /// </summary>
        /// <returns>
        ///     The <see cref="IEnumerable{Assembly}" />.
        /// </returns>
        IEnumerable<Assembly> GetExtensionAssemblies();

        /// <summary>
        ///     Loads the extensions from package ids.
        /// </summary>
        Task LoadExtensionsAsync();

        /// <summary>
        /// Call configure method of all start up objects that match with give arguments types.
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        void Configure(params object[] parameters);


        IAsyncEnumerable<ExtensionPackage> GetAvailableExtensionPackagesAsync(int skip, int take);
    }
}