using EasySqlParser.Internals.Dialect;
using Example.Test.Xunit;
using Xunit;

namespace EasySqlParser.Tests.Internals.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      MssqlDialectTest
    // https://github.com/domaframework/doma
    public class MssqlDialectTest
    {
        [Fact]
        public void testExpressionFunctions_prefix()
        {
            var dialect = new MssqlDialect();
            dialect.StartsWith("a$a%a_a[").Is("a$$a$%a$_a$[%");
        }

        [Fact]
        public void testExpressionFunctions_prefix_escape()
        {
            var dialect = new MssqlDialect();
            dialect.StartsWith("a!a%a_a[", '!').Is("a!!a!%a!_a![%");
        }
    }
}
