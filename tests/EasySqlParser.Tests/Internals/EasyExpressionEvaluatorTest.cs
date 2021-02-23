using System.Collections.Generic;
using EasySqlParser.Exceptions;
using EasySqlParser.Internals;
using Xunit;

namespace EasySqlParser.Tests.Internals
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.expr
    //   class      ExpressionParserTest
    // https://github.com/domaframework/doma
    public class EasyExpressionEvaluatorTest
    {
        [Fact]
        public void testTrue()
        {
            var expr = "true";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsTrue();
        }

        [Fact]
        public void testFalse()
        {
            var expr = "false";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsFalse();
        }

        [Fact]
        public void testNot()
        {
            var expr = "!true";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsFalse();
        }

        [Fact]
        public void testNot2()
        {
            var expr = "!false";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsTrue();
        }

        [Fact]
        public void testAnd()
        {
            var expr = "!false && !false";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsTrue();
        }

        [Fact]
        public void testAnd2()
        {
            var expr = "(true || false) && (true || false)";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsTrue();
        }

        [Fact]
        public void testAnd3()
        {
            var expr = "(true || false ) && !( true || false)";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsFalse();
        }

        [Fact]
        public void testAnd4()
        {
            var expr = "(true || false ) && true";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsTrue();
        }

        [Fact]
        public void testOr()
        {
            var expr = "false || true";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsTrue();
        }

        [Fact]
        public void testOr2()
        {
            var expr = "false || false";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsFalse();
        }
        
        [Fact]
        public void testOr44()
        {
            var expr = "false || true && false";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsFalse();
        }

        [Fact]
        public void testOr3()
        {
            var expr = "true && true || true && true";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsTrue();
        }

        [Fact]
        public void testOr4()
        {
            var expr = "true && false || true && true";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsTrue();
        }

        [Fact]
        public void testEq()
        {
            var expr = "10 == 10";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsTrue();
        }

        [Fact]
        public void testNotEq()
        {
            var expr = "11 == 10";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsFalse();
        }

        [Fact]
        public void testEq_null()
        {
            var expr = "null == null";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsTrue();
        }

        [Fact]
        public void testNe()
        {
            var expr = "1 != 2";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsTrue();
        }

        [Fact]
        public void testNotNe()
        {
            var expr = "11 != 11";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsFalse();
        }

        [Fact]
        public void testNe_null()
        {
            var expr = "null != null";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsFalse();
        }

        [Fact]
        public void testGe()
        {
            var expr = "11 >= 10";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsTrue();

            expr = "10 >= 10";
            result = evaluator.Evaluate(expr, null);
            result.IsTrue();
        }

        [Fact]
        public void testNotGe()
        {
            var expr = "9 >= 10";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsFalse();
        }

        [Fact]
        public void testLe()
        {
            var expr = "10 <= 11";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsTrue();

            expr = "10 <= 10";
            result = evaluator.Evaluate(expr, null);
            result.IsTrue();
        }

        [Fact]
        public void testNotLe()
        {
            var expr = "10 <= 9";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsFalse();
        }

        [Fact]
        public void testGt()
        {
            var expr = "11 > 10";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsTrue();
        }

        [Fact]
        public void testNotGt()
        {
            var expr = "10 > 10";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsFalse();

            expr = "9 > 10";
            result = evaluator.Evaluate(expr, null);
            result.IsFalse();
        }

        [Fact]
        public void testLt()
        {
            var expr = "10 < 11";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsTrue();
        }

        [Fact]
        public void testNotLt()
        {
            var expr = "10 < 10";
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, null);
            result.IsFalse();

            expr = "10 < 9";
            result = evaluator.Evaluate(expr, null);
            result.IsFalse();
        }

        [Fact]
        public void testUnsupportedToken()
        {
            var expr = "5 ? 5";
            var evaluator = new EasyExpressionEvaluator();
            var ex = Assert.Throws<ExpressionEvaluateException>(() => evaluator.Evaluate(expr, null));
            ex.IsNotNull();
            ex.InvalidOperator.Is("?");
        }

        // original tests

        [Fact]
        public void InvalidExpression1()
        {
            var expr = "5 !";
            var evaluator = new EasyExpressionEvaluator();
            var ex = Assert.Throws<ExpressionEvaluateException>(() => evaluator.Evaluate(expr, null));
            ex.IsNotNull();
            ex.MessageId.Is(ExceptionMessageId.EspA021);
            ex.InvalidOperator.Is("!");
        }

        [Fact]
        public void InvalidExpression2()
        {
            var expr = "5 !(";
            var evaluator = new EasyExpressionEvaluator();
            var ex = Assert.Throws<ExpressionEvaluateException>(() => evaluator.Evaluate(expr, null));
            ex.IsNotNull();
            ex.MessageId.Is(ExceptionMessageId.EspA002);
        }

        [Fact]
        public void InvalidExpression3()
        {
            var expr = "5 !)";
            var evaluator = new EasyExpressionEvaluator();
            var ex = Assert.Throws<ExpressionEvaluateException>(() => evaluator.Evaluate(expr, null));
            ex.IsNotNull();
            ex.MessageId.Is(ExceptionMessageId.EspA021);
            ex.InvalidOperator.Is("!)");
        }

        [Fact]
        public void PropertyAccess()
        {
            var expr = @"name !=null && name.Length>1";
            string name = "aa";
            var propertyValues = new Dictionary<string, ValueWrapper>();
            var wrapper = new ValueWrapper { Name = "name", Value = name, Type = typeof(string) };
            propertyValues.Add("name", wrapper);
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, propertyValues);
            result.IsTrue();
        }

        [Fact]
        public void PropertAssess_null()
        {
            var expr = @"name !=null && name.Length>1";
            string name = null;
            var propertyValues = new Dictionary<string, ValueWrapper>();
            var wrapper = new ValueWrapper { Name = "name", Value = name, Type = typeof(string) };
            propertyValues.Add("name", wrapper);
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, propertyValues);
            result.IsFalse();
        }

        [Fact]
        public void PropertyAccess_literalLeft()
        {
            var expr = @"null != name && name.Length>1";
            string name = null;
            var propertyValues = new Dictionary<string, ValueWrapper>();
            var wrapper = new ValueWrapper { Name = "name", Value = name, Type = typeof(string) };
            propertyValues.Add("name", wrapper);
            var evaluator = new EasyExpressionEvaluator();
            var ex = Assert.Throws<ExpressionEvaluateException>(() => evaluator.Evaluate(expr, propertyValues));
            ex.IsNotNull();
            ex.MessageId.Is(ExceptionMessageId.EspA003);
        }

        [Fact]
        public void PropertyNotFound()
        {
            var expr = @"name !=null && name.Nothing>1";
            string name = "aa";
            var propertyValues = new Dictionary<string, ValueWrapper>();
            var wrapper = new ValueWrapper { Name = "name", Value = name, Type = typeof(string) };
            propertyValues.Add("name", wrapper);
            var evaluator = new EasyExpressionEvaluator();
            var ex = Assert.Throws<ExpressionEvaluateException>(() => evaluator.Evaluate(expr, propertyValues));
            ex.IsNotNull();
            ex.MessageId.Is(ExceptionMessageId.EspA013);
        }



        [Fact]
        public void NullLeft()
        {
            var expr = @"null == name";
            string name = null;
            var propertyValues = new Dictionary<string, ValueWrapper>();
            var wrapper = new ValueWrapper { Name = "name", Value = name, Type = typeof(string) };
            propertyValues.Add("name", wrapper);
            var evaluator = new EasyExpressionEvaluator();
            var ex = Assert.Throws<ExpressionEvaluateException>(() => evaluator.Evaluate(expr, propertyValues));
            ex.IsNotNull();
            ex.MessageId.Is(ExceptionMessageId.EspA003);
        }

        [Fact]
        public void NullMultiple()
        {
            var expr = @"name == null && name2 == null";
            var propertyValues = new Dictionary<string, ValueWrapper>();
            var wrapper = new ValueWrapper { Name = "name", Value = null, Type = typeof(string) };
            propertyValues.Add("name", wrapper);
            wrapper = new ValueWrapper { Name = "name2", Value = null, Type = typeof(string) };
            propertyValues.Add("name2", wrapper);
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, propertyValues);
            result.IsTrue();
        }

        [Fact]
        public void InvalidLiteralString()
        {
            var expr = @"name == ""bbb";
            var name = "bbb";
            var propertyValues = new Dictionary<string, ValueWrapper>();
            var wrapper = new ValueWrapper { Name = "name", Value = name, Type = typeof(string) };
            propertyValues.Add("name", wrapper);
            var evaluator = new EasyExpressionEvaluator();
            var ex = Assert.Throws<ExpressionEvaluateException>(() => evaluator.Evaluate(expr, propertyValues));
            ex.IsNotNull();
            ex.MessageId.Is(ExceptionMessageId.EspA014);
        }

        [Fact]
        public void InvalidLiteralString2()
        {
            var expr = @"name == ""bbb""""";
            var name = "bbb";
            var propertyValues = new Dictionary<string, ValueWrapper>();
            var wrapper = new ValueWrapper { Name = "name", Value = name, Type = typeof(string) };
            propertyValues.Add("name", wrapper);
            var evaluator = new EasyExpressionEvaluator();
            var ex = Assert.Throws<ExpressionEvaluateException>(() => evaluator.Evaluate(expr, propertyValues));
            ex.IsNotNull();
            ex.MessageId.Is(ExceptionMessageId.EspA014);
        }

        [Fact]
        public void testLong()
        {
            var expr = "value == 1L";
            var value = 1L;
            var propertyValues = new Dictionary<string, ValueWrapper>();
            var wrapper = new ValueWrapper { Name = "value", Value = value, Type = typeof(long) };
            propertyValues.Add("value", wrapper);
            var evaluator = new EasyExpressionEvaluator();
            var result = evaluator.Evaluate(expr, propertyValues);
            result.IsTrue();

        }
    }
}
