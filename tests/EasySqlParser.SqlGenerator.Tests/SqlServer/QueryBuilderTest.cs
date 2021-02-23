using EasySqlParser.Configurations;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.SqlGenerator.Tests.SqlServer
{
    public class QueryBuilderTest
    {
        private readonly MockConfig _mockConfig;

        public QueryBuilderTest(ITestOutputHelper output)
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
            var employee = new Employee
                           {
                               Id = 1,
                               Name = "John Doe",
                               Salary = 100M
                           };
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Insert, _mockConfig);
            var builder = QueryBuilder<Employee>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("INSERT INTO [dbo].[EMP] ([ID], [NAME], [SALARY], [VERSION]) VALUES (@Id, @Name, @Salary, @VersionNo)");
            builder.DbDataParameters.Count.Is(4);
            builder.DbDataParameters[0].Value.Is(1);
            builder.DbDataParameters[1].Value.Is("John Doe");
            builder.DbDataParameters[2].Value.Is(100M);
            builder.DbDataParameters[3].Value.Is(1L);
        }

        [Fact]
        public void Test_Insert_ExcludeNull()
        {
            var employee = new Employee
                           {
                               Id = 1,
                               Salary = 100M
                           };
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Insert, _mockConfig, excludeNull:true);
            var builder = QueryBuilder<Employee>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("INSERT INTO [dbo].[EMP] ([ID], [SALARY], [VERSION]) VALUES (@Id, @Salary, @VersionNo)");
            builder.DbDataParameters.Count.Is(3);
            builder.DbDataParameters[0].Value.Is(1);
            builder.DbDataParameters[1].Value.Is(100M);
            builder.DbDataParameters[2].Value.Is(1L);
        }

        [Fact]
        public void Test_Update_Default()
        {
            var employee = new Employee
                           {
                               Id = 1,
                               Name = "John Doe",
                               VersionNo = 100L
                           };
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, _mockConfig);
            var builder = QueryBuilder<Employee>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("UPDATE [dbo].[EMP] SET [NAME] = @Name, [SALARY] = @Salary, [VERSION] = @VersionNo + 1 WHERE [ID] = @Id AND [VERSION] = @VersionNo");
            builder.DbDataParameters.Count.Is(4);
            builder.DbDataParameters[0].Value.Is("John Doe");
            builder.DbDataParameters[1].Value.Is(0M);
            builder.DbDataParameters[2].Value.Is(100L);
            builder.DbDataParameters[3].Value.Is(1);
        }

        [Fact]
        public void Test_Update_ExcludeNull()
        {
            var employee = new Employee
                           {
                               Id = 1,
                               VersionNo = 100L
                           };
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, _mockConfig, excludeNull:true);
            var builder = QueryBuilder<Employee>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("UPDATE [dbo].[EMP] SET [SALARY] = @Salary, [VERSION] = @VersionNo + 1 WHERE [ID] = @Id AND [VERSION] = @VersionNo");
            builder.DbDataParameters.Count.Is(3);
            builder.DbDataParameters[0].Value.Is(0M);
            builder.DbDataParameters[1].Value.Is(100L);
            builder.DbDataParameters[2].Value.Is(1);
        }

        [Fact]
        public void Test_Update_IgnoreVersion()
        {
            var employee = new Employee
                           {
                               Id = 1,
                               Name = "John Doe",
                               VersionNo = 100L
                           };
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Update, _mockConfig, ignoreVersion:true);
            var builder = QueryBuilder<Employee>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("UPDATE [dbo].[EMP] SET [NAME] = @Name, [SALARY] = @Salary, [VERSION] = @VersionNo WHERE [ID] = @Id");
            builder.DbDataParameters.Count.Is(4);
            builder.DbDataParameters[0].Value.Is("John Doe");
            builder.DbDataParameters[1].Value.Is(0M);
            builder.DbDataParameters[2].Value.Is(100L);
            builder.DbDataParameters[3].Value.Is(1);
        }

        [Fact]
        public void Test_Delete_Default()
        {
            var employee = new Employee
                           {
                               Id = 1,
                               Name = "John Doe",
                               VersionNo = 100L
                           };
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Delete, _mockConfig);
            var builder = QueryBuilder<Employee>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("DELETE FROM [dbo].[EMP] WHERE [ID] = @Id AND [VERSION] = @VersionNo");
            builder.DbDataParameters.Count.Is(2);
            builder.DbDataParameters[0].Value.Is(1);
            builder.DbDataParameters[1].Value.Is(100L);
        }

        [Fact]
        public void Test_Delete_IgnoreVersion()
        {
            var employee = new Employee
                           {
                               Id = 1,
                               Name = "John Doe",
                               VersionNo = 100L
                           };
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Delete, _mockConfig, ignoreVersion:true);
            var builder = QueryBuilder<Employee>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("DELETE FROM [dbo].[EMP] WHERE [ID] = @Id");
            builder.DbDataParameters.Count.Is(1);
            builder.DbDataParameters[0].Value.Is(1);
        }

        [Fact]
        public void Test_Select()
        {
            var (builderResult, entityInfo) = QueryBuilder<Employee>.GetSelectSql(x => x.Id == 1);
            builderResult.IsNotNull();
            builderResult.ParsedSql.Is("SELECT [ID], [NAME], [SALARY], [VERSION] FROM [dbo].[EMP] WHERE [ID] = @Id");
            builderResult.DbDataParameters.Count.Is(1);
            builderResult.DbDataParameters[0].Value.Is(1);
        }

        [Fact]
        public void Test_Count()
        {
            var builderResult = QueryBuilder<Employee>.GetCountSql();
            builderResult.IsNotNull();
            builderResult.ParsedSql.Is("SELECT COUNT(*) CNT FROM [dbo].[EMP]");
            builderResult.DbDataParameters.Count.Is(0);
        }
    }
}
