using System;
using System.Collections.Generic;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      IfBlockNode
    // https://github.com/domaframework/doma
    internal class IfBlockNode : AbstractSqlNode, IBlockNode
    {
        private IfNode _ifNode;
        internal List<ElseifNode> ElseifNodes { get; } = new List<ElseifNode>();
        private ElseNode _elseNode;
        internal EndNode EndNode { get; private set; }

        internal IfNode IfNode
        {
            get => _ifNode;
            set
            {
                _ifNode = value;
                AddNodeInternal(_ifNode);
            }
        }

        internal void AddElseifNode(ElseifNode elseifNode)
        {
            ElseifNodes.Add(elseifNode);
            AddNodeInternal(elseifNode);
        }

        internal ElseNode ElseNode
        {
            get => _elseNode;
            set
            {
                _elseNode = value;
                AddNodeInternal(_elseNode);
            }
        }

        internal bool IsElseNodeExists => ElseNode != null;

        public override TResult Accept<TParameter, TResult>(
            ISqlNodeVisitor<TParameter, TResult> visitor,
            TParameter parameter)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            return visitor.VisitIfBlockNode(this, parameter);
        }

        public void SetEndNode(EndNode endNode)
        {
            EndNode = endNode;
            AddNodeInternal(endNode);
        }

        public new void AddNode(ISqlNode node)
        {
            throw new InvalidOperationException($"TypeName : {GetType().Name}, Method : AddNode");
        }

        internal void AddNodeInternal(ISqlNode child)
        {
            if (child != null)
            {
                base.AddNode(child);
            }
        }
    }
}
