using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmappeeCore;
using System;

namespace TestSmappeeClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var services = new ServiceCollection();
            services.AddLogging();

            // Initialize Autofac
            var builder = new ContainerBuilder();
            // Use the Populate method to register services which were registered
            // to IServiceCollection
            builder.Populate(services);

            // Build the final container
            IContainer container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                //SmappeeCore.SmappeeExpertConfiguration conf = new SmappeeCore.SmappeeExpertConfiguration()
                //{
                //    SmappeLocalAddress = "2.238.201.154",
                //    Port = 4888
                //};

                SmappeeCore.SmappeeExpertConfiguration conf = new SmappeeCore.SmappeeExpertConfiguration()
                {
                    SmappeLocalAddress = "192.168.1.105",
                    Port = 80
                };


                ILogger<SmappeeExpertClient> logger = scope.Resolve<ILogger<SmappeeExpertClient>>();

                SmappeeCore.SmappeeExpertClient client = new SmappeeCore.SmappeeExpertClient(logger);

                var loggedIn =  client.Login(conf);

                var istantValue = client.GetInstantValue();

                var xxx = client.GetReportValue();
            }
        }
    }
}
