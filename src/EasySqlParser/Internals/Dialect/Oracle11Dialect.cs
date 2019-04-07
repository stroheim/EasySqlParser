using System;
using EasySqlParser.Internals.Dialect.Transformer;
using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Dialect
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
    internal class Oracle11Dialect : StandardDialect
    {
        internal override string ParameterPrefix { get; } = ":";
        internal override bool EnableNamedParameter { get; } = true;

        private static readonly char[] DefaultWildcards = { '%', '_', '％', '＿' };

        internal Oracle11Dialect() :
            base(DefaultWildcards)
        {

        }

        internal Oracle11Dialect(char[] wildcards) :
            base(wildcards)
        {

        }

        protected Oracle11Dialect(char escapeChar, char[] wildcards) :
            base(escapeChar, wildcards)
        {

        }

        internal override ISqlNode ToPagingSqlNode(ISqlNode node, long offset, long limit, string rowNumberColumn)
        {
            var transformer = new OraclePagingTransformer(offset, limit, rowNumberColumn);
            return transformer.Transform(node);
        }

        public override string ToLogFormatDateOnly(DateTime value)
        {
            return $"date'{value:yyyy-MM-dd}'";
        }

        public override string ToLogFormat(TimeSpan value)
        {
            return $"time'{value:hh\\:mm\\:ss}'";
        }

        public override string ToLogFormat(DateTime value)
        {
            return $"timestamp'{value:yyyy-MM-dd HH:mm:ss.fff}'";
        }

        public override string ToLogFormat(DateTimeOffset value)
        {
            return $"timestamp'{value:yyyy-MM-dd HH:mm:ss.fff}'";
        }
    }
}
