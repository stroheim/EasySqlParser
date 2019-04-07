using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      ElseNode
    // https://github.com/domaframework/doma
    internal class ElseNode : AbstractSqlNode, ISpaceStrippingNode
    {
        protected readonly string Text;

        internal ElseNode(string text)
        {
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

            return visitor.VisitElseNode(this, parameter);
        }
    }
}
