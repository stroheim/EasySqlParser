using System.Linq;
using EasySqlParser.Configurations;
using EasySqlParser.EntityFrameworkCore.Extensions;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace EasySqlParser.EntityFrameworkCore.Tests
{

    public class DatabaseFacadeExtensionForSqlServerTest :
        IClassFixture<DbContextFixture>
    {
        private readonly MockConfig _mockConfig;
        public DatabaseFacadeExtensionForSqlServerTest(DbContextFixture fixture, ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );
            Fixture = fixture;
            _output = output;
            _mockConfig = new MockConfig(QueryBehavior.AllColumns, _output.WriteLine);
            _mockConfig.WriteIndented = true;

        }
        public DbContextFixture Fixture { get; }
        private readonly ITestOutputHelper _output;

        [Fact]
        public void Test_insert_default()
        {
            using var context = Fixture.CreateContext();
            var employee = new Employee
                           {
                               Id = 11,
                               Name = "John Doe"
            };
            //_mockConfig.AddCache(context);
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _mockConfig);
            var affected = context.Database.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
        }

        [Fact]
        public void Test_insert_excludeNull()
        {
            var employee = new Employee
                           {
                               Id = 12,
                               Salary = 100M
                           };
            using var context = Fixture.CreateContext();
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _mockConfig, excludeNull: true);
            var affected = context.Database.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            var instance = context.Employees.Single(x => x.Id == 12);
            instance.Name.IsNull();
        }

        [Fact]
        public void Test_update_default()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 1);
            employee.Salary = 5000M;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _mockConfig);
            var affected = context.Database.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            employee.VersionNo.Is(2L);
            var instance = context.Employees.Single(x => x.Id == 1);
            instance.Salary.Is(5000M);

        }

        [Fact]
        public void Test_update_excludeNull()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 2);
            employee.Name = null;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _mockConfig, excludeNull:true);
            var affected = context.Database.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            var instance = context.Employees.Single(x => x.Id == 2);
            instance.Name.Is("Rob Walters");

        }

        [Fact]
        public void Test_update_ignoreVersion()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 3);
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _mockConfig, ignoreVersion: true);
            var affected = context.Database.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            var instance = context.Employees.Single(x => x.Id == 3);
            instance.VersionNo.Is(100L);

        }

        [Fact]
        public void Test_update_optimisticLockException()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 4);
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _mockConfig);
            var ex = Assert.Throws<OptimisticLockException>(
                () => context.Database.ExecuteNonQueryByQueryBuilder(parameter));
            ex.IsNotNull();
        }

        [Fact]
        public void Test_update_suppressOptimisticLockException()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 5);
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _mockConfig, suppressOptimisticLockException: true);
            var affected = context.Database.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(0);

        }

        [Fact]
        public void Test_delete_default()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 6);
            var parameter = new QueryBuilderParameter(employee, SqlKind.Delete, _mockConfig);
            var affected = context.Database.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            var cnt = context.Employees.AsNoTracking().Count(x => x.Id == 6);
            cnt.Is(0);

        }

        [Fact]
        public void Test_delete_ignoreVersion()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 7);
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Delete, _mockConfig, ignoreVersion: true);
            var affected = context.Database.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);

        }

    }

    //public class DatabaseFacadeExtensionTest
    //{
    //    private readonly MockConfig _mockConfig;
    //    public DatabaseFacadeExtensionTest(ITestOutputHelper output)
    //    {
    //        ConfigContainer.AddDefault(
    //            DbConnectionKind.SqlServer,
    //            () => new SqlParameter()
    //        );
    //        _mockConfig = new MockConfig(QueryBehavior.None, output.WriteLine);
    //    }

    //    [Fact]
    //    public void Test_Insert_Default()
    //    {
    //        var options = new DbContextOptionsBuilder<EfContext>()
    //            .UseInMemoryDatabase(databaseName: "Test_Insert_Default")
    //            .Options;
    //        var employee = new Employee
    //                       {
    //                           Id = 1,
    //                           Name = "John Doe",
    //                           Salary = 100M
    //                       };
    //        using var context = new EfContext(options);
    //        var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _mockConfig);
    //        var builder = QueryBuilder.GetQueryBuilderResult(parameter);
    //        builder.IsNotNull();
    //        builder.ParsedSql.Is("INSERT INTO [dbo].[EMP] ([ID], [NAME], [SALARY], [VERSION]) VALUES (@Id, @Name, @Salary, @VersionNo)");
    //        builder.DbDataParameters.Count.Is(4);
    //        builder.DbDataParameters[0].Value.Is(1);
    //        builder.DbDataParameters[1].Value.Is("John Doe");
    //        builder.DbDataParameters[2].Value.Is(100M);
    //        builder.DbDataParameters[3].Value.Is(1L);
    //    }


    //    [Fact]
    //    public void Test_Insert_ExcludeNull()
    //    {
    //        var options = new DbContextOptionsBuilder<EfContext>()
    //            .UseInMemoryDatabase(databaseName: "Test_Insert_ExcludeNull")
    //            .Options;
    //        var employee = new Employee
    //                       {
    //                           Id = 1,
    //                           Salary = 100M
    //                       };
    //        using var context = new EfContext(options);
    //        var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Insert, _mockConfig, excludeNull: true);
    //        var builder = context.Database.GetQueryBuilderResult(parameter);
    //        builder.IsNotNull();
    //        builder.ParsedSql.Is("INSERT INTO [dbo].[EMP] ([ID], [SALARY], [VERSION]) VALUES (@Id, @Salary, @VersionNo)");
    //        builder.DbDataParameters.Count.Is(3);
    //        builder.DbDataParameters[0].Value.Is(1);
    //        builder.DbDataParameters[1].Value.Is(100M);
    //        builder.DbDataParameters[2].Value.Is(1L);
    //    }

    //    [Fact]
    //    public void Test_Update_Default()
    //    {
    //        var options = new DbContextOptionsBuilder<EfContext>()
    //            .UseInMemoryDatabase(databaseName: "Test_Update_Default")
    //            .Options;
    //        var employee = new Employee
    //                       {
    //                           Id = 1,
    //                           Name = "John Doe",
    //                           VersionNo = 100L
    //                       };
    //        using var context = new EfContext(options);
    //        var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, _mockConfig);
    //        var builder = context.Database.GetQueryBuilderResult(parameter);
    //        builder.IsNotNull();
    //        builder.ParsedSql.Is("UPDATE [dbo].[EMP] SET [NAME] = @Name, [SALARY] = @Salary, [VERSION] = @VersionNo + 1 WHERE [ID] = @Id AND [VERSION] = @VersionNo");
    //        builder.DbDataParameters.Count.Is(4);
    //        builder.DbDataParameters[0].Value.Is("John Doe");
    //        builder.DbDataParameters[1].Value.Is(0M);
    //        builder.DbDataParameters[2].Value.Is(100L);
    //        builder.DbDataParameters[3].Value.Is(1);
    //    }

    //    [Fact]
    //    public void Test_Update_ExcludeNull()
    //    {
    //        var options = new DbContextOptionsBuilder<EfContext>()
    //            .UseInMemoryDatabase(databaseName: "Test_Update_ExcludeNull")
    //            .Options;
    //        var employee = new Employee
    //                       {
    //                           Id = 1,
    //                           VersionNo = 100L
    //                       };
    //        using var context = new EfContext(options);
    //        var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, _mockConfig, excludeNull: true);
    //        var builder = context.Database.GetQueryBuilderResult(parameter);
    //        builder.IsNotNull();
    //        builder.ParsedSql.Is("UPDATE [dbo].[EMP] SET [SALARY] = @Salary, [VERSION] = @VersionNo + 1 WHERE [ID] = @Id AND [VERSION] = @VersionNo");
    //        builder.DbDataParameters.Count.Is(3);
    //        builder.DbDataParameters[0].Value.Is(0M);
    //        builder.DbDataParameters[1].Value.Is(100L);
    //        builder.DbDataParameters[2].Value.Is(1);
    //    }

    //    [Fact]
    //    public void Test_Update_IgnoreVersion()
    //    {
    //        var options = new DbContextOptionsBuilder<EfContext>()
    //            .UseInMemoryDatabase(databaseName: "Test_Update_IgnoreVersion")
    //            .Options;
    //        var employee = new Employee
    //                       {
    //                           Id = 1,
    //                           Name = "John Doe",
    //                           VersionNo = 100L
    //                       };
    //        using var context = new EfContext(options);
    //        var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, _mockConfig, ignoreVersion: true);
    //        var builder = context.Database.GetQueryBuilderResult(parameter);
    //        builder.IsNotNull();
    //        builder.ParsedSql.Is("UPDATE [dbo].[EMP] SET [NAME] = @Name, [SALARY] = @Salary, [VERSION] = @VersionNo WHERE [ID] = @Id");
    //        builder.DbDataParameters.Count.Is(4);
    //        builder.DbDataParameters[0].Value.Is("John Doe");
    //        builder.DbDataParameters[1].Value.Is(0M);
    //        builder.DbDataParameters[2].Value.Is(100L);
    //        builder.DbDataParameters[3].Value.Is(1);
    //    }

    //    [Fact]
    //    public void Test_Delete_Default()
    //    {
    //        var options = new DbContextOptionsBuilder<EfContext>()
    //            .UseInMemoryDatabase(databaseName: "Test_Delete_Default")
    //            .Options;
    //        var employee = new Employee
    //                       {
    //                           Id = 1,
    //                           Name = "John Doe",
    //                           VersionNo = 100L
    //                       };
    //        using var context = new EfContext(options);
    //        var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Delete, _mockConfig);
    //        var builder = context.Database.GetQueryBuilderResult(parameter);
    //        builder.IsNotNull();
    //        builder.ParsedSql.Is("DELETE FROM [dbo].[EMP] WHERE [ID] = @Id AND [VERSION] = @VersionNo");
    //        builder.DbDataParameters.Count.Is(2);
    //        builder.DbDataParameters[0].Value.Is(1);
    //        builder.DbDataParameters[1].Value.Is(100L);
    //    }

    //    [Fact]
    //    public void Test_Delete_IgnoreVersion()
    //    {
    //        var options = new DbContextOptionsBuilder<EfContext>()
    //            .UseInMemoryDatabase(databaseName: "Test_Delete_IgnoreVersion")
    //            .Options;
    //        var employee = new Employee
    //                       {
    //                           Id = 1,
    //                           Name = "John Doe",
    //                           VersionNo = 100L
    //                       };
    //        using var context = new EfContext(options);
    //        var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Delete, _mockConfig, ignoreVersion: true);
    //        var builder = context.Database.GetQueryBuilderResult(parameter);
    //        builder.IsNotNull();
    //        builder.ParsedSql.Is("DELETE FROM [dbo].[EMP] WHERE [ID] = @Id");
    //        builder.DbDataParameters.Count.Is(1);
    //        builder.DbDataParameters[0].Value.Is(1);
    //    }

    //}
}
