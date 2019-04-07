// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace EasySqlParser
{
    /// <summary>
    /// Represents kind of database connection
    /// </summary>
    public enum DbConnectionKind
    {
        /// <summary>
        /// represents not initialized state.
        /// </summary>
        Unknown,

        /// <summary>
        /// DbConnection is System.Data.SqlClient.SqlConnection<br/>
        /// version 2012 or later
        /// </summary>
        SqlServer,

        /// <summary>
        /// DbConnection is System.Data.SqlClient.SqlConnection<br/>
        /// version less than 2008
        /// </summary>
        SqlServerLegacy,

        /// <summary>
        /// DbConnection is Oracle.DataAccess.Client.OracleConnection<br/>
        /// version 12c or later<br/>
        /// <b>Not System.Data.OracleClient.OracleConnection
        /// </b>
        /// </summary>
        Oracle,

        /// <summary>
        /// DbConnection is Oracle.DataAccess.Client.OracleConnection<br/>
        /// Version less than 11g<br/>
        /// <b>Not System.Data.OracleClient.OracleConnection
        /// </b>
        /// </summary>
        OracleLegacy,

        /// <summary>
        /// DbConnection is IBM.Data.DB2.DB2Connection
        /// </summary>
        DB2,

        /// <summary>
        /// DbConnection is IBM.Data.DB2.iSeries.iDB2Connection
        /// </summary>
        AS400,

        /// <summary>
        /// DbConnection is MySql.Data.MySqlClientMySqlConnection
        /// </summary>
        MySql,

        /// <summary>
        /// DbConnection is Npgsql.NpgsqlConnection
        /// </summary>
        PostgreSql,

        /// <summary>
        /// DbConnection is System.Data.SQLite.SQLiteConnection<br/>
        /// or<br/>
        /// Microsoft.Data.Sqlite.SqliteConnection
        /// </summary>
        SQLite,

        /// <summary>
        /// DbConnection is System.Data.Odbc.OdbcConnection
        /// </summary>
        Odbc,

        /// <summary>
        /// DbConnection is System.Data.OleDb.OleDbConnection
        /// </summary>
        OleDb,
    }
}
