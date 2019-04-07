namespace EasySqlParser.Exceptions
{
    /// <summary>
    /// The exception that is thrown when
    /// sql file not found
    /// or
    /// sql statement is empty.
    /// </summary>
    public class InvalidSqlFileException : EspException
    {
        /// <summary>
        /// file path for 2way sql
        /// </summary>
        public string SqlFilePath { get; }

        internal InvalidSqlFileException(ExceptionMessageId messageId, string sqlFilePath) 
            : base(messageId, sqlFilePath)
        {
            SqlFilePath = sqlFilePath;
        }
    }
}
