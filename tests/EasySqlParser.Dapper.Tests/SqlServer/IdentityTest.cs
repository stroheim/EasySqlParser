using System.Threading.Tasks;
using Dapper;
using EasySqlParser.Configurations;
using EasySqlParser.Dapper.Extensions;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Enums;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.Dapper.Tests.SqlServer
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
            var affected = _Fixture.Connection.Execute(parameter);
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
            var parameter = new QueryBuilderParameter(characters, SqlKind.Insert, localConfig);
            var affected = _Fixture.Connection.Execute(parameter);
            affected.Is(1);
            characters.Id.IsNot(0);
            _output.WriteLine($"{characters.Id}");
            _output.WriteLine($"{characters.CreateDate}");
        }

        [Fact]
        public async Task Test_insert_default_select_identity_only_async()
        {
            var characters = new Characters
                             {
                                 Name = "Mei Ling",
                                 Height = 160
                             };
            var localConfig = new MockConfig(QueryBehavior.IdentityOnly, _output.WriteLine);
            var parameter = new QueryBuilderParameter(characters, SqlKind.Insert, localConfig);
            var affected = await _Fixture.Connection.ExecuteAsync(parameter);
            affected.Is(1);
            characters.Id.IsNot(0);
            _output.WriteLine($"{characters.Id}");
            _output.WriteLine($"{characters.CreateDate}");
        }

        [Fact]
        public void Test_update_default()
        {
            var characters = _Fixture.Connection.ExecuteReaderFirst<Characters>(_mockConfig, x => x.Id == 1);
            characters.Name = "John Doe";
            var parameter = new QueryBuilderParameter(characters, SqlKind.Update, _mockConfig);
            var affected = _Fixture.Connection.Execute(parameter);
            affected.Is(1);
            characters.VersionNo.Is(2L);
            _output.WriteLine($"{characters.Id}");
            _output.WriteLine($"{characters.Name}");
            _output.WriteLine($"{characters.Height}");
            _output.WriteLine($"{characters.VersionNo}");

        }
    }
}
