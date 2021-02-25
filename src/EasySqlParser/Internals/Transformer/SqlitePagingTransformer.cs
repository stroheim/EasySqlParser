using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Transformer
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.dialect
    //   class      SqlitePagingTransformer
    // https://github.com/domaframework/doma
    internal class SqlitePagingTransformer : StandardPagingTransformer
    {
        protected static string MaximumLimit = long.MaxValue.ToString();
        internal SqlitePagingTransformer(long offset, long limit, string rowNumberColumn) : 
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

            var offset = Offset <= 0 ? "0" : Offset.ToString();
            var limit = Limit <= 0 ? MaximumLimit : Limit.ToString();
            orderBy.AddNode(new FragmentNode(" limit "));
            orderBy.AddNode(new FragmentNode(limit));
            orderBy.AddNode(new FragmentNode(" offset "));
            orderBy.AddNode(new FragmentNode(offset));

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
