using EasySqlParser.Internals.Dialect.Transformer;
using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      PostgresDialect
    // https://github.com/domaframework/doma
    internal class PostgresDialect : StandardDialect
    {
        internal override string ParameterPrefix { get; } = ":";
        internal override bool EnableNamedParameter { get; } = true;

        internal override bool SupportsIdentity { get; } = true;

        internal override bool SupportsSequence { get; } = true;

        internal override bool SupportsReturning { get; } = true;

        internal PostgresDialect() :
            base()
        {

        }

        internal PostgresDialect(char[] wildcards) :
            base(wildcards)
        {

        }

        internal PostgresDialect(char escapeChar, char[] wildcards) :
            base(escapeChar, wildcards)
        {

        }

        internal override ISqlNode ToPagingSqlNode(ISqlNode node, long offset, long limit, string rowNumberColumn)
        {
            var transformer = new PostgresPagingTransformer(offset, limit, rowNumberColumn);
            return transformer.Transform(node);
        }
    }
}
