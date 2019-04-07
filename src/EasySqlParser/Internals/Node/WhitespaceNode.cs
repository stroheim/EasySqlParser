using System;
using System.Collections.Generic;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      WhitespaceNode
    // https://github.com/domaframework/doma
    internal class WhitespaceNode : AbstractSqlNode
    {
        protected static readonly Dictionary<string, WhitespaceNode> WhitespaceNodes =
            new Dictionary<string, WhitespaceNode>();

        static WhitespaceNode()
        {
            WhitespaceNodes.Add('\u0009'.ToString(), new WhitespaceNode('\u0009'));
            WhitespaceNodes.Add('\u000B'.ToString(), new WhitespaceNode('\u000B'));
            WhitespaceNodes.Add('\u000C'.ToString(), new WhitespaceNode('\u000C'));
            WhitespaceNodes.Add('\u001C'.ToString(), new WhitespaceNode('\u001C'));
            WhitespaceNodes.Add('\u001D'.ToString(), new WhitespaceNode('\u001D'));
            WhitespaceNodes.Add('\u001E'.ToString(), new WhitespaceNode('\u001E'));
            WhitespaceNodes.Add('\u001F'.ToString(), new WhitespaceNode('\u001F'));
            WhitespaceNodes.Add('\u0020'.ToString(), new WhitespaceNode('\u0020'));
        }

        internal string Whitespace { get; }

        internal WhitespaceNode(char whitespace) :
            this(whitespace.ToString())
        {

        }

        internal WhitespaceNode(string whitespace)
        {
            Whitespace = whitespace;
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

            return visitor.VisitWhitespaceNode(this, parameter);
        }

        internal static WhitespaceNode Of(string whitespace)
        {
            WhitespaceNode node = WhitespaceNodes[whitespace];
            if (node != null)
            {
                return node;
            }
            return new WhitespaceNode(whitespace);
        }

    }
}
