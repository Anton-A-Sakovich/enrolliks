using System;
using AutoMapper;

namespace Enrolliks.Web.Controllers
{
    public static class DiscriminatedUnionAutomapperExtensions
    {
        public static IMappingExpression<T, DiscriminatedUnionModel<T>> CreateDiscriminatedUnionMap<T>(this Profile profile)
            where T : notnull
        {
            if (profile is null) throw new ArgumentNullException(nameof(profile));

            return profile.CreateMap<T, DiscriminatedUnionModel<T>>()
                .ForMember(destination => destination.Tag, opts => opts.Ignore())
                .ForMember(destination => destination.Value, opts => opts.MapFrom(source => source));
        }
    }
}
