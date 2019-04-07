using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      ElseifNode
    // https://github.com/domaframework/doma
    internal class ElseifNode : AbstractSqlNode, ISpaceStrippingNode
    {

        internal SqlLocation Location { get; }
        internal string Expression { get; }

        internal string Text { get; }

        internal ElseifNode(SqlLocation location, string expression, string text)
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

            return visitor.VisitElseifNode(this, parameter);
        }
    }
}
