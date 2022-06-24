// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionManagerException.cs" company="Stone Assemblies">
// Copyright © 2021 - 2022 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     The exception manager exception.
    /// </summary>
    [Serializable]
    public class ExtensionManagerException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionManagerException" /> class.
        /// </summary>
        public ExtensionManagerException()
        {
        }

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

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionManagerException" /> class.
        /// </summary>
        /// <param name="info">
        ///     The info.
        /// </param>
        /// <param name="context">
        ///     The context.
        /// </param>
        protected ExtensionManagerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}