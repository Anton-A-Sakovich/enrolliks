using Microsoft.EntityFrameworkCore;

namespace Enrolliks.Persistence.EntityFramework
{
    internal class EnrolliksContext : DbContext
    {
        public DbSet<Person> People { get; set; } = null!;

        public EnrolliksContext(DbContextOptions<EnrolliksContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Person>(entityBuilder =>
            {
                entityBuilder.HasKey(person => person.Name);
                entityBuilder.Property(person => person.Name).IsRequired();
            });
        }
    }
}
