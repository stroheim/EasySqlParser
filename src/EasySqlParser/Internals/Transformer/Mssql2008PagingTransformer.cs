using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Transformer
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.dialect
    //   class      Mssql2008PagingTransformer
    // https://github.com/domaframework/doma
    internal class Mssql2008PagingTransformer : StandardPagingTransformer
    {
        internal Mssql2008PagingTransformer(long offset, long limit, string rowNumberColumn) : 
            base(offset, limit, rowNumberColumn)
        {
        }

        internal override ISqlNode Transform(ISqlNode node)
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

            if (RowNumberColumnSpecified)
            {
                return base.VisitSelectStatementNode(node, parameter);
            }

            if (Offset > 0)
            {
                return base.VisitSelectStatementNode(node, parameter);
            }

            Processed = true;
            return AddTopNode(node);
        }

        protected ISqlNode AddTopNode(SelectStatementNode node)
        {
            var selectNode = new SelectClauseNode(node.SelectClauseNode.WordNode);
            selectNode.AddNode(new FragmentNode($" top ({Limit})"));
            foreach (var child in node.SelectClauseNode.Children)
            {
                selectNode.AddNode(child);
            }

            var result = new SelectStatementNode();
            result.SelectClauseNode = selectNode;
            result.FromClauseNode = node.FromClauseNode;
            result.WhereClauseNode = node.WhereClauseNode;
            result.GroupByClauseNode = node.GroupByClauseNode;
            result.HavingClauseNode = node.HavingClauseNode;
            result.OrderByClauseNode = node.OrderByClauseNode;
            result.ForUpdateClauseNode = node.ForUpdateClauseNode;
            result.OptionClauseNode = node.OptionClauseNode;
            return result;
        }
    }
}
