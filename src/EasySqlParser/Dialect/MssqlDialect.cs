using EasySqlParser.Internals.Dialect.Transformer;
using EasySqlParser.Internals.Node;

namespace EasySqlParser.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      MssqlDialect
    // https://github.com/domaframework/doma
    /// <summary>
    /// A dialect for Microsoft SQL Server 2012 and above.
    /// </summary>
    public class MssqlDialect : Mssql2008Dialect
    {
        private readonly bool _pagingForceOffsetFetch;
        //internal override string ParameterPrefix { get; } = "@";
        //internal override bool EnableNamedParameter { get; } = true;

        //private static readonly char[] DefaultWildcards = { '%', '_', '[' };

        /// <inheritdoc />
        public override bool SupportsSequence { get; } = true;


        /// <summary>
        /// Initializes a new instance of the <see cref="MssqlDialect"/> class.
        /// </summary>
        public MssqlDialect() :
            base(DefaultWildcards)
        {
            _pagingForceOffsetFetch = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MssqlDialect"/> class.
        /// </summary>
        /// <param name="wildcards">wild card characters for the SQL LIKE operator</param>
        public MssqlDialect(char[] wildcards) :
            base(wildcards)
        {
            _pagingForceOffsetFetch = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MssqlDialect"/> class.
        /// </summary>
        /// <param name="escapeChar">escape character for the SQL LIKE operator</param>
        /// <param name="wildcards">wild card characters for the SQL LIKE operator</param>
        public MssqlDialect(char escapeChar, char[] wildcards) :
            base(escapeChar, wildcards)
        {
            _pagingForceOffsetFetch = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MssqlDialect"/> class.
        /// </summary>
        /// <param name="pagingForceOffsetFetch">whether to use offset fetch for paging</param>
        public MssqlDialect(bool pagingForceOffsetFetch) :
            base(DefaultWildcards)
        {
            _pagingForceOffsetFetch = pagingForceOffsetFetch;
        }

        internal override ISqlNode ToPagingSqlNode(ISqlNode node, long offset, long limit, string rowNumberColumn)
        {
            var transformer = new MssqlPagingTransformer(offset, limit, _pagingForceOffsetFetch, rowNumberColumn);
            return transformer.Transform(node);
        }

        private string GetSequencePrefix(string prefix)
        {
            return base.GetSequencePrefix(prefix, "+");
        }

        /// <inheritdoc />
        public override string GetNextSequenceSql(string name, string schema)
        {
            return
                $"SELECT NEXT VALUE FOR {GetSequenceName(name, schema)}";
        }

        /// <inheritdoc />
        public override string GetNextSequenceSqlZeroPadding(string name, string schema, int length, string prefix = null)
        {
            return
                $"SELECT {GetSequencePrefix(prefix)}FORMAT(NEXT VALUE FOR {GetSequenceName(name, schema)}, 'D{length}')";
        }
    }
}
