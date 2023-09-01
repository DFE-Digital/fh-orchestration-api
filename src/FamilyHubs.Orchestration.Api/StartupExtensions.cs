using AutoMapper.EquivalencyExpression;
using FamilyHubs.Orchestration.Api.Endpoints;
using FamilyHubs.Orchestration.Api.Middleware;
using FamilyHubs.Orchestration.Core;
using FamilyHubs.Orchestration.Core.ClientServices;
using FamilyHubs.SharedKernel.GovLogin.AppStart;
using FamilyHubs.SharedKernel.Identity;
using MediatR;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

namespace FamilyHubs.Orchestration.Api;

public static class StartupExtensions
{
    public static void ConfigureHost(this WebApplicationBuilder builder)
    {
        // ApplicationInsights
        builder.Host.UseSerilog((_, services, loggerConfiguration) =>
        {
            string? logLevelString = builder.Configuration["LogLevel"];

            if (logLevelString == null)
            {
                logLevelString = "Information";
            }

            var parsed = Enum.TryParse<LogEventLevel>(logLevelString, out var logLevel);

            loggerConfiguration.WriteTo.ApplicationInsights(
                services.GetRequiredService<TelemetryConfiguration>(),
                TelemetryConverter.Traces,
                parsed ? logLevel : LogEventLevel.Information);

            loggerConfiguration.WriteTo.Console(
                parsed ? logLevel : LogEventLevel.Information);
        });
    }

    public static void RegisterApplicationComponents(this IServiceCollection services, IConfiguration configuration)
    {
        var gatewayApiBaseUrl = configuration["GatewayApiBaseUrl"];
        if (string.IsNullOrWhiteSpace(gatewayApiBaseUrl))
        {
            throw new ArgumentException("GatewayApiBaseUrl is not configured");
        }

        services.AddHttpClient<IClientService, ClientService>(client =>
        {
            client.BaseAddress = new Uri(gatewayApiBaseUrl);
        });

        services.AddAuthorizationPolicy(configuration);

        services.AddBearerAuthentication(configuration);

        services.RegisterMinimalEndPoints();

        services.RegisterAutoMapper();

        services.RegisterMediator();
    }

    private static void AddAuthorizationPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        List<string> userRoles = new List<string>()
        {
             RoleTypes.DfeAdmin, RoleTypes.LaProfessional, RoleTypes.VcsManager, RoleTypes.VcsProfessional, RoleTypes.VcsDualRole
        };

        string settingUserRoles = configuration.GetValue<string>("UserRoles") ?? string.Empty;
        if (!string.IsNullOrEmpty(settingUserRoles))
        {
            userRoles = settingUserRoles.Split(',').ToList();
        }

        services.AddAuthorization(options => {
            options.AddPolicy("ReferralUser", policy =>
                policy.RequireAssertion(context =>
                    userRoles.Exists(role => context.User.IsInRole(role))));
        });
    }

    private static void RegisterMinimalEndPoints(this IServiceCollection services)
    {
        services.AddTransient<MinimalGeneralEndPoints>();
        services.AddTransient<MinimalReferralAggregationEndPoints>();
    }

    private static void RegisterAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper((serviceProvider, cfg) =>
        {
            var auditProperties = new[] { "Created", "CreatedBy", "LastModified", "LastModifiedBy" };
            cfg.AddProfile<AutoMappingProfiles>();
            cfg.AddCollectionMappers();
            cfg.ShouldMapProperty = pi => !auditProperties.Contains(pi.Name);
        }, typeof(AutoMappingProfiles));
    }

    public static void RegisterMediator(this IServiceCollection services)
    {
        var assemblies = new[]
        {
            typeof(ClientService).Assembly
        };

        services.AddMediatR(config =>
        {
            config.Lifetime = ServiceLifetime.Transient;
            config.RegisterServicesFromAssemblies(assemblies);
        });

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddTransient<CorrelationMiddleware>();
        services.AddTransient<ExceptionHandlingMiddleware>();
    }

    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration, bool isProduction)
    {
        services.AddApplicationInsightsTelemetry();

        // Add services to the container.
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "FamilyHubs.Referral.Api", Version = "v1" });
            c.EnableAnnotations();
        });
    }

    public static void ConfigureWebApplication(this WebApplication webApplication)
    {
        webApplication.UseSerilogRequestLogging();

        webApplication.UseMiddleware<CorrelationMiddleware>();
        webApplication.UseMiddleware<ExceptionHandlingMiddleware>();

        // Configure the HTTP request pipeline.
        webApplication.UseSwagger();
        webApplication.UseSwaggerUI();

        webApplication.UseHttpsRedirection();

        webApplication.MapControllers();

        RegisterEndPoints(webApplication);
    }

    private static void RegisterEndPoints(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var genapi = scope.ServiceProvider.GetService<MinimalGeneralEndPoints>();
        genapi?.RegisterMinimalGeneralEndPoints(app);

        var referralaggregatorapi = scope.ServiceProvider.GetService<MinimalReferralAggregationEndPoints>();
        referralaggregatorapi?.RegisterReferralAggregationEndPoints(app);
    }
}
