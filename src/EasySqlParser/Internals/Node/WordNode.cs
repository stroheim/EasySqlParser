using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      WordNode
    // https://github.com/domaframework/doma
    internal class WordNode : AbstractSqlNode
    {
        internal string Word { get; }
        internal bool IsReserved { get; }

        internal WordNode(string word) :
            this(word, false)
        {

        }

        internal WordNode(string word, bool isReserved)
        {
            Word = word;
            IsReserved = isReserved;
        }

        public override TResult Accept<TParameter, TResult>(
            ISqlNodeVisitor<TParameter, TResult> visitor,
            TParameter parameter)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            return visitor.VisitWordNode(this, parameter);
        }

        public new void AddNode(ISqlNode node)
        {
            throw new InvalidOperationException($"TypeName : {GetType().Name}, Method : AddNode");
        }
    }
}
