using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      HavingClauseNode
    // https://github.com/domaframework/doma
    internal class HavingClauseNode : AbstractClauseNode
    {
        internal HavingClauseNode(string word) : base(word)
        {
        }

        internal HavingClauseNode(WordNode wordNode) : base(wordNode)
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

            return visitor.VisitHavingClauseNode(this, parameter);
        }
    }
}
