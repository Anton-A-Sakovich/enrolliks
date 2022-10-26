using System;
using Enrolliks.Persistence;
using Enrolliks.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityFrameworkPersistence(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder> options)
        {
            services.AddDbContext<EnrolliksContext>(options);
            services.AddScoped<IPeopleRepository, PeopleRepository>();
            return services;
        }
    }
}
