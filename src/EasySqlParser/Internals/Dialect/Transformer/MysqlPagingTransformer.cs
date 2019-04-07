using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Dialect.Transformer
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.dialect
    //   class      MysqlPagingTransformer
    // https://github.com/domaframework/doma
    internal class MysqlPagingTransformer : StandardPagingTransformer
    {
        protected const string MaximumLimit = "18446744073709551615";
        internal MysqlPagingTransformer(long offset, long limit, string rowNumberColumn) : 
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

            var selectNode = new SelectClauseNode("select");
            selectNode.AddNode(new FragmentNode(" sql_calc_found_rows"));
            foreach (var child in node.SelectClauseNode.Children)
            {
                selectNode.AddNode(child);
            }

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
            orderBy.AddNode(new FragmentNode(offset));
            orderBy.AddNode(new FragmentNode(", "));
            orderBy.AddNode(new FragmentNode(limit));

            var result = new SelectStatementNode();
            // customized
            //result.SelectClauseNode = node.SelectClauseNode;
            result.SelectClauseNode = selectNode;
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
