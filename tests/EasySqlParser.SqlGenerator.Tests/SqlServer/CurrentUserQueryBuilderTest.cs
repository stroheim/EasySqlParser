using System;
using System.Collections.Generic;
using System.Text;
using EasySqlParser.Configurations;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.SqlGenerator.Tests.SqlServer
{
    public class CurrentUserQueryBuilderTest
    {
        private readonly MockConfig _mockConfig;

        public CurrentUserQueryBuilderTest(ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );
            _mockConfig = new MockConfig(QueryBehavior.None, output.WriteLine);
            _mockConfig.CurrentUser = "Sariya Harnpadoungsataya";
        }

        [Fact]
        public void Test_Insert_Default()
        {
            var employee = new EmployeeWithDateAndUser
                           {
                               Id = 1,
                               Name = "John Doe",
                               Salary = 100M
                           };
            var parameter = new QueryBuilderParameter<EmployeeWithDateAndUser>(employee, SqlKind.Insert, _mockConfig);
            var builder = QueryBuilder<EmployeeWithDateAndUser>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("INSERT INTO [dbo].[EMP_WITH_DATE_USER] ([ID], [NAME], [SALARY], [DELETE_FLAG], [CREATE_DATETIME], [CREATE_USER], [VERSION]) VALUES (@Id, @Name, @Salary, 0, GETDATE(), @CreateUser, @VersionNo)");
            builder.DbDataParameters.Count.Is(5);
            builder.DbDataParameters[0].Value.Is(1);
            builder.DbDataParameters[1].Value.Is("John Doe");
            builder.DbDataParameters[2].Value.Is(100M);
            builder.DbDataParameters[3].Value.Is("Sariya Harnpadoungsataya");
            builder.DbDataParameters[4].Value.Is(1L);
        }

        [Fact]
        public void Test_Update_Default()
        {
            var employee = new EmployeeWithDateAndUser
            {
                               Id = 1,
                               Name = "John Doe",
                               VersionNo = 100L
                           };
            var parameter = new QueryBuilderParameter<EmployeeWithDateAndUser>(employee, SqlKind.Update, _mockConfig);
            var builder = QueryBuilder<EmployeeWithDateAndUser>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("UPDATE [dbo].[EMP_WITH_DATE_USER] SET [NAME] = @Name, [SALARY] = @Salary, [DELETE_FLAG] = 0, [UPDATE_DATETIME] = GETDATE(), [UPDATE_USER] = @UpdateUser, [VERSION] = @VersionNo + 1 WHERE [ID] = @Id AND [VERSION] = @VersionNo");
            builder.DbDataParameters.Count.Is(5);
            builder.DbDataParameters[0].Value.Is("John Doe");
            builder.DbDataParameters[1].Value.Is(0M);
            builder.DbDataParameters[2].Value.Is("Sariya Harnpadoungsataya");
            builder.DbDataParameters[3].Value.Is(100L);
            builder.DbDataParameters[4].Value.Is(1);
        }

        [Fact]
        public void Test_SoftDelete_Default()
        {
            var employee = new EmployeeWithDateAndUser
            {
                               Id = 1,
                               Name = "John Doe",
                               DeleteFlag = true,
                               VersionNo = 100L
                           };
            var parameter = new QueryBuilderParameter<EmployeeWithDateAndUser>(employee, SqlKind.SoftDelete, _mockConfig);
            var builder = QueryBuilder<EmployeeWithDateAndUser>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("UPDATE [dbo].[EMP_WITH_DATE_USER] SET [DELETE_FLAG] = 1, [DELETE_DATETIME] = GETDATE(), [DELETE_USER] = @DeleteUser, [VERSION] = @VersionNo + 1 WHERE [ID] = @Id AND [VERSION] = @VersionNo");
            builder.DbDataParameters.Count.Is(3);
            builder.DbDataParameters[0].Value.Is("Sariya Harnpadoungsataya");
            builder.DbDataParameters[1].Value.Is(100L);
            builder.DbDataParameters[2].Value.Is(1);
        }
    }
}
