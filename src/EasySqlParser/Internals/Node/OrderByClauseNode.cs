using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      OrderByClauseNode
    // https://github.com/domaframework/doma
    internal class OrderByClauseNode : AbstractClauseNode
    {
        internal OrderByClauseNode(string word) : base(word)
        {
        }

        internal OrderByClauseNode(WordNode wordNode) : base(wordNode)
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

            return visitor.VisitOrderByClauseNode(this, parameter);
        }
    }
}
