using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      EndNode
    // https://github.com/domaframework/doma
    internal class EndNode : AbstractSqlNode, ISpaceStrippingNode
    {
        protected string Text;

        internal EndNode(string text)
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

            return visitor.VisitEndNode(this, parameter);
        }
    }
}
