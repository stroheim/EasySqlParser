using System;
using System.Collections.Generic;
using System.Text;
using EasySqlParser.Configurations;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.SqlGenerator.Tests.SqlServer
{
    public class IdentityTest : IClassFixture<DatabaseFixture>
    {
        public IdentityTest(DatabaseFixture fixture, ITestOutputHelper output)
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
        private readonly MockConfig _mockConfig;
        private readonly ITestOutputHelper _output;

        public DatabaseFixture Fixture { get; }

        [Fact]
        public void Test_insert_default()
        {
            using var connection = Fixture.Connection;

            var characters = new Characters
                             {
                                 Name = "Roy Cambell",
                                 Height = 185
                             };
            var parameter = new QueryBuilderParameter<Characters>(characters, SqlKind.Insert, _mockConfig);
            var affected = connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            characters.Id.IsNot(0);
            _output.WriteLine($"{characters.Id}");
            _output.WriteLine($"{characters.CreateDate}");
        }
    }
}
