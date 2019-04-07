using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using EasySqlParser.Configurations;
using EasySqlParser.Exceptions;
using EasySqlParser.Internals;
using Example.Test.Xunit;
using Xunit;

namespace EasySqlParser.Tests.Internals
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql
    //   class      SqlParserTest
    // https://github.com/domaframework/doma
    public class DomaSqlParserTest
    {
        public DomaSqlParserTest()
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );
        }

        [Fact]
        public void testBindVariable()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
                           { Name = "name", ParameterType = typeof(string), ParameterValue = "hoge" });
            parameters.Add(new ParameterEmulator
                           { Name = "salary", ParameterType = typeof(int), ParameterValue = 10000 });
            var testSql = "select * from aaa where ename = /*name*/'aaa' and sal = /*salary*/-2000";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where ename = @name and sal = @salary");
            result.DebugSql.Is("select * from aaa where ename = 'hoge' and sal = 10000");
            result.DbDataParameters.Count.Is(2);
            result.DbDataParameters[0].Value.Is("hoge");
            result.DbDataParameters[1].Value.Is(10000);
        }

        [Fact]
        public void testBindVariable_StartsWith()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
                           { Name = "name", ParameterType = typeof(string), ParameterValue = "hoge" });
            var testSql = "select * from aaa where ename like /* @StartsWith(name)*/'aaa'";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where ename like @name");
            result.DebugSql.Is("select * from aaa where ename like 'hoge%'");
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].Value.Is("hoge%");
        }

        [Fact]
        public void testBindVariable_Contains()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
                           { Name = "name", ParameterType = typeof(string), ParameterValue = "hoge" });
            var testSql = "select * from aaa where ename like /* @Contains(name)*/'aaa'";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where ename like @name");
            result.DebugSql.Is("select * from aaa where ename like '%hoge%'");
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].Value.Is("%hoge%");
        }

        [Fact]
        public void testBindVariable_EndsWith()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
                           { Name = "name", ParameterType = typeof(string), ParameterValue = "hoge" });
            var testSql = "select * from aaa where ename like /* @EndsWith(name)*/'aaa'";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where ename like @name");
            result.DebugSql.Is("select * from aaa where ename like '%hoge'");
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].Value.Is("%hoge");
        }

        [Fact]
        public void testBindVariable_TruncateTime()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
                           {
                               Name = "birthDate", ParameterType = typeof(DateTime),
                               ParameterValue = new DateTime(1999, 9, 9, 10, 20, 30)
                           });
            var testSql = "select * from aaa where birthDate > /* @TruncateTime(birthDate) */'1999-10-12'";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where birthDate > @birthDate");
            result.DebugSql.Is("select * from aaa where birthDate > '1999-09-09'");
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].Value.Is(new DateTime(1999, 9, 9));
        }

        [Fact]
        public void UnknownBuiltinFunction()
        {
            var testSql = "select * from aaa where birthDate > /* @Dummy(birthDate) */'1999-10-12'";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<SqlParseException>(() => parser.Parse());
            ex.MessageId.Is(ExceptionMessageId.Esp2150);
        }

        [Fact]
        public void BuiltinFunctionArgsCountTooMany()
        {
            var testSql = "select * from aaa where birthDate > /* @TruncateTime(birthDate, '!') */'1999-10-12'";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<SqlParseException>(() => parser.Parse());
            ex.MessageId.Is(ExceptionMessageId.Esp2151);
        }

        [Fact]
        public void InvalidEscapeChar()
        {
            var testSql = "select * from aaa where birthDate > /* @StartsWith(birthDate, '!!') */'1999-10-12'";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<SqlParseException>(() => parser.Parse());
            ex.MessageId.Is(ExceptionMessageId.Esp2152);
        }

        [Fact]
        public void InvalidEscapeChar2()
        {
            var testSql = "select * from aaa where birthDate > /* @StartsWith(birthDate, '!!) */'1999-10-12'";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<SqlParseException>(() => parser.Parse());
            ex.MessageId.Is(ExceptionMessageId.Esp2152);
        }


        [Fact]
        public void testBindVariable_in()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "name", ParameterType = typeof(string[]), ParameterValue = new[] { "hoge", "foo" } });
            var testSql = "select * from aaa where ename in /*name*/('aaa', 'bbb')";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where ename in (@name1, @name2)");
            result.DebugSql.Is("select * from aaa where ename in ('hoge', 'foo')");
            result.DbDataParameters.Count.Is(2);
            result.DbDataParameters[0].Value.Is("hoge");
            result.DbDataParameters[1].Value.Is("foo");
        }

        [Fact]
        public void testBindVariable_in_empty_iterable()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "name", ParameterType = typeof(string[]), ParameterValue = new List<string>().ToArray() });
            var testSql = "select * from aaa where ename in /*name*/('aaa', 'bbb')";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where ename in (null)");
            result.DebugSql.Is("select * from aaa where ename in (null)");
            result.DbDataParameters.Count.Is(0);
        }

        [Fact]
        public void testBindVariable_endsWithBindVariableComment()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "name", ParameterType = typeof(string), ParameterValue = "hoge" });
            var testSql = "select * from aaa where ename = /*name*/";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<SqlParseException>(() => parser.Parse());
            ex.MessageId.Is(ExceptionMessageId.Esp2110);

        }

        [Fact]
        public void testBindVariable_emptyName()
        {
            var testSql = "select * from aaa where ename = /*   */'aaa'";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<SqlParseException>(() => parser.Parse());
            ex.MessageId.Is(ExceptionMessageId.Esp2120);
        }

        [Fact]
        public void testBindVariable_stringLiteral()
        {
            var testSql = "select * from aaa where ename = /*name*/'bbb'";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            node.IsNotNull();
        }

        [Fact]
        public void testBindVariable_intLiteral()
        {
            var testSql = "select * from aaa where ename = /*name*/10";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            node.IsNotNull();
        }

        [Fact]
        public void testBindVariable_floatLiteral()
        {
            var testSql = "select * from aaa where ename = /*name*/.0";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            node.IsNotNull();
        }

        [Fact]
        public void testBindVariable_booleanTrueLiteral()
        {
            var testSql = "select * from aaa where ename = /*name*/true";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            node.IsNotNull();
        }

        [Fact]
        public void testBindVariable_booleanFalseLiteral()
        {
            var testSql = "select * from aaa where ename = /*name*/false";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            node.IsNotNull();
        }

        [Fact]
        public void testBindVariable_nullLiteral()
        {
            var testSql = "select * from aaa where ename = /*name*/null";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            node.IsNotNull();
        }

        [Fact]
        public void testBindVariable_illegalLiteral()
        {
            var testSql = "select * from aaa where ename = /*name*/bbb";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<SqlParseException>(() => parser.Parse());
            ex.MessageId.Is(ExceptionMessageId.Esp2142);
        }

        [Fact]
        public void testBindVariable_enum()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "name", ParameterType = typeof(MyEnum), ParameterValue = MyEnum.BBB });
            parameters.Add(new ParameterEmulator
            { Name = "salary", ParameterType = typeof(int), ParameterValue = 10000 });
            var testSql = "select * from aaa where ename = /*name*/'aaa' and sal = /*salary*/-2000";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where ename = @name and sal = @salary");
            result.DebugSql.Is("select * from aaa where ename = 'BBB' and sal = 10000");
            result.DbDataParameters.Count.Is(2);
            result.DbDataParameters[0].Value.Is(MyEnum.BBB);
            result.DbDataParameters[1].Value.Is(10000);
        }

        [Fact]
        public void testLiteralVariable()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "name", ParameterType = typeof(string), ParameterValue = "hoge" });
            parameters.Add(new ParameterEmulator
            { Name = "salary", ParameterType = typeof(int), ParameterValue = 10000 });
            var testSql = "select * from aaa where ename = /*^name*/'aaa' and sal = /*^salary*/-2000";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where ename = 'hoge' and sal = 10000");
            result.DebugSql.Is("select * from aaa where ename = 'hoge' and sal = 10000");
            result.DbDataParameters.Count.Is(0);
        }

        [Fact]
        public void testLiteralVariable_emptyName()
        {
            var testSql = "select * from aaa where ename = /*^   */'aaa'";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<SqlParseException>(() => parser.Parse());
            ex.MessageId.Is(ExceptionMessageId.Esp2228);
        }

        [Fact]
        public void testLiteralVariable_in()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            {
                Name = "name",
                ParameterType = typeof(List<string>),
                ParameterValue = new List<string> { "hoge", "foo" }
            });
            var testSql = "select * from aaa where ename in /*^name*/('aaa', 'bbb')";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where ename in ('hoge', 'foo')");
            result.DebugSql.Is("select * from aaa where ename in ('hoge', 'foo')");
            result.DbDataParameters.Count.Is(0);
        }

        [Fact]
        public void testLiteralVariable_endsWithLiteralVariableComment()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "name", ParameterType = typeof(string), ParameterValue = "hoge" });
            var testSql = "select * from aaa where ename = /*^name*/";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<SqlParseException>(() => parser.Parse());
            ex.MessageId.Is(ExceptionMessageId.Esp2110);
        }

        [Fact]
        public void testLiteralVariable_illegalLiteral()
        {
            var testSql = "select * from aaa where ename = /*^name*/bbb";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<SqlParseException>(() => parser.Parse());
            ex.MessageId.Is(ExceptionMessageId.Esp2142);
        }

        [Fact]
        public void testEmbeddedVariable()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "name", ParameterType = typeof(string), ParameterValue = "hoge" });
            parameters.Add(new ParameterEmulator
            { Name = "salary", ParameterType = typeof(int), ParameterValue = 10000 });
            parameters.Add(new ParameterEmulator
            {
                Name = "orderBy",
                ParameterType = typeof(string),
                ParameterValue = "order by name asc, salary"
            });
            var testSql = "select * from aaa where ename = /*name*/'aaa' and sal = /*salary*/-2000 /*#orderBy*/";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where ename = @name and sal = @salary order by name asc, salary");
            result.DebugSql.Is("select * from aaa where ename = 'hoge' and sal = 10000 order by name asc, salary");
            result.DbDataParameters.Count.Is(2);
            result.DbDataParameters[0].Value.Is("hoge");
            result.DbDataParameters[1].Value.Is(10000);
        }

        [Fact]
        public void testEmbeddedVariable_inside()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "name", ParameterType = typeof(string), ParameterValue = "hoge" });
            parameters.Add(new ParameterEmulator
            { Name = "salary", ParameterType = typeof(int), ParameterValue = 10000 });
            parameters.Add(new ParameterEmulator
            { Name = "table", ParameterType = typeof(string), ParameterValue = "aaa" });
            var testSql = "select * from /*# table */ where ename = /*name*/'aaa' and sal = /*salary*/-2000";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where ename = @name and sal = @salary");
            result.DebugSql.Is("select * from aaa where ename = 'hoge' and sal = 10000");
            result.DbDataParameters.Count.Is(2);
            result.DbDataParameters[0].Value.Is("hoge");
            result.DbDataParameters[1].Value.Is(10000);

        }

        [Fact]
        public void testEmbeddedVariable_emptyName()
        {
            var testSql = "select * from aaa where ename = /*#   */'aaa'";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<SqlParseException>(() => parser.Parse());
            ex.MessageId.Is(ExceptionMessageId.Esp2121);
        }

        [Fact]
        public void testExpand()
        {
            var testSql = "select /*%expand*/* from aaa";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<UnsupportedSqlCommentException>(() => parser.Parse());
            ex.IsNotNull();
        }

        [Fact]
        public void testExpand_withSpace()
        {
            var testSql = "select /*%expand */* from aaa";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<UnsupportedSqlCommentException>(() => parser.Parse());
            ex.IsNotNull();
        }

        [Fact]
        public void testExpand_alias()
        {
            var testSql = "select /*%expand \"a\"*/* from aaa a";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<UnsupportedSqlCommentException>(() => parser.Parse());
            ex.IsNotNull();
        }

        [Fact]
        public void testExpand_notAsteriskChar()
        {
            var testSql = "select /*%expand*/+ from aaa";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<UnsupportedSqlCommentException>(() => parser.Parse());
            ex.IsNotNull();
        }

        [Fact]
        public void testExpand_word()
        {
            var testSql = "select /*%expand*/'hoge' from aaa";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<UnsupportedSqlCommentException>(() => parser.Parse());
            ex.IsNotNull();
        }

        [Fact]
        public void testIf()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "name", ParameterType = typeof(string), ParameterValue = "hoge" });
            var testSql = "select * from aaa where /*%if name != null*/bbb = /*name*/'ccc' /*%end*/";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where bbb = @name");
            result.DebugSql.Is("select * from aaa where bbb = 'hoge'");
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].Value.Is("hoge");
        }

        [Fact]
        public void testIf_fromClause()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "type", ParameterType = typeof(string), ParameterValue = "a" });
            var testSql = "select * from /*%if type == \"a\"*/aaa/*%else*/ bbb/*%end*/";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa");
            result.DebugSql.Is("select * from aaa");
        }

        [Fact]
        public void testIf_selectClause()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "type", ParameterType = typeof(string), ParameterValue = "a" });
            var testSql = "select /*%if type == \"a\"*/aaa /*%else*/ bbb /*%end*/from ccc";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select aaa from ccc");
            result.DebugSql.Is("select aaa from ccc");
        }

        [Fact]
        public void testIf_removeWhere()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "name", ParameterType = typeof(string), ParameterValue = null });
            var testSql = "select * from aaa where /*%if name != null*/bbb = /*name*/'ccc' /*%end*/";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa");
            result.DebugSql.Is("select * from aaa");
        }

        [Fact]
        public void testIf_removeAnd()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "name", ParameterType = typeof(string), ParameterValue = null });
            var testSql =
                "select * from aaa where \n/*%if name != null*/bbb = /*name*/\'ccc\' \n/*%else*/\n --comment\nand ddd is null\n /*%end*/";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where \n\n --comment\n ddd is null");
            result.DebugSql.Is("select * from aaa where \n\n --comment\n ddd is null");
            result.DbDataParameters.Count.Is(0);
        }

        [Fact]
        public void testIf_nest()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "name", ParameterType = typeof(string), ParameterValue = "hoge" });
            var testSql =
                "select * from aaa where /*%if name != null*/bbb = /*name*/\'ccc\' /*%if name == \"hoge\"*/and ddd = eee/*%end*//*%end*/";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where bbb = @name and ddd = eee");
            result.DebugSql.Is("select * from aaa where bbb = 'hoge' and ddd = eee");
            result.DbDataParameters.Count.Is(1);
        }

        [Fact]
        public void testIf_nestContinuously()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "name", ParameterType = typeof(string), ParameterValue = "hoge" });
            parameters.Add(new ParameterEmulator
            { Name = "name2", ParameterType = typeof(string), ParameterValue = null });
            var testSql =
                "select * from aaa where /*%if name != null*//*%if name2 == \"hoge\"*/ ddd = eee/*%end*//*%end*/";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa");
            result.DebugSql.Is("select * from aaa");
            result.DbDataParameters.Count.Is(0);
        }

        [Fact]
        public void testElseifBlock()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "name", ParameterType = typeof(string), ParameterValue = "" });
            var testSql =
                "select * from aaa where /*%if name == null*/bbb is null\n/*%elseif name ==\"\"*/\nbbb = /*name*/\'ccc\'/*%end*/";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where \nbbb = @name");
            result.DebugSql.Is("select * from aaa where \nbbb = ''");
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].Value.Is("");
        }

        [Fact]
        public void testElseBlock()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "name", ParameterType = typeof(string), ParameterValue = "hoge" });
            var testSql =
                "select * from aaa where /*%if name == null*/bbb is null\n/*%elseif name == \"\"*/\n/*%else*/ bbb = /*name*/\'ccc\'/*%end*/";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where  bbb = @name");
            result.DebugSql.Is("select * from aaa where  bbb = 'hoge'");
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].Value.Is("hoge");
        }

        [Fact]
        public void testUnion()
        {
            var parameters = new List<ParameterEmulator>();
            var testSql = "select * from aaa where /*%if false*//*%end*/union all select * from bbb";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa union all select * from bbb");
        }

        [Fact]
        public void testSelect()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "name", ParameterType = typeof(string), ParameterValue = "hoge" });
            parameters.Add(new ParameterEmulator
            { Name = "count", ParameterType = typeof(int), ParameterValue = 5 });
            var testSql =
                "select aaa.deptname, count(*) from aaa join bbb on aaa.id = bbb.id where aaa.name = /*name*/'ccc' group by aaa.deptname having count(*) > /*count*/10 order by aaa.name for update bbb";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select aaa.deptname, count(*) from aaa join bbb on aaa.id = bbb.id where aaa.name = @name group by aaa.deptname having count(*) > @count order by aaa.name for update bbb");
            result.DebugSql.Is("select aaa.deptname, count(*) from aaa join bbb on aaa.id = bbb.id where aaa.name = 'hoge' group by aaa.deptname having count(*) > 5 order by aaa.name for update bbb");
            result.DbDataParameters.Count.Is(2);
            result.DbDataParameters[0].Value.Is("hoge");
            result.DbDataParameters[1].Value.Is(5);
        }

        [Fact]
        public void testUpdate()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "no", ParameterType = typeof(int), ParameterValue = 10 });
            parameters.Add(new ParameterEmulator
            { Name = "name", ParameterType = typeof(string), ParameterValue = "hoge" });
            parameters.Add(new ParameterEmulator
            { Name = "id", ParameterType = typeof(int), ParameterValue = 100 });
            var testSql = "update aaa set no = /*no*/1, set name = /*name*/'name' where id = /*id*/1";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("update aaa set no = @no, set name = @name where id = @id");
            result.DebugSql.Is("update aaa set no = 10, set name = 'hoge' where id = 100");
            result.DbDataParameters.Count.Is(3);
            result.DbDataParameters[0].Value.Is(10);
            result.DbDataParameters[1].Value.Is("hoge");
            result.DbDataParameters[2].Value.Is(100);
        }

        [Fact]
        public void testFor()
        {
            var testSql = "select * from aaa where /*%for n : names*/name = /*n*/'a' /*%if n_has_next */or /*%end*//*%end*/";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<UnsupportedSqlCommentException>(() => parser.Parse());
            ex.IsNotNull();

        }

        [Fact]
        public void testFor_removeWhere()
        {
            var testSql = "select * from aaa where /*%for n : names*/name = /*n*/'a' /*%if n_has_next */or /*%end*//*%end*/";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<UnsupportedSqlCommentException>(() => parser.Parse());
            ex.IsNotNull();
        }

        [Fact]
        public void testFor_removeOr()
        {
            var testSql = "select * from aaa where /*%for n : names*/name = /*n*/'a' /*%if n_has_next */or /*%end*//*%end*/ or salary > 100";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<UnsupportedSqlCommentException>(() => parser.Parse());
            ex.IsNotNull();
        }

        [Fact]
        public void testFor_index()
        {
            var testSql = "select * from aaa where /*%for n : names*/name/*# n_index */ = /*n*/'a' /*%if n_has_next */or /*%end*//*%end*/";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<UnsupportedSqlCommentException>(() => parser.Parse());
            ex.IsNotNull();
        }

        [Fact]
        public void testValidate_ifEnd()
        {
            var parser = new DomaSqlParser("select * from aaa /*%if true*/");
            var ex = Assert.Throws<SqlParseException>(() => parser.Parse());
            ex.MessageId.Is(ExceptionMessageId.Esp2133);
        }

        [Fact]
        public void testValidate_ifEnd_selectClause()
        {
            var parser = new DomaSqlParser("select /*%if true*/* from aaa");
            var ex = Assert.Throws<SqlParseException>(() => parser.Parse());
            ex.MessageId.Is(ExceptionMessageId.Esp2133);
        }

        [Fact]
        public void testValidate_ifEnd_subquery()
        {
            var parser = new DomaSqlParser("select *, (select /*%if true */ from aaa) x from aaa");
            var ex = Assert.Throws<SqlParseException>(() => parser.Parse());
            ex.MessageId.Is(ExceptionMessageId.Esp2133);
        }

        [Fact]
        public void testValidate_forEnd()
        {
            var testSql = "select * from aaa /*%for name : names*/";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<UnsupportedSqlCommentException>(() => parser.Parse());
            ex.IsNotNull();
        }

        [Fact]
        public void testValidate_unclosedParens()
        {
            var parser = new DomaSqlParser("select * from (select * from bbb");
            var ex = Assert.Throws<SqlParseException>(() => parser.Parse());
            ex.MessageId.Is(ExceptionMessageId.Esp2135);
        }

        [Fact]
        public void testValidate_enclosedParensByIfBlock()
        {
            var parser = new DomaSqlParser("select * from /*%if true*/(select * from bbb)/*%end*/");
            var node = parser.Parse();
            node.IsNotNull();
        }

        [Fact]
        public void testParens_removeAnd()
        {
            var parameters = new List<ParameterEmulator>();
            parameters.Add(new ParameterEmulator
            { Name = "name", ParameterType = typeof(string), ParameterValue = null });
            var testSql =
                "select * from aaa where (\n/*%if name != null*/bbb = /*name*/\'ccc\'\n/*%else*/\nand ddd is null\n /*%end*/)";
            var parser = new DomaSqlParser(testSql);
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select * from aaa where (\n\n ddd is null\n )");
            result.DebugSql.Is("select * from aaa where (\n\n ddd is null\n )");
            result.DbDataParameters.Count.Is(0);
        }

        [Fact]
        public void testEmptyParens()
        {
            var parameters = new List<ParameterEmulator>();
            var parser = new DomaSqlParser("select rank()");
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select rank()");
        }

        [Fact]
        public void testEmptyParens_whiteSpace()
        {
            var parameters = new List<ParameterEmulator>();
            var parser = new DomaSqlParser("select rank(   )");
            var node = parser.Parse();
            var builder = new DomaSqlBuilder(node, parameters);
            var result = builder.Build();
            result.ParsedSql.Is("select rank(   )");
        }

        [Fact]
        public void testManyEol()
        {
            var sql = File.ReadAllText("manyEol.sql");
            var parser = new DomaSqlParser(sql);
            var node = parser.Parse();
            node.IsNotNull();
        }

        [Fact]
        public void testPopulate()
        {
            var testSql = "update employee set /*%populate*/ id = id where age < 30";
            var parser = new DomaSqlParser(testSql);
            var ex = Assert.Throws<UnsupportedSqlCommentException>(() => parser.Parse());
            ex.IsNotNull();
        }
    }

    public enum MyEnum
    {
        AAA,
        BBB,
        CCC
    }
}
