using System;
using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Transformer
{
    // Porting org.seasar.doma.internal.jdbc.dialect.StandardCountCalculatingTransformer
    [Obsolete("will remove",true)]
    internal class StandardCountCalculatingTransformer : SimpleSqlNodeVisitor<object, ISqlNode>
    {
        protected bool Processed;

        public ISqlNode Transform(ISqlNode node)
        {
            var result = new AnonymousNode();
            foreach (var child in node.Children)
            {
                result.AddNode(child.Accept(this, null));
            }

            return result;
        }

        protected override ISqlNode DefaultAction(ISqlNode node, object parameter)
        {
            return node;
        }
    }
}
