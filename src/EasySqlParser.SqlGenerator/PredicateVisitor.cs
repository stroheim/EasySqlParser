using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using EasySqlParser.Configurations;
using EasySqlParser.Extensions;
using EasySqlParser.SqlGenerator.Metadata;

namespace EasySqlParser.SqlGenerator
{
    // code base from
    // https://www.codeproject.com/Articles/1241363/Expression-Tree-Traversal-Via-Visitor-Pattern-in-P
    // AND
    // https://github.com/xin9le/DeclarativeSql/blob/master/src/DeclarativeSql/Sql/Clauses/Where.cs

    internal class PredicateVisitor : ExpressionVisitor
    {


        private readonly QueryStringBuilder _builder;
        private readonly EntityTypeInfo _entityInfo;
        private readonly Stack<string> _names;
        private ParameterExpression _lambdaParameterExpression;
        private int _complexityLevel;
        internal Dictionary<string, object> ParameterObjects { get; }

        private const string CompareString = "CompareString";


        internal PredicateVisitor(QueryStringBuilder builder, EntityTypeInfo entityInfo)
        {
            ParameterObjects = new Dictionary<string, object>();
            _builder = builder;
            _entityInfo = entityInfo;
            _builder.AppendLine(" ");
            _builder.AppendLine("WHERE ");
            _names = new Stack<string>();
        }

        internal void BuildPredicate(LambdaExpression predicate)
        {
            _lambdaParameterExpression = predicate.Parameters[0];
            if (_builder.WriteIndented)
            {
                _builder.AppendIndent(5);
            }
            Visit(predicate.Body);
            //foreach (var pair in ParameterObjects)
            //{
            //    _builder.AppendParameter(pair.Key, pair.Value);
            //}
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType != ExpressionType.Not)
                throw new NotSupportedException("Only not(\"!\") unary operator is supported!");
            return VisitNotUnary(node.Operand);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    return VisitAndAlsoOrElse(node);
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    if (TryGetMemberName(node.Left, out var leftName))
                    {
                        _names.Push(leftName);
                        return VisitConstantWithName(node.Right, leftName, leftName, 
                            GetOperand(node.NodeType));
                    }

                    if (TryGetMemberName(node.Right, out var rightName))
                    {
                        _names.Push(rightName);
                        return VisitConstantWithName(node.Left, rightName, rightName, 
                            GetOperand(node.NodeType));

                    }

                    if (IsVbCompareString(node.Left))
                    {
                        return VisitVbCompareString(node.Left, GetOperand(node.NodeType));
                    }

                    throw new NotSupportedException("");
                
            }

