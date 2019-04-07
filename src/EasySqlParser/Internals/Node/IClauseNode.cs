
namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      ClauseNode
    // https://github.com/domaframework/doma
    internal interface IClauseNode : IAddableSqlNode
    {
        WordNode WordNode { get; }
    }
}
