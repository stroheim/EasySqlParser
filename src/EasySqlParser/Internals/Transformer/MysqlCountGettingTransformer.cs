using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Transformer
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.dialect
    //   class      MysqlCountGettingTransformer
    // https://github.com/domaframework/doma
    internal class MysqlCountGettingTransformer : StandardCountGettingTransformer
    {
        public override ISqlNode VisitSelectStatementNode(SelectStatementNode node, object parameter)
        {
            if (Processed)
            {
                return node;
            }

            Processed = true;
            var selectNode = new SelectClauseNode("select");
            selectNode.AddNode(new FragmentNode(" found_rows()"));

            var result = new SelectStatementNode();
            result.SelectClauseNode = selectNode;
            return result;
        }
    }
}
