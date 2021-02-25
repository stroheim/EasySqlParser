using EasySqlParser.Exceptions;
using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Transformer
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.dialect
    //   class      MssqlPagingTransformer
    // https://github.com/domaframework/doma
    /// <summary>
    /// A dialect for Microsoft SQL Server.
    /// </summary>
    /// <remarks>
    /// 2012以降のSQLServer
    /// </remarks>
    internal class MssqlPagingTransformer : Mssql2008PagingTransformer
    {
        private readonly bool _forceOffsetFetch;

        internal MssqlPagingTransformer(long offset, long limit, bool forceOffsetFetch, string rowNumberColumn) : 
            base(offset, limit, rowNumberColumn)
        {
            _forceOffsetFetch = forceOffsetFetch;
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
            if (!_forceOffsetFetch && Offset <= 0)
            {
                return AddTopNode(node);
            }

            var originalOrderBy = node.OrderByClauseNode;
            if (originalOrderBy == null)
            {
                throw new SqlTransformException(ExceptionMessageId.Esp2201);
            }

            var orderBy = new OrderByClauseNode(originalOrderBy.WordNode);
            foreach (var child in originalOrderBy.Children)
            {
                orderBy.AddNode(child);
            }

            var offset = Offset <= 0 ? "0" : Offset.ToString();

            orderBy.AddNode(new FragmentNode(" offset "));
            orderBy.AddNode(new FragmentNode(offset));
            orderBy.AddNode(new FragmentNode(" rows"));
            if (Limit > 0)
            {
                orderBy.AddNode(new FragmentNode(" fetch next "));
                orderBy.AddNode(new FragmentNode(Limit.ToString()));
                orderBy.AddNode(new FragmentNode(" rows only"));
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
