using Enrolliks.Persistence.People;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEnrolliksPersistence(this IServiceCollection services)
        {
            services.AddScoped<IPersonValidator, PersonValidator>();
            services.AddScoped<IPeopleManager, PeopleManager>();
            return services;
        }
    }
}
