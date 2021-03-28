﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasySqlParser.Configurations;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.SqlGenerator.Tests.SqlServer
{
    public class SequenceTest : IClassFixture<SequenceFixture>
    {
        public SequenceTest(SequenceFixture fixture, ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );

            _fixture = fixture;
            _mockConfig = new MockConfig(QueryBehavior.AllColumns, output.WriteLine);
            _mockConfig.WriteIndented = true;
            _output = output;

        }

        private readonly SequenceFixture _fixture;
        private readonly MockConfig _mockConfig;
        private readonly ITestOutputHelper _output;

        [Fact]
        public void Test_insert_default()
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
            series.Id.IsNot(0);
            _output.WriteLine($"{series.Id}");
            _output.WriteLine($"{series.CreateDate}");

        }

        [Fact]
        public async Task Test_insert_default_async()
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
            series.Id.IsNot(0);
            _output.WriteLine($"{series.Id}");
            _output.WriteLine($"{series.CreateDate}");
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
            employee.ByteCol.Is((byte)1);
            employee.ShortCol.Is((short)1);
            employee.IntCol.Is(1);
            employee.LongCol.Is(1L);
            employee.DecimalCol.Is(1M);
            employee.StringCol.Is("T000001");
        }

        [Fact]
        public async Task Test_multiple_sequence_async()
        {
            var employee = new EmployeeSeqForAsync
            {
                               Name = "Jane Doe"
                           };
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _mockConfig);
            var affected = await _fixture.Connection.ExecuteNonQueryByQueryBuilderAsync(parameter);
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