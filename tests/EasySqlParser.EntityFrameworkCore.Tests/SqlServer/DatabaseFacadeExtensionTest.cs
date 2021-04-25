using System.IO;
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
        private readonly SqlServerConfig _sqlServerConfig;
        public DatabaseFacadeExtensionForSqlServerTest(FacadeFixture fixture, ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );
            Fixture = fixture;
            _output = output;
            _sqlServerConfig = new SqlServerConfig(fixture.CreateContext(), output);

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
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _sqlServerConfig);
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
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _sqlServerConfig, currentUser: "Sariya Harnpadoungsataya");
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
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _sqlServerConfig, excludeNull: true);
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
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _sqlServerConfig, excludeNull: true);
            var affected = await context.Database.ExecuteNonQueryAsync(parameter);
            affected.Is(1);
        }

        [Fact]
        public void Test_update_default()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 1);
            employee.Salary = 5000M;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _sqlServerConfig);
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
            var employee = context.Database.ExecuteReaderFirst<EmployeeWithDateAndUser>(_sqlServerConfig, x => x.Id == 1);
            employee.Salary = 5000M;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _sqlServerConfig, currentUser: "Sariya Harnpadoungsataya");
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
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _sqlServerConfig, excludeNull:true);
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
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _sqlServerConfig, ignoreVersion: true);
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
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _sqlServerConfig);
            var ex = Assert.Throws<OptimisticLockException>(
                () => context.Database.ExecuteNonQuery(parameter));
            ex.IsNotNull();
        }

        [Fact]
        public async Task Test_update_optimisticLockException_async()
        {
            await using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 4);
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _sqlServerConfig);
            var ex = await Assert.ThrowsAsync<OptimisticLockException>(
                async () => await context.Database.ExecuteNonQueryAsync(parameter));
            ex.IsNotNull();
        }

        [Fact]
        public void Test_update_suppressOptimisticLockException()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 5);
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _sqlServerConfig, suppressOptimisticLockException: true);
            var affected = context.Database.ExecuteNonQuery(parameter);
            affected.Is(0);

        }

        [Fact]
        public void Test_delete_default()
        {
            using var context = Fixture.CreateContext();
            var employee = context.Employees.AsNoTracking().Single(x => x.Id == 6);
            var parameter = new QueryBuilderParameter(employee, SqlKind.Delete, _sqlServerConfig);
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
            var parameter = new QueryBuilderParameter(employee, SqlKind.Delete, _sqlServerConfig, ignoreVersion: true);
            var affected = context.Database.ExecuteNonQuery(parameter);
            affected.Is(1);

        }

        [Fact]
        public async Task Test_soft_delete_async()
        {
            await using var context = Fixture.CreateContext();
            var employee =
                await context.Database.ExecuteReaderFirstAsync<EmployeeWithDateAndUser>(_sqlServerConfig, x => x.Id == 7);
            var parameter = new QueryBuilderParameter(employee, SqlKind.SoftDelete, _sqlServerConfig,
                currentUser: "Sariya Harnpadoungsataya");
            var affected = await context.Database.ExecuteNonQueryAsync(parameter);
            affected.Is(1);

        }

        [Fact]
        public void Test_ExecuteReader()
        {
            using var context = Fixture.CreateContext();
            var results = context.Database.ExecuteReader<Employee>(_sqlServerConfig, x => x.Id > 0);
            results.Count.IsNot(0);

        }

        [Fact]
        public void Test_ExecuteReader2()
        {
            using var context = Fixture.CreateContext();
            var condition = new Employee
                            {
                                Id = 10
                            };
            var sqlPath = Path.Combine("SqlServer", "SelectEmployees.sql");
            var parser = new SqlParser(sqlPath, condition);
            var parserResult = parser.Parse();
            _output.WriteLine(parserResult.DebugSql);
            var results = context.Database.ExecuteReader<Employee>(_sqlServerConfig, parserResult);
            results.Count.IsNot(0);
        }

        [Fact]
        public void Test_ExecuteReaderFirst()
        {
            using var context = Fixture.CreateContext();
            var result = context.Database.ExecuteReaderFirst<Employee>(_sqlServerConfig, x => x.Id == 10);
            result.IsNotNull();
        }

        [Fact]
        public void Test_ExecuteReaderFirst2()
        {
            using var context = Fixture.CreateContext();
            var condition = new Employee
                            {
                                Id = 10
                            };
            var sqlPath = Path.Combine("SqlServer", "SelectEmployees.sql");
            var parser = new SqlParser(sqlPath, condition);
            var parserResult = parser.Parse();
            _output.WriteLine(parserResult.DebugSql);
            var result = context.Database.ExecuteReaderFirst<Employee>(_sqlServerConfig, parserResult);
            result.IsNotNull();
        }

        [Fact]
        public async Task Test_ExecuteReader_async()
        {
            await using var context = Fixture.CreateContext();

            var results = await context.Database.ExecuteReaderAsync<Employee>(_sqlServerConfig, x => x.Id > 0);
            results.Count.IsNot(0);
        }

        [Fact]
        public async Task Test_ExecuteReader2_async()
        {
            await using var context = Fixture.CreateContext();

            var condition = new Employee
                            {
                                Id = 10
                            };
            var sqlPath = Path.Combine("SqlServer", "SelectEmployees.sql");
            var parser = new SqlParser(sqlPath, condition);
            var parserResult = parser.Parse();
            _output.WriteLine(parserResult.DebugSql);
            var results = await context.Database.ExecuteReaderAsync<Employee>(_sqlServerConfig, parserResult);
            results.Count.IsNot(0);
        }


        [Fact]
        public async Task Test_ExecuteReaderFirst_async()
        {
            await using var context = Fixture.CreateContext();
            var result = await context.Database.ExecuteReaderFirstAsync<Employee>(_sqlServerConfig, x => x.Id == 10);
            result.IsNotNull();
        }

        [Fact]
        public async Task Test_ExecuteReaderFirst2_async()
        {
            await using var context = Fixture.CreateContext();
            var condition = new Employee
                            {
                                Id = 10
                            };
            var sqlPath = Path.Combine("SqlServer", "SelectEmployees.sql");
            var parser = new SqlParser(sqlPath, condition);
            var parserResult = parser.Parse();
            _output.WriteLine(parserResult.DebugSql);
            var result = await context.Database.ExecuteReaderFirstAsync<Employee>(_sqlServerConfig, parserResult);
            result.IsNotNull();
        }

        [Fact]
        public void Test_Count()
        {
            using var context = Fixture.CreateContext();
            var result = context.Database.ExecuteScalar<int>($"SELECT COUNT(*) CNT FROM EMP WHERE ID > {0}");
            result.IsNot(0);
        }

        [Fact]
        public void Test_Count2()
        {
            using var context = Fixture.CreateContext();
            var condition = new Employee
                            {
                                Id = 10
                            };
            var sqlPath = Path.Combine("SqlServer", "CountEmployees.sql");
            var parser = new SqlParser(sqlPath, condition);
            var parserResult = parser.Parse();
            _output.WriteLine(parserResult.DebugSql);
            var result = context.Database.ExecuteScalar<int>(parserResult);
            result.IsNot(0);
        }


        [Fact]
        public async Task Test_Count_async()
        {
            await using var context = Fixture.CreateContext();
            var result = await context.Database.ExecuteScalarAsync<int>($"SELECT COUNT(*) CNT FROM EMP WHERE ID > {0}");
            result.IsNot(0);

        }

        [Fact]
        public async Task Test_Count2_async()
        {
            await using var context = Fixture.CreateContext();
            var condition = new Employee
                            {
                                Id = 10
                            };
            var sqlPath = Path.Combine("SqlServer", "CountEmployees.sql");
            var parser = new SqlParser(sqlPath, condition);
            var parserResult = parser.Parse();
            _output.WriteLine(parserResult.DebugSql);
            var result = await context.Database.ExecuteScalarAsync<int>(parserResult);
            result.IsNot(0);

        }

    }

}
