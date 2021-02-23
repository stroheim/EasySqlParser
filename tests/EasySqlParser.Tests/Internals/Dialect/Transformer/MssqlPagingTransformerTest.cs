using System.Collections.Generic;
using EasySqlParser.Configurations;
using EasySqlParser.Internals;
using EasySqlParser.Internals.Dialect.Transformer;
using Xunit;

namespace EasySqlParser.Tests.Internals.Dialect.Transformer
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.dialect
    //   class      MssqlPagingTransformerTest
    // https://github.com/domaframework/doma
    public class MssqlPagingTransformerTest
    {
        private readonly SqlParserConfig _config;
        public MssqlPagingTransformerTest()
        {
            _config = ConfigContainer.CreateConfigForTest(DbConnectionKind.SqlServerLegacy, nameof(MssqlPagingTransformerTest));
        }

        [Fact]
        public void testOffsetLimit()
        {
            var expected =
                "select emp.id from emp order by emp.id offset 5 rows fetch next 10 rows only";
            var transformer = new MssqlPagingTransformer(5, 10, false, null);
            var parser = new DomaSqlParser("select emp.id from emp order by emp.id");
            var node = transformer.Transform(parser.Parse());
            var parameters = new List<ParameterEmulator>();
            var builder = new DomaSqlBuilder(node, parameters, _config);
            var result = builder.Build();
            result.ParsedSql.Is(expected);
        }

        [Fact]
        public void testOffsetLimit_forceOffsetFetch()
        {
            var expected =
                "select emp.id from emp order by emp.id offset 5 rows fetch next 10 rows only";
            var transformer = new MssqlPagingTransformer(5, 10, true, null);
            var parser = new DomaSqlParser("select emp.id from emp order by emp.id");
            var node = transformer.Transform(parser.Parse());
            var parameters = new List<ParameterEmulator>();
            var builder = new DomaSqlBuilder(node, parameters, _config);
            var result = builder.Build();
            result.ParsedSql.Is(expected);
        }

        [Fact]
        public void testOffsetLimit_option()
        {
            var expected =
                "select emp.id from emp order by emp.id  offset 5 rows fetch next 10 rows only option (maxrecursion 0)";
            var transformer = new MssqlPagingTransformer(5, 10, false, null);
            var parser = new DomaSqlParser("select emp.id from emp order by emp.id option (maxrecursion 0)");
            var node = transformer.Transform(parser.Parse());
            var parameters = new List<ParameterEmulator>();
            var builder = new DomaSqlBuilder(node, parameters, _config);
            var result = builder.Build();
            result.ParsedSql.Is(expected);
        }
    }
}
