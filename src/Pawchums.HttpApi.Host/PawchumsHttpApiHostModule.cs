using Hangfire;
using Hangfire.PostgreSql;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Pawchums.EntityFrameworkCore;
using Pawchums.MultiTenancy;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Volo.Abp;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.MultiTenancy;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs.Hangfire;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Hangfire;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Volo.Abp.Swashbuckle;
using Volo.Abp.VirtualFileSystem;

namespace Pawchums;

[DependsOn(
    typeof(PawchumsHttpApiModule),
    typeof(AbpAutofacModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpDistributedLockingModule),
    typeof(AbpAspNetCoreMvcUiMultiTenancyModule),
    typeof(AbpAspNetCoreAuthenticationJwtBearerModule),
    typeof(PawchumsApplicationModule),
    typeof(PawchumsEntityFrameworkCoreModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpBackgroundJobsHangfireModule),
    typeof(AbpSwashbuckleModule)
)]
public class PawchumsHttpApiHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        ConfigureConventionalControllers();
        ConfigureAuthentication(context, configuration);
        ConfigureCache(configuration);
        ConfigureVirtualFileSystem(context);
        ConfigureDataProtection(context, configuration, hostingEnvironment);
        ConfigureDistributedLocking(context, configuration);
        ConfigureCors(context, configuration);
        ConfigureSwaggerServices(context, configuration);
        ConfigureAntiForgery();
        ConfigureHangfire(context, configuration);

    }

    private void ConfigureCache(IConfiguration configuration)
    {
        Configure<AbpDistributedCacheOptions>(options => { options.KeyPrefix = "Pawchums:"; });
    }
    private void ConfigureHangfire(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddHangfire(config =>
        {
            config.UsePostgreSqlStorage(options =>
            {
                options.UseNpgsqlConnection(configuration.GetConnectionString("Default"));
            });
        });
    }

    private void ConfigureVirtualFileSystem(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<PawchumsDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Pawchums.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<PawchumsDomainModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Pawchums.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<PawchumsApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Pawchums.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<PawchumsApplicationModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Pawchums.Application"));
            });
        }
    }

    private void ConfigureConventionalControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(PawchumsApplicationModule).Assembly);
        });
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddAbpJwtBearer(options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata");
                options.Audience = "Pawchums";
            });

        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private static void ConfigureSwaggerServices(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAbpSwaggerGenWithOAuth(
            configuration["AuthServer:Authority"]!,
            new Dictionary<string, string>
            {
                    {"Pawchums", "Pawchums API"}
            },
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Pawchums API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            });
    }

    private void ConfigureDataProtection(
        ServiceConfigurationContext context,
        IConfiguration configuration,
        IWebHostEnvironment hostingEnvironment)
    {
        var dataProtectionBuilder = context.Services.AddDataProtection().SetApplicationName("Pawchums");
        if (!hostingEnvironment.IsDevelopment())
        {
            var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]!);
            dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, "Pawchums-Protection-Keys");
        }
    }

    private void ConfigureDistributedLocking(
        ServiceConfigurationContext context,
        IConfiguration configuration)
    {
        context.Services.AddSingleton<IDistributedLockProvider>(sp =>
        {
            var connection = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]!);
            return new RedisDistributedSynchronizationProvider(connection.GetDatabase());
        });
    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(configuration["App:CorsOrigins"]?
                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(o => o.RemovePostFix("/"))
                        .ToArray() ?? Array.Empty<string>())
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    private void ConfigureAntiForgery()
    {
        Configure<Volo.Abp.AspNetCore.Mvc.AntiForgery.AbpAntiForgeryOptions>(options =>
        {
            options.AutoValidate = false; // Disable automatic anti-forgery validation for APIs
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();
        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Pawchums API");

            var configuration = context.GetConfiguration();
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            options.OAuthScopes("Pawchums");
        });

        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
        app.UseAbpHangfireDashboard();

    }
}
