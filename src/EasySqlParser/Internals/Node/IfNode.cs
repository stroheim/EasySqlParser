using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      IfNode
    // https://github.com/domaframework/doma
    internal class IfNode : AbstractSqlNode, ISpaceStrippingNode
    {
        internal SqlLocation Location { get; }
        internal string Expression { get; }
        internal string Text { get; }

        internal IfNode(SqlLocation location, string expression, string text)
        {
            Location = location;
            Expression = expression;
            Text = text;
        }
        public void ClearChildren()
        {
            Children.Clear();
        }

        public override TResult Accept<TParameter, TResult>(
            ISqlNodeVisitor<TParameter, TResult> visitor,
            TParameter parameter)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            return visitor.VisitIfNode(this, parameter);
        }
    }
}
