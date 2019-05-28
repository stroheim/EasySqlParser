using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Text;
using EasySqlParser.Configurations;
using EasySqlParser.Internals;
using Example.Test.Xunit;
using Xunit;

namespace EasySqlParser.Tests.Internals
{
    public class OdbcSqlParserTest
    {
        private readonly SqlParserConfig _config;
        public OdbcSqlParserTest()
        {
            ConfigContainer.AddAdditional(
                DbConnectionKind.Odbc,
                () => new OdbcParameter(),
                nameof(OdbcSqlParserTest)
            );
            _config = ConfigContainer.AdditionalConfigs[nameof(OdbcSqlParserTest)];
        }

        [Fact]
        public void testBindVariable()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
                           { Name = "name", ParameterType = typeof(string), ParameterValue = "hoge" });
            parameters.Add(new ParameterEmulator
                           { Name = "salary", ParameterType = typeof(int), ParameterValue = 10000 });
            var testSql = "select * from aaa where ename = /*name*/'aaa' and sal = /*salary*/-2000";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters, _config);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where ename = ? and sal = ?");
            result.DebugSql.Is("select * from aaa where ename = 'hoge' and sal = 10000");
            result.DbDataParameters.Count.Is(2);
            result.DbDataParameters[0].ParameterName.Is("?name");
            result.DbDataParameters[0].Value.Is("hoge");
            result.DbDataParameters[1].ParameterName.Is("?salary");
            result.DbDataParameters[1].Value.Is(10000);
        }

        [Fact]
        public void testBindVariable_in()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
                           { Name = "name", ParameterType = typeof(string[]), ParameterValue = new[] { "hoge", "foo" } });
            var testSql = "select * from aaa where ename in /*name*/('aaa', 'bbb')";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters, _config);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where ename in (?, ?)");
            result.DebugSql.Is("select * from aaa where ename in ('hoge', 'foo')");
            result.DbDataParameters.Count.Is(2);
            result.DbDataParameters[0].ParameterName.Is("?name1");
            result.DbDataParameters[0].Value.Is("hoge");
            result.DbDataParameters[1].ParameterName.Is("?name2");
            result.DbDataParameters[1].Value.Is("foo");
        }
    }
}
