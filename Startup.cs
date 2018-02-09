using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZeegZag.Crawler2.Services;
using ZeegZag.Crawler2.Services.Database;
using ZeegZag.Data.Entity;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace ZeegZag.Crawler2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddDbContext<admin_zeegzagContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvcWithDefaultRoute();
            
            app.UseSocketClient();

            

            ServiceManager.InitalizeServices(Configuration, 5000);
        }
        
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member