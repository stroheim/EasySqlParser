using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Dialect.Transformer
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.dialect
    //   class      PostgresPagingTransformer
    // https://github.com/domaframework/doma
    internal class PostgresPagingTransformer : StandardPagingTransformer
    {
        internal PostgresPagingTransformer(long offset, long limit, string rowNumberColumn) : 
            base(offset, limit, rowNumberColumn)
        {
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

            Processed = true;
            var originalOrderBy = node.OrderByClauseNode;
            OrderByClauseNode orderBy;
            if (originalOrderBy != null)
            {
                orderBy = new OrderByClauseNode(originalOrderBy.WordNode);
                foreach (var child in originalOrderBy.Children)
                {
                    orderBy.AddNode(child);
                }
            }
            else
            {
                orderBy = new OrderByClauseNode("");
            }

            if (Limit > 0)
            {
                orderBy.AddNode(new FragmentNode(" limit "));
                orderBy.AddNode(new FragmentNode(Limit.ToString()));
            }

            if (Offset >= 0)
            {
                orderBy.AddNode(new FragmentNode(" offset "));
                orderBy.AddNode(new FragmentNode(Offset.ToString()));
            }

            var result = new SelectStatementNode();
            result.SelectClauseNode = node.SelectClauseNode;
            result.FromClauseNode = node.FromClauseNode;
            result.WhereClauseNode = node.WhereClauseNode;
            result.GroupByClauseNode = node.GroupByClauseNode;
            result.HavingClauseNode = node.HavingClauseNode;
            result.OrderByClauseNode = orderBy;
            result.ForUpdateClauseNode = node.ForUpdateClauseNode;
            result.OptionClauseNode = node.OptionClauseNode;
            return result;
        }
    }
}
