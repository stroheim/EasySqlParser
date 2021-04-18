using Microsoft.EntityFrameworkCore;

namespace EasySqlParser.EntityFrameworkCore.Tests.SqlServer
{
    public class EfContext : DbContext
    {

        public EfContext(DbContextOptions<EfContext> options)
            : base(options)
        {

        }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<EmployeeWithDateAndUser> EmployeeWithDateAndUsers { get; set; }

        public DbSet<Characters> Characters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.HasDefaultSchema("dbo");
            //modelBuilder.Entity<Employee>()
            //    .ToTable("EMP")
            //    .HasKey(b => b.Id);
            //modelBuilder.Entity<Employee>()
            //    .Property(b => b.Id)
            //    .HasColumnName("ID")
            //    .IsRequired()
            //    .ValueGeneratedNever();
            //modelBuilder.Entity<Employee>()
            //    .Property(b => b.Name)
            //    .HasColumnName("NAME");
            //modelBuilder.Entity<Employee>()
            //    .Property(b => b.Salary)
            //    .HasColumnName("SALARY");
            //modelBuilder.Entity<Employee>()
            //    .Property(b => b.VersionNo)
            //    .HasColumnName("VERSION");
                
            //base.OnModelCreating(modelBuilder);
        }
    }
}
