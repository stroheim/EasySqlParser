namespace EasySqlParser.SqlGenerator.Enums
{
    /// <summary>
    ///     Type of query command execution.
    /// </summary>
    public enum CommandExecutionType
    {
        /// <summary>
        ///     Executes a query, and get records.
        /// </summary>
        ExecuteReader,
        /// <summary>
        ///     Executes a query with no results.
        /// </summary>
        ExecuteNonQuery,
        /// <summary>
        ///     Executes a query with a single scalar result.
        /// </summary>
        ExecuteScalar
    }
}
