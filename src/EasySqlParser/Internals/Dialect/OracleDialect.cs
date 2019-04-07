namespace EasySqlParser.Internals.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      OracleDialect
    // https://github.com/domaframework/doma
    /// <summary>
    /// A dialect for Oracle Database.
    /// </summary>
    /// <remarks>
    /// 12c 以降のOracle
    /// </remarks>
    internal class OracleDialect : Oracle11Dialect
    {
        private static readonly char[] DefaultWildcards = { '%', '_' };

        internal OracleDialect() :
            base(DefaultWildcards)
        {

        }

        internal OracleDialect(char[] wildcards) :
            base(wildcards)
        {

        }

        protected OracleDialect(char escapeChar, char[] wildcards) :
            base(escapeChar, wildcards)
        {

        }
    }
}
