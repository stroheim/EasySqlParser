using EasySqlParser.Internals.Dialect.Transformer;
using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      SqliteDialect
    // https://github.com/domaframework/doma
    internal class SqliteDialect : StandardDialect
    {
        internal override string ParameterPrefix { get; } = "@";
        internal override bool EnableNamedParameter { get; } = true;

        internal override bool UseSqlite { get; } = true;


        internal SqliteDialect() :
            base()
        {

        }

        internal SqliteDialect(char[] wildcards) :
            base(wildcards)
        {

        }

        internal SqliteDialect(char escapeChar, char[] wildcards) :
            base(escapeChar, wildcards)
        {

        }

        internal override ISqlNode ToPagingSqlNode(ISqlNode node, long offset, long limit, string rowNumberColumn)
        {
            var transformer = new SqlitePagingTransformer(offset, limit, rowNumberColumn);
            return transformer.Transform(node);
        }

        internal override string GetIdentityWhereClause(string columnName)
        {
            return "rowid = last_insert_rowid()";
        }
    }
}
