using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Dialect.Transformer
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.dialect
    //   class      Db2PagingTransformer
    // https://github.com/domaframework/doma
    internal class Db2PagingTransformer : StandardPagingTransformer
    {
        internal Db2PagingTransformer(long offset, long limit, string rowNumberColumn) : 
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

            if (Offset > 0)
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
            orderBy.AddNode(new FragmentNode($" fetch first {Limit} rows only"));

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
