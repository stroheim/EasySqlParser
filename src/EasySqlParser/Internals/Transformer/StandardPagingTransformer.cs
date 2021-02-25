using EasySqlParser.Exceptions;
using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Transformer
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.dialect
    //   class      StandardPagingTransformer
    // https://github.com/domaframework/doma
    internal class StandardPagingTransformer : SimpleSqlNodeVisitor<object, ISqlNode>
    {
        protected readonly long Offset;
        protected readonly long Limit;
        protected bool Processed;
        protected readonly bool RowNumberColumnSpecified;

        internal StandardPagingTransformer(long offset, long limit, string rowNumberColumn)
        {
            Offset = offset;
            Limit = limit;
            RowNumberColumnSpecified = !string.IsNullOrEmpty(rowNumberColumn);
            if (string.IsNullOrEmpty(rowNumberColumn))
            {
                RowNumberColumnName = "esp_rownumber_";
            }
            else
            {
                RowNumberColumnName = rowNumberColumn;
            }
        }

        internal virtual ISqlNode Transform(ISqlNode node)
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

            var originalOrderBy = node.OrderByClauseNode;
            if (originalOrderBy == null)
            {
                throw new SqlTransformException(ExceptionMessageId.Esp2201);
            }

            var subStatement = new SelectStatementNode();
            subStatement.SelectClauseNode = node.SelectClauseNode;
            subStatement.FromClauseNode = node.FromClauseNode;
            subStatement.WhereClauseNode = node.WhereClauseNode;
            subStatement.GroupByClauseNode = node.GroupByClauseNode;
            subStatement.HavingClauseNode = node.HavingClauseNode;

            var orderBy = new OrderByClauseNode(originalOrderBy.WordNode);
            foreach (var child in originalOrderBy.Children)
            {
                if (child is WordNode wordNode)
                {
                    var word = wordNode.Word;
                    var names = word.Split('.');
                    if (names.Length == 2)
                    {
                        orderBy.AddNode(new WordNode("temp_." + names[1]));
                    }
                    else
                    {
                        orderBy.AddNode(child);
                    }
                }
                else
                {
                    orderBy.AddNode(child);
                }
            }

            var selectNode = new SelectClauseNode("select");
            selectNode.AddNode(new FragmentNode(" * "));
            var fromNode = new FromClauseNode("from");
            fromNode.AddNode(new FragmentNode(" ( select temp_.*, row_number() over( "));
            fromNode.AddNode(orderBy);
            fromNode.AddNode(new FragmentNode($" ) as {RowNumberColumnName} from ( "));
            fromNode.AddNode(subStatement);
            fromNode.AddNode(new FragmentNode(") as temp_ ) as temp2_ "));
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

                var bias = Offset < 0 ? 0 : Offset;
                whereNode.AddNode(new FragmentNode($"{RowNumberColumnName} <= "));
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

        protected readonly string RowNumberColumnName;

        protected override ISqlNode DefaultAction(ISqlNode node, object parameter)
        {
            return node;
        }
    }
}
