using System;
using EasySqlParser.Configurations;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.SqlGenerator.Tests.SqlServer
{
    public class QueryBuilderTest
    {
        private readonly MockConfig _mockConfig;
        private readonly ITestOutputHelper _output;

        public QueryBuilderTest(ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );
            _mockConfig = new MockConfig(QueryBehavior.None, output.WriteLine);
            _output = output;
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
        public void Test_Insert_MultipleKey()
        {
            var employee = new EmployeeMultipleKey
                           {
                               Key1 = "1",
                               Key2 = "1",
                               Name = "11"
                           };
            var parameter = new QueryBuilderParameter<EmployeeMultipleKey>(employee, SqlKind.Insert, _mockConfig);
            var builder = QueryBuilder<EmployeeMultipleKey>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is(
                "INSERT INTO [dbo].[EMP_MULTIPLE_KEY] ([KEY_COL1], [KEY_COL2], [NAME]) VALUES (@Key1, @Key2, @Name)");
            builder.DbDataParameters.Count.Is(3);

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
        public void Test_Update_IgnoreCreateDate()
        {
            var characters = new Characters
                             {
                                 Id = 1,
                                 Name = "John Doe",
                                 VersionNo = 1L
                             };
            var parameter = new QueryBuilderParameter<Characters>(characters, SqlKind.Update, _mockConfig);
            var builder = QueryBuilder<Characters>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            builder.ParsedSql.Is("UPDATE [MetalGearCharacters] SET [NAME] = @Name, [HEIGHT] = @Height, [VERSION] = @VersionNo + 1 WHERE [ID] = @Id AND [VERSION] = @VersionNo");

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

        [Fact]
        public void Test_IdentityOnly()
        {
            var employee = new EmployeeIdentity
                           {
                               Name = "John Doe"
                           };
            var localConfig = new MockConfig(QueryBehavior.IdentityOnly, _output.WriteLine);
            localConfig.WriteIndented = true;
            var parameter = new QueryBuilderParameter<EmployeeIdentity>(employee, SqlKind.Insert, localConfig);
            var builder = QueryBuilder<EmployeeIdentity>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            _output.WriteLine(builder.ParsedSql);
            if (!localConfig.WriteIndented)
            {
                var sqls = builder.ParsedSql.Split(new[] { "\r\n" }, StringSplitOptions.None);
                sqls[0].Is("INSERT INTO [dbo].[EMP_IDENTITY] ([NAME], [VERSION]) VALUES (@Name, @VersionNo);");
                sqls[1].Is("SELECT [ID] FROM [dbo].[EMP_IDENTITY] WHERE [ID] = scope_identity();");
            }
            builder.DbDataParameters.Count.Is(2);
            builder.DbDataParameters[0].Value.Is("John Doe");
            builder.DbDataParameters[1].Value.Is(1L);
        }

        [Fact]
        public void Test_IdentityAllColumns()
        {
            var employee = new EmployeeIdentity
                           {
                               Name = "John Doe"
                           };
            var localConfig = new MockConfig(QueryBehavior.AllColumns, _output.WriteLine);
            localConfig.WriteIndented = true;
            var parameter = new QueryBuilderParameter<EmployeeIdentity>(employee, SqlKind.Insert, localConfig);
            var builder = QueryBuilder<EmployeeIdentity>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            _output.WriteLine(builder.ParsedSql);
            if (!localConfig.WriteIndented)
            {
                var sqls = builder.ParsedSql.Split(new[] { "\r\n" }, StringSplitOptions.None);
                sqls[0].Is("INSERT INTO [dbo].[EMP_IDENTITY] ([NAME], [VERSION]) VALUES (@Name, @VersionNo);");
                sqls[1].Is("SELECT [ID], [NAME], [VERSION] FROM [dbo].[EMP_IDENTITY] WHERE [ID] = scope_identity();");
            }
            builder.DbDataParameters.Count.Is(2);
            builder.DbDataParameters[0].Value.Is("John Doe");
            builder.DbDataParameters[1].Value.Is(1L);
        }

        [Fact]
        public void Test_IdentityOrAllColumns()
        {
            var employee = new Employee
                           {
                               Id = 1,
                               Name = "John Doe"
                           };
            var localConfig = new MockConfig(QueryBehavior.IdentityOrAllColumns, _output.WriteLine);
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Insert, localConfig);
            var builder = QueryBuilder<Employee>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            _output.WriteLine(builder.ParsedSql);
            var sqls = builder.ParsedSql.Split(new[] { "\r\n" }, StringSplitOptions.None);
            sqls[0].Is("INSERT INTO [dbo].[EMP] ([ID], [NAME], [SALARY], [VERSION]) VALUES (@Id, @Name, @Salary, @VersionNo);");
            sqls[1].Is("SELECT [ID], [NAME], [SALARY], [VERSION] FROM [dbo].[EMP] WHERE [ID] = @Id;");

            builder.DbDataParameters.Count.Is(4);
            builder.DbDataParameters[0].Value.Is(1);
            builder.DbDataParameters[1].Value.Is("John Doe");
            builder.DbDataParameters[2].Value.Is(0M);
            builder.DbDataParameters[3].Value.Is(1L);

        }

        [Fact]
        public void Test_AllColumns()
        {
            var employee = new EmployeeWithDateAndUser
                           {
                               Name = "John Doe"
                           };
            var localConfig = new MockConfig(QueryBehavior.AllColumns, _output.WriteLine);
            localConfig.WriteIndented = true;
            var parameter = new QueryBuilderParameter<EmployeeWithDateAndUser>(employee, SqlKind.Insert, localConfig);
            var builder = QueryBuilder<EmployeeWithDateAndUser>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            _output.WriteLine(builder.ParsedSql);
        }
    }
}
