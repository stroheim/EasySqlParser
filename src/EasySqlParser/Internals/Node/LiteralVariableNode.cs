using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      LiteralVariableNode
    // https://github.com/domaframework/doma
    internal class LiteralVariableNode : ValueNode
    {
        internal LiteralVariableNode(SqlLocation location, string variableName, string text) : 
            base(location, variableName, text)
        {
        }

        public override TResult Accept<TParameter, TResult>(
            ISqlNodeVisitor<TParameter, TResult> visitor,
            TParameter parameter)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            return visitor.VisitLiteralVariableNode(this, parameter);
        }
    }
}
