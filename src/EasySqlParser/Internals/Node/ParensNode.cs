using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      ParensNode
    // https://github.com/domaframework/doma
    internal class ParensNode : AbstractSqlNode
    {
        internal SqlLocation Location { get; }
        internal bool IsAttachedWithValue { get; set; }
        internal bool IsEmpty { get; set; } = true;
        internal OtherNode OpenedParensNode { get; }
        internal OtherNode ClosedParensNode { get; private set; }

        internal ParensNode(SqlLocation location)
        {
            Location = location;
            OpenedParensNode=OtherNode.Of("(");
        }

        internal void Close()
        {
            ClosedParensNode=OtherNode.Of(")");
        }

        public override TResult Accept<TParameter, TResult>(
            ISqlNodeVisitor<TParameter, TResult> visitor,
            TParameter parameter)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            return visitor.VisitParensNode(this, parameter);
        }
    }
}
