using System;
using System.Collections.Generic;
using System.Text;
using EasySqlParser.Internals.Dialect;
using Xunit;

namespace EasySqlParser.Tests.Internals.Dialect
{
    public class PostgresDialectTest
    {
        [Fact]
        public void testGetNextSequenceSql()
        {
            var dialect = new PostgresDialect();
            dialect.GetNextSequenceSql("seq", null).Is("SELECT NEXT VALUE FOR \"seq\"");
            dialect.GetNextSequenceSql("seq", "aaa").Is("SELECT NEXT VALUE FOR \"aaa\".\"seq\"");
        }

        [Fact]
        public void testGetNextSequenceSqlZeroPadding()
        {
            var dialect = new PostgresDialect();
            dialect.GetNextSequenceSqlZeroPadding("seq", null, 5).Is("SELECT LPAD(CAST(NEXT VALUE FOR \"seq\" AS VARCHAR), 5, '0')");
            dialect.GetNextSequenceSqlZeroPadding("seq", null, 5, "P").Is("SELECT 'P' || LPAD(CAST(NEXT VALUE FOR \"seq\" AS VARCHAR), 5, '0')");
        }

    }
}
