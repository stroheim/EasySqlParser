using EasySqlParser.Internals.Dialect.Transformer;
using EasySqlParser.Internals.Node;

namespace EasySqlParser.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      SqliteDialect
    // https://github.com/domaframework/doma
    /// <summary>
    /// A dialect for SQLite.
    /// </summary>
    public class SqliteDialect : StandardDialect
    {
        /// <inheritdoc />
        public override string ParameterPrefix { get; } = "@";

        /// <inheritdoc />
        public override bool EnableNamedParameter { get; } = true;


        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteDialect"/> class.
        /// </summary>
        public SqliteDialect() :
            base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteDialect"/> class.
        /// </summary>
        /// <param name="wildcards">wild card characters for the SQL LIKE operator</param>
        public SqliteDialect(char[] wildcards) :
            base(wildcards)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteDialect"/> class.
        /// </summary>
        /// <param name="escapeChar">escape character for the SQL LIKE operator</param>
        /// <param name="wildcards">wild card characters for the SQL LIKE operator</param>
        public SqliteDialect(char escapeChar, char[] wildcards) :
            base(escapeChar, wildcards)
        {

        }

        internal override ISqlNode ToPagingSqlNode(ISqlNode node, long offset, long limit, string rowNumberColumn)
        {
            var transformer = new SqlitePagingTransformer(offset, limit, rowNumberColumn);
            return transformer.Transform(node);
        }

        /// <inheritdoc />
        public override string GetIdentityWhereClause(string columnName)
        {
            return "rowid = last_insert_rowid()";
        }
    }
}
