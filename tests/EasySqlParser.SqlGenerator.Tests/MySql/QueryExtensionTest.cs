using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasySqlParser.Configurations;
using EasySqlParser.SqlGenerator.Enums;
using MySql.Data.MySqlClient;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.SqlGenerator.Tests.MySql
{
    public class QueryExtensionTest : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;
        private readonly ITestOutputHelper _output;
        private readonly MockConfig _mockConfig;

        public QueryExtensionTest(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
            _mockConfig = new MockConfig(QueryBehavior.AllColumns, _output.WriteLine);
            _mockConfig.WriteIndented = true;

            ConfigContainer.AddDefault(
                DbConnectionKind.MySql,
                () => new MySqlParameter()
            );

        }

        [Fact]
        public void Test_insert_default()
        {
            var employee = new Employee
                           {
                               Id = 11,
                               Name = "Jane Doe"
                           };
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _mockConfig);
            var affected = _fixture.Connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            _output.WriteLine(employee.GetDebugString());
        }

        [Fact]
        public async Task Test_insert_default_async()
        {
            var employee = new Employee
                           {
                               Id = 12,
                               Name = "Scott Rodgers"
                           };
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _mockConfig);
            var affected = await _fixture.Connection.ExecuteNonQueryByQueryBuilderAsync(parameter);
            affected.Is(1);
            _output.WriteLine(employee.GetDebugString());
        }

        [Fact]
        public void Test_insert_identity()
        {
            var characters = new Characters
                             {
                                 Name = "Roy Cambell",
                                 Height = 185
                             };
            var parameter = new QueryBuilderParameter(characters, SqlKind.Insert, _mockConfig);
            var affected = _fixture.Connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            _output.WriteLine(characters.GetDebugString());
        }

        [Fact]
        public async Task Test_insert_identity_async()
        {
            var characters = new Characters
                             {
                                 Name = "Naomi Hunter",
                                 Height = 165
                             };
            var parameter = new QueryBuilderParameter(characters, SqlKind.Insert, _mockConfig);
            var affected = await _fixture.Connection.ExecuteNonQueryByQueryBuilderAsync(parameter);
            affected.Is(1);
            _output.WriteLine(characters.GetDebugString());
        }

    }
}
