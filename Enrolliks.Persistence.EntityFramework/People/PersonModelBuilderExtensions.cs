using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enrolliks.Persistence.EntityFramework.People
{
    internal static class PersonModelBuilderExtensions
    {
        public static EntityTypeBuilder<PersonEntity> Configure(this EntityTypeBuilder<PersonEntity> builder)
        {
            builder.HasKey(person => person.Name);
            builder.Property(person => person.Name).IsRequired();
            return builder;
        }
    }
}
