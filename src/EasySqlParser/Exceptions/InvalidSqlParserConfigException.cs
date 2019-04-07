using System;

namespace EasySqlParser.Exceptions
{
    /// <summary>
    /// The exception that is thrown when found invalid config.
    /// </summary>
    public class InvalidSqlParserConfigException : EspException
    {
        internal InvalidSqlParserConfigException(ExceptionMessageId messageId) :
            base(messageId)
        {

        }

    }

}
