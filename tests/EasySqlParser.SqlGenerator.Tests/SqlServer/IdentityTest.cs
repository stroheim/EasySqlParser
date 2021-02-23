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

        }
        private readonly MockConfig _mockConfig;

        public DatabaseFixture Fixture { get; }

        [Fact]
        public void Test_insert_default()
        {
            using var connection = Fixture.Connection;

            var characters = new Characters
                             {
                                 Name = "Roy Cambell",
                                 Height = 185,
                                 CreateDate = DateTime.Now
                             };
            var parameter = new QueryBuilderParameter<Characters>(characters, SqlKind.Insert, _mockConfig);
            var affected = connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
        }
    }
}
