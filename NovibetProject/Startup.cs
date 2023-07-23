using Microsoft.EntityFrameworkCore;
using NovibetProject.Services;
using System;

namespace NovibetProject
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Configure the database context            
            services.AddDbContext<IpDetailsContext>();

            // Register the IP details service 
            services.AddScoped<IpDetailsService>();

            // Add a hosted service to update the IP details periodically
            services.AddSingleton<IHostedService, IpDetailsUpdaterHostedService>();

            // Add caching for the IP details service
            services.AddMemoryCache();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IpDetailsContext dbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Migrate the database to the latest version
            dbContext.Database.Migrate();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
