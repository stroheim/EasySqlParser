using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      ForBlockNode
    // https://github.com/domaframework/doma
    internal class ForBlockNode : AbstractSqlNode, IBlockNode
    {
        internal static readonly string HasNextSuffix = "_has_next";
        internal static readonly string IndexSuffix = "_index";

        private ForNode _forNode;

        internal ForNode ForNode
        {
            get => _forNode;
            set
            {
                _forNode = value;
                AddNodeInternal(_forNode);
            }
        }

        internal EndNode EndNode { get; private set; }


        public override TResult Accept<TParameter, TResult>(
            ISqlNodeVisitor<TParameter, TResult> visitor,
            TParameter parameter)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            return visitor.VisitForBlockNode(this, parameter);
        }

        public void SetEndNode(EndNode endNode)
        {
            EndNode = endNode;
        }

        public new void AddNode(ISqlNode node)
        {
            throw new InvalidOperationException($"TypeName : {GetType().Name}, Method : AddNode");
        }

        protected void AddNodeInternal(ISqlNode child)
        {
            if (child != null)
            {
                base.AddNode(child);
            }
        }
    }
}
