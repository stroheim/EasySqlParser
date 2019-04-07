using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      ForUpdateClauseNode
    // https://github.com/domaframework/doma
    internal class ForUpdateClauseNode : AbstractClauseNode
    {
        internal ForUpdateClauseNode(string word) : base(word)
        {
        }

        internal ForUpdateClauseNode(WordNode wordNode) : base(wordNode)
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

            return visitor.VisitForUpdateClauseNode(this, parameter);
        }
    }
}
