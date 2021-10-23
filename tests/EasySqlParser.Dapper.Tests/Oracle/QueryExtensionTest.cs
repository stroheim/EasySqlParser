﻿using System;
using System.Threading.Tasks;
using EasySqlParser.Configurations;
using EasySqlParser.Dapper.Extensions;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Enums;
using Oracle.ManagedDataAccess.Client;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.Dapper.Tests.Oracle
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
            var affected = _fixture.Connection.Execute(parameter);
            affected.Is(1);
            var instance = _fixture.Connection.ExecuteReaderFirst<Employee>(_mockConfig, x => x.Id == 11);
            instance.VersionNo.Is(1L);
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
            var affected = await _fixture.Connection.ExecuteAsync(parameter);
            affected.Is(1);
            var instance = await _fixture.Connection.ExecuteReaderFirstAsync<Employee>(_mockConfig, x => x.Id == 12);
            instance.VersionNo.Is(1L);
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
            var instance = _fixture.Connection.ExecuteReaderFirst<Characters>(_mockConfig, x => x.Name == "Roy Cambell");
            instance.VersionNo.Is(1L);
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
            var affected = await _fixture.Connection.ExecuteAsync(parameter);
            affected.Is(1);
            var instance = await _fixture.Connection.ExecuteReaderFirstAsync<Characters>(_mockConfig, x => x.Name == "Naomi Hunter");
            instance.VersionNo.Is(1L);
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
        public async Task Test_insert_sequence_async()
        {
            var series = new MetalGearSeries
                         {
                             Name = "METAL GEAR SOLID",
                             ReleaseDate = new DateTime(1998, 9, 3),
                             Platform = "PlayStation"
                         };
            var parameter = new QueryBuilderParameter(series, SqlKind.Insert, _mockConfig);
            var affected = await _fixture.Connection.ExecuteAsync(parameter);
            affected.Is(1);
            _output.WriteLine(series.GetDebugString());
        }

        [Fact]
        public async Task Test_multiple_sequence_async()
        {
            var employee = new EmployeeSeq
                           {
                               Name = "John Doe"
                           };
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _mockConfig);
            var affected = await _fixture.Connection.ExecuteAsync(parameter);
            affected.Is(1);
            employee.LongCol.Is(1L);
            employee.StringCol.Is("T000001");
        }

    }
}
