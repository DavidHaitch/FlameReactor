using Blazored.Modal;
using Certes;
using FlameReactor.DB;
using FlameReactor.WebUI.Data;
using FluffySpoon.AspNet.LetsEncrypt;
using FluffySpoon.AspNet.LetsEncrypt.Certes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlameReactor.WebUI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => options.EnableEndpointRouting = false);
            string ConnectionStr = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<FlameReactorContext>(options => options.UseSqlite(ConnectionStr));
            services.AddRazorPages().AddRazorRuntimeCompilation();
            services.AddServerSideBlazor();
            services.AddBlazoredModal();
            services.AddHttpContextAccessor();

            var leOptions = new LetsEncryptOptions()
            {
                Email = Configuration["LetsEncrypt:Email"], //LetsEncrypt will send you an e-mail here when the certificate is about to expire
                UseStaging = false, //switch to true for testing
                Domains = new[] { Configuration["LetsEncrypt:Domain"] },
                TimeUntilExpiryBeforeRenewal = TimeSpan.FromDays(28), //renew automatically 30 days before expiry
                TimeAfterIssueDateBeforeRenewal = TimeSpan.FromDays(7), //renew automatically 7 days after the last certificate was issued
                CertificateSigningRequest = new CsrInfo() //these are your certificate details
                {
                    CountryName = Configuration["LetsEncrypt:CSRInfo:CountryName"],
                    Locality = Configuration["LetsEncrypt:CSRInfo:Locality"],
                    Organization = Configuration["LetsEncrypt:CSRInfo:Organization"],
                    OrganizationUnit = Configuration["LetsEncrypt:CSRInfo:OrganizationUnit"],
                    State = Configuration["LetsEncrypt:CSRInfo:State"]
                }
            };
            //the following line adds the automatic renewal service.
            services.AddFluffySpoonLetsEncrypt(leOptions);

            //the following line tells the library to persist the certificate to a file, so that if the server restarts, the certificate can be re-used without generating a new one.
            services.AddFluffySpoonLetsEncryptFileCertificatePersistence();
            services.AddFluffySpoonLetsEncryptFileChallengePersistence();

            //the following line tells the library to persist challenges in-memory. challenges are the "/.well-known" URL codes that LetsEncrypt will call.
            services.AddFluffySpoonLetsEncryptMemoryChallengePersistence();

            services.AddAntiforgery(o => o.SuppressXFrameOptionsHeader = true);
            services.AddDbContext<FlameReactorContext>();
            services.AddSingleton<EmberService>(sp =>
            {
                return new EmberService("wwwroot/Flames/Pool", new DbContextOptionsBuilder<FlameReactorContext>().UseSqlite(@"Data Source=.\FlameReactor.sqlite").Options); 
            });
            services.AddSingleton<AppState>();
            services.AddSingleton(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseFluffySpoonLetsEncrypt();
            app.UseMvcWithDefaultRoute();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
