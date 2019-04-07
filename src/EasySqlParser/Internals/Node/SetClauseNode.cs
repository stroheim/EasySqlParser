using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      SetClauseNode
    // https://github.com/domaframework/doma
    internal class SetClauseNode : AbstractClauseNode
    {
        internal SetClauseNode(string word) : base(word)
        {
        }

        internal SetClauseNode(WordNode wordNode) : base(wordNode)
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

            return visitor.VisitSetClauseNode(this, parameter);
        }
    }
}
