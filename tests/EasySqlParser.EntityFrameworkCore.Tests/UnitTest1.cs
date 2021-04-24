using System;
using System.Linq;
using EasySqlParser.Configurations;
using EasySqlParser.EntityFrameworkCore.Extensions;
using EasySqlParser.EntityFrameworkCore.Tests.SqlServer;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.EntityFrameworkCore.Tests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _output;
        public UnitTest1(ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );
            _output = output;
        }

        [Fact]
        public void Test1()
        {
            var options = new DbContextOptionsBuilder<EfContext>()
                .UseInMemoryDatabase(databaseName: "foo_bar")
                .Options;

            using (var context = new EfContext(options))
            {
                var entity = new Employee
                             {
                                 Name = "John Doe",
                                 Salary = 10000,

                             };
                context.Employees.Add(entity);
                context.SaveChanges();
            }

            using (var context = new EfContext(options))
            {
                context.Employees.Count().Is(1);
                var result = context.Employees.First();
                _output.WriteLine($"{result.Id}");
                _output.WriteLine($"{result.Name}");
                _output.WriteLine($"{result.Salary}");
                _output.WriteLine($"{result.VersionNo}");
            }
        }

        [Fact]
        public void Test2()
        {
            using var context = new SampleDbContextBoolToIntExplicit(_output);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            using var transaction = context.Database.BeginTransaction();
            var config = new MockConfig(QueryBehavior.AllColumns, _output.WriteLine);
            config.AddCache(context);
            context.Users.Add(new User {Name = "John Doe", IsActive = true});
            context.SaveChanges();
            var count = context.Users.Count(x => x.IsActive);
            count.Is(1);

            var user = new User {Name = "Jane Doe", IsActive = true};
            var parameter = new QueryBuilderParameter(user, SqlKind.Insert, config);
            var affected = context.Database.ExecuteNonQuery(parameter);
            affected.Is(1);
            transaction.Commit();

        }

        [Fact]
        public void Test3()
        {
            using var context = new SampleDbContextExplicit(_output);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            using var transaction = context.Database.BeginTransaction();
            var config = new MockConfig(QueryBehavior.AllColumns, _output.WriteLine);
            config.AddCache(context);
            context.Riders.Add(new Rider {Mount = EquineBeast.Horse});
            context.SaveChanges();
            var count = context.Riders.Count();
            count.Is(1);

            var rider = new Rider {Mount = EquineBeast.Unicorn};
            var parameter = new QueryBuilderParameter(rider, SqlKind.Insert, config);
            var affected = context.Database.ExecuteNonQuery(parameter);
            affected.Is(1);
            transaction.Commit();



        }
    }
}
