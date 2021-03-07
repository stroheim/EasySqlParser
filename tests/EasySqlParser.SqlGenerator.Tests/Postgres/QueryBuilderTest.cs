using System;
using System.Collections.Generic;
using System.Text;
using EasySqlParser.Configurations;
using Npgsql;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.SqlGenerator.Tests.Postgres
{
    public class QueryBuilderTest
    {
        private readonly ITestOutputHelper _output;
        public QueryBuilderTest(ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.PostgreSql,
                () => new NpgsqlParameter()
            );

            _output = output;
        }

        [Fact]
        public void Test_IdentityOnly()
        {
            var employee = new EmployeeIdentity
                           {
                               Name = "John Doe"
                           };
            var localConfig = new MockConfig(QueryBehavior.IdentityOnly, _output.WriteLine);
            //localConfig.WriteIndented = true;
            var parameter = new QueryBuilderParameter<EmployeeIdentity>(employee, SqlKind.Insert, localConfig);
            var builder = QueryBuilder<EmployeeIdentity>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            if (!localConfig.WriteIndented)
            {
                builder.ParsedSql.Is(@"INSERT INTO ""EMP_IDENTITY"" (""NAME"", ""VERSION"") VALUES (:Name, :VersionNo) RETURNING ""ID""");
            }
            builder.DbDataParameters.Count.Is(2);
        }

        [Fact]
        public void Test_IdentityAllColumns()
        {
            var employee = new EmployeeIdentity
                           {
                               Name = "John Doe"
                           };
            var localConfig = new MockConfig(QueryBehavior.AllColumns, _output.WriteLine);
            localConfig.WriteIndented = true;
            var parameter = new QueryBuilderParameter<EmployeeIdentity>(employee, SqlKind.Insert, localConfig);
            var builder = QueryBuilder<EmployeeIdentity>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            if (!localConfig.WriteIndented)
            {
                builder.ParsedSql.Is(@"INSERT INTO ""EMP_IDENTITY"" (""NAME"", ""VERSION"") VALUES (:Name, :VersionNo) RETURNING ""ID"", ""NAME"", ""VERSION""");
            }
            builder.DbDataParameters.Count.Is(2);
            _output.WriteLine(builder.ParsedSql);

        }

    }
}
