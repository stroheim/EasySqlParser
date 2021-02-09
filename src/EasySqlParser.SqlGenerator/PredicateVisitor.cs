using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace EasySqlParser.SqlGenerator
{
    // code base from
    // https://www.codeproject.com/Articles/1241363/Expression-Tree-Traversal-Via-Visitor-Pattern-in-P

    public class PredicateVisitor : ExpressionVisitor
    {
        private readonly Dictionary<string, object> _keyValues;
        private readonly Stack<string> _names;
        private readonly Stack<object> _values;

        public PredicateVisitor()
        {
            _keyValues = new Dictionary<string, object>();
            _names = new Stack<string>();
            _values = new Stack<object>();
        }


        public Dictionary<string, object> GetKeyValues(LambdaExpression predicate)
        {
            Visit(predicate.Body);
            var result = new Dictionary<string, object>(_keyValues);
            _keyValues.Clear();
            return result;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Visit(node.Left);
            Visit(node.Right);
            if (_names.Count != 0)
            {
                _keyValues.Add(_names.Pop(), _values.Pop());
            }
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression.NodeType == ExpressionType.Constant ||
                node.Expression.NodeType == ExpressionType.MemberAccess)
            {
                _names.Push(node.Member.Name);
                Visit(node.Expression);
            }
            else
            {
                _names.Push(node.Member.Name);
            }

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _values.Push(GetValue(node.Value));
            return node;
        }


        private object GetValue(object input)
        {
            var type = input.GetType();
            if (type.IsClass && type != typeof(string)
                             && type != typeof(int)
                             && type != typeof(long)
                             && type != typeof(decimal)
                             && type != typeof(byte)
                             && type != typeof(byte[])
                             && type != typeof(float)
                             && type != typeof(double)
                             && type != typeof(sbyte)
                             && type != typeof(short)
                             && type != typeof(uint)
                             && type != typeof(ulong)
                             && type != typeof(ushort)
                             && type != typeof(DateTime)
                             && type != typeof(DateTimeOffset)
                             && type != typeof(TimeSpan))
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
            else
            {
                return input;
            }
        }

    }
}
