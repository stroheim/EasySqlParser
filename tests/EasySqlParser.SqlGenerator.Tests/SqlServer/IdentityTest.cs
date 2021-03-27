using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasySqlParser.Configurations;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.SqlGenerator.Tests.SqlServer
{
    public class IdentityTest : IClassFixture<IdentityFixture>
    {
        public IdentityTest(IdentityFixture fixture, ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );

            _Fixture = fixture;
            _mockConfig = new MockConfig(QueryBehavior.AllColumns, output.WriteLine);
            _mockConfig.WriteIndented = true;
            _output = output;

        }
        private readonly MockConfig _mockConfig;
        private readonly ITestOutputHelper _output;

        private readonly IdentityFixture _Fixture;

        [Fact]
        public void Test_insert_default()
        {

            var characters = new Characters
                             {
                                 Name = "Roy Cambell",
                                 Height = 185
                             };
            var parameter = new QueryBuilderParameter(characters, SqlKind.Insert, _mockConfig);
            var affected = _Fixture.Connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            characters.Id.IsNot(0);
            _output.WriteLine($"{characters.Id}");
            _output.WriteLine($"{characters.CreateDate}");
        }

        [Fact]
        public void Test_insert_default_select_identity_only()
        {
            var characters = new Characters
                             {
                                 Name = "Naomi Hunter",
                                 Height = 165
                             };
            var localConfig = new MockConfig(QueryBehavior.IdentityOnly, _output.WriteLine);
            localConfig.WriteIndented = true;
            var parameter = new QueryBuilderParameter(characters, SqlKind.Insert, localConfig);
            var affected = _Fixture.Connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            characters.Id.IsNot(0);
            _output.WriteLine($"{characters.Id}");
            _output.WriteLine($"{characters.CreateDate}");
        }

        [Fact]
        public void Test_update_default()
        {
            var characters = _Fixture.Connection.ExecuteReaderByQueryBuilder<Characters>(x => x.Id == 1, _mockConfig).Single();
            characters.Name = "John Doe";
            var parameter = new QueryBuilderParameter(characters, SqlKind.Update, _mockConfig);
            var affected = _Fixture.Connection.ExecuteNonQueryByQueryBuilder(parameter);
            affected.Is(1);
            characters.VersionNo.Is(2L);
            _output.WriteLine($"{characters.Id}");
            _output.WriteLine($"{characters.Name}");
            _output.WriteLine($"{characters.Height}");
            _output.WriteLine($"{characters.VersionNo}");

        }
    }
}
