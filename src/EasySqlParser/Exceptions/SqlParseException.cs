namespace EasySqlParser.Exceptions
{
    /// <summary>
    /// The exception that is thrown when parse a sql file.
    /// </summary>
    public sealed class SqlParseException : EspException
    {
        internal int LineNumber { get; }
        internal int Position { get; }

        /// <summary>
        /// A sql statement
        /// </summary>
        public string Sql { get; }


        internal string VariableComment { get; }

        internal string TestValue { get; }

        internal SqlParseException(ExceptionMessageId messageId, string sql, int lineNumber, int position) :
            base(messageId, sql, lineNumber, position)
        {
            Sql = sql;
            LineNumber = lineNumber;
            Position = position;
        }

        internal SqlParseException(ExceptionMessageId messageId, string sql, int lineNumber, int position,
            string variableComment) :
            base(messageId, sql, lineNumber, position, variableComment)
        {
            Sql = sql;
            LineNumber = lineNumber;
            Position = position;
            VariableComment = variableComment;
        }

        internal SqlParseException(ExceptionMessageId messageId, string sql, int lineNumber, int position,
            string variableComment, string testValue) :
            base(messageId, sql, lineNumber, position, variableComment, testValue)
        {
            Sql = sql;
            LineNumber = lineNumber;
            Position = position;
            VariableComment = variableComment;
            TestValue = testValue;
        }

    }
}
