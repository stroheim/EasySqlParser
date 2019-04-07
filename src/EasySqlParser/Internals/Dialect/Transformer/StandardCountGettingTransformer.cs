using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Dialect.Transformer
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.dialect
    //   class      StandardCountGettingTransformer
    // https://github.com/domaframework/doma
    internal class StandardCountGettingTransformer: SimpleSqlNodeVisitor<object, ISqlNode>
    {
        protected bool Processed;

        internal ISqlNode Transform(ISqlNode node)
        {
            var result = new AnonymousNode();
            foreach (var child in node.Children)
            {
                result.AddNode(child.Accept(this, null));
            }

            return result;
        }

        public override ISqlNode VisitSelectStatementNode(SelectStatementNode node, object parameter)
        {
            if (Processed)
            {
                return node;
            }

            Processed = true;
            var subStatement = new SelectStatementNode();
            subStatement.SelectClauseNode = node.SelectClauseNode;
            subStatement.FromClauseNode = node.FromClauseNode;
            subStatement.WhereClauseNode = node.WhereClauseNode;
            subStatement.GroupByClauseNode = node.GroupByClauseNode;
            subStatement.HavingClauseNode = node.HavingClauseNode;

            var selectNode = new SelectClauseNode("select");
            selectNode.AddNode(new FragmentNode(" count(*) "));
            var fromNode=new FromClauseNode("from");
            fromNode.AddNode(new FragmentNode(" ( "));
            fromNode.AddNode(subStatement);
            fromNode.AddNode(new FragmentNode(") t_"));

            var result = new SelectStatementNode();
            result.SelectClauseNode = selectNode;
            result.FromClauseNode = fromNode;
            result.OptionClauseNode = node.OptionClauseNode;
            return result;
        }

        protected override ISqlNode DefaultAction(ISqlNode node, object parameter)
        {
            return node;
        }
    }
}
