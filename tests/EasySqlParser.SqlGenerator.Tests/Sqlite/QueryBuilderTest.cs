﻿using System;
using System.Collections.Generic;
using System.Text;
using EasySqlParser.Configurations;
using Microsoft.Data.Sqlite;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.SqlGenerator.Tests.Sqlite
{
    public class QueryBuilderTest
    {
        private readonly ITestOutputHelper _output;

        public QueryBuilderTest(ITestOutputHelper output)
        {
            _output = output;
            ConfigContainer.AddDefault(
                DbConnectionKind.SQLite,
                () => new SqliteParameter()
            );
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
            _output.WriteLine(builder.ParsedSql);
            if (!localConfig.WriteIndented)
            {
                var sqls = builder.ParsedSql.Split(new[] { "\r\n" }, StringSplitOptions.None);
                sqls[0].Is(@"INSERT INTO ""EMP_IDENTITY"" (""NAME"", ""VERSION"") VALUES (@Name, @VersionNo);");
                sqls[1].Is(@"SELECT ""ID"" FROM ""EMP_IDENTITY"" WHERE rowid = last_insert_rowid();");
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
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, localConfig);
            var builder = QueryBuilder.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            if (!localConfig.WriteIndented)
            {
                var sqls = builder.ParsedSql.Split(new[] { "\r\n" }, StringSplitOptions.None);
                sqls[0].Is(@"INSERT INTO ""EMP_IDENTITY"" (""NAME"", ""VERSION"") VALUES (@Name, @VersionNo);");
                sqls[1].Is(@"SELECT ""ID"", ""NAME"", ""VERSION"" FROM ""EMP_IDENTITY"" WHERE rowid = last_insert_rowid();");
            }
            builder.DbDataParameters.Count.Is(2);
            _output.WriteLine(builder.ParsedSql);

        }

    }
}