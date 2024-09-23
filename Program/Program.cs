using Azure.Identity;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseMigration
{
    internal class Program
    {
        private static IConfiguration? Configuration { get; set; }

        private static void Main(string[] args)
        {
            var migrateUp = true; // Default to migrating up
            long? targetVersion = null; // Version to migrate down to, if applicable

            if (args.Length > 0)
            {
                if (args[0] == "down")
                {
                    migrateUp = false;
                    if (args.Length > 1 && long.TryParse(args[1], out long version))
                    {
                        targetVersion = version;
                    }
                    else
                    {
                        Console.WriteLine("Please specify a valid version number to rollback to.");
                        return;
                    }
                }
                else if (args[0] != "up")
                {
                    Console.WriteLine("Invalid argument. Please specify 'up' or 'down'. Defaulting to 'up'.");
                }
            }
            
            // Create service collection and configure our services
            var services = new ServiceCollection();
            ConfigureServices(services);

            // Generate a provider
            using var serviceProvider = services.BuildServiceProvider();

            // Using a service scope allows for creating scoped services within this method
            using var scope = serviceProvider.CreateScope(); 
            // Get the runner from the service provider
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            if (migrateUp)
            {
                // Execute the migrations up
                runner.MigrateUp();
            }
            else
            {
                // Execute the migrations down (rollback)
                if (targetVersion.HasValue)
                {
                    runner.MigrateDown(targetVersion.Value); // Rollback to the specified version
                }
                else
                {
                    Console.WriteLine("No version specified for rollback.");
                }
            }

        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Build configuration
            var builder = new ConfigurationBuilder()
                .AddAzureAppConfiguration(options =>
                {
                    // var appConfigEndpoint = Environment.GetEnvironmentVariable("AppConfigUrl");
                    var appConfigEndpoint = "https://appconfig-net-core-alpha.azconfig.io";
                    if (string.IsNullOrEmpty(appConfigEndpoint))
                        throw new InvalidOperationException("App configuration endpoint is not set.");

                    var credential = new DefaultAzureCredential();
                    options.Connect(new Uri(appConfigEndpoint), credential)
                           .ConfigureKeyVault(kv => kv.SetCredential(credential));
                });

            Configuration = builder.Build();

            // Get connection string
            var connectionString = Configuration["DatabaseConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Database connection string is not configured.");

            // Add FluentMigrator services
            services.AddFluentMigratorCore()
                    .ConfigureRunner(rb => rb
                        .AddSqlServer()
                        .WithGlobalConnectionString(connectionString)
                        .WithGlobalCommandTimeout(new TimeSpan(1, 0, 0))
                        .ScanIn(typeof(Program).Assembly).For.Migrations())
                    .AddLogging(lb => lb.AddFluentMigratorConsole());
        }
    }
    
}