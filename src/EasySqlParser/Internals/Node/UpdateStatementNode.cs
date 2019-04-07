using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      UpdateStatementNode
    // https://github.com/domaframework/doma
    internal class UpdateStatementNode : AbstractSqlNode, IWhereClauseAwareNode
    {
        private UpdateClauseNode _updateClauseNode;
        private SetClauseNode _setClauseNode;
        private WhereClauseNode _whereClauseNode;

        internal UpdateClauseNode UpdateClauseNode
        {
            get => _updateClauseNode;
            set
            {
                _updateClauseNode = value;
                AddNodeInternal(_updateClauseNode);
            }
        }

        internal SetClauseNode SetClauseNode
        {
            get => _setClauseNode;
            set
            {
                _setClauseNode = value;
                AddNodeInternal(_setClauseNode);
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

            return visitor.VisitUpdateStatementNode(this, parameter);
        }
    }
}
