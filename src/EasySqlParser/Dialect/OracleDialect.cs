namespace EasySqlParser.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      OracleDialect
    // https://github.com/domaframework/doma
    /// <summary>
    /// A dialect for Oracle Database 12c and above.
    /// </summary>
    /// <remarks>
    /// 12c 以降のOracle
    /// </remarks>
    public class OracleDialect : Oracle11Dialect
    {
        private static readonly char[] DefaultWildcards = { '%', '_' };

        /// <inheritdoc />
        public override bool SupportsIdentity { get; } = true;


        /// <summary>
        /// Initializes a new instance of the <see cref="OracleDialect"/> class.
        /// </summary>
        public OracleDialect() :
            base(DefaultWildcards)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleDialect"/> class.
        /// </summary>
        /// <param name="wildcards">wild card characters for the SQL LIKE operator</param>
        public OracleDialect(char[] wildcards) :
            base(wildcards)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleDialect"/> class.
        /// </summary>
        /// <param name="escapeChar">escape character for the SQL LIKE operator</param>
        /// <param name="wildcards">wild card characters for the SQL LIKE operator</param>
        public OracleDialect(char escapeChar, char[] wildcards) :
            base(escapeChar, wildcards)
        {

        }
    }
}
