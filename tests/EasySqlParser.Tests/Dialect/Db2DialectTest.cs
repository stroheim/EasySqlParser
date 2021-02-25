using EasySqlParser.Dialect;
using Xunit;

namespace EasySqlParser.Tests.Dialect
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


        [Fact]
        public void testGetNextSequenceSql()
        {
            var dialect = new Db2Dialect();
            dialect.GetNextSequenceSql("seq", null).Is("SELECT NEXT VALUE FOR \"seq\"");
            dialect.GetNextSequenceSql("seq", "aaa").Is("SELECT NEXT VALUE FOR \"aaa\".\"seq\"");
        }

        [Fact]
        public void testGetNextSequenceSqlZeroPadding()
        {
            var dialect = new Db2Dialect();
            dialect.GetNextSequenceSqlZeroPadding("seq", null, 5).Is("SELECT LPAD(CAST(NEXT VALUE FOR \"seq\" AS VARCHAR(5)), 5, '0')");
            dialect.GetNextSequenceSqlZeroPadding("seq", null, 5, "P").Is("SELECT 'P' || LPAD(CAST(NEXT VALUE FOR \"seq\" AS VARCHAR(5)), 5, '0')");
        }
    }
}
