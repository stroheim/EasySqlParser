
namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      BlockNode
    // https://github.com/domaframework/doma
    internal interface IBlockNode : IAddableSqlNode
    {
        void SetEndNode(EndNode endNode);
    }
}
