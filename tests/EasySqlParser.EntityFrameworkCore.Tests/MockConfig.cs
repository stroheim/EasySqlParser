using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySqlParser.SqlGenerator;

namespace EasySqlParser.EntityFrameworkCore.Tests
{
    internal class MockConfig : IQueryBuilderConfiguration
    {
        public MockConfig(QueryBehavior queryBehavior, Action<string> loggerAction)
        {
            CommandTimeout = 30;
            WriteIndented = false;
            QueryBehavior = queryBehavior;
            LoggerAction = loggerAction;
        }

        public int CommandTimeout { get; }
        public bool WriteIndented { get; }
        public QueryBehavior QueryBehavior { get; }
        public Action<string> LoggerAction { get; }
    }

}
