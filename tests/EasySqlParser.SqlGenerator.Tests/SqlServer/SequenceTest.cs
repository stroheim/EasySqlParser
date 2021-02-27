using System;
using System.Collections.Generic;
using System.Text;
using EasySqlParser.Configurations;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.SqlGenerator.Tests.SqlServer
{
    public class SequenceTest : IClassFixture<DatabaseFixture>
    {
        public SequenceTest(DatabaseFixture fixture, ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );

            Fixture = fixture;
            _mockConfig = new MockConfig(QueryBehavior.AllColumns, output.WriteLine);
            _mockConfig.WriteIndented = true;
            _output = output;

        }
        public DatabaseFixture Fixture { get; }
        private readonly MockConfig _mockConfig;
        private readonly ITestOutputHelper _output;

        [Fact]
        public void Test_insert_default()
        {
            using var connection = Fixture.Connection;

            var series = new MetalGearSeries
                         {
                             Name = "METAL GEAR2 SOLID SNAKE",
                             ReleaseDate = new DateTime(1990, 7, 20),
                             Platform = "MSX2"
                         };

            var parameter = new QueryBuilderParameter<MetalGearSeries>(series, SqlKind.Insert, _mockConfig);
            var affected = connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            series.Id.IsNot(0);
            _output.WriteLine($"{series.Id}");
            _output.WriteLine($"{series.CreateDate}");

        }

        [Fact]
        public void Test_multiple_sequence()
        {
            using var connection = Fixture.Connection;
            var employee = new EmployeeSeq
                           {
                               Name = "John Doe"
                           };
            var parameter = new QueryBuilderParameter<EmployeeSeq>(employee, SqlKind.Insert, _mockConfig);
            var affected = connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            employee.ByteCol.Is((byte)1);
            employee.ShortCol.Is((short)1);
            employee.IntCol.Is(1);
            employee.LongCol.Is(1L);
            employee.DecimalCol.Is(1M);
            employee.StringCol.Is("T000001");
        }

    }
}
