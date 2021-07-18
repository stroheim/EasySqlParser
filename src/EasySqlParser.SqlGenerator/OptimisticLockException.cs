using EasySqlParser.Exceptions;

namespace EasySqlParser.SqlGenerator
{
    // DbUpdateConcurrencyException is in Microsoft.EntityFrameworkCore.dll
    /// <summary>
    ///     The exception that is thrown when optimistic locking is failed.
    /// </summary>
    public sealed class OptimisticLockException : EspException
    {
        /// <summary>
        ///     Gets the parameterized sql.
        /// </summary>
        public string ParsedSql { get; }

        /// <summary>
        ///     Gets the debug sql.
        /// </summary>
        public string DebugSql { get; }

        /// <summary>
        ///     Gets the sql file path.
        /// </summary>
        public string SqlFilePath { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OptimisticLockException"/> class.
        /// </summary>
        /// <param name="parsedSql"></param>
        /// <param name="debugSql"></param>
        /// <param name="sqlFilePath"></param>
        public OptimisticLockException(string parsedSql, string debugSql, string sqlFilePath) 
            : base(ExceptionMessageId.Esp2003, sqlFilePath, parsedSql)
        {
            ParsedSql = parsedSql;
            DebugSql = debugSql;
            SqlFilePath = sqlFilePath;
        }
    }
}