            throw new NotSupportedException("");
            //return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            return VisitBoolean(node, false);
        }

        //protected override Expression VisitConstant(ConstantExpression node)
        //{
        //    _values.Push(GetValue(node.Value));
        //    return node;
        //}

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return VisitMethodCall(node, false);
        }

        private (object value, bool isIEnumerable) GetValue(object input)
        {
            if (input == null)
            {
                return (null, false);
            }
            var type = input.GetType();
            if (type == typeof(string))
            {
                return (input, false);
            }
            if (input is IEnumerable)
            {
                return (input, true);

            }
            if (!type.IsEspKnownType())
            {
                var name = _names.Pop();
                var fieldInfo = type.GetField(name);
                object value;
                if (fieldInfo != null)
                {
                    value = fieldInfo.GetValue(input);
                }
                else
                {
                    value = type.GetProperty(name)?.GetValue(input);
                }

                return GetValue(value);

            }

            return (input, false);
        }

        private Expression VisitNotUnary(Expression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return VisitBoolean((MemberExpression) node, true);
                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)node, true);

            }

            return node;
        }

        private Expression VisitBoolean(MemberExpression node, bool isNotType)
        {
            var name = node.Member.Name;
            _names.Push(name);
            var parameterName = $"p_{name}";

            Type type = null;
            if (node.Member is PropertyInfo propertyInfo)
            {
                type = propertyInfo.PropertyType;
            }else if (node.Member is FieldInfo fieldInfo)
            {
                type = fieldInfo.FieldType;
            }

            if (type == null)
            {
                throw new InvalidCastException("");
            }



            var columnInfo = _entityInfo.Columns.Single(x => x.PropertyInfo.Name == name);
            _builder.AppendColumn(columnInfo);
            _builder.AppendSql(" = ");
            if (isNotType)
            {
                //_builder.AppendSql($"{name} = {parameterName}");
                //ParameterObjects.Add(parameterName, 0);
                _builder.AppendParameter(parameterName, false);
                return node;
            }
            if (node.Expression.NodeType == ExpressionType.Parameter
                && type == typeof(bool))
            {
                //_builder.AppendSql($"{name} = {parameterName}");
                //ParameterObjects.Add(parameterName, 1);
                _builder.AppendParameter(parameterName, true);
                return node;
            }

            throw new NotSupportedException("");
        }

        private bool TryGetMemberName(Expression expression, out string name)
        {
            if (expression is MemberExpression memberExpression)
            {
                if (memberExpression.Expression == _lambdaParameterExpression)
                {
                    name = memberExpression.Member.Name;
                    return true;
                }
            }

            if (expression.NodeType == ExpressionType.Convert && expression is UnaryExpression unary)
            {
                return TryGetMemberName(unary.Operand, out name);
            }

            name = null;
            return false;
        }


        private Expression VisitMethodCall(MethodCallExpression node, bool isNotType)
        {
            switch (node.Method.Name)
            {
                case nameof(string.Contains):
                    return VisitContains(node, isNotType);
                case nameof(string.StartsWith):
                    return VisitStringStartsWith(node, isNotType);
                case nameof(string.EndsWith):
                    return VisitStringEndsWith(node, isNotType);
                case nameof(string.IsNullOrEmpty):
                    return VisitStringIsNullOrEmpty(node, isNotType);
                case nameof(string.Equals):
                    return VisitStringEquals(node, isNotType);
                case CompareString:
                    if (IsVbCompareString(node))
                    {
                        return VisitVbCompareString(node, isNotType ? " <> " : " = ");
                    }
                    break;
            }

            return node;
        }


        private static bool IsVbCompareString(Expression node)
        {
            if (node is MethodCallExpression methodCallExpression)
            {
                return (methodCallExpression.Method.Name == CompareString
                        && methodCallExpression.Method.DeclaringType?.Name == "Operators"
                        && methodCallExpression.Method.DeclaringType?.Namespace == "Microsoft.VisualBasic.CompilerServices"
                        && methodCallExpression.Object == null
                        && methodCallExpression.Arguments.Count == 3);

            }

            return false;
        }


        // VB.NET CompareString
        private Expression VisitVbCompareString(Expression node, string operand)
        {
            var methodCallExpression = (MethodCallExpression)node;
            if (TryGetMemberName(methodCallExpression.Arguments[0], out var name))
            {
                _names.Push(name);
                VisitConstantWithName(methodCallExpression.Arguments[1], name, name, operand);
            }

            return node;
        }


        private Expression VisitContains(MethodCallExpression node, bool isNotType)
        {
            bool IsStaticContains(MethodCallExpression expression)
            {
                return expression.Object == null && expression.Arguments.Count == 2;
            }

            bool IsInstanceContains(MethodCallExpression expression)
            {
                return expression.Object != null
                       && (
                           expression.Object.Type == typeof(IEnumerable)
                           || expression.Object.Type.GetInterfaces().Contains(typeof(IEnumerable))
                       )
                       && expression.Object.Type != typeof(string)
                       && expression.Arguments.Count == 1;
            }

            if (IsStaticContains(node))
            {
                if (TryGetMemberName(node.Arguments[1], out var name))
                {
                    var operand = isNotType ? " NOT IN (" : " IN (";
                    _names.Push(name);
                    VisitConstantWithName(node.Arguments[0], name, name, operand);
                    _builder.AppendSql(") ");
                    return node;
                }
                return node;

            }

            if (IsInstanceContains(node))
            {
                if (TryGetMemberName(node.Arguments[0], out var name))
                {
                    var operand = isNotType ? " NOT IN (" : " IN (";
                    _names.Push(name);
                    VisitConstantWithName(node.Object, name, name, operand);
                    _builder.AppendSql(") ");
                    return node;
                }
                return node;
            }

            return VisitStringContains(node, isNotType);
        }

        private Expression VisitStringEquals(MethodCallExpression node, bool isNotType)
        {
            Console.WriteLine(node.Method.Name);
            if (node.Object == null || node.Object.Type != typeof(string)) return node;
            if (TryGetMemberName(node.Object, out var name))
            {
                var operand = isNotType ? " <> " : " = ";
                _names.Push(name);
                return VisitConstantWithName(node.Arguments[0], name, name, operand);
            }

            return node;
        }

        private Expression VisitStringContains(MethodCallExpression node, bool isNotType)
        {
            if (node.Object == null || node.Object.Type != typeof(string)) return node;
            if (TryGetMemberName(node.Object, out var name))
            {
                var operand = isNotType ? " NOT LIKE " : " LIKE ";
                _names.Push(name);
                return VisitConstantWithName(node.Arguments[0], name, name, operand);
            }
            return node;
        }

        private Expression VisitStringStartsWith(MethodCallExpression node, bool isNotType)
        {
            if (node.Object != null && node.Object.Type != typeof(string)) return node;
            if (TryGetMemberName(node.Object, out var name))
            {
                var operand = isNotType ? " NOT LIKE " : " LIKE ";
                _names.Push(name);
                return VisitConstantWithName(node.Arguments[0], name, name, operand);
            }
            return node;
        }

        private Expression VisitStringEndsWith(MethodCallExpression node, bool isNotType)
        {
            if (node.Object != null && node.Object.Type != typeof(string)) return node;
            if (TryGetMemberName(node.Object, out var name))
            {
                var operand = isNotType ? " NOT LIKE " : " LIKE ";
                _names.Push(name);
                return VisitConstantWithName(node.Arguments[0], name, name, operand);
            }

            return node;
        }

        private Expression VisitStringIsNullOrEmpty(MethodCallExpression node, bool isNotType)
        {
            if (node.Object != null && node.Arguments.Count != 1) return node;
            if (TryGetMemberName(node.Arguments[0], out var name))
            {
                var columnInfo = _entityInfo.Columns.Single(x => x.PropertyInfo.Name == name);
                _builder.AppendColumn(columnInfo);
                _builder.AppendSql(" IS");
                if (isNotType)
                {
                    _builder.AppendSql(" NOT");
                }
                _builder.AppendSql(" NULL");
                if (isNotType)
                {
                    _builder.AppendSql(" AND ");
                }
                else
                {
                    _builder.AppendSql(" OR ");
                }
                _builder.AppendColumn(columnInfo);
                if (isNotType)
                {
                    _builder.AppendSql(" <> ''");
                }
                else
                {
                    _builder.AppendSql(" = ''");
                }

                //if (isNotType)
                //{
                //    _builder.AppendSql($"{name} IS NOT NULL AND {name} <> ''");
                //}
                //else
                //{
                //    _builder.AppendSql($"{name} IS NULL OR {name} = ''");
                //}
            }

            return node;
        }

        private Expression VisitAndAlsoOrElse(BinaryExpression expression)
        {

            var selfOperand = GetSupportedOperand(expression.NodeType);

            var leftOperand = GetSupportedOperand(expression.Left.NodeType);
            var requireBrackets = RequiresBrackets(selfOperand, leftOperand);
            if (requireBrackets)
            {
                _builder.AppendSql("(");
                _complexityLevel++;
            }

            Visit(expression.Left);

            if (requireBrackets)
            {
                _builder.AppendSql(")");
                _complexityLevel--;
            }

            if (_complexityLevel == 0)
            {
                _builder.AppendLine();
                if (_builder.WriteIndented && selfOperand == SupportedOperand.OrElse)
                {
                    _builder.AppendSql(" ");
                }
            }

            if (selfOperand == SupportedOperand.AndAlso)
            {
                _builder.AppendSql(" AND ");
            }

            if (selfOperand == SupportedOperand.OrElse)
            {
                _builder.AppendSql(" OR ");
            }

            var rightOperand = GetSupportedOperand(expression.Right.NodeType);
            requireBrackets = RequiresBrackets(selfOperand, rightOperand);
            if (requireBrackets)
            {
                _builder.AppendSql("(");
                _complexityLevel++;
            }

            Visit(expression.Right);

            if (requireBrackets)
            {
                _builder.AppendSql(")");
                _complexityLevel--;
            }


            bool RequiresBrackets(SupportedOperand self, SupportedOperand side)
            {
                var result = (self != side) && (side == SupportedOperand.AndAlso || side == SupportedOperand.OrElse);
                return result;
            }

            return expression;

        }

        private void CreateInParameters(string parameterName, IEnumerable values)
        {
            var index = 1;
            foreach (var value in values)
            {
                if (value == null) continue;
                var name = $"p_in_{parameterName}{index}";
                //ParameterObjects.Add(name, value);
                //_builder.AppendSql(name);
                _builder.AppendParameter(name, value);
                _builder.AppendSql(", ");
                index++;
            }

            if (index > 1)
            {
                _builder.CutBack(2);
                return;
            }

            throw new InvalidOperationException("IEnumerable value is empty");
        }

        private Expression VisitConstantWithName(Expression node, string innerMemberName, 
            string outerMemberName, string operand)
        {
            if (node is ConstantExpression constantExpression)
            {
                _names.Push(innerMemberName);
                var (value, isIEnumerable) = GetValue(constantExpression.Value);
                var columnInfo = _entityInfo.Columns.Single(x => x.PropertyInfo.Name == outerMemberName);
                _builder.AppendColumn(columnInfo);
                //_builder.AppendSql(outerMemberName);
                if (value == null)
                {
                    if (operand == " = ")
                    {
                        _builder.AppendSql(" IS NULL ");
                    }
                    else
                    {
                        _builder.AppendSql(" IS NOT NULL ");
                    }
                    return constantExpression;
                }
                _builder.AppendSql(operand);
                if (isIEnumerable)
                {
                    // 配列の処理
                    CreateInParameters(outerMemberName, (IEnumerable) value);
                    return constantExpression;
                }

                var parameterName = "p_" + outerMemberName;
                if (!_builder.HasSameParameterName(parameterName))
                {
                    _builder.AppendParameter(parameterName, value);
                    return constantExpression;
                }
                //if (!ParameterObjects.ContainsKey(parameterName))
                //{
                //    _builder.AppendSql(parameterName);
                //    ParameterObjects.Add(parameterName, value);
                //    return constantExpression;
                //}
                var index = 1;
                var localParameterName = $"{parameterName}_{index}";
                while (_builder.HasSameParameterName(localParameterName))
                {
                    localParameterName = $"{parameterName}_{index}";
                    index++;
                }
                _builder.AppendParameter(localParameterName, value);
                //_builder.AppendSql(parameterName);
                //ParameterObjects.Add(parameterName, value);
                return constantExpression;
            }

            if (node is MemberExpression memberExpression)
            {
                return VisitConstantWithName(memberExpression.Expression, 
                    memberExpression.Member.Name, outerMemberName, operand);
            }


            return node;
        }

        private static string GetOperand(ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.Equal:
                    return " = ";
                case ExpressionType.NotEqual:
                    return " <> ";
                case ExpressionType.GreaterThan:
                    return " > ";
                case ExpressionType.GreaterThanOrEqual:
                    return " >= ";
                case ExpressionType.LessThan:
                    return " < ";
                case ExpressionType.LessThanOrEqual:
                    return " <= ";
                case ExpressionType.AndAlso:
                    return " AND ";
                case ExpressionType.OrElse:
                    return " OR ";
                default:
                    return expressionType.ToString();
            }
        }

        private static SupportedOperand GetSupportedOperand(ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.AndAlso:
                    return SupportedOperand.AndAlso;
                case ExpressionType.OrElse:
                    return SupportedOperand.OrElse;
                case ExpressionType.LessThan:
                    return SupportedOperand.LessThan;
                case ExpressionType.LessThanOrEqual:
                    return SupportedOperand.LessThanOrEqual;
                case ExpressionType.GreaterThan:
                    return SupportedOperand.GreaterThan;
                case ExpressionType.GreaterThanOrEqual:
                    return SupportedOperand.GreaterThanOrEqual;
                case ExpressionType.Equal:
                    return SupportedOperand.Equal;
                case ExpressionType.NotEqual:
                    return SupportedOperand.NotEqual;
                default:
                    return SupportedOperand.Unknown;
            }
        }

        private enum SupportedOperand
        {
            Unknown,
            AndAlso,
            OrElse,
            LessThan,
            LessThanOrEqual,
            GreaterThan,
            GreaterThanOrEqual,
            Equal,
            NotEqual
        }
    }

}
