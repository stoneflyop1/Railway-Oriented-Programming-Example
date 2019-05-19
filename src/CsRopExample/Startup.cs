using System.Web.Http;
using CsRopExample.Controllers;
using CsRopExample.DataAccessLayer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json.Serialization;

namespace CsRopExample
{
    class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2).AddJsonOptions(options => 
                options.SerializerSettings.ContractResolver = 
                    new CamelCasePropertyNamesContractResolver());
        
            services.AddScoped<ICustomerDao, CustomerDao>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<LogginMiddleware>();
            app.UseMvc();
        }


    }

}
