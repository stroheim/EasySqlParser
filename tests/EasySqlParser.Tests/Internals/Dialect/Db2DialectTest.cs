using EasySqlParser.Internals.Dialect;
using Example.Test.Xunit;
using Xunit;

namespace EasySqlParser.Tests.Internals.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      Db2DialectTest
    // https://github.com/domaframework/doma
    public class Db2DialectTest
    {
        [Fact]
        public void testExpressionFunctions_prefix()
        {
            var dialect = new Db2Dialect();
            dialect.StartsWith("a$a%a_a％a＿").Is("a$$a$%a$_a$％a$＿%");
        }

        [Fact]
        public void testExpressionFunctions_prefix_escape()
        {
            var dialect = new Db2Dialect();
            dialect.StartsWith("a!a%a_a％a＿", '!').Is("a!!a!%a!_a!％a!＿%");
        }

        [Fact]
        public void testExpressionFunctions_prefix_escapeWithDefault()
        {
            var dialect = new Db2Dialect();
            dialect.StartsWith("a\\a%a_a％a＿", '\\').Is("a\\\\a\\%a\\_a\\％a\\＿%");
        }
    }
}
