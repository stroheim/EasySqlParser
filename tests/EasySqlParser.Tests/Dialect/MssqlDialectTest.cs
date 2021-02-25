using EasySqlParser.Dialect;
using Xunit;

namespace EasySqlParser.Tests.Dialect
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

        [Fact]
        public void testGetNextSequenceSql()
        {
            var dialect = new MssqlDialect();
            dialect.GetNextSequenceSql("seq", null).Is("SELECT NEXT VALUE FOR [seq]");
            dialect.GetNextSequenceSql("seq", "aaa").Is("SELECT NEXT VALUE FOR [aaa].[seq]");
        }

        [Fact]
        public void testGetNextSequenceSqlZeroPadding()
        {
            var dialect = new MssqlDialect();
            dialect.GetNextSequenceSqlZeroPadding("seq", null, 5).Is("SELECT FORMAT(NEXT VALUE FOR [seq], 'D5')");
            dialect.GetNextSequenceSqlZeroPadding("seq", null, 5, "P").Is("SELECT 'P' + FORMAT(NEXT VALUE FOR [seq], 'D5')");
        }
    }
}
