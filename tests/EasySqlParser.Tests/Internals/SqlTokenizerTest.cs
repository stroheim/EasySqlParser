using EasySqlParser.Exceptions;
using EasySqlParser.Internals;
using Xunit;
using Example.Test.Xunit;

namespace EasySqlParser.Tests.Internals
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql
    //   class      SqlTokenizerTest
    // https://github.com/domaframework/doma
    public class SqlTokenizerTest
    {
        [Fact]
        public void testEof()
        {
            var tokenizer = new SqlTokenizer("where");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testDelimiter()
        {
            var tokenizer = new SqlTokenizer("where;");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.DELIMITER);
            tokenizer.Token.Is(";");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testLineComment()
        {
            var tokenizer = new SqlTokenizer("where--aaa\r\nbbb");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.LINE_COMMENT);
            tokenizer.Token.Is("--aaa");
            tokenizer.Next().Is(SqlTokenType.EOL);
            tokenizer.Token.Is("\r\n");
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("bbb");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testBlockComment()
        {
            var tokenizer = new SqlTokenizer("where /*+aaa*/bbb");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Next().Is(SqlTokenType.BLOCK_COMMENT);
            tokenizer.Token.Is("/*+aaa*/");
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("bbb");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testBlockComment_empty()
        {
            var tokenizer = new SqlTokenizer("where /**/bbb");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Next().Is(SqlTokenType.BLOCK_COMMENT);
            tokenizer.Token.Is("/**/");
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("bbb");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testQuote()
        {
            var tokenizer = new SqlTokenizer("where 'aaa'");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Next().Is(SqlTokenType.QUOTE);
            tokenizer.Token.Is("'aaa'");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testQuote_escaped()
        {
            var tokenizer = new SqlTokenizer("where 'aaa'''");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Next().Is(SqlTokenType.QUOTE);
            tokenizer.Token.Is("'aaa'''");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testQuote_notClosed()
        {
            var tokenizer = new SqlTokenizer("where 'aaa");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            var ex = Assert.Throws<SqlParseException>(() => tokenizer.Next());
            ex.IsNotNull();
        }

        [Fact]
        public void testQuote_escaped_notClosed()
        {
            var tokenizer = new SqlTokenizer("where 'aaa''bbb''");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            var ex = Assert.Throws<SqlParseException>(() => tokenizer.Next());
            ex.IsNotNull();
        }

        [Fact]
        public void testUnion()
        {
            var tokenizer = new SqlTokenizer("union");
            tokenizer.Next().Is(SqlTokenType.UNION_WORD);
            tokenizer.Token.Is("union");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testExcept()
        {
            var tokenizer = new SqlTokenizer("except");
            tokenizer.Next().Is(SqlTokenType.EXCEPT_WORD);
            tokenizer.Token.Is("except");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testMinus()
        {
            var tokenizer = new SqlTokenizer("minus");
            tokenizer.Next().Is(SqlTokenType.MINUS_WORD);
            tokenizer.Token.Is("minus");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testIntersect()
        {
            var tokenizer = new SqlTokenizer("intersect");
            tokenizer.Next().Is(SqlTokenType.INTERSECT_WORD);
            tokenizer.Token.Is("intersect");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testSelect()
        {
            var tokenizer = new SqlTokenizer("select");
            tokenizer.Next().Is(SqlTokenType.SELECT_WORD);
            tokenizer.Token.Is("select");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testFrom()
        {
            var tokenizer = new SqlTokenizer("from");
            tokenizer.Next().Is(SqlTokenType.FROM_WORD);
            tokenizer.Token.Is("from");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testWhere()
        {
            var tokenizer = new SqlTokenizer("where");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testGroupBy()
        {
            var tokenizer = new SqlTokenizer("group by");
            tokenizer.Next().Is(SqlTokenType.GROUP_BY_WORD);
            tokenizer.Token.Is("group by");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testHaving()
        {
            var tokenizer = new SqlTokenizer("having");
            tokenizer.Next().Is(SqlTokenType.HAVING_WORD);
            tokenizer.Token.Is("having");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testOrderBy()
        {
            var tokenizer = new SqlTokenizer("order by");
            tokenizer.Next().Is(SqlTokenType.ORDER_BY_WORD);
            tokenizer.Token.Is("order by");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testForUpdateBy()
        {
            var tokenizer = new SqlTokenizer("for update");
            tokenizer.Next().Is(SqlTokenType.FOR_UPDATE_WORD);
            tokenizer.Token.Is("for update");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testOption()
        {
            var tokenizer = new SqlTokenizer("option (");
            tokenizer.Next().Is(SqlTokenType.OPTION_WORD);
            tokenizer.Token.Is("option");
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Next().Is(SqlTokenType.OPENED_PARENS);
            tokenizer.Token.Is("(");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testUpdate()
        {
            var tokenizer = new SqlTokenizer("update");
            tokenizer.Next().Is(SqlTokenType.UPDATE_WORD);
            tokenizer.Token.Is("update");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testSet()
        {
            var tokenizer = new SqlTokenizer("set");
            tokenizer.Next().Is(SqlTokenType.SET_WORD);
            tokenizer.Token.Is("set");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testAnd()
        {
            var tokenizer = new SqlTokenizer("and");
            tokenizer.Next().Is(SqlTokenType.AND_WORD);
            tokenizer.Token.Is("and");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testOr()
        {
            var tokenizer = new SqlTokenizer("or");
            tokenizer.Next().Is(SqlTokenType.OR_WORD);
            tokenizer.Token.Is("or");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testBindBlockComment()
        {
            var tokenizer = new SqlTokenizer("where /*aaa*/bbb");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Next().Is(SqlTokenType.BIND_VARIABLE_BLOCK_COMMENT);
            tokenizer.Token.Is("/*aaa*/");
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("bbb");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testBindBlockComment_followingQuote()
        {
            var tokenizer = new SqlTokenizer("where /*aaa*/'2001-01-01 12:34:56'");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Next().Is(SqlTokenType.BIND_VARIABLE_BLOCK_COMMENT);
            tokenizer.Token.Is("/*aaa*/");
            tokenizer.Next().Is(SqlTokenType.QUOTE);
            tokenizer.Token.Is("'2001-01-01 12:34:56'");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testBindBlockComment_followingWordAndQuote()
        {
            var tokenizer = new SqlTokenizer("where /*aaa*/timestamp'2001-01-01 12:34:56' and");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Next().Is(SqlTokenType.BIND_VARIABLE_BLOCK_COMMENT);
            tokenizer.Token.Is("/*aaa*/");
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("timestamp'2001-01-01 12:34:56'");
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Next().Is(SqlTokenType.AND_WORD);
            tokenizer.Token.Is("and");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testBindBlockComment_spaceIncluded()
        {
            var tokenizer = new SqlTokenizer("where /* aaa */bbb");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Next().Is(SqlTokenType.BIND_VARIABLE_BLOCK_COMMENT);
            tokenizer.Token.Is("/* aaa */");
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("bbb");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testBindBlockComment_startWithStringLiteral()
        {
            var tokenizer = new SqlTokenizer("where /*\"aaa\"*/bbb");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Next().Is(SqlTokenType.BIND_VARIABLE_BLOCK_COMMENT);
            tokenizer.Token.Is("/*\"aaa\"*/");
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("bbb");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testBindBlockComment_startWithCharLiteral()
        {
            var tokenizer = new SqlTokenizer("where /*'a'*/bbb");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Next().Is(SqlTokenType.BIND_VARIABLE_BLOCK_COMMENT);
            tokenizer.Token.Is("/*'a'*/");
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("bbb");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testLiteralBlockComment()
        {
            var tokenizer = new SqlTokenizer("where /*^aaa*/bbb");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Next().Is(SqlTokenType.LITERAL_VARIABLE_BLOCK_COMMENT);
            tokenizer.Token.Is("/*^aaa*/");
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("bbb");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testLiteralBlockComment_followingQuote()
        {
            var tokenizer = new SqlTokenizer("where /*^aaa*/'2001-01-01 12:34:56'");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Next().Is(SqlTokenType.LITERAL_VARIABLE_BLOCK_COMMENT);
            tokenizer.Token.Is("/*^aaa*/");
            tokenizer.Next().Is(SqlTokenType.QUOTE);
            tokenizer.Token.Is("'2001-01-01 12:34:56'");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testLiteralBlockComment_spaceIncluded()
        {
            var tokenizer = new SqlTokenizer("where /*^ aaa */bbb");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Next().Is(SqlTokenType.LITERAL_VARIABLE_BLOCK_COMMENT);
            tokenizer.Token.Is("/*^ aaa */");
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("bbb");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testIfBlockComment()
        {
            var tokenizer = new SqlTokenizer("where /*%if true*/bbb");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Next().Is(SqlTokenType.IF_BLOCK_COMMENT);
            tokenizer.Token.Is("/*%if true*/");
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("bbb");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testForBlockComment()
        {
            var tokenizer = new SqlTokenizer("where /*%for element : list*/bbb");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            var ex = Assert.Throws<UnsupportedSqlCommentException>(() => tokenizer.Next());
            ex.IsNotNull();
            //tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            //tokenizer.Token.Is(" ");
            //tokenizer.Next().Is(SqlTokenType.FOR_BLOCK_COMMENT);
            //tokenizer.Token.Is("/*%for element : list*/");
            //tokenizer.Next().Is(SqlTokenType.WORD);
            //tokenizer.Token.Is("bbb");
            //tokenizer.Next().Is(SqlTokenType.EOF);
            //tokenizer.Token.IsNull();
        }

        [Fact]
        public void testEndBlockComment()
        {
            var tokenizer = new SqlTokenizer("where bbb/*%end*/");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("bbb");
            tokenizer.Next().Is(SqlTokenType.END_BLOCK_COMMENT);
            tokenizer.Token.Is("/*%end*/");
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testExpandBlockComment()
        {
            var tokenizer = new SqlTokenizer("select /*%expand*/* from");
            tokenizer.Next().Is(SqlTokenType.SELECT_WORD);
            tokenizer.Token.Is("select");
            var ex = Assert.Throws<UnsupportedSqlCommentException>(() => tokenizer.Next());
            ex.IsNotNull();
            //tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            //tokenizer.Token.Is(" ");
            //tokenizer.Next().Is(SqlTokenType.EXPAND_BLOCK_COMMENT);
            //tokenizer.Token.Is("/*%expand*/");
            //tokenizer.Next().Is(SqlTokenType.OTHER);
            //tokenizer.Token.Is("*");
            //tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            //tokenizer.Token.Is(" ");
            //tokenizer.Next().Is(SqlTokenType.FROM_WORD);
            //tokenizer.Token.Is("from");
            //tokenizer.Next().Is(SqlTokenType.EOF);
            //tokenizer.Token.IsNull();
        }

        [Fact]
        public void testExpandBlockComment_alias()
        {
            var tokenizer = new SqlTokenizer("select /*%expand e*/* from");
            tokenizer.Next().Is(SqlTokenType.SELECT_WORD);
            tokenizer.Token.Is("select");
            var ex = Assert.Throws<UnsupportedSqlCommentException>(() => tokenizer.Next());
            ex.IsNotNull();
            //tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            //tokenizer.Token.Is(" ");
            //tokenizer.Next().Is(SqlTokenType.EXPAND_BLOCK_COMMENT);
            //tokenizer.Token.Is("/*%expand e*/");
            //tokenizer.Next().Is(SqlTokenType.OTHER);
            //tokenizer.Token.Is("*");
            //tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            //tokenizer.Token.Is(" ");
            //tokenizer.Next().Is(SqlTokenType.FROM_WORD);
            //tokenizer.Token.Is("from");
            //tokenizer.Next().Is(SqlTokenType.EOF);
            //tokenizer.Token.IsNull();
        }

        [Fact]
        public void testPopulateBlockComment()
        {
            var tokenizer = new SqlTokenizer("set /*%populate*/ id = id");
            tokenizer.Next().Is(SqlTokenType.SET_WORD);
            tokenizer.Token.Is("set");
            var ex = Assert.Throws<UnsupportedSqlCommentException>(() => tokenizer.Next());
            ex.IsNotNull();
            //tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            //tokenizer.Token.Is(" ");
            //tokenizer.Next().Is(SqlTokenType.POPULATE_BLOCK_COMMENT);
            //tokenizer.Token.Is("/*%populate*/");
            //tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            //tokenizer.Token.Is(" ");
            //tokenizer.Next().Is(SqlTokenType.WORD);
            //tokenizer.Token.Is("id");
            //tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            //tokenizer.Token.Is(" ");
            //tokenizer.Next().Is(SqlTokenType.OTHER);
            //tokenizer.Token.Is("=");
            //tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            //tokenizer.Token.Is(" ");
            //tokenizer.Next().Is(SqlTokenType.WORD);
            //tokenizer.Token.Is("id");
            //tokenizer.Next().Is(SqlTokenType.EOF);
            //tokenizer.Token.IsNull();
        }

        [Fact]
        public void testLineNumber()
        {
            var tokenizer = new SqlTokenizer("aaa\nbbb\nccc\n/* \nddd\n */");
            tokenizer.LineNumber.Is(1);
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("aaa");
            tokenizer.LineNumber.Is(1);
            tokenizer.Next().Is(SqlTokenType.EOL);
            tokenizer.Token.Is("\n");
            tokenizer.LineNumber.Is(2);
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("bbb");
            tokenizer.LineNumber.Is(2);
            tokenizer.Next().Is(SqlTokenType.EOL);
            tokenizer.Token.Is("\n");
            tokenizer.LineNumber.Is(3);
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("ccc");
            tokenizer.LineNumber.Is(3);
            tokenizer.Next().Is(SqlTokenType.EOL);
            tokenizer.Token.Is("\n");
            tokenizer.LineNumber.Is(4);
            tokenizer.Next().Is(SqlTokenType.BIND_VARIABLE_BLOCK_COMMENT);
            tokenizer.Token.Is("/* \nddd\n */");
            tokenizer.LineNumber.Is(6);
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testColumnNumber()
        {
            var tokenizer = new SqlTokenizer("aaa bbb\nc\nd eee\n");
            tokenizer.Position.Is(0);
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("aaa");
            tokenizer.Position.Is(3);
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Position.Is(4);
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("bbb");
            tokenizer.Position.Is(7);
            tokenizer.Next().Is(SqlTokenType.EOL);
            tokenizer.Token.Is("\n");
            tokenizer.Position.Is(0);
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("c");
            tokenizer.Position.Is(1);
            tokenizer.Next().Is(SqlTokenType.EOL);
            tokenizer.Token.Is("\n");
            tokenizer.Position.Is(0);
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("d");
            tokenizer.Position.Is(1);
            tokenizer.Next().Is(SqlTokenType.WHITESPACE);
            tokenizer.Token.Is(" ");
            tokenizer.Position.Is(2);
            tokenizer.Next().Is(SqlTokenType.WORD);
            tokenizer.Token.Is("eee");
            tokenizer.Position.Is(5);
            tokenizer.Next().Is(SqlTokenType.EOL);
            tokenizer.Token.Is("\n");
            tokenizer.Position.Is(0);
            tokenizer.Next().Is(SqlTokenType.EOF);
            tokenizer.Token.IsNull();
        }

        [Fact]
        public void testIllegalDirective()
        {
            var tokenizer = new SqlTokenizer("where /*%*/bbb");
            tokenizer.Next().Is(SqlTokenType.WHERE_WORD);
            tokenizer.Token.Is("where");
            var ex = Assert.Throws<SqlParseException>(() => tokenizer.Next());
            ex.IsNotNull();
        }
    }
}
