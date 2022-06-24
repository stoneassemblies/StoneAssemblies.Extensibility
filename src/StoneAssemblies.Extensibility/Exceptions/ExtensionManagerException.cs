// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionManagerException.cs" company="Stone Assemblies">
// Copyright © 2021 - 2022 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility
{
    using System;

    /// <summary>
    ///     The exception manager exception.
    /// </summary>
    public class ExtensionManagerException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionManagerException" /> class.
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="innerException">
        ///     The inner exception.
        /// </param>
        public ExtensionManagerException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}