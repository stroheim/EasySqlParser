using System.Collections.Generic;
using EasySqlParser.Configurations;
using EasySqlParser.Exceptions;
using EasySqlParser.Internals;
using EasySqlParser.Internals.Transformer;
using Xunit;

namespace EasySqlParser.Tests.Internals.Transformer
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.dialect
    //   class      StandardPagingTransformerTest
    // https://github.com/domaframework/doma
    public class StandardPagingTransformerTest
    {
        private readonly SqlParserConfig _config;
        public StandardPagingTransformerTest()
        {
            _config = ConfigContainer.CreateConfigForTest(DbConnectionKind.Odbc, nameof(StandardPagingTransformerTest));
        }

        [Fact]
        public void testOffsetLimit()
        {
            var expected =
                "select * from ( select temp_.*, row_number() over( order by temp_.id ) as esp_rownumber_ from ( select emp.id from emp ) as temp_ ) as temp2_ where esp_rownumber_ > 5 and esp_rownumber_ <= 15";
            var transformer = new StandardPagingTransformer(5,10, null);
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
            var transformer = new StandardPagingTransformer(5, -1, null);
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
            var expected =
                "select * from ( select temp_.*, row_number() over( order by temp_.id ) as esp_rownumber_ from ( select emp.id from emp ) as temp_ ) as temp2_ where esp_rownumber_ <= 10";
            var transformer = new StandardPagingTransformer(-1, 10, null);
            var parser = new DomaSqlParser("select emp.id from emp order by emp.id");
            var node = transformer.Transform(parser.Parse());
            var parameters = new List<ParameterEmulator>();
            var builder = new DomaSqlBuilder(node, parameters, _config);
            var result = builder.Build();
            result.ParsedSql.Is(expected);
        }

        [Fact]
        public void testOrderByClauseUnspecified()
        {
            var transformer = new StandardPagingTransformer(5, 10, null);
            var parser = new DomaSqlParser("select * from emp");
            var ex = Assert.Throws<SqlTransformException>(() => transformer.Transform(parser.Parse()));
            ex.IsNotNull();
            ex.MessageId.Is(ExceptionMessageId.Esp2201);
        }
    }
}
