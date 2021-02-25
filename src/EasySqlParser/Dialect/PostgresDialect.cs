using EasySqlParser.Internals.Dialect.Transformer;
using EasySqlParser.Internals.Node;

namespace EasySqlParser.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      PostgresDialect
    // https://github.com/domaframework/doma
    /// <summary>
    /// A dialect for PostgreSQL.
    /// </summary>
    public class PostgresDialect : StandardDialect
    {
        /// <inheritdoc />
        public override string ParameterPrefix { get; } = ":";

        /// <inheritdoc />
        public override bool EnableNamedParameter { get; } = true;

        /// <inheritdoc />
        public override bool SupportsIdentity { get; } = true;

        /// <inheritdoc />
        public override bool SupportsSequence { get; } = true;

        /// <inheritdoc />
        public override bool SupportsReturning { get; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresDialect"/> class.
        /// </summary>
        public PostgresDialect() :
            base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresDialect"/> class.
        /// </summary>
        /// <param name="wildcards">wild card characters for the SQL LIKE operator</param>
        public PostgresDialect(char[] wildcards) :
            base(wildcards)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresDialect"/> class.
        /// </summary>
        /// <param name="escapeChar">escape character for the SQL LIKE operator</param>
        /// <param name="wildcards">wild card characters for the SQL LIKE operator</param>
        public PostgresDialect(char escapeChar, char[] wildcards) :
            base(escapeChar, wildcards)
        {

        }

        internal override ISqlNode ToPagingSqlNode(ISqlNode node, long offset, long limit, string rowNumberColumn)
        {
            var transformer = new PostgresPagingTransformer(offset, limit, rowNumberColumn);
            return transformer.Transform(node);
        }

        private string GetSequencePrefix(string prefix)
        {
            return base.GetSequencePrefix(prefix, "||");
        }

        /// <inheritdoc />
        public override string GetNextSequenceSql(string name, string schema)
        {
            return $"SELECT NEXT VALUE FOR {GetSequenceName(name, schema)}";
        }

        /// <inheritdoc />
        public override string GetNextSequenceSqlZeroPadding(string name, string schema, int length, string prefix = null)
        {
            return $"SELECT {GetSequencePrefix(prefix)}LPAD(CAST(NEXT VALUE FOR {GetSequenceName(name, schema)} AS VARCHAR), {length}, '0')";
        }
    }
}
