using System;
using EasySqlParser.Internals.Helpers;

namespace EasySqlParser.Exceptions
{
    /// <summary>
    /// Base exception EasySqlParser
    /// </summary>
    public class EspException : Exception
    {
        /// <summary>
        /// A kind of exception message
        /// </summary>
        public ExceptionMessageId MessageId { get; }

        internal object[] Arguments { get; }

        /// <summary>
        /// Create a new EspException instance 
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="args"></param>
        public EspException(ExceptionMessageId messageId, params object[] args) :
            base(ExceptionMessageIdHelper.ToMessage(messageId, args))
        {
            MessageId = messageId;
            Arguments = args;
        }
    }
}
