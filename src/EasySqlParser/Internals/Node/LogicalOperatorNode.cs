using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      LogicalOperatorNode
    // https://github.com/domaframework/doma
    internal class LogicalOperatorNode : AbstractSqlNode
    {
        internal WordNode WordNode { get; }

        internal LogicalOperatorNode(string word) :
            this(new WordNode(word, true))
        {

        }
        internal LogicalOperatorNode(WordNode wordNode)
        {
            WordNode = wordNode;
        }

        public override TResult Accept<TParameter, TResult>(
            ISqlNodeVisitor<TParameter, TResult> visitor,
            TParameter parameter)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            return visitor.VisitLogicalOperatorNode(this, parameter);
        }
    }
}
