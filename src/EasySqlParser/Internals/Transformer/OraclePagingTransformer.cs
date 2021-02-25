using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Transformer
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.dialect
    //   class      OraclePagingTransformer
    // https://github.com/domaframework/doma
    internal class OraclePagingTransformer : StandardPagingTransformer
    {
        internal OraclePagingTransformer(long offset, long limit, string rowNumberColumn) : 
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
            var subStatement = new SelectStatementNode();
            subStatement.SelectClauseNode = node.SelectClauseNode;
            subStatement.FromClauseNode = node.FromClauseNode;
            subStatement.WhereClauseNode = node.WhereClauseNode;
            subStatement.GroupByClauseNode = node.GroupByClauseNode;
            subStatement.HavingClauseNode = node.HavingClauseNode;
            subStatement.OrderByClauseNode = node.OrderByClauseNode;

            var selectNode = new SelectClauseNode("select");
            selectNode.AddNode(new FragmentNode(" * "));
            var fromNode = new FromClauseNode("from");
            fromNode.AddNode(new FragmentNode($" ( select temp_.*, rownum {RowNumberColumnName} from ( "));
            fromNode.AddNode(subStatement);
            fromNode.AddNode(new FragmentNode(" ) temp_ ) "));
            var whereNode = new WhereClauseNode("where");
            whereNode.AddNode(new FragmentNode(" "));
            if (Offset >= 0)
            {
                whereNode.AddNode(new FragmentNode($"{RowNumberColumnName} > "));
                whereNode.AddNode(new FragmentNode(Offset.ToString()));
            }

            if (Limit > 0)
            {
                if (Offset >= 0)
                {
                    whereNode.AddNode(new FragmentNode(" and "));
                }
                whereNode.AddNode(new FragmentNode($"{RowNumberColumnName} <= "));
                var bias = Offset < 0 ? 0 : Offset;
                whereNode.AddNode(new FragmentNode((bias + Limit).ToString()));
            }

            var result = new SelectStatementNode();
            result.SelectClauseNode = selectNode;
            result.FromClauseNode = fromNode;
            result.WhereClauseNode = whereNode;
            result.ForUpdateClauseNode = node.ForUpdateClauseNode;
            result.OptionClauseNode = node.OptionClauseNode;
            return result;
        }
    }
}
