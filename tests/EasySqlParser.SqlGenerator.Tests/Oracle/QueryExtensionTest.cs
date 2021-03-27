using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasySqlParser.Configurations;
using Oracle.ManagedDataAccess.Client;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.SqlGenerator.Tests.Oracle
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
                DbConnectionKind.Oracle,
                () => new OracleParameter()
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

        [Fact]
        public void Test_insert_sequence()
        {
            var series = new MetalGearSeries
                         {
                             Name = "METAL GEAR2 SOLID SNAKE",
                             ReleaseDate = new DateTime(1990, 7, 20),
                             Platform = "MSX2"
                         };
            var parameter = new QueryBuilderParameter(series, SqlKind.Insert, _mockConfig);
            var affected = _fixture.Connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            _output.WriteLine(series.GetDebugString());
        }

        [Fact]
        public async Task Test_insert_sequence_async()
        {
            var series = new MetalGearSeries
                         {
                             Name = "METAL GEAR SOLID",
                             ReleaseDate = new DateTime(1998, 9, 3),
                             Platform = "PlayStation"
                         };
            var parameter = new QueryBuilderParameter(series, SqlKind.Insert, _mockConfig);
            var affected = await _fixture.Connection.ExecuteNonQueryByQueryBuilderAsync(parameter);
            affected.Is(1);
            _output.WriteLine(series.GetDebugString());
        }

        [Fact]
        public void Test_multiple_sequence()
        {
            var employee = new EmployeeSeq
                           {
                               Name = "John Doe"
                           };
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _mockConfig);
            var affected = _fixture.Connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            employee.LongCol.Is(1L);
            employee.StringCol.Is("T000001");
        }

    }
}
