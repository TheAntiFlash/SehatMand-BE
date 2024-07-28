using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SehatMand.Infrastructure.Persistence;

namespace SehatMand.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
   
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<SmDbContext>();
        services.AddScoped<DbContext, SmDbContext>();

    }
}