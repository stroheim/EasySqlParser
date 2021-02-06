using System;
using System.Collections.Generic;
using System.Text;
using EasySqlParser.Exceptions;

namespace EasySqlParser.SqlGenerator
{
    public sealed class OptimisticLockException : EspException
    {
        public OptimisticLockException(ExceptionMessageId messageId, params object[] args) 
            : base(messageId, args)
        {
        }
    }
}
