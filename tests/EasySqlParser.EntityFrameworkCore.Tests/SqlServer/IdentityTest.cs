using System;
using System.Threading.Tasks;
using EasySqlParser.Configurations;
using EasySqlParser.EntityFrameworkCore.Extensions;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Enums;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.EntityFrameworkCore.Tests.SqlServer
{
    public class IdentityTest : IClassFixture<IdentityFixture>
    {
        public IdentityTest(IdentityFixture fixture, ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );

            Fixture = fixture;
            _output = output;
            _sqlServerConfig = new SqlServerConfig(fixture.CreateContext(), output);
        }

        public IdentityFixture Fixture { get; }
        private readonly SqlServerConfig _sqlServerConfig;
        private readonly ITestOutputHelper _output;

        [Fact]
        public void Test_insert_default()
        {
            using var context = Fixture.CreateContext();
            var characters = new Characters
                             {
                                 Name = "Roy Cambell",
                                 Height = 185
                             };
            var parameter = new QueryBuilderParameter(characters, SqlKind.Insert, _sqlServerConfig);
            var affected = context.Database.ExecuteNonQuery(parameter);
            affected.Is(1);
            characters.Id.IsNot(0);
            _output.WriteLine($"{characters.Id}");
            _output.WriteLine($"{characters.CreateDate}");
        }
        
        [Fact]
        public void Test_insert_default_select_identity_only()
        {
            using var context = Fixture.CreateContext();
            var characters = new Characters
                             {
                                 Name = "Naomi Hunter",
                                 Height = 165
                             };
            var localConfig = new MockConfig(QueryBehavior.IdentityOnly, _output.WriteLine);
            localConfig.AddCache(context);
            var parameter = new QueryBuilderParameter(characters, SqlKind.Insert, localConfig);
            var affected = context.Database.ExecuteNonQuery(parameter);
            affected.Is(1);
            characters.Id.IsNot(0);
            _output.WriteLine($"{characters.Id}");
            _output.WriteLine($"{characters.CreateDate}");

        }

        [Fact]
        public async Task Test_insert_default_select_identity_only_async()
        {
            await using var context = Fixture.CreateContext();
            var characters = new Characters
                             {
                                 Name = "Mei Ling",
                                 Height = 160
                             };
            var localConfig = new MockConfig(QueryBehavior.IdentityOnly, _output.WriteLine);
            localConfig.AddCache(context);
            var parameter = new QueryBuilderParameter(characters, SqlKind.Insert, localConfig);
            var affected = await context.Database.ExecuteNonQueryAsync(parameter);
            affected.Is(1);
            characters.Id.IsNot(0);
            _output.WriteLine($"{characters.Id}");
            _output.WriteLine($"{characters.CreateDate}");
        }

        [Fact]
        public void Test_update_default()
        {
            using var context = Fixture.CreateContext();
            var characters = context.Database.ExecuteReaderFirst<Characters>(_sqlServerConfig, x => x.Id == 1);
            characters.Name = "John Doe";
            var parameter = new QueryBuilderParameter(characters, SqlKind.Update, _sqlServerConfig);
            var affected = context.Database.ExecuteNonQuery(parameter);
            affected.Is(1);
            characters.VersionNo.Is(2L);
            _output.WriteLine($"{characters.Id}");
            _output.WriteLine($"{characters.Name}");
            _output.WriteLine($"{characters.Height}");
            _output.WriteLine($"{characters.VersionNo}");

        }

        [Fact]
        public async Task Test_update_default_select_nothing_async()
        {
            await using var context = Fixture.CreateContext();
            var localConfig = new MockConfig(QueryBehavior.None, _output.WriteLine);
            localConfig.AddCache(context);
            var characters = new Characters
                             {
                                 Name = "Nastasha Romanenko",
                                 Height = 171
                             };
            var parameter = new QueryBuilderParameter(characters, SqlKind.Insert, localConfig);
            await context.Database.ExecuteNonQueryAsync(parameter);
            characters =
                await context.Database.ExecuteReaderFirstAsync<Characters>(localConfig,
                    x => x.Name == "Nastasha Romanenko");
            characters.CreateDate = DateTime.Now;
            parameter = new QueryBuilderParameter(characters, SqlKind.Update, localConfig);
            var affected = await context.Database.ExecuteNonQueryAsync(parameter);
            affected.Is(1);
            characters.VersionNo.Is(2L);


        }
    }
}
