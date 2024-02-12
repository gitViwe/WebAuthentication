using API.Persistence;
using API.Storage.AuthenticationCeremony.Implementation;
using API.Storage.RegistrationCeremony.Implementation;
using gitViwe.Shared.Cache;
using Microsoft.EntityFrameworkCore;
using WebAuthn.Net.Configuration.DependencyInjection;
using WebAuthn.Net.Storage.SqlServer.Configuration.Options;
using WebAuthn.Net.Storage.SqlServer.Models;
using WebAuthn.Net.Storage.SqlServer.Services.ContextFactory;
using WebAuthn.Net.Storage.SqlServer.Storage;

namespace API.Extension;

public static class WebApplicationBuilderExtension
{
    public static WebApplicationBuilder RegisterWebAuthnSqlServer(
        this WebApplicationBuilder builder,
        Action<SqlServerOptions> configureSqlServer)
    {
        builder.Services
            .AddWebAuthnCore<DefaultSqlServerContext>()
            .AddRegistrationCeremonyStorage<DefaultSqlServerContext, DefaultCacheRegistrationCeremonyStorage<DefaultSqlServerContext>>()
            .AddAuthenticationCeremonyStorage<DefaultSqlServerContext, DefaultCacheAuthenticationCeremonyStorage<DefaultSqlServerContext>>()
            .AddDefaultFidoMetadataStorage()
            .AddContextFactory<DefaultSqlServerContext, DefaultSqlServerContextFactory>()
            .AddCredentialStorage<DefaultSqlServerContext, DefaultSqlServerCredentialStorage<DefaultSqlServerContext>>();

        builder.Services.AddOptions<SqlServerOptions>();
        builder.Services.Configure(configureSqlServer);

        builder.Services.AddScoped<WebAuthentication>();

        return builder;
    }

    public static WebApplicationBuilder RegisterRedisCache(this WebApplicationBuilder builder)
    {
        builder.Services.AddGitViweRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("Redis");
            options.InstanceName = "redis_demo";
            options.AbsoluteExpirationInMinutes = 5;
            options.SlidingExpirationInMinutes = 2;
        });

        return builder;
    }

    public static WebApplicationBuilder RegisterDbContext(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<WebAuthenticationDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL"));
        });

        return builder;
    }

    public static WebApplicationBuilder RegisterCorsPolicy(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policyBuilder =>
            {
                policyBuilder
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            });
        });

        return builder;
    }
}
