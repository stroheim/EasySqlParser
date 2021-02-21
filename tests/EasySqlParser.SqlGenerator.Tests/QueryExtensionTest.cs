using System.Linq;
using System.Threading.Tasks;
using EasySqlParser.Configurations;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.SqlGenerator.Tests
{
    public class QueryExtensionTest : IClassFixture<DatabaseFixture>
    {
        private readonly MockConfig _mockConfig;
        public QueryExtensionTest(DatabaseFixture fixture, ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );

            Fixture = fixture;
            _output = output;
            _mockConfig = new MockConfig(QueryBehavior.None, _output.WriteLine);
        }

        public DatabaseFixture Fixture { get; }
        private readonly ITestOutputHelper _output;

        [Fact]
        public void Test_insert_default()
        {
            using var connection = Fixture.Connection;
            var employee = new Employee
                           {
                               Id = 11,
                               Name = "Jane Doe"
                           };

            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Insert, _mockConfig);
            var affected = connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);

        }

        [Fact]
        public async Task Test_insert_default_async()
        {
            await using var connection = Fixture.Connection;
            var employee = new Employee
                           {
                               Id = 12,
                               Name = "Terri Duffy"
            };

            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Insert, _mockConfig);
            var affected = await connection.ExecuteNonQueryByQueryBuilderAsync(parameter);
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
            using var connection = Fixture.Connection;
            var affected = connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            var instance =
                connection.ExecuteReaderByQueryBuilder<Employee>((x) => x.Id == employee.Id,
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
            await using var connection = Fixture.Connection;
            var affected =
                await connection.ExecuteNonQueryByQueryBuilderAsync(parameter);
            affected.Is(1);
            var instance =
                connection.ExecuteReaderByQueryBuilder<Employee>((x) => x.Id == employee.Id,
                    _mockConfig).Single();
            instance.Name.IsNull();
        }

        [Fact]
        public void Test_update_default()
        {
            using var connection = Fixture.Connection;
            var employee =
                connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 1, _mockConfig)
                    .Single();

            employee.Salary = 5000M;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, _mockConfig);
            var affected = connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            employee.VersionNo.Is(2L);
            var instance=
                connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 1, _mockConfig)
                    .Single();
            instance.Salary.Is(5000M);
        }

        [Fact]
        public void Test_update_excludeNull()
        {
            using var connection = Fixture.Connection;
            var employee =
                connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 2, _mockConfig)
                    .Single();
            employee.Name = null;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, _mockConfig, excludeNull: true);
            var affected = connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            var instance =
                connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 2, _mockConfig)
                    .Single();
            instance.Name.Is("Rob Walters");

        }

        [Fact]
        public void Test_update_ignoreVersion()
        {
            using var connection = Fixture.Connection;
            var employee =
                connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 3, _mockConfig)
                    .Single();
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, _mockConfig, ignoreVersion: true);
            var affected = connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            var instance =
                connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 3, _mockConfig)
                    .Single();
            instance.VersionNo.Is(100L);

        }

        [Fact]
        public void Test_update_optimisticLockException()
        {
            using var connection = Fixture.Connection;
            var employee =
                connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 4, _mockConfig)
                    .Single();
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, _mockConfig);
            var ex = Assert.Throws<OptimisticLockException>(
                () => connection.ExecuteNonQueryByQueryBuilder(parameter));
            ex.IsNotNull();

        }

        [Fact]
        public void Test_update_suppressOptimisticLockException()
        {
            using var connection = Fixture.Connection;
            var employee =
                connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 5, _mockConfig)
                    .Single();
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, _mockConfig, suppressOptimisticLockException: true);
            var affected = connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(0);

        }

        [Fact]
        public void Test_delete_default()
        {
            using var connection = Fixture.Connection;
            var employee =
                connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 6, _mockConfig)
                    .Single();
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Delete, _mockConfig);
            var affected = connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            var cnt = connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 6, _mockConfig)
                .Count();
            cnt.Is(0);

        }

        [Fact]
        public void Test_delete_ignoreVersion()
        {
            using var connection = Fixture.Connection;
            var employee =
                connection.ExecuteReaderByQueryBuilder<Employee>(x => x.Id == 7, _mockConfig)
                    .Single();
            employee.VersionNo = 100L;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Delete, _mockConfig, ignoreVersion: true);
            var affected = connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);

        }
    }

   
}
