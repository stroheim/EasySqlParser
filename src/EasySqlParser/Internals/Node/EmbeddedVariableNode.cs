using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      EmbeddedVariableNode
    // https://github.com/domaframework/doma
    internal class EmbeddedVariableNode : AbstractSqlNode
    {
        internal SqlLocation Location { get; }
        internal string VariableName { get; }
        internal string Text { get; }

        internal EmbeddedVariableNode(SqlLocation location, string variableName, string text)
        {
            Location = location;
            VariableName = variableName;
            Text = text;
        }

        public override TResult Accept<TParameter, TResult>(
            ISqlNodeVisitor<TParameter, TResult> visitor,
            TParameter parameter)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            return visitor.VisitEmbeddedVariableNode(this, parameter);
        }
    }
}
