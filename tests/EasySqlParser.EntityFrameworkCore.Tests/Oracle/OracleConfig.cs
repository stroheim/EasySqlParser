using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasySqlParser.SqlGenerator.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace EasySqlParser.EntityFrameworkCore.Tests.Oracle
{
    public class OracleConfig : EfCoreQueryBuilderConfiguration
    {

        private readonly ITestOutputHelper _output;
        public OracleConfig(DbContext dbContext, 
            ITestOutputHelper output, 
            int commandTimeout = 30, 
            bool writeIndented = true, 
            QueryBehavior queryBehavior = QueryBehavior.AllColumns, 
            ExcludeNullBehavior excludeNullBehavior = ExcludeNullBehavior.NullOnly, 
            IEnumerable<Assembly> additionalAssemblies = null) : base(
            dbContext, NullLogger<OracleConfig>.Instance, commandTimeout, writeIndented, queryBehavior, excludeNullBehavior, additionalAssemblies)
        {
            _output = output;
        }

        public override void WriteLog(string message)
        {
            _output.WriteLine(message);
        }
    }
}
