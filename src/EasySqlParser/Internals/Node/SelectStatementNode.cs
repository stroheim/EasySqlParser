using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      SelectStatementNode
    // https://github.com/domaframework/doma
    internal class SelectStatementNode : AbstractSqlNode, IWhereClauseAwareNode
    {
        private SelectClauseNode _selectClauseNode;
        private FromClauseNode _fromClauseNode;
        private WhereClauseNode _whereClauseNode;
        private GroupByClauseNode _groupByClauseNode;
        private HavingClauseNode _havingClauseNode;
        private OrderByClauseNode _orderByClauseNode;
        private ForUpdateClauseNode _forUpdateClauseNode;
        private OptionClauseNode _optionClauseNode;

        internal SelectClauseNode SelectClauseNode
        {
            get => _selectClauseNode;
            set
            {
                _selectClauseNode = value;
                AddNodeInternal(_selectClauseNode);
            }
        }

        internal FromClauseNode FromClauseNode
        {
            get => _fromClauseNode;
            set
            {
                _fromClauseNode = value;
                AddNodeInternal(_fromClauseNode);
            }
        }

        public WhereClauseNode WhereClauseNode
        {
            get => _whereClauseNode;
            set
            {
                _whereClauseNode = value;
                AddNodeInternal(_whereClauseNode);
            }
        }

        internal GroupByClauseNode GroupByClauseNode
        {
            get => _groupByClauseNode;
            set
            {
                _groupByClauseNode = value;
                AddNodeInternal(_groupByClauseNode);
            }
        }

        internal HavingClauseNode HavingClauseNode
        {
            get => _havingClauseNode;
            set
            {
                _havingClauseNode = value;
                AddNodeInternal(_havingClauseNode);
            }
        }

        internal OrderByClauseNode OrderByClauseNode
        {
            get => _orderByClauseNode;
            set
            {
                _orderByClauseNode = value;
                AddNodeInternal(_orderByClauseNode);
            }
        }

        internal ForUpdateClauseNode ForUpdateClauseNode
        {
            get => _forUpdateClauseNode;
            set
            {
                _forUpdateClauseNode = value;
                AddNodeInternal(_forUpdateClauseNode);
            }
        }

        internal OptionClauseNode OptionClauseNode
        {
            get => _optionClauseNode;
            set
            {
                _optionClauseNode = value;
                AddNodeInternal(_optionClauseNode);
            }
        }

        public new void AddNode(ISqlNode node)
        {
            throw new InvalidOperationException($"TypeName : {GetType().Name}, Method : AddNode");
        }

        private void AddNodeInternal(ISqlNode child)
        {
            if (child != null)
            {
                base.AddNode(child);
            }
        }

        public override TResult Accept<TParameter, TResult>(
            ISqlNodeVisitor<TParameter, TResult> visitor,
            TParameter parameter)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            return visitor.VisitSelectStatementNode(this, parameter);
        }
    }
}
