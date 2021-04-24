using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit.Abstractions;

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

    // code base from
    // https://github.com/dotnet/EntityFramework.Docs/blob/main/samples/core/Modeling/ValueConversions/EnumToStringConversions.cs

    public class SampleDbContextExplicit : SampleDbContextBase
    {
        private readonly ITestOutputHelper _output;
        public SampleDbContextExplicit(ITestOutputHelper output)
        {
            _output = output;
        }

        public DbSet<Rider> Riders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .LogTo(_output.WriteLine, new[] { RelationalEventId.CommandExecuted })
                .UseSqlServer(
                    $"Server=localhost,51433;Database={nameof(SampleDbContextExplicit)};User Id=sa;Password=Mitsuhide@1582;ConnectRetryCount=0")
                .EnableSensitiveDataLogging();
        }

        #region ExplicitConversion
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Rider>()
                .Property(e => e.Mount)
                .HasConversion(
                    v => v.ToString(),
                    v => (EquineBeast)Enum.Parse(typeof(EquineBeast), v));
        }
        #endregion
    }

    public class SampleDbContextByClrType : SampleDbContextBase
    {
        #region ConversionByClrType
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Rider>()
                .Property(e => e.Mount)
                .HasConversion<string>();
        }
        #endregion
    }

    public class SampleDbContextByDatabaseType : SampleDbContextBase
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rider2>();
        }
    }

    public class SampleDbContextByConverterInstance : SampleDbContextBase
    {
        #region ConversionByConverterInstance
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var converter = new ValueConverter<EquineBeast, string>(
                v => v.ToString(),
                v => (EquineBeast)Enum.Parse(typeof(EquineBeast), v));

            modelBuilder
                .Entity<Rider>()
                .Property(e => e.Mount)
                .HasConversion(converter);
        }
        #endregion
    }

    public class SampleDbContextByClrTypeWithFacets : SampleDbContextBase
    {
        #region ConversionByClrTypeWithFacets
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Rider>()
                .Property(e => e.Mount)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsUnicode(false);
        }
        #endregion
    }

    public class SampleDbContextByConverterInstanceWithFacets : SampleDbContextBase
    {
        #region ConversionByConverterInstanceWithFacets
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var converter = new ValueConverter<EquineBeast, string>(
                v => v.ToString(),
                v => (EquineBeast)Enum.Parse(typeof(EquineBeast), v));

            modelBuilder
                .Entity<Rider>()
                .Property(e => e.Mount)
                .HasConversion(converter)
                .HasMaxLength(20)
                .IsUnicode(false);
        }
        #endregion
    }

    public class SampleDbContextByConverterInstanceWithMappingHints : SampleDbContextBase
    {
        #region ConversionByConverterInstanceWithMappingHints
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var converter = new ValueConverter<EquineBeast, string>(
                v => v.ToString(),
                v => (EquineBeast)Enum.Parse(typeof(EquineBeast), v),
                new ConverterMappingHints(size: 20, unicode: false));

            modelBuilder
                .Entity<Rider>()
                .Property(e => e.Mount)
                .HasConversion(converter);
        }
        #endregion
    }

    public class SampleDbContextByBuiltInInstance : SampleDbContextBase
    {
        #region ConversionByBuiltInInstance
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var converter = new EnumToStringConverter<EquineBeast>();

            modelBuilder
                .Entity<Rider>()
                .Property(e => e.Mount)
                .HasConversion(converter);
        }
        #endregion
    }

    public class SampleDbContextBoolToInt : SampleDbContextBase
    {
        #region ConversionByBuiltInBoolToInt
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<User>()
                .Property(e => e.IsActive)
                .HasConversion<int>();
        }
        #endregion
    }

    public class SampleDbContextBoolToIntExplicit : SampleDbContextBase
    {
        private readonly ITestOutputHelper _output;
        public SampleDbContextBoolToIntExplicit(ITestOutputHelper output)
        {
            _output = output;
        }

        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .LogTo(_output.WriteLine, new[] {RelationalEventId.CommandExecuted})
                .UseSqlServer(
                    $"Server=localhost,51433;Database={nameof(SampleDbContextBoolToIntExplicit)};User Id=sa;Password=Mitsuhide@1582;ConnectRetryCount=0")
                .EnableSensitiveDataLogging();
        }

        #region ConversionByBuiltInBoolToIntExplicit
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var converter = new BoolToZeroOneConverter<int>();
            modelBuilder
                .Entity<User>()
                .Property(e => e.IsActive)
                .HasConversion(converter);
        }
        #endregion
    }

    public class SampleDbContextRider2 : SampleDbContextBase
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region ConversionByDatabaseTypeFluent
            modelBuilder
                .Entity<Rider2>()
                .Property(e => e.Mount)
                .HasColumnType("nvarchar(24)");
            #endregion
        }
    }
    public class SampleDbContextBase : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted })
                .UseSqlServer(@"Server=localhost,51433;Database=SampleDbContextBase;User Id=sa;Password=Mitsuhide@1582;ConnectRetryCount=0")
                .EnableSensitiveDataLogging();
    }

    #region BeastAndRider
    public class Rider
    {
        public int Id { get; set; }
        public EquineBeast Mount { get; set; }
    }

    public enum EquineBeast
    {
        Donkey,
        Mule,
        Horse,
        Unicorn
    }
    #endregion

    #region ConversionByDatabaseType
    public class Rider2
    {
        public int Id { get; set; }

        [Column(TypeName = "nvarchar(24)")]
        public EquineBeast Mount { get; set; }
    }
    #endregion

    public class User
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }

        public string Name { get; set; }
    }


}
