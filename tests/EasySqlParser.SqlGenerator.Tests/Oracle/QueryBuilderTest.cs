using System;
using System.Collections.Generic;
using System.Text;
using EasySqlParser.Configurations;
using Oracle.ManagedDataAccess.Client;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.SqlGenerator.Tests.Oracle
{
    public class QueryBuilderTest
    {
        private readonly ITestOutputHelper _output;
        public QueryBuilderTest(ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.Oracle,
                () => new OracleParameter()
            );

            _output = output;
        }

        [Fact]
        public void Test_IdentityOnly()
        {
            var employee = new EmployeeIdentity
                           {
                               Name = "John Doe"
                           };
            var localConfig = new MockConfig(QueryBehavior.IdentityOnly, _output.WriteLine);
            //localConfig.WriteIndented = true;
            var parameter = new QueryBuilderParameter<EmployeeIdentity>(employee, SqlKind.Insert, localConfig);
            var builder = QueryBuilder<EmployeeIdentity>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            if (!localConfig.WriteIndented)
            {
                builder.ParsedSql.Is(@"INSERT INTO ""EMP_IDENTITY"" (""NAME"", ""VERSION"") VALUES (:Name, :VersionNo) RETURNING ""ID"" INTO :p_Id");
            }
            builder.DbDataParameters.Count.Is(3);
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
            if (!localConfig.WriteIndented)
            {
                builder.ParsedSql.Is(@"INSERT INTO ""EMP_IDENTITY"" (""NAME"", ""VERSION"") VALUES (:Name, :VersionNo) RETURNING ""ID"", ""NAME"", ""VERSION"" INTO :p_Id, :p_Name, :p_VersionNo");
            }
            builder.DbDataParameters.Count.Is(5);
            _output.WriteLine(builder.ParsedSql);

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
            localConfig.WriteIndented = true;
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Insert, localConfig);
            var builder = QueryBuilder<Employee>.GetQueryBuilderResult(parameter);
            builder.IsNotNull();
            if (!localConfig.WriteIndented)
            {
                builder.ParsedSql.Is(@"INSERT INTO ""EMP"" (""ID"", ""NAME"", ""SALARY"", ""VERSION"") VALUES (:Id, :Name, :Salary, :VersionNo) RETURNING ""ID"", ""NAME"", ""SALARY"", ""VERSION"" INTO :p_Id, :p_Name, :p_Salary, :p_VersionNo");
            }
            _output.WriteLine(builder.ParsedSql);
            _output.WriteLine(builder.DebugSql);
            builder.DbDataParameters.Count.Is(8);

        }

        [Fact]
        public void Test_AllColumns()
        {
            var employee = new EmployeeWithDateAndUser
                           {
                               Id = 1,
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
