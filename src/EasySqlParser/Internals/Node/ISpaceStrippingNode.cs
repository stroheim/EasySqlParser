namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      SpaceStrippingNode
    // https://github.com/domaframework/doma
    internal interface ISpaceStrippingNode : IAddableSqlNode
    {
        void ClearChildren();
    }
}
