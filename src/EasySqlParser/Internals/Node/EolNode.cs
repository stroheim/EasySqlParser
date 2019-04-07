using System;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      EolNode
    // https://github.com/domaframework/doma
    internal class EolNode : AbstractSqlNode
    {
        internal string Eol { get; }

        internal EolNode(string eol)
        {
            Eol = eol;
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

            return visitor.VisitEolNode(this, parameter);
        }
    }
}
