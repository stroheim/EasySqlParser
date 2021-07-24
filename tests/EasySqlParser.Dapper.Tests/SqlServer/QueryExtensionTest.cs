using System.Threading.Tasks;
using EasySqlParser.Configurations;
using EasySqlParser.Dapper.Extensions;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Enums;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.Dapper.Tests.SqlServer
{
    public class QueryExtensionTest : IClassFixture<QueryExtensionFixture>
    {
        private readonly MockConfig _mockConfig;
        public QueryExtensionTest(QueryExtensionFixture fixture, ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );

            _fixture = fixture;
            _mockConfig = new MockConfig(QueryBehavior.AllColumns, output.WriteLine);
        }

        private readonly QueryExtensionFixture _fixture;

        [Fact]
        public void Test_insert_default()
        {
            var employee = new Employee
                           {
                               Id = 11,
                               Name = "Jane Doe"
                           };

            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _mockConfig);
            var affected = _fixture.Connection.Execute(parameter);
            affected.Is(1);

        }

        [Fact]
        public void Test_insert_with_date_and_user()
        {
            var employee = new EmployeeWithDateAndUser
                           {
                               Id = 11,
                               Name = "Jane Doe"
                           };
            employee.CreateUser = "Sariya Harnpadoungsataya";
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _mockConfig);
            var affected = _fixture.Connection.Execute(parameter);
            affected.Is(1);

        }

        [Fact]
        public async Task Test_insert_default_async()
        {
            var employee = new Employee
                           {
                               Id = 12,
                               Name = "Terri Duffy"
            };

            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _mockConfig);
            var affected = await _fixture.Connection.ExecuteAsync(parameter);
            affected.Is(1);
        }


        [Fact]
        public void Test_insert_excludeNull()
        {
            var employee = new Employee
                           {
                               Id = 13,
                               Salary = 100M
                           };
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _mockConfig, excludeNull: true);
            var affected = _fixture.Connection.Execute(parameter);
            affected.Is(1);
            var instance =
                _fixture.Connection.ExecuteReaderSingle<Employee>(_mockConfig, x => x.Id == employee.Id);
            instance.Name.IsNull();
        }

        [Fact]
        public async Task Test_insert_excludeNull_async()
        {
            var employee = new Employee
                           {
                               Id = 14,
                               Salary = 100M
                           };
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _mockConfig, excludeNull: true);
            var affected =
                await _fixture.Connection.ExecuteAsync(parameter);
            affected.Is(1);
            var instance =
                _fixture.Connection.ExecuteReaderSingle<Employee>(_mockConfig, x => x.Id == employee.Id);
            instance.Name.IsNull();
        }

        [Fact]
        public void Test_update_default()
        {
            var employee =
                _fixture.Connection.ExecuteReaderSingle<Employee>(_mockConfig, x => x.Id == 1);

            employee.Salary = 5000M;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _mockConfig);
            var affected = _fixture.Connection.Execute(parameter);
            affected.Is(1);
            employee.VersionNo.Is(2L);

            var instance =
                _fixture.Connection.ExecuteReaderSingle<Employee>(_mockConfig, x => x.Id == 1);
            instance.Salary.Is(5000M);
        }

        [Fact]
        public void Test_update_with_date_and_user()
        {
            var employee =
                _fixture.Connection.ExecuteReaderSingle<EmployeeWithDateAndUser>(_mockConfig, x => x.Id == 1);
            employee.Salary = 5000M;
            employee.UpdateUser = "Sariya Harnpadoungsataya";
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _mockConfig);
            var affected = _fixture.Connection.Execute(parameter);
            affected.Is(1);
            employee.VersionNo.Is(2L);

        }

        [Fact]
        public void Test_update_excludeNull()
        {
            var employee =
                _fixture.Connection.ExecuteReaderSingle<Employee>(_mockConfig, x => x.Id == 2);
            employee.Name = null;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _mockConfig, excludeNull: true);
            var affected = _fixture.Connection.Execute(parameter);
            affected.Is(1);
            var instance =
                _fixture.Connection.ExecuteReaderSingle<Employee>(_mockConfig, x => x.Id == 2);
            instance.Name.Is("Rob Walters");

        }

        [Fact]
        public void Test_update_ignoreVersion()
        {
            var employee =
                _fixture.Connection.ExecuteReaderSingle<Employee>(_mockConfig, x => x.Id == 3);
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _mockConfig, ignoreVersion: true);
            var affected = _fixture.Connection.Execute(parameter);
            affected.Is(1);
            var instance =
                _fixture.Connection.ExecuteReaderSingle<Employee>(_mockConfig, x => x.Id == 3);
            instance.VersionNo.Is(100L);

        }

        [Fact]
        public void Test_update_optimisticLockException()
        {
            var employee =
                _fixture.Connection.ExecuteReaderSingle<Employee>(_mockConfig, x => x.Id == 4);
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _mockConfig);
            var ex = Assert.Throws<OptimisticLockException>(
                () => _fixture.Connection.Execute(parameter));
            ex.IsNotNull();

        }

        [Fact]
        public void Test_update_suppressOptimisticLockException()
        {
            var employee =
                _fixture.Connection.ExecuteReaderSingle<Employee>(_mockConfig, x => x.Id == 5);
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _mockConfig, suppressOptimisticLockException: true);
            var affected = _fixture.Connection.Execute(parameter);
            affected.Is(0);

        }

        [Fact]
        public void Test_delete_default()
        {
            var employee =
                _fixture.Connection.ExecuteReaderSingle<Employee>(_mockConfig, x => x.Id == 6);
            var parameter = new QueryBuilderParameter(employee, SqlKind.Delete, _mockConfig);
            var affected = _fixture.Connection.Execute(parameter);
            affected.Is(1);
            var cnt = _fixture.Connection.GetCount<Employee>(_mockConfig, x => x.Id == 6);
            cnt.Is(0);

        }

        [Fact]
        public void Test_delete_ignoreVersion()
        {
            var employee =
                _fixture.Connection.ExecuteReaderSingle<Employee>(_mockConfig, x => x.Id == 7);
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Delete, _mockConfig, ignoreVersion: true);
            var affected = _fixture.Connection.Execute(parameter);
            affected.Is(1);

        }

        [Fact]
        public void Test_soft_delete()
        {
            var employee =
                _fixture.Connection.ExecuteReaderSingle<EmployeeWithDateAndUser>(_mockConfig, x => x.Id == 7);
            employee.DeleteUser = "Sariya Harnpadoungsataya";
            var parameter = new QueryBuilderParameter(employee, SqlKind.SoftDelete, _mockConfig);
            var affected = _fixture.Connection.Execute(parameter);
            affected.Is(1);
            employee.VersionNo.Is(2L);
        }
    }

   
}
