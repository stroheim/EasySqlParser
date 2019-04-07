namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      WhereClauseAwareNode
    // https://github.com/domaframework/doma
    internal interface IWhereClauseAwareNode : ISqlNode
    {
        WhereClauseNode WhereClauseNode { get; set; }
    }
}
