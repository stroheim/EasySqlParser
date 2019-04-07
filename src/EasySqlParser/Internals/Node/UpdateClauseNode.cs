using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      UpdateClauseNode
    // https://github.com/domaframework/doma
    internal class UpdateClauseNode : AbstractClauseNode
    {
        internal UpdateClauseNode(string word) : base(word)
        {
        }

        internal UpdateClauseNode(WordNode wordNode) : base(wordNode)
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

            return visitor.VisitUpdateClauseNode(this, parameter);
        }
    }
}
