using System.Data.SqlClient;
using System.IO;
using EasySqlParser.Configurations;
using Xunit;

namespace EasySqlParser.Tests
{

    // Porting from DOMA
    //   package    org.seasar.doma.jdbc
    //   class      GreedyCacheSqlFileRepositoryTest
    // https://github.com/domaframework/doma
    public class SqlParserTest
    {

        public SqlParserTest()
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );
        }

        [Fact]
        public void testGetSqlFile()
        {
            ConfigContainer.EnableCache = true;
            var filePath = "manyEol.sql";
            var parser = new SqlParser(filePath, new {Name = ""});
            parser.PrepareParse();
            var sqlFileInfo = parser.SqlFileInfo;
            sqlFileInfo.IsNotNull();
            var parser2 = new SqlParser(filePath, new { Name = "" });
            parser2.PrepareParse();
            var sqlFileInfo2 = parser2.SqlFileInfo;
            sqlFileInfo.IsSameReferenceAs(sqlFileInfo2);
            filePath.Is(sqlFileInfo.FilePath);
        }

        [Fact]
        public void testClearCache()
        {
            ConfigContainer.EnableCache = true;
            var filePath = Path.GetFullPath("manyEol.sql");
            var parser = new SqlParser(filePath, new { Name = "" });
            parser.PrepareParse();
            var sqlFileInfo = parser.SqlFileInfo;
            sqlFileInfo.IsNotNull();
            SqlParser.ClearCache(filePath);
            //parser.ClearCache();
            var parser2 = new SqlParser(filePath, new { Name = "" });
            parser2.PrepareParse();
            var sqlFileInfo2 = parser2.SqlFileInfo;
            sqlFileInfo.IsNotSameReferenceAs(sqlFileInfo2);
            filePath.Is(sqlFileInfo.FilePath);
        }
    }
}
