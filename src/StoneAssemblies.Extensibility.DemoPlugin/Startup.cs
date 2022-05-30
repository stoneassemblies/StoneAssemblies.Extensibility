// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="Stone Assemblies">
//     Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility.DemoPlugin
{
    using System;
    using System.Collections;

    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    ///     The plugin startup.
    /// </summary>
    public class Startup
    {
        /// <summary>
        ///     The configuration.
        /// </summary>
#pragma warning disable IDE0052 // Remove unread private members
        private readonly IConfiguration configuration;
#pragma warning restore IDE0052 // Remove unread private members

        /// <summary>
        ///     The extension manager.
        /// </summary>
#pragma warning disable IDE0052 // Remove unread private members
        private readonly IExtensionManager extensionManager;
#pragma warning restore IDE0052 // Remove unread private members

        /// <summary>
        ///     Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        /// <param name="extensionManager">
        ///     The extension manager.
        /// </param>
        public Startup(IConfiguration configuration, IExtensionManager extensionManager)
        {
            this.configuration = configuration;
            this.extensionManager = extensionManager;
        }

        /// <summary>
        ///     The configure services.
        /// </summary>
        /// <param name="serviceCollection">
        ///     The service collection.
        /// </param>
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            var implementationInstance = new SqlConnection();
            
            //implementationInstance.ConnectionString =
            //    "Server=localhost;Database=Users;User Id=sa;Password=Password123!;TrustServerCertificate=true";
            //implementationInstance.Open();

            serviceCollection.AddSingleton(implementationInstance);
        }

        /// <summary>
        /// The configure.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        public void Configure(Action<string> action, IList objects)
        {
            action(string.Empty);
            objects.Add("Hello");
        }
    }
}