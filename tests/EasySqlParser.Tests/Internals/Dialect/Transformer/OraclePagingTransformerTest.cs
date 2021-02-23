using System.Collections.Generic;
using EasySqlParser.Configurations;
using EasySqlParser.Internals;
using EasySqlParser.Internals.Dialect.Transformer;
using Xunit;

namespace EasySqlParser.Tests.Internals.Dialect.Transformer
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.dialect
    //   class      OraclePagingTransformerTest
    // https://github.com/domaframework/doma
    public class OraclePagingTransformerTest
    {
        private readonly SqlParserConfig _config;
        public OraclePagingTransformerTest()
        {
            _config = ConfigContainer.CreateConfigForTest(DbConnectionKind.Oracle, nameof(OraclePagingTransformerTest));
        }

        [Fact]
        public void testOffsetLimit()
        {
            var expected =
                "select * from ( select temp_.*, rownum esp_rownumber_ from ( select * from emp order by emp.id ) temp_ ) where esp_rownumber_ > 5 and esp_rownumber_ <= 15";
            var transformer = new OraclePagingTransformer(5, 10, null);
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
            var expected =
                "select * from ( select temp_.*, rownum esp_rownumber_ from ( select * from emp order by emp.id  ) temp_ ) where esp_rownumber_ > 5 and esp_rownumber_ <= 15 for update";
            var transformer = new OraclePagingTransformer(5, 10, null);
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
            var expected =
                "select * from ( select temp_.*, rownum esp_rownumber_ from ( select * from emp order by emp.id ) temp_ ) where esp_rownumber_ > 5";
            var transformer = new OraclePagingTransformer(5, -1, null);
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
            var expected =
                "select * from ( select temp_.*, rownum esp_rownumber_ from ( select * from emp order by emp.id ) temp_ ) where esp_rownumber_ <= 10";
            var transformer = new OraclePagingTransformer(-1, 10, null);
            var parser = new DomaSqlParser("select * from emp order by emp.id");
            var node = transformer.Transform(parser.Parse());
            var parameters = new List<ParameterEmulator>();
            var builder = new DomaSqlBuilder(node, parameters, _config);
            var result = builder.Build();
            result.ParsedSql.Is(expected);
        }
    }
}
