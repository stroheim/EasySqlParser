using System.Threading.Tasks;
using EasySqlParser.Configurations;
using EasySqlParser.Dapper.Extensions;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Enums;
using Microsoft.Data.Sqlite;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.Dapper.Tests.Sqlite
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
                DbConnectionKind.SQLite,
                () => new SqliteParameter()
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
            _output.WriteLine(characters.GetDebugString());
        }

        [Fact]
        public void Test_insert_identity_select_identity_only()
        {
            var characters = new Characters
                             {
                                 Name = "Mei Ling",
                                 Height = 160
                             };
            var localConfig = new MockConfig(QueryBehavior.IdentityOnly, _output.WriteLine);
            var parameter = new QueryBuilderParameter(characters, SqlKind.Insert, localConfig);
            var affected = _fixture.Connection.Execute(parameter);
            affected.Is(1);
            _output.WriteLine(characters.GetDebugString());
        }

        [Fact]
        public async Task Test_insert_identity_select_identity_only_async()
        {
            var characters = new Characters
                             {
                                 Name = "Nastasha Romanenko",
                                 Height = 171
                             };
            var localConfig = new MockConfig(QueryBehavior.IdentityOnly, _output.WriteLine);
            var parameter = new QueryBuilderParameter(characters, SqlKind.Insert, localConfig);
            var affected = await _fixture.Connection.ExecuteAsync(parameter);
            affected.Is(1);
            _output.WriteLine(characters.GetDebugString());
        }


    }
}
