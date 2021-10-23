using System;
using System.Collections.Generic;
using System.Text;
using EasySqlParser.Configurations;
using EasySqlParser.SqlGenerator.Enums;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.SqlGenerator.Tests.SqlServer
{
    public class QueryStringBuilderTest
    {
        private readonly MockConfig _mockConfig;
        private readonly ITestOutputHelper _output;

        public QueryStringBuilderTest(ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );
            _mockConfig = new MockConfig(QueryBehavior.None, output.WriteLine);
            _output = output;

        }

        [Fact]
        public void Test_IsNull()
        {
            var employee = new EmployeeWithDateAndUser();

            _mockConfig.ExcludeNullBehavior = ExcludeNullBehavior.All;
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _mockConfig, excludeNull: true);
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, _mockConfig.WriteIndented);
            // empty
            var result = builder.IsNull(parameter, "");
            result.IsTrue();
            // short
            result = builder.IsNull(parameter, default(short));
            result.IsTrue();
            // int
            result = builder.IsNull(parameter, default(int));
            result.IsTrue();
            // long
            result = builder.IsNull(parameter, default(long));
            result.IsTrue();
            // decimal
            result = builder.IsNull(parameter, default(decimal));
            result.IsTrue();
            // ushort
            result = builder.IsNull(parameter, default(ushort));
            result.IsTrue();
            // uint
            result = builder.IsNull(parameter, default(uint));
            result.IsTrue();
            // ulong
            result = builder.IsNull(parameter, default(ulong));
            result.IsTrue();
            // byte
            result = builder.IsNull(parameter, default(byte));
            result.IsTrue();
            // sbyte
            result = builder.IsNull(parameter, default(sbyte));
            result.IsTrue();
            // float
            result = builder.IsNull(parameter, default(float));
            result.IsTrue();
            // double
            result = builder.IsNull(parameter, default(double));
            result.IsTrue();
            // bool
            result = builder.IsNull(parameter, default(bool));
            result.IsTrue();
            // datetime
            result = builder.IsNull(parameter, default(DateTime));
            result.IsTrue();
        }
    }
}
