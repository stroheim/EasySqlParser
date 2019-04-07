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
    //   class      Db2PagingTransformerTest
    // https://github.com/domaframework/doma
    public class Db2PagingTransformerTest
    {
        private readonly SqlParserConfig _config;
        public Db2PagingTransformerTest()
        {
            _config = ConfigContainer.CreateConfigForTest(DbConnectionKind.DB2, nameof(Db2PagingTransformerTest));
        }


        [Fact]
        public void testOffsetLimit()
        {
            var expected =
                "select * from ( select temp_.*, row_number() over( order by temp_.id ) as esp_rownumber_ from ( select emp.id from emp ) as temp_ ) as temp2_ where esp_rownumber_ > 5 and esp_rownumber_ <= 15";
            var transformer = new Db2PagingTransformer(5, 10, null);
            var parser = new DomaSqlParser("select emp.id from emp order by emp.id");
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
                "select * from ( select temp_.*, row_number() over( order by temp_.id ) as esp_rownumber_ from ( select emp.id from emp ) as temp_ ) as temp2_ where esp_rownumber_ > 5";
            var transformer = new Db2PagingTransformer(5, -1, null);
            var parser = new DomaSqlParser("select emp.id from emp order by emp.id");
            var node = transformer.Transform(parser.Parse());
            var parameters = new List<ParameterEmulator>();
            var builder = new DomaSqlBuilder(node, parameters, _config);
            var result = builder.Build();
            result.ParsedSql.Is(expected);
        }

        [Fact]
        public void testLimitOnly()
        {
            var expected = "select emp.id from emp order by emp.id fetch first 10 rows only";
            var transformer = new Db2PagingTransformer(-1, 10, null);
            var parser = new DomaSqlParser("select emp.id from emp order by emp.id");
            var node = transformer.Transform(parser.Parse());
            var parameters = new List<ParameterEmulator>();
            var builder = new DomaSqlBuilder(node, parameters, _config);
            var result = builder.Build();
            result.ParsedSql.Is(expected);
        }
    }
}
