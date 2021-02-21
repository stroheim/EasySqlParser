using EasySqlParser.Internals.Dialect.Transformer;
using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      Mssql2008Dialect
    // https://github.com/domaframework/doma
    /// <summary>
    /// A dialect for Microsoft SQL Server 2008 and below.
    /// </summary>
    /// <remarks>
    /// 2008以前のSQLServer
    /// </remarks>
    internal class Mssql2008Dialect : StandardDialect
    {
        internal override string ParameterPrefix { get; } = "@";
        internal override bool EnableNamedParameter { get; } = true;

        internal override char OpenQuote { get; } = '[';

        internal override char CloseQuote { get; } = ']';

        internal override bool SupportsIdentity { get; } = true;

        protected static readonly char[] DefaultWildcards = { '%', '_', '[' };

        internal Mssql2008Dialect() :
            base(DefaultWildcards)
        {

        }

        internal Mssql2008Dialect(char[] wildcards) :
            base(wildcards)
        {

        }

        protected Mssql2008Dialect(char escapeChar, char[] wildcards) :
            base(escapeChar, wildcards)
        {

        }

        internal override ISqlNode ToPagingSqlNode(ISqlNode node, long offset, long limit, string rowNumberColumn)
        {
            var transformer = new Mssql2008PagingTransformer(offset, limit, rowNumberColumn);
            return transformer.Transform(node);
        }

        internal override string GetIdentityWhereClause(string columnName)
        {
            
            return $"{ApplyQuote(columnName)} = scope_identity()";
        }
    }
}
