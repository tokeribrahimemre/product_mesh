using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductMesh.Product.Application.Interfaces;
using ProductMesh.Product.Domain.Interfaces;
using ProductMesh.Product.Infrastructure.Caching;
using ProductMesh.Product.Infrastructure.Consumers;
using ProductMesh.Product.Infrastructure.Persistence;
using ProductMesh.Shared.Interfaces;
using StackExchange.Redis;

namespace ProductMesh.Product.Infrastructure;

// Dependency Inversion Principle: Application layer depends on abstractions (IProductRepository, ICacheService)
// defined in Application/Domain. Infrastructure provides concrete implementations, registered here.
public static class DependencyInjection
{
    public static IServiceCollection AddProductInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register MediatR notification handlers from Infrastructure assembly (e.g., domain event → integration event handlers)
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        // EF Core + SQL Server
        services.AddDbContext<ProductDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("ProductDb")));

        // Redis
        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
        services.AddScoped<ICacheService, RedisCacheService>();

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ProductDbContext>());

        // MassTransit + RabbitMQ for event-driven architecture
        services.AddMassTransit(x =>
        {
            // SAGA Pattern consumers for compensating actions
            x.AddConsumer<ProductCreationFailedConsumer>();
            x.AddConsumer<ProductLoggedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                });

                cfg.ReceiveEndpoint("product-saga-failed", e =>
                {
                    e.ConfigureConsumer<ProductCreationFailedConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5)));
                });

                cfg.ReceiveEndpoint("product-saga-logged", e =>
                {
                    e.ConfigureConsumer<ProductLoggedConsumer>(context);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
