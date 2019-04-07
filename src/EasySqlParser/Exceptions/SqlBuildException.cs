using System;

namespace EasySqlParser.Exceptions
{
     /// <summary>
    /// The exception that is thrown when build a sql.
    /// </summary>
    public sealed class SqlBuildException : EspException
    {
        internal int LineNumber { get; }
        internal int Position { get; }

        /// <summary>
        /// A sql statement
        /// </summary>
        public string Sql { get; }

        internal string VariableComment { get; }

        internal int Index { get; }

        internal Type DataType { get; }


        internal SqlBuildException(ExceptionMessageId messageId, string sql, int lineNumber, int position) :
            base(messageId, sql, lineNumber, position)
        {
            Sql = sql;
            LineNumber = lineNumber;
            Position = position;
        }

        internal SqlBuildException(ExceptionMessageId messageId, string sql, int lineNumber, int position,
            string variableComment):
            base(messageId, sql, lineNumber, position, variableComment)
        {
            Sql = sql;
            LineNumber = lineNumber;
            Position = position;
            VariableComment = variableComment;
        }

        internal SqlBuildException(ExceptionMessageId messageId, string sql, int lineNumber, int position,
            string variableComment, int index) :
            base(messageId, sql, lineNumber, position, variableComment, index)
        {
            Sql = sql;
            LineNumber = lineNumber;
            Position = position;
            VariableComment = variableComment;
            Index = index;
        }

        internal SqlBuildException(ExceptionMessageId messageId, string sql, int lineNumber, int position,
            string variableComment, Type dataType) :
            base(messageId, sql, lineNumber, position, variableComment, dataType)
        {
            Sql = sql;
            LineNumber = lineNumber;
            Position = position;
            VariableComment = variableComment;
            DataType = dataType;
        }

    }
}
