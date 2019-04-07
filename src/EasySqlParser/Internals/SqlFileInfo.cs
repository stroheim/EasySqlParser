using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc
    //   class      SqlFile
    // https://github.com/domaframework/doma
    internal class SqlFileInfo
    {
        internal string FilePath { get; set; }

        internal string RawSql { get; set; }

        internal ISqlNode SqlNode { get; set; }

        public override string ToString()
        {
            return SqlNode.ToString();
        }
    }
}
