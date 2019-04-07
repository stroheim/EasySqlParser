using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      ForNode
    // https://github.com/domaframework/doma
    internal class ForNode : AbstractSqlNode, ISpaceStrippingNode
    {
        internal SqlLocation Location { get; }
        internal string Identifier { get; }
        internal string Expression { get; }
        internal string Text { get; }

        internal ForNode(SqlLocation location, string identifier, string expression, string text)
        {
            Location = location;
            Identifier = identifier;
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

            return visitor.VisitForNode(this, parameter);
        }
    }
}
