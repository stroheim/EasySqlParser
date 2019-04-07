using System;
using System.Collections.Generic;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      OtherNode
    // https://github.com/domaframework/doma
    internal class OtherNode : AbstractSqlNode
    {
        protected static readonly Dictionary<string,OtherNode> OtherNodes=
            new Dictionary<string, OtherNode>();

        static OtherNode()
        {
            OtherNodes.Add(",", new OtherNode(","));
            OtherNodes.Add("=", new OtherNode("="));
            OtherNodes.Add(">", new OtherNode(">"));
            OtherNodes.Add("<", new OtherNode("<"));
            OtherNodes.Add("-", new OtherNode("-"));
            OtherNodes.Add("+", new OtherNode("+"));
            OtherNodes.Add("*", new OtherNode("*"));
            OtherNodes.Add("/", new OtherNode("/"));
            OtherNodes.Add("(", new OtherNode("("));
            OtherNodes.Add(")", new OtherNode(")"));
            OtherNodes.Add(";", new OtherNode(";"));
        }

        public string Other { get; }

        private OtherNode(string other)
        {
            Other = other;
        }

        public new void AddNode(ISqlNode node)
        {
            throw new InvalidOperationException($"TypeName : {GetType().Name}, Method : AddNode");
        }

        public override TResult Accept<TParameter, TResult>(
            ISqlNodeVisitor<TParameter, TResult> visitor,
            TParameter parameter)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            return visitor.VisitOtherNode(this, parameter);
        }

        internal static OtherNode Of(string other)
        {
            var node = OtherNodes[other];
            if (node != null)
            {
                return node;
            }
            return new OtherNode(other);
        }
    }
}
