using System.Collections.Generic;
using System.Linq.Expressions;

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
            _names.Push(node.Member.Name);
            if (node.Expression.NodeType == ExpressionType.Constant ||
                node.Expression.NodeType == ExpressionType.MemberAccess)
            {
                Visit(node.Expression);
            }
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _values.Push(node.Value);
            return node;
        }

    }
}
