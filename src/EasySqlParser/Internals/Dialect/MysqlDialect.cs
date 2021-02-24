using EasySqlParser.Internals.Dialect.Transformer;
using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      MysqlDialect
    // https://github.com/domaframework/doma
    /// <summary>
    /// A dialect for MySQL.
    /// </summary>
    public class MysqlDialect : StandardDialect
    {
        /// <inheritdoc />
        public override string ParameterPrefix { get; } = "@";

        /// <inheritdoc />
        public override bool EnableNamedParameter { get; } = true;

        /// <inheritdoc />
        public override bool SupportsIdentity { get; } = true;

        internal override char OpenQuote { get; } = '`';

        internal override char CloseQuote { get; } = '`';

        /// <summary>
        /// Initializes a new instance of the <see cref="MysqlDialect"/> class.
        /// </summary>
        public MysqlDialect() :
            base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MysqlDialect"/> class.
        /// </summary>
        /// <param name="wildcards">wild card characters for the SQL LIKE operator</param>
        public MysqlDialect(char[] wildcards) :
            base(wildcards)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MysqlDialect"/> class.
        /// </summary>
        /// <param name="escapeChar">escape character for the SQL LIKE operator</param>
        /// <param name="wildcards">wild card characters for the SQL LIKE operator</param>
        public MysqlDialect(char escapeChar, char[] wildcards) :
            base(escapeChar, wildcards)
        {

        }

        internal override ISqlNode ToPagingSqlNode(ISqlNode node, long offset, long limit, string rowNumberColumn)
        {
            var transformer = new MysqlPagingTransformer(offset, limit, rowNumberColumn);
            return transformer.Transform(node);
        }

        internal override ISqlNode ToCountGettingSqlNode(ISqlNode node)
        {
            var transformer = new MysqlCountGettingTransformer();
            return transformer.Transform(node);
        }

        /// <inheritdoc />
        public override string GetIdentityWhereClause(string columnName)
        {

            return $"{ApplyQuote(columnName)} = LAST_INSERT_ID()";
        }
    }
}
