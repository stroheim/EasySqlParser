using System.Linq;
using EasySqlParser.Configurations;
using EasySqlParser.EntityFrameworkCore.Extensions;
using EasySqlParser.SqlGenerator;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Sdk;

namespace EasySqlParser.EntityFrameworkCore.Tests
{

    public class DatabaseFacadeExtensionForSqlServerTest :
        IClassFixture<DbContextFixture>
    {
        public DatabaseFacadeExtensionForSqlServerTest(DbContextFixture fixture)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );
            Fixture = fixture;
        }
        public DbContextFixture Fixture { get; }

        [Fact]
        public void Test_insert_default()
        {
            using var context = Fixture.CreateContext();
            var employee = new Employee
                           {
                               Id = 2,
                               Name = "Solid Snake"
                           };

            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Insert);
            var affected = context.Database.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
        }

        [Fact]
        public void Test_insert_excludeNull()
        {
            var employee = new Employee
                           {
                               Id = 2,
                               Salary = 100M
                           };
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Insert, excludeNull: true);
            using var context = Fixture.CreateContext();
            var affected = context.Database.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            var instance = context.Employees.Single(x => x.Id == 2);
            instance.Name.IsNull();
        }

        [Fact]
        public void Test_update_default()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 1);
            employee.Salary = 5000M;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update);
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
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 1);
            employee.Name = null;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, excludeNull:true);
            var affected = context.Database.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            var instance = context.Employees.Single(x => x.Id == 1);
            instance.Name.Is("John Doe");

        }

        [Fact]
        public void Test_update_ignoreVersion()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 1);
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, ignoreVersion: true);
            var affected = context.Database.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            var instance = context.Employees.Single(x => x.Id == 1);
            instance.VersionNo.Is(100L);

        }

        [Fact]
        public void Test_update_dbUpdateConcurrencyException()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 1);
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update);
            var ex = Assert.Throws<DbUpdateConcurrencyException>(
                () => context.Database.ExecuteNonQueryByQueryBuilder(parameter));
            ex.IsNotNull();
        }

        [Fact]
        public void Test_update_suppressDbUpdateConcurrencyException()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 1);
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, suppressDbUpdateConcurrencyException: true);
            var affected = context.Database.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(0);

        }

        [Fact]
        public void Test_delete_default()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 1);
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Delete);
            var affected = context.Database.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            var cnt = context.Employees.Count();
            cnt.Is(0);

        }

        [Fact]
        public void Test_delete_ignoreVersion()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 1);
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Delete, ignoreVersion: true);
            var affected = context.Database.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);

        }

    }

    public class DatabaseFacadeExtensionTest
    {
        public DatabaseFacadeExtensionTest()
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );

        }

        [Fact]
        public void Test_Insert_Default()
        {
            var options = new DbContextOptionsBuilder<EfContext>()
                .UseInMemoryDatabase(databaseName: "Test_Insert_Default")
                .Options;
            var employee = new Employee
                           {
                               Id = 1,
                               Name = "John Doe",
                               Salary = 100M
                           };
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Insert);
            using var context = new EfContext(options);
            var builder = context.Database.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("INSERT INTO [dbo].[EMP] ([ID], [NAME], [SALARY], [VERSION]) VALUES (@Id, @Name, @Salary, @VersionNo)");
            builder.DbDataParameters.Count.Is(4);
            builder.DbDataParameters[0].Value.Is(1);
            builder.DbDataParameters[1].Value.Is("John Doe");
            builder.DbDataParameters[2].Value.Is(100M);
            builder.DbDataParameters[3].Value.Is(1L);
        }


        [Fact]
        public void Test_Insert_ExcludeNull()
        {
            var options = new DbContextOptionsBuilder<EfContext>()
                .UseInMemoryDatabase(databaseName: "Test_Insert_ExcludeNull")
                .Options;
            var employee = new Employee
                           {
                               Id = 1,
                               Salary = 100M
                           };
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Insert, excludeNull: true);
            using var context = new EfContext(options);
            var builder = context.Database.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("INSERT INTO [dbo].[EMP] ([ID], [SALARY], [VERSION]) VALUES (@Id, @Salary, @VersionNo)");
            builder.DbDataParameters.Count.Is(3);
            builder.DbDataParameters[0].Value.Is(1);
            builder.DbDataParameters[1].Value.Is(100M);
            builder.DbDataParameters[2].Value.Is(1L);
        }

        [Fact]
        public void Test_Update_Default()
        {
            var options = new DbContextOptionsBuilder<EfContext>()
                .UseInMemoryDatabase(databaseName: "Test_Update_Default")
                .Options;
            var employee = new Employee
                           {
                               Id = 1,
                               Name = "John Doe",
                               VersionNo = 100L
                           };
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update);
            using var context = new EfContext(options);
            var builder = context.Database.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("UPDATE [dbo].[EMP] SET [NAME] = @Name, [SALARY] = @Salary, [VERSION] = @VersionNo + 1 WHERE [ID] = @Id AND [VERSION] = @VersionNo");
            builder.DbDataParameters.Count.Is(4);
            builder.DbDataParameters[0].Value.Is("John Doe");
            builder.DbDataParameters[1].Value.Is(0M);
            builder.DbDataParameters[2].Value.Is(100L);
            builder.DbDataParameters[3].Value.Is(1);
        }

        [Fact]
        public void Test_Update_ExcludeNull()
        {
            var options = new DbContextOptionsBuilder<EfContext>()
                .UseInMemoryDatabase(databaseName: "Test_Update_ExcludeNull")
                .Options;
            var employee = new Employee
                           {
                               Id = 1,
                               VersionNo = 100L
                           };
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, excludeNull: true);
            using var context = new EfContext(options);
            var builder = context.Database.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("UPDATE [dbo].[EMP] SET [SALARY] = @Salary, [VERSION] = @VersionNo + 1 WHERE [ID] = @Id AND [VERSION] = @VersionNo");
            builder.DbDataParameters.Count.Is(3);
            builder.DbDataParameters[0].Value.Is(0M);
            builder.DbDataParameters[1].Value.Is(100L);
            builder.DbDataParameters[2].Value.Is(1);
        }

        [Fact]
        public void Test_Update_IgnoreVersion()
        {
            var options = new DbContextOptionsBuilder<EfContext>()
                .UseInMemoryDatabase(databaseName: "Test_Update_IgnoreVersion")
                .Options;
            var employee = new Employee
                           {
                               Id = 1,
                               Name = "John Doe",
                               VersionNo = 100L
                           };
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, ignoreVersion: true);
            using var context = new EfContext(options);
            var builder = context.Database.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("UPDATE [dbo].[EMP] SET [NAME] = @Name, [SALARY] = @Salary, [VERSION] = @VersionNo WHERE [ID] = @Id");
            builder.DbDataParameters.Count.Is(4);
            builder.DbDataParameters[0].Value.Is("John Doe");
            builder.DbDataParameters[1].Value.Is(0M);
            builder.DbDataParameters[2].Value.Is(100L);
            builder.DbDataParameters[3].Value.Is(1);
        }

        [Fact]
        public void Test_Delete_Default()
        {
            var options = new DbContextOptionsBuilder<EfContext>()
                .UseInMemoryDatabase(databaseName: "Test_Delete_Default")
                .Options;
            var employee = new Employee
                           {
                               Id = 1,
                               Name = "John Doe",
                               VersionNo = 100L
                           };
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Delete);
            using var context = new EfContext(options);
            var builder = context.Database.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("DELETE FROM [dbo].[EMP] WHERE [ID] = @Id AND [VERSION] = @VersionNo");
            builder.DbDataParameters.Count.Is(2);
            builder.DbDataParameters[0].Value.Is(1);
            builder.DbDataParameters[1].Value.Is(100L);
        }

        [Fact]
        public void Test_Delete_IgnoreVersion()
        {
            var options = new DbContextOptionsBuilder<EfContext>()
                .UseInMemoryDatabase(databaseName: "Test_Delete_IgnoreVersion")
                .Options;
            var employee = new Employee
                           {
                               Id = 1,
                               Name = "John Doe",
                               VersionNo = 100L
                           };
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Delete, ignoreVersion: true);
            using var context = new EfContext(options);
            var builder = context.Database.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("DELETE FROM [dbo].[EMP] WHERE [ID] = @Id");
            builder.DbDataParameters.Count.Is(1);
            builder.DbDataParameters[0].Value.Is(1);
        }

    }
}
