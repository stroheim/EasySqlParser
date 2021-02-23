using System;
using EasySqlParser.Internals.Dialect;
using Example.Test.Xunit;
using Xunit;

namespace EasySqlParser.Tests.Internals.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      Oracle11DialectTest
    // https://github.com/domaframework/doma
    public class Oracle11DialectTest
    {
        [Fact]
        public void testExpressionFunctions_prefix()
        {
            var dialect = new Oracle11Dialect();
            dialect.StartsWith("a$a%a_a％a＿").Is("a$$a$%a$_a$％a$＿%");
        }

        [Fact]
        public void testExpressionFunctions_prefix_escape()
        {
            var dialect = new Oracle11Dialect();
            dialect.StartsWith("a!a%a_a％a＿", '!').Is("a!!a!%a!_a!％a!＿%");
        }

        [Fact]
        public void testExpressionFunctions_prefix_escapeWithBackslash()
        {
            var dialect = new Oracle11Dialect();
            dialect.StartsWith("a\\a%a_a％a＿", '\\').Is("a\\\\a\\%a\\_a\\％a\\＿%");
        }

        [Fact]
        public void testDateFormat()
        {
            var dialect = new Oracle11Dialect();
            var date = new DateTime(2009, 1, 23);
            dialect.ToLogFormatDateOnly(date).Is("date'2009-01-23'");
        }

        [Fact]
        public void testTimeFormat()
        {
            var dialect = new Oracle11Dialect();
            var time = new TimeSpan(1, 23, 45);
            dialect.ToLogFormat(time).Is("time'01:23:45'");
        }

        [Fact]
        public void testTimestampFormat()
        {
            var dialect = new Oracle11Dialect();
            var datetime = new DateTime(2009, 1, 23, 1, 23, 45, 123);
            dialect.ToLogFormat(datetime).Is("timestamp'2009-01-23 01:23:45.123'");

            var datetimeoffset = new DateTimeOffset(2009, 1, 23, 1, 23, 45, 123, TimeSpan.Zero);
            dialect.ToLogFormat(datetimeoffset).Is("timestamp'2009-01-23 01:23:45.123'");
        }

        // original tests

        [Fact]
        public void testGetNextSequenceSql()
        {
            var dialect = new Oracle11Dialect();
            dialect.GetNextSequenceSql("seq", null).Is("SELECT \"seq\".NEXTVAL FROM DUAL");
            dialect.GetNextSequenceSql("seq", "aaa").Is("SELECT \"aaa\".\"seq\".NEXTVAL FROM DUAL");
        }

        [Fact]
        public void testGetNextSequenceSqlZeroPadding()
        {
            var dialect = new Oracle11Dialect();
            dialect.GetNextSequenceSqlZeroPadding("seq", null, 5).Is("SELECT LPAD(\"seq\".NEXTVAL, 5, '0') FROM DUAL");
            dialect.GetNextSequenceSqlZeroPadding("seq", null, 5, "P").Is("SELECT 'P' || LPAD(\"seq\".NEXTVAL, 5, '0') FROM DUAL");
        }
    }
}
