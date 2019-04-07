using System.Collections.Generic;
using EasySqlParser.Configurations;
using EasySqlParser.Internals;
using EasySqlParser.Internals.Dialect.Transformer;
using Example.Test.Xunit;
using Xunit;

namespace EasySqlParser.Tests.Internals.Dialect.Transformer
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.dialect
    //   class      SqlitePagingTransformerTest
    // https://github.com/domaframework/doma
    public class SqlitePagingTransformerTest
    {
        private readonly SqlParserConfig _config;
        public SqlitePagingTransformerTest()
        {
            _config = ConfigContainer.CreateConfigForTest(DbConnectionKind.SQLite, nameof(SqlitePagingTransformerTest));
        }

        [Fact]
        public void testOffsetLimit()
        {
            var expected = "select * from emp order by emp.id limit 10 offset 5";
            var transformer = new SqlitePagingTransformer(5, 10, null);
            var parser = new DomaSqlParser("select * from emp order by emp.id");
            var node = transformer.Transform(parser.Parse());
            var parameters = new List<ParameterEmulator>();
            var builder = new DomaSqlBuilder(node, parameters, _config);
            var result = builder.Build();
            result.ParsedSql.Is(expected);
        }

        [Fact]
        public void testOffsetLimit_forUpdate()
        {
            var expected = "select * from emp order by emp.id  limit 10 offset 5 for update";
            var transformer = new SqlitePagingTransformer(5, 10, null);
            var parser = new DomaSqlParser("select * from emp order by emp.id for update");
            var node = transformer.Transform(parser.Parse());
            var parameters = new List<ParameterEmulator>();
            var builder = new DomaSqlBuilder(node, parameters, _config);
            var result = builder.Build();
            result.ParsedSql.Is(expected);
        }

        [Fact]
        public void testOffsetOnly()
        {
            var expected = "select * from emp order by emp.id limit 9223372036854775807 offset 5";
            var transformer = new SqlitePagingTransformer(5, -1, null);
            var parser = new DomaSqlParser("select * from emp order by emp.id");
            var node = transformer.Transform(parser.Parse());
            var parameters = new List<ParameterEmulator>();
            var builder = new DomaSqlBuilder(node, parameters, _config);
            var result = builder.Build();
            result.ParsedSql.Is(expected);
        }

        [Fact]
        public void testLimitOnly()
        {
            var expected = "select * from emp order by emp.id limit 10 offset 0";
            var transformer = new SqlitePagingTransformer(-1, 10, null);
            var parser = new DomaSqlParser("select * from emp order by emp.id");
            var node = transformer.Transform(parser.Parse());
            var parameters = new List<ParameterEmulator>();
            var builder = new DomaSqlBuilder(node, parameters, _config);
            var result = builder.Build();
            result.ParsedSql.Is(expected);
        }
    }
}
