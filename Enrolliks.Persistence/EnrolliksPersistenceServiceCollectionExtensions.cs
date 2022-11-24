using Enrolliks.Persistence.People;
using Enrolliks.Persistence.Skills;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EnrolliksPersistenceServiceCollectionExtensions
    {
        public static IServiceCollection AddEnrolliksPersistence(this IServiceCollection services)
        {
            services.AddScoped<IPersonValidator, PersonValidator>();
            services.AddScoped<IPeopleManager, PeopleManager>();

            services.AddScoped<ISkillValidator, SkillValidator>();
            services.AddScoped<ISkillsManager, SkillsManager>();

            return services;
        }
    }
}
