using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enrolliks.Persistence.EntityFramework.Skills
{
    internal static class SkillModelBuilderExtensions
    {
        public static EntityTypeBuilder<SkillEntity> Configure(this EntityTypeBuilder<SkillEntity> builder)
        {
            builder.HasKey(skill => skill.Id);

            builder.Property(skill => skill.Id).IsRequired().HasMaxLength(128);

            builder.Property(skill => skill.Name).IsRequired().HasMaxLength(128);

            builder.HasIndex(skill => new { skill.Name }).IsUnique();

            return builder;
        }
    }
}
