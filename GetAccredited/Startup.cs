using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetAccredited.Models;
using GetAccredited.Models.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GetAccredited
{
    public class Startup
    {
        // This property stores the configuration information from appsettings.json
        private IConfiguration Configuration { get; }

        // Constructor
        public Startup(IConfiguration configuration) => Configuration = configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("GetAccredited")));
            services.AddDbContext<ApplicationIdentityDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("GetAccreditedIdentity")));
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
                .AddDefaultTokenProviders();
            services.AddScoped<IAccountRepository, EFAccountRepository>();
            services.AddScoped<IAccreditationRepository, EFAccreditationRepository>();
            services.AddScoped<IOrganizationRepository, EFOrganizationRepository>();
            services.AddScoped<IAppointmentRepository, EFAppointmentRepository>();
            services.AddMvc(options => { options.EnableEndpointRouting = false; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseStatusCodePages();
            app.UseMvc(configureRoutes =>
            {
                configureRoutes.MapRoute(
                    name: "default",
                    template: "{controller=Account}/{action=Login}");
            });

            Utility.EnsureRolesCreatedAsync(app).Wait();
            Utility.EnsureAdminCreatedAsync(app).Wait();
            Utility.EnsureOrganizationsAdded(app);
        }
    }
}