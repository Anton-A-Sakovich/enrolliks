using Enrolliks.Persistence.EntityFramework.People;
using Enrolliks.Persistence.EntityFramework.Skills;
using Microsoft.EntityFrameworkCore;

namespace Enrolliks.Persistence.EntityFramework
{
    internal class EnrolliksContext : DbContext
    {
        public DbSet<PersonEntity> People { get; set; } = null!;

        public DbSet<SkillEntity> Skills { get; set; } = null!;

        public EnrolliksContext(DbContextOptions<EnrolliksContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PersonEntity>().Configure();
            modelBuilder.Entity<SkillEntity>().Configure();
        }
    }
}
