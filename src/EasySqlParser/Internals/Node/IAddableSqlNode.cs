namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      AppendableSqlNode
    // https://github.com/domaframework/doma
    internal interface IAddableSqlNode : ISqlNode
    {
        void AddNode(ISqlNode child);
    }
}
