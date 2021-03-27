using EasySqlParser.Internals.Node;
using EasySqlParser.Internals.Transformer;

namespace EasySqlParser.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      Db2Dialect
    // https://github.com/domaframework/doma
    /// <summary>
    /// A dialect for Db2.
    /// </summary>
    public class Db2Dialect : StandardDialect
    {
        /// <inheritdoc />
        public override string ParameterPrefix { get; } = "@";

        /// <inheritdoc />
        public override bool EnableNamedParameter { get; } = true;

        /// <inheritdoc />
        public override bool SupportsIdentity { get; } = true;

        /// <inheritdoc />
        public override bool SupportsSequence { get; } = true;

        /// <inheritdoc />
        public override bool SupportsFinalTable { get; } = true;

        private static readonly char[] DefaultWildcards = { '%', '_', '％', '＿' };

        /// <summary>
        /// Initializes a new instance of the <see cref="Db2Dialect"/> class.
        /// </summary>
        public Db2Dialect() :
            base(DefaultWildcards)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Db2Dialect"/> class.
        /// </summary>
        /// <param name="wildcards">wild card characters for the SQL LIKE operator</param>
        public Db2Dialect(char[] wildcards) :
            base(wildcards)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Db2Dialect"/> class.
        /// </summary>
        /// <param name="escapeChar">escape character for the SQL LIKE operator</param>
        /// <param name="wildcards">wild card characters for the SQL LIKE operator</param>
        public Db2Dialect(char escapeChar, char[] wildcards) :
            base(escapeChar, wildcards)
        {

        }

        internal override ISqlNode ToPagingSqlNode(ISqlNode node, long offset, long limit, string rowNumberColumn)
        {
            var transformer = new Db2PagingTransformer(offset, limit, rowNumberColumn);
            return transformer.Transform(node);
        }

        private string GetSequencePrefix(string prefix)
        {
            return base.GetSequencePrefix(prefix, "||");
        }

        /// <inheritdoc />
        public override string GetNextSequenceSql(string name, string schema)
        {
            return $"SELECT NEXT VALUE FOR {GetSequenceName(name, schema)} FROM SYSIBM.DUAL";
        }

        /// <inheritdoc />
        public override string GetNextSequenceSqlZeroPadding(string name, string schema, int length, string prefix = null)
        {
            return $"SELECT {GetSequencePrefix(prefix)}LPAD(CAST(NEXT VALUE FOR {GetSequenceName(name, schema)} AS VARCHAR({length})), {length}, '0') FROM SYSIBM.DUAL";
        }
    }
}
