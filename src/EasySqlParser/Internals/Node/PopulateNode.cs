using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      PopulateNode
    // https://github.com/domaframework/doma
    internal class PopulateNode : AbstractSqlNode
    {
        internal SqlLocation Location { get; }
        internal string Text { get; }

        internal PopulateNode(SqlLocation location, string text)
        {
            Location = location;
            Text = text;
        }

        public override TResult Accept<TParameter, TResult>(
            ISqlNodeVisitor<TParameter, TResult> visitor,
            TParameter parameter)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            return visitor.VisitPopulateNode(this, parameter);
        }
    }
}
