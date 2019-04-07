namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      AbstractClauseNode
    // https://github.com/domaframework/doma
    internal abstract class AbstractClauseNode : AbstractSqlNode, IClauseNode
    {
        public WordNode WordNode { get; }

        protected AbstractClauseNode(string word) :
            this(new WordNode(word, true))
        {

        }

        protected AbstractClauseNode(WordNode wordNode)
        {
            WordNode = wordNode;
        }
    }
}
