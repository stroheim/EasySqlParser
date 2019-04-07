using System.Collections.Generic;

namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      SqlNode
    // https://github.com/domaframework/doma
    internal interface ISqlNode
    {

        List<ISqlNode> Children { get; }

        TResult Accept<TParameter, TResult>(
            ISqlNodeVisitor<TParameter, TResult> visitor, 
            TParameter parameter
        );
    }
}
