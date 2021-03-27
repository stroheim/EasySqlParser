using System;
using System.Collections.Generic;
using System.Text;
using EasySqlParser.Configurations;
using IBM.Data.DB2.Core;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.SqlGenerator.Tests.Db2
{
    public class QueryBuilderTest
    {
        private readonly ITestOutputHelper _output;
        public QueryBuilderTest(ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.DB2,
                () => new DB2Parameter()
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
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, localConfig);
            var builder = QueryBuilder.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            if (!localConfig.WriteIndented)
            {
                builder.ParsedSql.Is(@"SELECT t_.""ID"" FROM FINAL TABLE (INSERT INTO ""EMP_IDENTITY"" (""NAME"", ""VERSION"") VALUES (@Name, @VersionNo)) AS t_");
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
            //localConfig.WriteIndented = true;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, localConfig);
            var builder = QueryBuilder.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            if (!localConfig.WriteIndented)
            {
                builder.ParsedSql.Is(@"SELECT t_.""ID"", t_.""NAME"", t_.""VERSION"" FROM FINAL TABLE (INSERT INTO ""EMP_IDENTITY"" (""NAME"", ""VERSION"") VALUES (@Name, @VersionNo)) AS t_");
            }
            builder.DbDataParameters.Count.Is(2);

        }

        [Fact]
        public void Test_IdentityOrAllColumns()
        {
            var employee = new Employee
                           {
                               Id = 1,
                               Name = "John Doe"
                           };
            var localConfig = new MockConfig(QueryBehavior.IdentityOrAllColumns, _output.WriteLine);
            localConfig.WriteIndented = true;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, localConfig);
            var builder = QueryBuilder.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            _output.WriteLine(builder.ParsedSql);
            builder.DbDataParameters.Count.Is(4);

        }

        [Fact]
        public void Test_AllColumns()
        {
            var employee = new EmployeeWithDateAndUser
                           {
                               Id = 1,
                               Name = "John Doe"
                           };
            var localConfig = new MockConfig(QueryBehavior.AllColumns, _output.WriteLine);
            localConfig.WriteIndented = true;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, localConfig);
            var builder = QueryBuilder.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            _output.WriteLine(builder.ParsedSql);

        }

    }
}
