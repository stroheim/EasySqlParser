﻿using EasySqlParser.Internals.Dialect.Transformer;
using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      Db2Dialect
    // https://github.com/domaframework/doma
    internal class Db2Dialect : StandardDialect
    {

        internal override string ParameterPrefix { get; } = "@";
        internal override bool EnableNamedParameter { get; } = true;

        internal override bool SupportsIdentity { get; } = true;

        internal override bool SupportsSequence { get; } = true;

        internal override bool SupportsFinalTable { get; } = true;

        private static readonly char[] DefaultWildcards = { '%', '_', '％', '＿' };

        internal Db2Dialect() :
            base(DefaultWildcards)
        {

        }

        internal Db2Dialect(char[] wildcards) :
            base(wildcards)
        {

        }

        protected Db2Dialect(char escapeChar, char[] wildcards) :
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

        public override string GetNextSequenceSql(string name, string schema)
        {
            return $"SELECT NEXT VALUE FOR {GetSequenceName(name, schema)}";
        }

        public override string GetNextSequenceSqlZeroPadding(string name, string schema, int length, string prefix = null)
        {
            return $"SELECT {GetSequencePrefix(prefix)}LPAD(CAST(NEXT VALUE FOR {GetSequenceName(name, schema)} AS VARCHAR({length})), {length}, '0')";
        }
    }
}
