using System;
using AutoMapper;
using Enrolliks.Persistence.EntityFramework;
using Enrolliks.Persistence.EntityFramework.People;
using Enrolliks.Persistence.EntityFramework.Skills;
using Enrolliks.Persistence.People;
using Enrolliks.Persistence.Skills;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EnrolliksPersistenceEFServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityFrameworkPersistence(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder> options)
        {
            services.AddEnrolliksPersistence();
            services.AddDbContext<EnrolliksContext>(options);

            services.AddScoped<IPeopleRepository, PeopleRepository>();
            services.AddSingleton<Profile>(new PersonProfile());

            services.AddScoped<ISkillsRepository, SkillsRepository>();
            services.AddSingleton<Profile>(new SkillProfile());

            return services;
        }
    }
}
