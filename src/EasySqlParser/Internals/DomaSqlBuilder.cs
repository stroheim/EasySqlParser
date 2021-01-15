using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EasySqlParser.Configurations;
using EasySqlParser.Exceptions;
using EasySqlParser.Extensions;
using EasySqlParser.Internals.Helpers;
using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql
    //   class      Context
    // https://github.com/domaframework/doma
    internal class Context
    {
        private static readonly char WhiteSpace = ' ';
        private readonly SqlParserConfig _config;

        private readonly StringBuilder _rawSqlBuilder = new StringBuilder(200);
        private readonly StringBuilder _formattedSqlBuilder = new StringBuilder(200);

        internal Dictionary<string, IDbDataParameter> SqlParameters { get; } =
            new Dictionary<string, IDbDataParameter>();
        internal Context(SqlParserConfig config)
        {
            _config = config;
        }

        internal Context(Context context) :
            this(context._config)
        {

        }

        internal bool IsAvailable { get; set; }

        internal void AppendWhitespaceIfNecessary()
        {
            if (EndsWithWordPart())
            {
                _rawSqlBuilder.Append(WhiteSpace);
                _formattedSqlBuilder.Append(WhiteSpace);
            }
        }

        internal bool EndsWithWordPart()
        {
            if (_rawSqlBuilder.Length == 0)
            {
                return false;
            }

            var c = _rawSqlBuilder[_rawSqlBuilder.Length - 1];
            return SqlTokenHelper.IsWordPart(c);
        }

        internal void AppendRawSql(string sql)
        {
            _rawSqlBuilder.Append(sql);
        }

        internal void AppendFormattedSql(string sql)
        {
            _formattedSqlBuilder.Append(sql);
        }

        internal void CutBackSqlBuilder(int size)
        {
            _rawSqlBuilder.Length = _rawSqlBuilder.Length - size;
        }

        internal void CutBackFormattedSqlBuilder(int size)
        {
            _formattedSqlBuilder.Length = _formattedSqlBuilder.Length - size;
        }

        internal string GetRawSql()
        {
            return _rawSqlBuilder.ToString();
        }

        internal string GetFormattedSql()
        {
            return _formattedSqlBuilder.ToString();
        }


        internal void AddLiteralValue(ValueObject valueObject, string parameterKey, object parameterValue,
            Type parameterType, ValueNode node)
        {

            if (parameterValue == null)
            {
                _rawSqlBuilder.Append("null");
                _formattedSqlBuilder.Append("null");
                return;
            }

            var convertedValue = parameterValue;
            var isDateOnly = false;
            if (valueObject.IsDateTime && node.UseBuiltinFunction)
            {
                convertedValue = valueObject.TruncateTime((DateTime) convertedValue);
                isDateOnly = true;
            }
            if (valueObject.IsDateTimeOffset && node.UseBuiltinFunction)
            {
                convertedValue = valueObject.TruncateTime((DateTimeOffset) convertedValue);
                isDateOnly = true;
            }

            if (valueObject.IsString && node.UseBuiltinFunction)
            {
                switch (node.BuiltinFunctionName)
                {
                    case BuiltinFunctionName.StartsWith:
                        convertedValue = valueObject.GetStartsWithValue((string) convertedValue, node.EscapeChar);
                        break;
                    case BuiltinFunctionName.Contains:
                        convertedValue = valueObject.GetContainsValue((string) convertedValue, node.EscapeChar);
                        break;
                    case BuiltinFunctionName.EndsWith:
                        convertedValue = valueObject.GetEndsWithValue((string)convertedValue, node.EscapeChar);
                        break;
                    case BuiltinFunctionName.Escape:
                        convertedValue = valueObject.GetEscapedValue((string)convertedValue, node.EscapeChar);
                        break;
                }
            }

            if (isDateOnly)
            {
                if (valueObject.IsDateTime)
                {
                    var value = valueObject.ToLogFormatDateOnly((DateTime) convertedValue);
                    _rawSqlBuilder.Append(value);
                    _formattedSqlBuilder.Append(value);
                }

                if (valueObject.IsDateTimeOffset)
                {
                    var value = valueObject.ToLogFormatDateOnly((DateTimeOffset) convertedValue);
                    _rawSqlBuilder.Append(value);
                    _formattedSqlBuilder.Append(value);
                }
            }
            else
            {
                var value = valueObject.ToLogFormat(convertedValue);
                _rawSqlBuilder.Append(value);
                _formattedSqlBuilder.Append(value);
            }

        }

        internal void AppendParameter(ValueObject valueObject, string parameterKey, object parameterValue,
            Type parameterType, ValueNode node)
        {

            var convertedValue = parameterValue;
            // if convertedValue is null, valueObject.IsDateTime / valueObject.IsDateTimeOffset / valueObject.IsString is false
            var isDateOnly = false;
            if (valueObject.IsDateTime && node.UseBuiltinFunction)
            {
                convertedValue = valueObject.TruncateTime((DateTime) convertedValue);
                isDateOnly = true;
            }
            if (valueObject.IsDateTimeOffset && node.UseBuiltinFunction)
            {
                convertedValue = valueObject.TruncateTime((DateTimeOffset)convertedValue);
                isDateOnly = true;
            }

            if (valueObject.IsString && node.UseBuiltinFunction)
            {
                switch (node.BuiltinFunctionName)
                {
                    case BuiltinFunctionName.StartsWith:
                        convertedValue = valueObject.GetStartsWithValue((string)convertedValue, node.EscapeChar);
                        break;
                    case BuiltinFunctionName.Contains:
                        convertedValue = valueObject.GetContainsValue((string)convertedValue, node.EscapeChar);
                        break;
                    case BuiltinFunctionName.EndsWith:
                        convertedValue = valueObject.GetEndsWithValue((string)convertedValue, node.EscapeChar);
                        break;
                    case BuiltinFunctionName.Escape:
                        convertedValue = valueObject.GetEscapedValue((string)convertedValue, node.EscapeChar);
                        break;
                }
            }

            var param = _config.DataParameterCreator()
                .AddName(parameterKey)
                .AddValue(convertedValue ?? DBNull.Value);
            if (valueObject.EasySqlParameterAttribute != null)
            {
                param.AddDbType(valueObject.EasySqlParameterAttribute.DbType);
            }

            if (!SqlParameters.ContainsKey(parameterKey))
            {
                SqlParameters.Add(parameterKey, param);
            }

            // parameterKey is sql parameter name
            var localParameterKey = parameterKey;
            if (!_config.Dialect.EnableNamedParameter)
            {
                localParameterKey = _config.Dialect.ParameterPrefix;
            }

            _rawSqlBuilder.Append(localParameterKey);
            // if convertedValue is null, always isDateOnly is false
            if (isDateOnly)
            {
                if (valueObject.IsDateTime)
                {
                    _formattedSqlBuilder.Append(valueObject.ToLogFormatDateOnly((DateTime)convertedValue));
                }

                if (valueObject.IsDateTimeOffset)
                {
                    _formattedSqlBuilder.Append(valueObject.ToLogFormatDateOnly((DateTimeOffset)convertedValue));
                }
            }
            else
            {
                _formattedSqlBuilder.Append(valueObject.ToLogFormat(convertedValue));
            }
        }


        internal void AddAllParameters(Dictionary<string, IDbDataParameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                if (!SqlParameters.ContainsKey(parameter.Key))
                {
                    SqlParameters.Add(parameter.Key, parameter.Value);
                }
            }
        }

    }


    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql
    //   class      NodePreparedSqlBuilder
    // https://github.com/domaframework/doma
    internal class DomaSqlBuilder: ISqlNodeVisitor<Context, object>
    {
        private readonly SqlParserConfig _config;
        private readonly ISqlNode _node;
        private readonly Dictionary<string, ValueWrapper> _propertyValues;
        private readonly Dictionary<string, ValueObject> _valueObjects;
        internal EasyExpressionEvaluator ExpressionEvaluator { get; }
        //private readonly EasyExpressionEvaluator _evaluator;

        // for unit test
        internal DomaSqlBuilder(ISqlNode node, List<ParameterEmulator> emulators,
            SqlParserConfig config = null, string sqlFilePath = null)
        {
            _node = node;
            _config = config ?? ConfigContainer.DefaultConfig;
            var valueObjectWrapper = ValueObjectCache.GetValueObjects(emulators, _config);
            _propertyValues = valueObjectWrapper.PropertyValues;
            _valueObjects = valueObjectWrapper.ValueObjects;
            if (string.IsNullOrEmpty(sqlFilePath))
            {
                ExpressionEvaluator = new EasyExpressionEvaluator();
            }
            else
            {
                ExpressionEvaluator = new EasyExpressionEvaluator(sqlFilePath);
            }
        }

        internal DomaSqlBuilder(ISqlNode node, object model,
            SqlParserConfig config , EasyExpressionEvaluator evaluator)
        {
            _node = node;
            _config = config;
            var valueObjectWrapper = ValueObjectCache.GetValueObjects(model, _config);
            _propertyValues = valueObjectWrapper.PropertyValues;
            _valueObjects = valueObjectWrapper.ValueObjects;
            ExpressionEvaluator = evaluator;
        }

        internal DomaSqlBuilder(ISqlNode node, string name, object value,
            SqlParserConfig config, EasyExpressionEvaluator evaluator)
        {
            _node = node;
            _config = config;
            var valueObjectWrapper = ValueObjectCache.GetValueObjects(name, value, _config);
            _propertyValues = valueObjectWrapper.PropertyValues;
            _valueObjects = valueObjectWrapper.ValueObjects;
            ExpressionEvaluator = evaluator;
        }

        internal SqlParserResult Build()
        {
            var context = new Context(_config);
            _node.Accept(this, context);
            var result = new SqlParserResult();
            result.ParsedSql = context.GetRawSql().Trim();
            result.DebugSql = context.GetFormattedSql().Trim();
            result.DbDataParameters = context.SqlParameters.Values.ToList();
            return result;
        }


        public object VisitAnonymousNode(AnonymousNode node, Context parameter)
        {
            foreach (var child in node.Children)
            {
                child.Accept(this, parameter);
            }

            return null;
        }

        public object VisitBindVariableNode(BindVariableNode node, Context parameter)
        {
            return VisitValueNode(node, parameter, parameter.AppendParameter);
        }

        private object VisitValueNode(ValueNode node, Context parameter,
            Action<ValueObject, string, object, Type, ValueNode> action)
        {
            parameter.IsAvailable = true;
            if (node.IsWordNodeIgnored)
            {
                HandleSingleValueNode(node, parameter, action);
            }
            else if (node.IsParensNodeIgnored)
            {
                var parensNode = node.ParensNode;
                var openedNode = parensNode.OpenedParensNode;
                openedNode.Accept(this, parameter);
                HandleIterableValueNode(node, parameter, action);

                var closedNode = parensNode.ClosedParensNode;
                closedNode.Accept(this, parameter);
            }

            return null;
        }

        // adjust parameter signature [HandleIterableValueNode]
        // ReSharper disable once UnusedParameter.Local
        private void HandleSingleValueNode(ValueNode node, Context parameter,
            Action<ValueObject, string, object, Type, ValueNode> action)
        {
            var name = node.VariableName;
            if (!_valueObjects.ContainsKey(name))
            {
                throw new ArgumentException($"Invalid argument found {name}");
            }

            var valueObject = _valueObjects[name];
            var param = valueObject.DbParameters.First();
            action(valueObject, param.Key, param.Value, valueObject.DataType, node);
        }

        private void HandleIterableValueNode(ValueNode node, Context parameter,
            Action<ValueObject, string, object, Type, ValueNode> action)
        {
            var name = node.VariableName;
            if (!_valueObjects.ContainsKey(name))
            {
                throw new ArgumentException($"Invalid argument found {name}");
            }

            var valueObject = _valueObjects[name];
            if (!valueObject.IsEnumerable)
            {
                var location = node.Location;
                throw new SqlBuildException(ExceptionMessageId.Esp2112, location.Sql, location.LineNumber,
                    location.Position, node.Text, valueObject.DataType);
            }

            var index = 0;
            foreach (var dbParameter in valueObject.DbParameters)
            {
                if (dbParameter.Value == null)
                {
                    var location = node.Location;
                    throw new SqlBuildException(ExceptionMessageId.Esp2115, location.Sql, location.LineNumber,
                        location.Position, node.Text, index);
                }

                action(valueObject, dbParameter.Key, dbParameter.Value, valueObject.GenericParameterType, node);
                parameter.AppendRawSql(", ");
                parameter.AppendFormattedSql(", ");
                index++;
            }

            if (index == 0)
            {
                parameter.AppendRawSql("null");
                parameter.AppendFormattedSql("null");
            }
            else
            {
                parameter.CutBackSqlBuilder(2);
                parameter.CutBackFormattedSqlBuilder(2);
            }
        }

        public object VisitCommentNode(CommentNode node, Context parameter)
        {
            var comment = node.Comment;
            parameter.AppendRawSql(comment);
            parameter.AppendFormattedSql(comment);
            return null;
        }

        public object VisitElseifNode(ElseifNode node, Context parameter)
        {
            foreach (var child in node.Children)
            {
                child.Accept(this, parameter);
            }

            return null;
        }

        public object VisitElseNode(ElseNode node, Context parameter)
        {
            foreach (var child in node.Children)
            {
                child.Accept(this, parameter);
            }

            return null;
        }

        public object VisitEmbeddedVariableNode(EmbeddedVariableNode node, Context parameter)
        {
            var location = node.Location;
            var name = node.VariableName;
            if (!_valueObjects.ContainsKey(name))
            {
                throw new ArgumentException($"Invalid argument found {name}");
            }
            var value = _propertyValues[name];
            //var isValid = _evaluator.Evaluate(name, _propertyValues);
            if (value != null && value.Value != null)
            {
                var fragment = value.Value.ToString();
                if (fragment.IndexOf('\'') > -1)
                {
                    throw new SqlBuildException(ExceptionMessageId.Esp2116, location.Sql, location.LineNumber,
                        location.Position, node.Text);
                }

                if (fragment.IndexOf(';') > -1)
                {
                    throw new SqlBuildException(ExceptionMessageId.Esp2117, location.Sql, location.LineNumber,
                        location.Position, node.Text);
                }

                if (fragment.IndexOf("--", StringComparison.Ordinal) > -1)
                {
                    throw new SqlBuildException(ExceptionMessageId.Esp2122, location.Sql, location.LineNumber,
                        location.Position, node.Text);
                }

                if (fragment.IndexOf("/*", StringComparison.Ordinal) > -1)
                {
                    throw new SqlBuildException(ExceptionMessageId.Esp2123, location.Sql, location.LineNumber,
                        location.Position, node.Text);
                }

                if (!StartWithClauseKeyword(fragment))
                {
                    parameter.IsAvailable = true;
                }

                parameter.AppendRawSql(fragment);
                parameter.AppendFormattedSql(fragment);
            }

            foreach (var child in node.Children)
            {
                child.Accept(this, parameter);
            }

            return null;
        }

        private const string ClauseKeywordPattern = "(select|from|where|group by|having|order by|for update)";

        private static readonly Regex ClauseKeywordRegex =
            new Regex(ClauseKeywordPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private bool StartWithClauseKeyword(string fragment)
        {
            return ClauseKeywordRegex.IsMatch(SqlTokenHelper.TrimWhitespace(fragment));
        }

        public object VisitEndNode(EndNode node, Context parameter)
        {
            foreach (var child in node.Children)
            {
                child.Accept(this, parameter);
            }

            return null;
        }

        public object VisitEolNode(EolNode node, Context parameter)
        {
            var eol = node.Eol;
            parameter.AppendRawSql(eol);
            parameter.AppendFormattedSql(eol);
            return null;
        }

        public object VisitExpandNode(ExpandNode node, Context parameter)
        {
            throw new UnsupportedSqlCommentException(ExceptionMessageId.EspB001, nameof(node));
        }

        public object VisitForBlockNode(ForBlockNode node, Context parameter)
        {
            throw new UnsupportedSqlCommentException(ExceptionMessageId.EspB001, nameof(node));
        }

        public object VisitForNode(ForNode node, Context parameter)
        {
            foreach (var child in node.Children)
            {
                child.Accept(this, parameter);
            }

            return null;
        }

        public object VisitForUpdateClauseNode(ForUpdateClauseNode node, Context parameter)
        {
            var wordNode = node.WordNode;
            wordNode.Accept(this, parameter);
            foreach (var child in node.Children)
            {
                child.Accept(this, parameter);
            }

            return null;
        }

        public object VisitFragmentNode(FragmentNode node, Context parameter)
        {
            parameter.IsAvailable = true;
            var fragment = node.Fragment;
            parameter.AppendRawSql(fragment);
            parameter.AppendFormattedSql(fragment);
            return null;
        }

        public object VisitFromClauseNode(FromClauseNode node, Context parameter)
        {
            var wordNode = node.WordNode;
            wordNode.Accept(this, parameter);
            foreach (var child in node.Children)
            {
                child.Accept(this, parameter);
            }

            return null;
        }

        public object VisitGroupByClauseNode(GroupByClauseNode node, Context parameter)
        {
            var wordNode = node.WorNode;
            wordNode.Accept(this, parameter);
            foreach (var child in node.Children)
            {
                child.Accept(this, parameter);
            }

            return null;
        }

        public object VisitHavingClauseNode(HavingClauseNode node, Context parameter)
        {
            return HandleConditionalClauseNode(node, parameter);
        }

        private object HandleConditionalClauseNode(IClauseNode node, Context parameter)
        {
            var context = new Context(parameter);
            foreach (var child in node.Children)
            {
                child.Accept(this, context);
            }

            if (context.IsAvailable)
            {
                node.WordNode.Accept(this, parameter);
                parameter.IsAvailable = true;
                parameter.AppendRawSql(context.GetRawSql());
                parameter.AppendFormattedSql(context.GetFormattedSql());
                parameter.AddAllParameters(context.SqlParameters);
            }
            else
            {
                var fragment = context.GetRawSql();
                if (StartWithClauseKeyword(fragment))
                {
                    parameter.IsAvailable = true;
                    parameter.AppendRawSql(context.GetRawSql());
                    parameter.AppendFormattedSql(context.GetFormattedSql());
                    parameter.AddAllParameters(context.SqlParameters);
                }
            }

            return null;
        }

        public object VisitIfBlockNode(IfBlockNode node, Context parameter)
        {
            if (!HandleIfNode(node, parameter))
            {
                if (!HandleElseifNode(node, parameter))
                {
                    HandleElseNode(node, parameter);
                }

            }

            var endNode = node.EndNode;
            return endNode.Accept(this, parameter);
        }

        private bool HandleIfNode(IfBlockNode node, Context parameter)
        {
            var ifNode = node.IfNode;
            var expression = ifNode.Expression;
            var isValid = ExpressionEvaluator.Evaluate(expression, _propertyValues);
            if (isValid)
            {
                ifNode.Accept(this, parameter);
                return true;
            }

            return false;
        }

        private bool HandleElseifNode(IfBlockNode node, Context parameter)
        {
            foreach (var elseifNode in node.ElseifNodes)
            {
                var expression = elseifNode.Expression;
                var isValid = ExpressionEvaluator.Evaluate(expression, _propertyValues);
                if (isValid)
                {
                    elseifNode.Accept(this, parameter);
                    return true;
                }
            }

            return false;
        }

        private void HandleElseNode(IfBlockNode node, Context parameter)
        {
            var elseNode = node.ElseNode;
            elseNode?.Accept(this, parameter);
        }

        public object VisitIfNode(IfNode node, Context parameter)
        {
            foreach (var child in node.Children)
            {
                child.Accept(this, parameter);
            }

            return null;
        }

        public object VisitLiteralVariableNode(LiteralVariableNode node, Context parameter)
        {
            if (node.Text.IndexOf('\'') > -1)
            {
                var location = node.Location;
                throw new SqlBuildException(ExceptionMessageId.Esp2224, location.Sql, location.LineNumber,
                    location.Position, node.Text);
            }
            return VisitValueNode(node, parameter, parameter.AddLiteralValue);
        }

        public object VisitLogicalOperatorNode(LogicalOperatorNode node, Context parameter)
        {
            if (parameter.IsAvailable)
            {
                var wordNode = node.WordNode;
                wordNode.Accept(this, parameter);
            }

            foreach (var child in node.Children)
            {
                child.Accept(this, parameter);
            }
            return null;
        }

        public object VisitOptionClauseNode(OptionClauseNode node, Context parameter)
        {
            var wordNode = node.WordNode;
            wordNode.Accept(this, parameter);
            foreach (var child in node.Children)
            {
                child.Accept(this, parameter);
            }
            return null;
        }

        public object VisitOrderByClauseNode(OrderByClauseNode node, Context parameter)
        {
            var wordNode = node.WordNode;
            wordNode.Accept(this, parameter);
            foreach (var child in node.Children)
            {
                child.Accept(this, parameter);
            }
            return null;
        }

        public object VisitOtherNode(OtherNode node, Context parameter)
        {
            parameter.IsAvailable = true;
            var other = node.Other;
            parameter.AppendRawSql(other);
            parameter.AppendFormattedSql(other);
            return null;
        }

        public object VisitParensNode(ParensNode node, Context parameter)
        {
            if (node.IsAttachedWithValue)
            {
                return null;
            }

            var context = new Context(parameter);
            if (node.IsEmpty)
            {
                context.IsAvailable = true;
            }

            foreach (var child in node.Children)
            {
                child.Accept(this, context);
            }

            if (context.IsAvailable)
            {
                node.OpenedParensNode.Accept(this, parameter);
                parameter.IsAvailable = true;
                parameter.AppendRawSql(context.GetRawSql());
                parameter.AppendFormattedSql(context.GetFormattedSql());
                parameter.AddAllParameters(context.SqlParameters);
                node.ClosedParensNode.Accept(this, parameter);
            }
            return null;
        }

        public object VisitPopulateNode(PopulateNode node, Context parameter)
        {
            throw new UnsupportedSqlCommentException(ExceptionMessageId.EspB001, nameof(node));
        }

        public object VisitSelectClauseNode(SelectClauseNode node, Context parameter)
        {
            var wordNode = node.WordNode;
            wordNode.Accept(this, parameter);
            foreach (var child in node.Children)
            {
                child.Accept(this, parameter);
            }
            return null;
        }

        public virtual object VisitSelectStatementNode(SelectStatementNode node, Context parameter)
        {
            foreach (var child in node.Children)
            {
                child.Accept(this, parameter);
            }
            return null;
        }

        public object VisitSetClauseNode(SetClauseNode node, Context parameter)
        {
            var wordNode = node.WordNode;
            wordNode.Accept(this, parameter);
            foreach (var child in node.Children)
            {
                child.Accept(this, parameter);
            }
            return null;
        }

        public object VisitUpdateClauseNode(UpdateClauseNode node, Context parameter)
        {
            var wordNode = node.WordNode;
            wordNode.Accept(this, parameter);
            foreach (var child in node.Children)
            {
                child.Accept(this, parameter);
            }
            return null;
        }

        public object VisitUpdateStatementNode(UpdateStatementNode node, Context parameter)
        {
            foreach (var child in node.Children)
            {
                child.Accept(this, parameter);
            }
            return null;
        }

        public object VisitWhereClauseNode(WhereClauseNode node, Context parameter)
        {
            return HandleConditionalClauseNode(node, parameter);
        }


        public object VisitWhitespaceNode(WhitespaceNode node, Context parameter)
        {
            var whitespace = node.Whitespace;
            parameter.AppendRawSql(whitespace);
            parameter.AppendFormattedSql(whitespace);
            return null;
        }

        public object VisitWordNode(WordNode node, Context parameter)
        {
            parameter.IsAvailable = true;
            var word = node.Word;
            if (node.IsReserved)
            {
                parameter.AppendWhitespaceIfNecessary();
            }
            parameter.AppendRawSql(word);
            parameter.AppendFormattedSql(word);
            return null;
        }
    }
}
