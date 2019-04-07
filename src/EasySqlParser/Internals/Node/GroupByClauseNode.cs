using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      GroupByClauseNode
    // https://github.com/domaframework/doma
    internal class GroupByClauseNode : AbstractClauseNode
    {
        internal GroupByClauseNode(string word) : base(word)
        {
        }

        internal GroupByClauseNode(WordNode wordNode) : base(wordNode)
        {
        }

        internal WordNode WorNode => WordNode;

        public override TResult Accept<TParameter, TResult>(
            ISqlNodeVisitor<TParameter, TResult> visitor,
            TParameter parameter)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            return visitor.VisitGroupByClauseNode(this, parameter);
        }
    }
}
