﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Volo.Abp;
using Volo.Abp.AspNetCore.Modularity;
using Volo.Abp.AspNetCore.Mvc.UI;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Lepton;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Mvc.UI.Theming;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.UI;
using Volo.Abp.VirtualFileSystem;
using Volo.Blog;
using Volo.BlogTestApp.EntityFrameworkCore;

namespace Volo.BlogTestApp
{
    [DependsOn(
        typeof(BlogWebModule),
        typeof(BlogApplicationModule),
        typeof(BlogTestAppEntityFrameworkCoreModule),
        typeof(AbpAutofacModule),
        typeof(AbpAspNetCoreMvcUiBasicThemeModule),
        typeof(AbpAspNetCoreMvcUiLeptonThemeModule)
    )]
    public class BlogTestAppModule : AbpModule
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            var hostingEnvironment = services.GetHostingEnvironment();
            var configuration = services.BuildConfiguration();

            services.Configure<DbConnectionOptions>(options =>
            {
                const string connStringName = "SqlServer";
                options.ConnectionStrings.Default = configuration.GetConnectionString(connStringName);
            });

            services.Configure<AbpDbContextOptions>(options =>
            {
                options.UseSqlServer();
            });

            if (hostingEnvironment.IsDevelopment())
            {
                services.Configure<VirtualFileSystemOptions>(options =>
                {
                    options.FileSets.ReplaceEmbeddedByPyhsical<AbpUiModule>(Path.Combine(hostingEnvironment.ContentRootPath, "..\\..\\..\\abp\\src\\Volo.Abp.UI"));
                    options.FileSets.ReplaceEmbeddedByPyhsical<AbpAspNetCoreMvcUiModule>(Path.Combine(hostingEnvironment.ContentRootPath, "..\\..\\..\\abp\\src\\Volo.Abp.AspNetCore.Mvc.UI"));
                    options.FileSets.ReplaceEmbeddedByPyhsical<AbpAspNetCoreMvcUiBootstrapModule>(Path.Combine(hostingEnvironment.ContentRootPath, "..\\..\\..\\abp\\src\\Volo.Abp.AspNetCore.Mvc.UI.Bootstrap"));
                    options.FileSets.ReplaceEmbeddedByPyhsical<AbpAspNetCoreMvcUiThemeSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, "..\\..\\..\\abp\\src\\Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared"));
                    options.FileSets.ReplaceEmbeddedByPyhsical<AbpAspNetCoreMvcUiBasicThemeModule>(Path.Combine(hostingEnvironment.ContentRootPath, "..\\..\\..\\abp\\src\\Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic"));
                    options.FileSets.ReplaceEmbeddedByPyhsical<AbpAspNetCoreMvcUiLeptonThemeModule>(Path.Combine(hostingEnvironment.ContentRootPath, "..\\..\\..\\abp-lepton-mvc\\src\\Volo.Abp.AspNetCore.Mvc.UI.Theme.Lepton"));
                    options.FileSets.ReplaceEmbeddedByPyhsical<BlogDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, "..\\..\\src\\Volo.Blog.Domain"));
                    options.FileSets.ReplaceEmbeddedByPyhsical<BlogWebModule>(Path.Combine(hostingEnvironment.ContentRootPath, "..\\..\\src\\Volo.Blog.Web"));
                });
            }

            services.AddSwaggerGen(
                options =>
                {
                    options.SwaggerDoc("v1", new Info { Title = "Blog API", Version = "v1" });
                    options.DocInclusionPredicate((docName, description) => true);
                });


            var cultures = new List<CultureInfo> { new CultureInfo("en"), new CultureInfo("tr") };
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("en");
                options.SupportedCultures = cultures;
                options.SupportedUICultures = cultures;
            });

            services.Configure<ThemingOptions>(options =>
            {
                options.DefaultThemeName = BasicTheme.Name;
            });

            services.AddAssemblyOf<BlogTestAppModule>();
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();

            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();
            app.UseVirtualFiles();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Support APP API");
            });

            app.UseAuthentication();

            app.UseRequestLocalization(app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "defaultWithArea",
                    template: "{area}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
