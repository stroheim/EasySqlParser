using System;
using EasySqlParser.Configurations;
using EasySqlParser.Dapper.Extensions;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Enums;
using EasySqlParser.SqlGenerator.Tests.Db2;
using IBM.Data.DB2.Core;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.Dapper.Tests.Db2
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
            ConfigContainer.AddDefault(
                DbConnectionKind.DB2,
                () => new DB2Parameter()
            );
        }

        [Fact]
        public void Test_insert_default()
        {
            //using var connection = Fixture.Connection;
            var employee = new Employee
                           {
                               Id = 11,
                               Name = "Jane Doe"
                           };
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _mockConfig);
            var affected = _fixture.Connection.Execute(parameter);
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
            var affected = _fixture.Connection.Execute(parameter);
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
            var affected = _fixture.Connection.Execute(parameter);
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
            var affected = _fixture.Connection.Execute(parameter);
            affected.Is(1);
            employee.ShortCol.Is((short)1);
            employee.IntCol.Is(1);
            employee.LongCol.Is(1L);
            employee.DecimalCol.Is(1M);
            employee.StringCol.Is("T000001");
        }

    }
}
