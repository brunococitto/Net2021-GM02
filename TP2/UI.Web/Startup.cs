using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Data.Database;
using Business.Logic;
using FluentValidation.AspNetCore;
using UI.Web.Providers;
using jsreport.AspNetCore;
using jsreport.Binary;
using jsreport.Local;
using UI.Web.Models;

namespace UI.Web
{
    public class Startup
    {
        private readonly IConfiguration config;
        public Startup(IConfiguration configuration)
        {
            config = configuration;
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddFluentValidation(
                fv => {
                    fv.RegisterValidatorsFromAssemblyContaining<MateriaValidator>();
                    fv.RegisterValidatorsFromAssemblyContaining<UsuarioEditValidator>();
                    fv.ValidatorOptions.LanguageManager.Culture = new System.Globalization.CultureInfo("es");
                }
                );
            services.AddScoped<MateriaAdapter>();
            services.AddScoped<PlanAdapter>();
            services.AddScoped<EspecialidadAdapter>();
            services.AddScoped<ComisionAdapter>();
            services.AddScoped<MateriaLogic>();
            services.AddScoped<PlanLogic>();
            services.AddScoped<EspecialidadLogic>();
            services.AddScoped<ComisionLogic>();
            services.AddScoped<PersonaLogic>();
            services.AddScoped<PersonaAdapter>();
            services.AddScoped<CursoLogic>();
            services.AddScoped<CursoAdapter>();
            services.AddScoped<AlumnoInscripcionAdapter>();
            services.AddScoped<AlumnoInscripcionLogic>();
            services.AddScoped<UsuarioAdapter>();
            services.AddScoped<UsuarioLogic>();
            services.AddScoped<DocenteCursoLogic>();
            services.AddScoped<DocenteCursoAdapter>();

            services.AddScoped<IHasher, Hasher>();
            services.AddScoped<IUsuarioManager, UsuarioManager>();

            services.AddJsReport(new LocalReporting()
                .UseBinary(JsReportBinary.GetBinary())
                .KillRunningJsReportProcesses()
                .AsUtility()
                .Create());

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Error/401";
            });

            services.AddDbContext<AcademyContext>(opt =>
            {
               opt.UseSqlServer(config.GetConnectionString("ConnStringLocal"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            /*if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }*/

            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();

            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
