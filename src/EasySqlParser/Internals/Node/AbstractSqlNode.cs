using System;
using System.Collections.Generic;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      AbstractSqlNode
    // https://github.com/domaframework/doma
    internal abstract class AbstractSqlNode : IAddableSqlNode
    {
        public List<ISqlNode> Children { get; } = new List<ISqlNode>();

        public abstract TResult Accept<TParameter, TResult>(
            ISqlNodeVisitor<TParameter, TResult> visitor,
            TParameter parameter
        );

        public void AddNode(ISqlNode child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }
            Children.Add(child);
        }
    }
}
