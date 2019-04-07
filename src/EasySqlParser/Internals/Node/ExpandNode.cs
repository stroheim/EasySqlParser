using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      ExpandNode
    // https://github.com/domaframework/doma
    internal class ExpandNode : AbstractSqlNode
    {
        internal SqlLocation Location { get; }
        internal string Alias { get; }
        internal string Text { get; }

        internal ExpandNode(SqlLocation location, string alias, string text)
        {
            Location = location;
            Alias = alias;
            Text = text;
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

            return visitor.VisitExpandNode(this, parameter);
        }
    }
}
