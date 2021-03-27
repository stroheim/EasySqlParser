using EasySqlParser.Dialect;
using Xunit;

namespace EasySqlParser.Tests.Dialect
{
    public class PostgresDialectTest
    {
        [Fact]
        public void testGetNextSequenceSql()
        {
            var dialect = new PostgresDialect();
            dialect.GetNextSequenceSql("seq", null).Is("SELECT nextval('\"seq\"')");
            dialect.GetNextSequenceSql("seq", "aaa").Is("SELECT nextval('\"aaa\".\"seq\"')");
        }

        [Fact]
        public void testGetNextSequenceSqlZeroPadding()
        {
            var dialect = new PostgresDialect();
            dialect.GetNextSequenceSqlZeroPadding("seq", null, 5).Is("SELECT LPAD(CAST(nextval('\"seq\"') AS VARCHAR), 5, '0')");
            dialect.GetNextSequenceSqlZeroPadding("seq", null, 5, "P").Is("SELECT 'P' || LPAD(CAST(nextval('\"seq\"') AS VARCHAR), 5, '0')");
        }

    }
}
