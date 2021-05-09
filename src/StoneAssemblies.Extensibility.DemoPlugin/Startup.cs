// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="Stone Assemblies">
//     Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility.DemoPlugin
{
    using System.Data.SqlClient;

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
        ///     Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        ///     The configure services.
        /// </summary>
        /// <param name="serviceCollection">
        ///     The service collection.
        /// </param>
#pragma warning disable CA1822 // Mark members as static
        public void ConfigureServices(IServiceCollection serviceCollection)
#pragma warning restore CA1822 // Mark members as static
        {
            serviceCollection.AddSingleton(new SqlConnection());
        }
    }
}