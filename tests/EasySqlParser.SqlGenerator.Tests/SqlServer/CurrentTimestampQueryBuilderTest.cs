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
    public class CurrentTimestampQueryBuilderTest
    {
        private readonly MockConfig _mockConfig;

        public CurrentTimestampQueryBuilderTest(ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );
            _mockConfig = new MockConfig(QueryBehavior.None, output.WriteLine);
        }

        [Fact]
        public void Test_Insert_Default()
        {
            var employee = new EmployeeWithDate()
                           {
                               Id = 1,
                               Name = "John Doe",
                               Salary = 100M
                           };
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _mockConfig);
            var builder = QueryBuilder.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("INSERT INTO [dbo].[EMP_WITH_DATE] ([ID], [NAME], [SALARY], [DELETE_FLAG], [CREATE_DATETIME], [VERSION]) VALUES (@Id, @Name, @Salary, 0, GETDATE(), @VersionNo)");
            builder.DbDataParameters.Count.Is(4);
            builder.DbDataParameters[0].Value.Is(1);
            builder.DbDataParameters[1].Value.Is("John Doe");
            builder.DbDataParameters[2].Value.Is(100M);
            builder.DbDataParameters[3].Value.Is(1L);
        }

        [Fact]
        public void Test_Update_Default()
        {
            var employee = new EmployeeWithDate
            {
                               Id = 1,
                               Name = "John Doe",
                               VersionNo = 100L
                           };
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _mockConfig);
            var builder = QueryBuilder.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("UPDATE [dbo].[EMP_WITH_DATE] SET [NAME] = @Name, [SALARY] = @Salary, [DELETE_FLAG] = 0, [UPDATE_DATETIME] = GETDATE(), [VERSION] = @VersionNo + 1 WHERE [ID] = @Id AND [VERSION] = @VersionNo");
            builder.DbDataParameters.Count.Is(4);
            builder.DbDataParameters[0].Value.Is("John Doe");
            builder.DbDataParameters[1].Value.Is(0M);
            builder.DbDataParameters[2].Value.Is(100L);
            builder.DbDataParameters[3].Value.Is(1);
        }

        [Fact]
        public void Test_SoftDelete_Default()
        {
            var employee = new EmployeeWithDate
            {
                               Id = 1,
                               Name = "John Doe",
                               DeleteFlag = true,
                               VersionNo = 100L
                           };
            var parameter = new QueryBuilderParameter(employee, SqlKind.SoftDelete, _mockConfig);
            var builder = QueryBuilder.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("UPDATE [dbo].[EMP_WITH_DATE] SET [DELETE_FLAG] = 1, [DELETE_DATETIME] = GETDATE(), [VERSION] = @VersionNo + 1 WHERE [ID] = @Id AND [VERSION] = @VersionNo");
            builder.DbDataParameters.Count.Is(2);
            builder.DbDataParameters[0].Value.Is(100L);
            builder.DbDataParameters[1].Value.Is(1);
        }

    }
}
