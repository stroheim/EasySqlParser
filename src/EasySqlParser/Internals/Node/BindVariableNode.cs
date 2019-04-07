using System;

namespace EasySqlParser.Internals.Node
{

    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      BindVariableNode
    // https://github.com/domaframework/doma
    internal class BindVariableNode : ValueNode
    {
        internal BindVariableNode(SqlLocation location, string variableName, string text) : 
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

            return visitor.VisitBindVariableNode(this, parameter);
        }
    }
}
