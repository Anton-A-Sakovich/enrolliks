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
using Microsoft.Extensions.Hosting;

namespace Enrolliks.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Enrolliks",
                    Version = "v1",
                });
            });

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

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(options =>
                {
                    options.RouteTemplate = "/api/swagger/{documentName}/swagger.json";
                });

                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("v1/swagger.json", "Enrolliks v1");
                    options.RoutePrefix = "api/swagger";
                });
            }

            app.Use(async (HttpContext context, Func<Task> next) =>
            {
                if (context.GetEndpoint() is null)
                    await Results.File("index.html", "text/html").ExecuteAsync(context);
                else
                    await next();
            });

            app.UseEndpoints(builder =>
            {
                builder.MapControllers();
            });

            if (app.Environment.IsDevelopment())
            {
                app.Services.GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();
            }

            app.Run();
        }
    }
}