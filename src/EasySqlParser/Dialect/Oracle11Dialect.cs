using System;
using EasySqlParser.Internals.Dialect.Transformer;
using EasySqlParser.Internals.Node;

namespace EasySqlParser.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      Oracle11Dialect
    // https://github.com/domaframework/doma
    /// <summary>
    /// A dialect for Oracle Database 11g and below.
    /// </summary>
    /// <remarks>
    /// 11g 以前のOracle
    /// </remarks>
    public class Oracle11Dialect : StandardDialect
    {
        /// <inheritdoc />
        public override string ParameterPrefix { get; } = ":";

        /// <inheritdoc />
        public override bool EnableNamedParameter { get; } = true;

        /// <inheritdoc />
        public override bool SupportsSequence { get; } = true;

        /// <inheritdoc />
        public override bool SupportsReturning { get; } = true;

        private static readonly char[] DefaultWildcards = { '%', '_', '％', '＿' };

        /// <summary>
        /// Initializes a new instance of the <see cref="Oracle11Dialect"/> class.
        /// </summary>
        public Oracle11Dialect() :
            base(DefaultWildcards)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Oracle11Dialect"/> class.
        /// </summary>
        /// <param name="wildcards">wild card characters for the SQL LIKE operator</param>
        public Oracle11Dialect(char[] wildcards) :
            base(wildcards)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Oracle11Dialect"/> class.
        /// </summary>
        /// <param name="escapeChar">escape character for the SQL LIKE operator</param>
        /// <param name="wildcards">wild card characters for the SQL LIKE operator</param>
        public Oracle11Dialect(char escapeChar, char[] wildcards) :
            base(escapeChar, wildcards)
        {

        }

        internal override ISqlNode ToPagingSqlNode(ISqlNode node, long offset, long limit, string rowNumberColumn)
        {
            var transformer = new OraclePagingTransformer(offset, limit, rowNumberColumn);
            return transformer.Transform(node);
        }

        internal override string ToLogFormatDateOnly(DateTime value)
        {
            return $"date'{value:yyyy-MM-dd}'";
        }

        internal override string ToLogFormat(TimeSpan value)
        {
            return $"time'{value:hh\\:mm\\:ss}'";
        }

        internal override string ToLogFormat(DateTime value)
        {
            return $"timestamp'{value:yyyy-MM-dd HH:mm:ss.fff}'";
        }

        internal override string ToLogFormat(DateTimeOffset value)
        {
            return $"timestamp'{value:yyyy-MM-dd HH:mm:ss.fff}'";
        }

        private string GetSequencePrefix(string prefix)
        {
            return base.GetSequencePrefix(prefix, "||");
        }

        /// <inheritdoc />
        public override string GetNextSequenceSql(string name, string schema)
        {
            return $"SELECT {GetSequenceName(name, schema)}.NEXTVAL FROM DUAL";
        }

        /// <inheritdoc />
        public override string GetNextSequenceSqlZeroPadding(string name, string schema, int length, string prefix = null)
        {
            return $"SELECT {GetSequencePrefix(prefix)}LPAD({GetSequenceName(name, schema)}.NEXTVAL, {length}, '0') FROM DUAL";
        }
    }
}
