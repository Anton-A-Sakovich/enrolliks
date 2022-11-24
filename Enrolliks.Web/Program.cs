using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Enrolliks.Web.Controllers;
using Enrolliks.Web.Controllers.People;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enrolliks.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAutoMapper((services, config) =>
            {
                config.AddProfiles(services.GetRequiredService<IEnumerable<Profile>>());
                config.AddProfile<PeopleMappingProfile>();
            },
            Array.Empty<Type>());

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new DiscriminatedUnionConverterFactory());
                    options.JsonSerializerOptions.Converters.Add(new ExceptionConverter());
                });

            builder.Services.AddEntityFrameworkPersistence((provider, options) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                options.UseSqlServer(configuration.GetConnectionString("Default"));
            });

            var app = builder.Build();

            app.UseStaticFiles();
            app.UseRouting();
            app.Use(async (HttpContext context, Func<Task> next) =>
            {
                if (context.GetEndpoint() is null)
                    await Results.File("index.html", "text/html").ExecuteAsync(context);
                else
                    await next();
            });

            app.MapControllers();

#if DEBUG
            app.Services.GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();
#endif

            app.Run();
        }
    }
}