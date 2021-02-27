using System;
using System.Collections.Generic;
using System.Text;

namespace EasySqlParser.SqlGenerator.Tests
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
        public bool WriteIndented { get; internal set; }
        public QueryBehavior QueryBehavior { get; }
        public Action<string> LoggerAction { get; }

        public string CurrentUser { get; set; }
    }
}
