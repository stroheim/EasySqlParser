using System;
using System.Collections.Generic;
using System.Text;
using EasySqlParser.Exceptions;

namespace EasySqlParser.SqlGenerator
{
    // DbUpdateConcurrencyException is in Microsoft.EntityFrameworkCore.dll
    public sealed class OptimisticLockException : EspException
    {
        public string ParsedSql { get; }

        public string DebugSql { get; }

        public string SqlFilePath { get; }

        public OptimisticLockException(string parsedSql, string debugSql, string sqlFilePath) 
            : base(ExceptionMessageId.Esp2003, sqlFilePath, parsedSql)
        {
            ParsedSql = parsedSql;
            DebugSql = debugSql;
            SqlFilePath = sqlFilePath;
        }
    }
}
