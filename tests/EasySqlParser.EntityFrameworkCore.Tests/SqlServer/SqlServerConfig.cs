using System.Collections.Generic;
using System.Reflection;
using EasySqlParser.SqlGenerator.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace EasySqlParser.EntityFrameworkCore.Tests.SqlServer
{
    public class SqlServerConfig : EfCoreQueryBuilderConfiguration
    {
        private readonly ITestOutputHelper _output;
        public SqlServerConfig(
            DbContext dbContext,
            ITestOutputHelper output,
            int commandTimeout = 30, 
            bool writeIndented = true, 
            QueryBehavior queryBehavior = QueryBehavior.AllColumns, 
            ExcludeNullBehavior excludeNullBehavior = ExcludeNullBehavior.NullOnly, 
            IEnumerable<Assembly> additionalAssemblies = null) : base(
            dbContext,
            NullLogger<SqlServerConfig>.Instance, 
            commandTimeout, 
            writeIndented, 
            queryBehavior, 
            excludeNullBehavior, 
            additionalAssemblies)
        {
            _output = output;
        }

        public override void WriteLog(string message)
        {
            _output.WriteLine(message);
        }

    }
}
