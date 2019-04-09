﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Mvc.UI.Theming;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.Web;
using Volo.Abp.Localization;
using Volo.Abp.Localization.Resources.AbpValidation;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.Identity;
using Volo.Abp.Threading;
using Volo.Abp.UI;
using Volo.Abp.VirtualFileSystem;
using Volo.Docs.Admin;
using Volo.Docs.Localization;
using Volo.Docs.Utils;
using VoloDocs.EntityFrameworkCore;

namespace Volo.Docs
{  
    [DependsOn(
        typeof(DocsWebModule),
        typeof(DocsAdminWebModule),
        typeof(DocsApplicationModule),
        typeof(DocsAdminApplicationModule),
        typeof(VoloDocsEntityFrameworkCoreModule),
        typeof(AbpAutofacModule),
        typeof(AbpAccountWebModule),
        typeof(AbpIdentityWebModule),
        typeof(AbpIdentityApplicationModule),
        typeof(AbpPermissionManagementDomainIdentityModule),
        typeof(AbpPermissionManagementApplicationModule),
        typeof(AbpAspNetCoreMvcUiBasicThemeModule)
    )]
    public class VoloDocsWebModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
            {
                options.AddAssemblyResource(typeof(DocsResource), typeof(VoloDocsWebModule).Assembly);
            });
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var hostingEnvironment = context.Services.GetHostingEnvironment();
            var configuration = context.Services.GetConfiguration();

            Configure<DbConnectionOptions>(options =>
            {
                options.ConnectionStrings.Default = configuration["ConnectionString"];
            });

            Configure<AbpDbContextOptions>(options =>
            {
                options.UseSqlServer();
            });

            if (hostingEnvironment.IsDevelopment())
            {
                Configure<VirtualFileSystemOptions>(options =>
                {
                    options.FileSets.ReplaceEmbeddedByPhysical<AbpUiModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}..{0}..{0}framework{0}src{0}Volo.Abp.UI", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<AbpAspNetCoreMvcUiModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}..{0}..{0}framework{0}src{0}Volo.Abp.AspNetCore.Mvc.UI", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<AbpAspNetCoreMvcUiBootstrapModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}..{0}..{0}framework{0}src{0}Volo.Abp.AspNetCore.Mvc.UI.Bootstrap", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<AbpAspNetCoreMvcUiThemeSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}..{0}..{0}framework{0}src{0}Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<AbpAspNetCoreMvcUiBasicThemeModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}..{0}..{0}framework{0}src{0}Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<DocsDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}Volo.Docs.Domain", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<DocsWebModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}Volo.Docs.Web", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<DocsWebModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}Volo.Docs.Admin.Web", Path.DirectorySeparatorChar)));
                });
            }

            context.Services.AddSwaggerGen(
                options =>
                {
                    options.SwaggerDoc("v1", new Info
                    {
                        Title = "Docs API",
                        Version = "v1"
                    });
                    options.DocInclusionPredicate((docName, description) => true);
                    options.CustomSchemaIds(type => type.FullName);
                });

            var cultures = new List<CultureInfo> { new CultureInfo("en"), new CultureInfo("tr") };
            Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("en");
                options.SupportedCultures = cultures;
                options.SupportedUICultures = cultures;
            });

            Configure<VirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<VoloDocsWebModule>();
            });

            Configure<AbpLocalizationOptions>(options =>
            {
                options.Resources
                    .Get<DocsResource>()
                    .AddBaseTypes(typeof(AbpValidationResource))
                    .AddBaseTypes(typeof(AbpUiModule))
                    .AddVirtualJson("/Localization/Resources/VoloDocs/Web");
            });

            Configure<ThemingOptions>(options =>
            {
                options.DefaultThemeName = BasicTheme.Name;
            });

            Configure<RazorPagesOptions>(options =>
            {
                options.Conventions.AddPageRoute("/Error", "error/{statusCode}");
            });
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            var env = context.GetEnvironment();

            app.UseVirtualFiles();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Support APP API");
            });

            app.UseAuthentication();

            app.UseRequestLocalization(app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

            app.UseStatusCodePagesWithReExecute("/error/{0}");
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "defaultWithArea",
                    template: "{area}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            using (var scope = context.ServiceProvider.CreateScope())
            {
                AsyncHelper.RunSync(async () =>
                {
                    await scope.ServiceProvider
                        .GetRequiredService<IDataSeeder>()
                        .SeedAsync();
                });
            }
        }
    }
}