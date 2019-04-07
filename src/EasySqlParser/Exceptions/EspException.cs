using System;
using EasySqlParser.Internals.Helpers;

namespace EasySqlParser.Exceptions
{
    /// <summary>
    /// Base exception EasySqlParser
    /// </summary>
    public class EspException : Exception
    {
        public ExceptionMessageId MessageId { get; }

        internal object[] Arguments { get; }
        
        public EspException(ExceptionMessageId messageId, params object[] args) :
            base(ExceptionMessageIdHelper.ToMessage(messageId, args))
        {
            MessageId = messageId;
            Arguments = args;
        }
    }
}
