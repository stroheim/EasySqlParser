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
    public class Mssql2008Dialect : StandardDialect
    {
        /// <inheritdoc />
        public override string ParameterPrefix { get; } = "@";

        /// <inheritdoc />
        public override bool EnableNamedParameter { get; } = true;

        internal override char OpenQuote { get; } = '[';

        internal override char CloseQuote { get; } = ']';

        /// <inheritdoc />
        public override bool SupportsIdentity { get; } = true;

        /// <summary>
        /// The default wild card characters for the SQL LIKE operator.
        /// </summary>
        protected static readonly char[] DefaultWildcards = { '%', '_', '[' };

        /// <summary>
        /// Initializes a new instance of the <see cref="Mssql2008Dialect"/> class.
        /// </summary>
        public Mssql2008Dialect() :
            base(DefaultWildcards)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mssql2008Dialect"/> class.
        /// </summary>
        /// <param name="wildcards">wild card characters for the SQL LIKE operator</param>
        public Mssql2008Dialect(char[] wildcards) :
            base(wildcards)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mssql2008Dialect"/> class.
        /// </summary>
        /// <param name="escapeChar">escape character for the SQL LIKE operator</param>
        /// <param name="wildcards">wild card characters for the SQL LIKE operator</param>
        public Mssql2008Dialect(char escapeChar, char[] wildcards) :
            base(escapeChar, wildcards)
        {

        }

        internal override ISqlNode ToPagingSqlNode(ISqlNode node, long offset, long limit, string rowNumberColumn)
        {
            var transformer = new Mssql2008PagingTransformer(offset, limit, rowNumberColumn);
            return transformer.Transform(node);
        }

        /// <inheritdoc />
        public override string GetIdentityWhereClause(string columnName)
        {
            
            return $"{ApplyQuote(columnName)} = scope_identity()";
        }
    }
}
