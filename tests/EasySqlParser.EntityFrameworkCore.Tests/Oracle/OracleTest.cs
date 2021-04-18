using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySqlParser.Configurations;
using EasySqlParser.EntityFrameworkCore.Extensions;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Enums;
using Oracle.ManagedDataAccess.Client;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.EntityFrameworkCore.Tests.Oracle
{
    public class OracleTest : IClassFixture<DbContextFixture>
    {
        public DbContextFixture Fixture { get; }
        private readonly OracleConfig _config;
        private readonly ITestOutputHelper _output;
        public OracleTest(DbContextFixture fixture, ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.Oracle,
                () => new OracleParameter()
            );

            Fixture = fixture;
            _output = output;
            _config = new OracleConfig(fixture.CreateContext(), output);

        }

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
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _config);
            var affected = context.Database.ExecuteNonQuery(parameter);
            affected.Is(1);
        }

        [Fact]
        public async Task Test_insert_default_async()
        {
            await using var context = Fixture.CreateContext();
            var employee = new Employee
                           {
                               Id = 12,
                               Name = "Scott Rodgers"
                           };
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _config);
            var affected = await context.Database.ExecuteNonQueryAsync(parameter);
            affected.Is(1);
        }

        [Fact]
        public void Test_insert_identity()
        {
            using var context = Fixture.CreateContext();
            var characters = new Characters
                             {
                                 Name = "Roy Cambell",
                                 Height = 185
                             };
            var parameter = new QueryBuilderParameter(characters, SqlKind.Insert, _config);
            var affected = context.Database.ExecuteNonQuery(parameter);
            affected.Is(1);
            _output.WriteLine(characters.GetDebugString());
        }

        [Fact]
        public async Task Test_insert_identity_async()
        {
            await using var context = Fixture.CreateContext();
            var characters = new Characters
                             {
                                 Name = "Naomi Hunter",
                                 Height = 165
                             };
            var parameter = new QueryBuilderParameter(characters, SqlKind.Insert, _config);
            var affected = await context.Database.ExecuteNonQueryAsync(parameter);
            affected.Is(1);

        }

        [Fact]
        public void Test_insert_sequence()
        {
            using var context = Fixture.CreateContext();
            var series = new MetalGearSeries
                         {
                             Name = "METAL GEAR2 SOLID SNAKE",
                             ReleaseDate = new DateTime(1990, 7, 20),
                             Platform = "MSX2"
                         };
            var parameter = new QueryBuilderParameter(series, SqlKind.Insert, _config);
            var affected = context.Database.ExecuteNonQuery(parameter);
            affected.Is(1);
            _output.WriteLine(series.GetDebugString());

        }

        [Fact]
        public async Task Test_insert_sequence_async()
        {
            await using var context = Fixture.CreateContext();

            var series = new MetalGearSeries
                         {
                             Name = "METAL GEAR SOLID",
                             ReleaseDate = new DateTime(1998, 9, 3),
                             Platform = "PlayStation"
                         };
            var parameter = new QueryBuilderParameter(series, SqlKind.Insert, _config);
            var affected = await context.Database.ExecuteNonQueryAsync(parameter);
            affected.Is(1);
            _output.WriteLine(series.GetDebugString());

        }

        [Fact]
        public void Test_multiple_sequence()
        {
            using var context = Fixture.CreateContext();
            var employee = new EmployeeSeq
                           {
                               Name = "John Doe"
                           };
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _config);
            var affected = context.Database.ExecuteNonQuery(parameter);
            affected.Is(1);
            employee.LongCol.Is(1L);
            employee.StringCol.Is("T000001");

        }
    }
}
