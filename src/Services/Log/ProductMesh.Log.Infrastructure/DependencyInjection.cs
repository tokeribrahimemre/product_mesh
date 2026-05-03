using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductMesh.Log.Application.Interfaces;
using ProductMesh.Log.Domain.Interfaces;
using ProductMesh.Log.Infrastructure.Consumers;
using ProductMesh.Log.Infrastructure.Persistence;
using ProductMesh.Log.Infrastructure.Services;

namespace ProductMesh.Log.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLogInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<LogDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("LogDb")));

        services.AddScoped<ILogRepository, LogRepository>();
        services.AddScoped<ILogService, LogService>();

        // MassTransit + RabbitMQ — consumers that collect logs from all microservices
        services.AddMassTransit(x =>
        {
            x.AddConsumer<ProductCreatedConsumer>();
            x.AddConsumer<ProductUpdatedConsumer>();
            x.AddConsumer<LogEventConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                });

                cfg.ReceiveEndpoint("log-product-created", e =>
                {
                    e.ConfigureConsumer<ProductCreatedConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5)));
                });

                cfg.ReceiveEndpoint("log-product-updated", e =>
                {
                    e.ConfigureConsumer<ProductUpdatedConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5)));
                });

                cfg.ReceiveEndpoint("log-events", e =>
                {
                    e.ConfigureConsumer<LogEventConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5)));
                });
            });
        });

        return services;
    }
}
