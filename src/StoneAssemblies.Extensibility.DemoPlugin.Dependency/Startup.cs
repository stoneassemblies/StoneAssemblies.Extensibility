namespace StoneAssemblies.Extensibility.DemoPlugin.Dependency
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

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
        ///     Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="logger">
        ///     The logger. 
        /// </param>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        public Startup(ILogger<Startup> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        /// <summary>
        ///     The configure services.
        /// </summary>
        /// <param name="serviceCollection">
        ///     The service collection.
        /// </param>
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<AppDbContext>(
                (provider, builder) =>
                    {
                        var action = provider.GetRequiredService<Action<DbContextOptionsBuilder>>();
                        action(builder);
                    });
        }
    }
}
