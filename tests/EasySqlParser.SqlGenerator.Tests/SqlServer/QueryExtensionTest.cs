using System.Linq;
using System.Threading.Tasks;
using EasySqlParser.Configurations;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.SqlGenerator.Tests.SqlServer
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
            _mockConfig = new MockConfig(QueryBehavior.None, output.WriteLine);
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

            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Insert, _mockConfig);
            var affected = _fixture.Connection.ExecuteNonQueryByQueryBuilder(parameter);
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

            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Insert, _mockConfig);
            var affected = await _fixture.Connection.ExecuteNonQueryByQueryBuilderAsync(parameter);
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
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Insert, _mockConfig, excludeNull: true);
            var affected = _fixture.Connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            var instance =
                _fixture.Connection.ExecuteReaderByQueryBuilder<Employee>((x) => x.Id == employee.Id,
                    _mockConfig).Single();
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
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Insert, _mockConfig, excludeNull: true);
            var affected =
                await _fixture.Connection.ExecuteNonQueryByQueryBuilderAsync(parameter);
            affected.Is(1);
            var instance =
                _fixture.Connection.ExecuteReaderByQueryBuilder<Employee>((x) => x.Id == employee.Id,
                    _mockConfig).Single();
            instance.Name.IsNull();
        }

        [Fact]
        public void Test_update_default()
        {
            var employee =
                _fixture.Connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 1, _mockConfig)
                    .Single();

            employee.Salary = 5000M;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, _mockConfig);
            var affected = _fixture.Connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            employee.VersionNo.Is(2L);
            var instance=
                _fixture.Connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 1, _mockConfig)
                    .Single();
            instance.Salary.Is(5000M);
        }

        [Fact]
        public void Test_update_excludeNull()
        {
            var employee =
                _fixture.Connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 2, _mockConfig)
                    .Single();
            employee.Name = null;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, _mockConfig, excludeNull: true);
            var affected = _fixture.Connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            var instance =
                _fixture.Connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 2, _mockConfig)
                    .Single();
            instance.Name.Is("Rob Walters");

        }

        [Fact]
        public void Test_update_ignoreVersion()
        {
            var employee =
                _fixture.Connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 3, _mockConfig)
                    .Single();
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, _mockConfig, ignoreVersion: true);
            var affected = _fixture.Connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            var instance =
                _fixture.Connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 3, _mockConfig)
                    .Single();
            instance.VersionNo.Is(100L);

        }

        [Fact]
        public void Test_update_optimisticLockException()
        {
            var employee =
                _fixture.Connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 4, _mockConfig)
                    .Single();
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, _mockConfig);
            var ex = Assert.Throws<OptimisticLockException>(
                () => _fixture.Connection.ExecuteNonQueryByQueryBuilder(parameter));
            ex.IsNotNull();

        }

        [Fact]
        public void Test_update_suppressOptimisticLockException()
        {
            var employee =
                _fixture.Connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 5, _mockConfig)
                    .Single();
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, _mockConfig, suppressOptimisticLockException: true);
            var affected = _fixture.Connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(0);

        }

        [Fact]
        public void Test_delete_default()
        {
            var employee =
                _fixture.Connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 6, _mockConfig)
                    .Single();
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Delete, _mockConfig);
            var affected = _fixture.Connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            var cnt = _fixture.Connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 6, _mockConfig)
                .Count();
            cnt.Is(0);

        }

        [Fact]
        public void Test_delete_ignoreVersion()
        {
            var employee =
                _fixture.Connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 7, _mockConfig)
                    .Single();
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Delete, _mockConfig, ignoreVersion: true);
            var affected = _fixture.Connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);

        }
    }

   
}
