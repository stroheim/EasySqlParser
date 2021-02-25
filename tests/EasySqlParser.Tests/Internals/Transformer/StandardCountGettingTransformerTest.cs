using System.Collections.Generic;
using EasySqlParser.Configurations;
using EasySqlParser.Internals;
using EasySqlParser.Internals.Transformer;
using Xunit;

namespace EasySqlParser.Tests.Internals.Transformer
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.dialect
    //   class      StandardCountGettingTransformerTest
    // https://github.com/domaframework/doma
    public class StandardCountGettingTransformerTest
    {
        private readonly SqlParserConfig _config;
        public StandardCountGettingTransformerTest()
        {
            _config = ConfigContainer.CreateConfigForTest(DbConnectionKind.Odbc, nameof(StandardCountGettingTransformerTest));
        }

        [Fact]
        public void test()
        {
            var expected = "select count(*) from ( select * from emp) t_";
            var transformer = new StandardCountGettingTransformer();
            var parser = new DomaSqlParser("select * from emp");
            var node = transformer.Transform(parser.Parse());
            var parameters = new List<ParameterEmulator>();
            var builder = new DomaSqlBuilder(node, parameters, _config);
            var result = builder.Build();
            result.ParsedSql.Is(expected);
        }

        [Fact]
        public void testOrderBy()
        {
            var expected = "select count(*) from ( select * from emp ) t_";
            var transformer = new StandardCountGettingTransformer();
            var parser = new DomaSqlParser("select * from emp order by emp.id");
            var node = transformer.Transform(parser.Parse());
            var parameters = new List<ParameterEmulator>();
            var builder = new DomaSqlBuilder(node, parameters, _config);
            var result = builder.Build();
            result.ParsedSql.Is(expected);
        }

    }
}
