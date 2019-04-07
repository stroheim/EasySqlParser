namespace EasySqlParser.Exceptions
{
    /// <summary>
    /// The exception that is thrown when found unsupported sql comment
    /// </summary>
    public sealed class UnsupportedSqlCommentException : EspException
    {
        internal UnsupportedSqlCommentException(ExceptionMessageId messageId, string nodeName) :
            base(messageId, nodeName)
        {

        }
    }
}
