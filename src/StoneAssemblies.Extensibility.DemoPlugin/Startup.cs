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
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     The plugin startup.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<Startup> logger;

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
        /// <param name="logger">
        ///     The logger. 
        /// </param>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        /// <param name="extensionManager">
        ///     The extension manager.
        /// </param>
        public Startup(ILogger<Startup> logger, IConfiguration configuration, IExtensionManager extensionManager)
        {
            this.logger = logger;
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
            _ = new Microsoft.Graph.Error();
            _ = new AppDbContext(new DbContextOptions<AppDbContext>());

            var implementationInstance = new SqlConnection();

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