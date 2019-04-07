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
    //   class      Mssql2008PagingTransformerTest
    // https://github.com/domaframework/doma
    public class Mssql2008PagingTransformerTest
    {
        private readonly SqlParserConfig _config;
        public Mssql2008PagingTransformerTest()
        {
            _config = ConfigContainer.CreateConfigForTest(DbConnectionKind.SqlServer, nameof(Mssql2008PagingTransformerTest));
        }

        [Fact]
        public void testOffsetLimit()
        {
            var expected =
                "select * from ( select temp_.*, row_number() over( order by temp_.id ) as esp_rownumber_ from ( select emp.id from emp ) as temp_ ) as temp2_ where esp_rownumber_ > 5 and esp_rownumber_ <= 15";
            var transformer = new Mssql2008PagingTransformer(5, 10, null);
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
            var transformer = new Mssql2008PagingTransformer(5, -1, null);
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
            var expected = "select top (10) emp.id from emp order by emp.id";
            var transformer = new Mssql2008PagingTransformer(-1, 10, null);
            var parser = new DomaSqlParser("select emp.id from emp order by emp.id");
            var node = transformer.Transform(parser.Parse());
            var parameters = new List<ParameterEmulator>();
            var builder = new DomaSqlBuilder(node, parameters, _config);
            var result = builder.Build();
            result.ParsedSql.Is(expected);
        }
    }
}
