using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      WhereClauseNode
    // https://github.com/domaframework/doma
    internal class WhereClauseNode : AbstractClauseNode
    {
        internal WhereClauseNode(string word) : base(word)
        {
        }

        internal WhereClauseNode(WordNode wordNode) : base(wordNode)
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

            return visitor.VisitWhereClauseNode(this, parameter);
        }
    }
}
