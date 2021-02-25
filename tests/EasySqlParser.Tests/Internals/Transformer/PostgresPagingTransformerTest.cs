using System.Collections.Generic;
using EasySqlParser.Configurations;
using EasySqlParser.Internals;
using EasySqlParser.Internals.Transformer;
using Xunit;

namespace EasySqlParser.Tests.Internals.Transformer
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.dialect
    //   class      PostgresPagingTransformerTest
    // https://github.com/domaframework/doma
    public class PostgresPagingTransformerTest
    {
        private readonly SqlParserConfig _config;
        public PostgresPagingTransformerTest()
        {
            _config = ConfigContainer.CreateConfigForTest(DbConnectionKind.PostgreSql, nameof(PostgresPagingTransformerTest));
        }

        [Fact]
        public void testOffsetLimit()
        {
            var expected = "select * from emp order by emp.id limit 10 offset 5";
            var transformer = new PostgresPagingTransformer(5, 10, null);
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
            var transformer = new PostgresPagingTransformer(5, 10, null);
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
            var expected = "select * from emp order by emp.id offset 5";
            var transformer = new PostgresPagingTransformer(5, -1, null);
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
            var expected = "select * from emp order by emp.id limit 10";
            var transformer = new PostgresPagingTransformer(-1, 10, null);
            var parser = new DomaSqlParser("select * from emp order by emp.id");
            var node = transformer.Transform(parser.Parse());
            var parameters = new List<ParameterEmulator>();
            var builder = new DomaSqlBuilder(node, parameters, _config);
            var result = builder.Build();
            result.ParsedSql.Is(expected);
        }
    }
}
