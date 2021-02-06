using EasySqlParser.Internals.Dialect.Transformer;
using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      MysqlDialect
    // https://github.com/domaframework/doma
    internal class MysqlDialect : StandardDialect
    {
        internal override string ParameterPrefix { get; } = "@";
        internal override bool EnableNamedParameter { get; } = true;

        internal override char OpenQuote { get; } = '`';

        internal override char CloseQuote { get; } = '`';

        internal MysqlDialect() :
            base()
        {

        }

        internal MysqlDialect(char[] wildcards) :
            base(wildcards)
        {

        }

        internal MysqlDialect(char escapeChar, char[] wildcards) :
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
    }
}
