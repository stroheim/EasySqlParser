using System.Linq;
using System.Threading.Tasks;
using EasySqlParser.Configurations;
using EasySqlParser.EntityFrameworkCore.Extensions;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.EntityFrameworkCore.Tests.SqlServer
{

    public class DatabaseFacadeExtensionForSqlServerTest :
        IClassFixture<FacadeFixture>
    {
        private readonly MockConfig _mockConfig;
        public DatabaseFacadeExtensionForSqlServerTest(FacadeFixture fixture, ITestOutputHelper output)
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
        public FacadeFixture Fixture { get; }
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
            var affected = context.Database.ExecuteNonQuery(parameter);
            affected.Is(1);
        }

        [Fact]
        public void Test_insert_with_date_and_user()
        {
            using var context = Fixture.CreateContext();
            var employee = new EmployeeWithDateAndUser
                           {
                               Id = 11,
                               Name = "Jane Doe"
                           };
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _mockConfig, currentUser: "Sariya Harnpadoungsataya");
            var affected = context.Database.ExecuteNonQuery(parameter);
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
            var affected = context.Database.ExecuteNonQuery(parameter);
            affected.Is(1);
            var instance = context.Employees.Single(x => x.Id == 12);
            instance.Name.IsNull();
        }

        [Fact]
        public async Task Test_insert_excludeNull_async()
        {
            await using var context = Fixture.CreateContext();
            var employee = new Employee
                           {
                               Id = 14,
                               Salary = 100M
                           };
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _mockConfig, excludeNull: true);
            var affected = await context.Database.ExecuteNonQueryAsync(parameter);
            affected.Is(1);
        }

        [Fact]
        public void Test_update_default()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 1);
            employee.Salary = 5000M;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _mockConfig);
            var affected = context.Database.ExecuteNonQuery(parameter);
            affected.Is(1);
            employee.VersionNo.Is(2L);
            var instance = context.Employees.Single(x => x.Id == 1);
            instance.Salary.Is(5000M);

        }

        [Fact]
        public void Test_update_with_date_and_user()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Database.ExecuteReaderFirst<EmployeeWithDateAndUser>(_mockConfig, x => x.Id == 1);
            employee.Salary = 5000M;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _mockConfig, currentUser: "Sariya Harnpadoungsataya");
            var affected = context.Database.ExecuteNonQuery(parameter);
            affected.Is(1);
            employee.VersionNo.Is(2L);

        }

        [Fact]
        public void Test_update_excludeNull()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 2);
            employee.Name = null;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _mockConfig, excludeNull:true);
            var affected = context.Database.ExecuteNonQuery(parameter);
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
            var affected = context.Database.ExecuteNonQuery(parameter);
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
                () => context.Database.ExecuteNonQuery(parameter));
            ex.IsNotNull();
        }

        [Fact]
        public void Test_update_suppressOptimisticLockException()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 5);
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _mockConfig, suppressOptimisticLockException: true);
            var affected = context.Database.ExecuteNonQuery(parameter);
            affected.Is(0);

        }

        [Fact]
        public void Test_delete_default()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 6);
            var parameter = new QueryBuilderParameter(employee, SqlKind.Delete, _mockConfig);
            var affected = context.Database.ExecuteNonQuery(parameter);
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
            var affected = context.Database.ExecuteNonQuery(parameter);
            affected.Is(1);

        }

        [Fact]
        public async Task Test_soft_delete_async()
        {
            await using var context = Fixture.CreateContext();
            var employee =
                await context.Database.ExecuteReaderFirstAsync<EmployeeWithDateAndUser>(_mockConfig, x => x.Id == 7);
            var parameter = new QueryBuilderParameter(employee, SqlKind.SoftDelete, _mockConfig,
                currentUser: "Sariya Harnpadoungsataya");
            var affected = await context.Database.ExecuteNonQueryAsync(parameter);
            affected.Is(1);

        }

        [Fact]
        public void Test_ExecuteReader()
        {
            using var context = Fixture.CreateContext();
            var results = context.Database.ExecuteReader<Employee>(_mockConfig, x => x.Id > 0);
            results.Count.IsNot(0);

        }

        [Fact]
        public async Task Test_ExecuteReader_async()
        {
            await using var context = Fixture.CreateContext();

            var results = await context.Database.ExecuteReaderAsync<Employee>(_mockConfig, x => x.Id > 0);
            results.Count.IsNot(0);
        }
    }

}
