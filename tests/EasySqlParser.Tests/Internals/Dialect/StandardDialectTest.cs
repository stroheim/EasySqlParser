using System;
using EasySqlParser.Internals.Dialect;
using Example.Test.Xunit;
using Xunit;

namespace EasySqlParser.Tests.Internals.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      StandardDialectTest
    // https://github.com/domaframework/doma
    public class StandardDialectTest
    {

        [Fact]
        public void testExpressionFunctions_escape()
        {
            var dialect = new StandardDialect();
            dialect.Escape("a$a%a_").Is("a$$a$%a$_");
        }

        [Fact]
        public void testExpressionFunctions_escape_withExclamation()
        {
            var dialect = new StandardDialect();
            dialect.Escape("a!a%a_",'!').Is("a!!a!%a!_");
        }

        [Fact]
        public void testExpressionFunctions_escape_withBackslash()
        {
            var dialect = new StandardDialect();
            dialect.Escape("a\\a%a_", '\\').Is("a\\\\a\\%a\\_");

            //var input = "a\\a%a_";
            //var result = Regex.Replace(input, "[\\%\\\\_]", "\\$0");
            //result.Is("a\\\\a\\%a\\_");

            //var input = "a\\a%a_";
            //var result = Regex.Replace(input, @"[\\%\\_]", "\\$0");
            //result.Is("a\\\\a\\%a\\_");

            //var input = "a\\a%a_";
            //var result = Regex.Replace(input, "[\\\\%\\\\_]", "\\$0");
            //result.Is("a\\\\a\\%a\\_");
        }

        [Fact]
        public void testExpressionFunctions_prefix()
        {
            var dialect = new StandardDialect();
            dialect.StartsWith("a$a%a_").Is("a$$a$%a$_%");
        }

        [Fact]
        public void testExpressionFunctions_prefix_escape()
        {
            var dialect = new StandardDialect();
            dialect.StartsWith("a!a%a_", '!').Is("a!!a!%a!_%");
        }

        [Fact]
        public void testExpressionFunctions_prefix_escapeWithBackslash()
        {
            var dialect = new StandardDialect();
            dialect.StartsWith("a\\a%a_", '\\').Is("a\\\\a\\%a\\_%");
        }

        [Fact]
        public void testExpressionFunctions_suffix()
        {
            var dialect = new StandardDialect();
            dialect.EndsWith("a$a%a_").Is("%a$$a$%a$_");
        }

        [Fact]
        public void testExpressionFunctions_suffix_escape()
        {
            var dialect = new StandardDialect();
            dialect.EndsWith("a!a%a_", '!').Is("%a!!a!%a!_");
        }

        [Fact]
        public void testExpressionFunctions_suffix_escapeWithBackslash()
        {
            var dialect = new StandardDialect();
            dialect.EndsWith("a\\a%a_", '\\').Is("%a\\\\a\\%a\\_");
        }

        [Fact]
        public void testExpressionFunctions_infix()
        {
            var dialect = new StandardDialect();
            dialect.Contains("a$a%a_").Is("%a$$a$%a$_%");
        }

        [Fact]
        public void testExpressionFunctions_infix_escape()
        {
            var dialect = new StandardDialect();
            dialect.Contains("a!a%a_", '!').Is("%a!!a!%a!_%");
        }

        [Fact]
        public void testExpressionFunctions_infix_escapeWithBackslash()
        {
            var dialect = new StandardDialect();
            dialect.Contains("a\\a%a_", '\\').Is("%a\\\\a\\%a\\_%");
        }

        [Fact]
        public void testExpressionFunctions_roundDonwTimePart()
        {
            var dialect = new StandardDialect();
            var date = new DateTime(2009, 1, 23, 12, 34, 56);
            dialect.TruncateTime(date).Is(new DateTime(2009, 1, 23));
        }

        // original tests

        [Fact]
        public void testSequenceName()
        {
            var dialect = new StandardDialect();
            dialect.GetSequenceName("seq", null).Is("\"seq\"");
            dialect.GetSequenceName("seq", "dbo").Is("\"dbo\".\"seq\"");
        }

        [Fact]
        public void testGetSequencePrefix()
        {
            var dialect = new StandardDialect();
            dialect.GetSequencePrefix(null, null).Is("");
            dialect.GetSequencePrefix("P", "+").Is("'P' + ");
            dialect.GetSequencePrefix("P", "||").Is("'P' || ");

        }
    }
}
