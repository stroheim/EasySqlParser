using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using EasySqlParser.Configurations;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.SqlGenerator.Tests
{
    public class QueryExtensionTest : IClassFixture<DatabaseFixture>
    {
        public QueryExtensionTest(DatabaseFixture fixture, ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );

            Fixture = fixture;
            _output = output;
        }

        public DatabaseFixture Fixture { get; }
        private readonly ITestOutputHelper _output;

        [Fact]
        public void Test_insert_default()
        {
            using var connection = Fixture.Connection;
            var employee = new Employee
                           {
                               Id = 2,
                               Name = "Solid Snake"
                           };

            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Insert);
            var affected = connection.ExecuteNonQueryByQueryBuilder(parameter, loggerAction: _output.WriteLine);
            affected.Is(1);

        }

        [Fact]
        public void Test_insert_excludeNull()
        {
            var employee = new Employee
                           {
                               Id = 2,
                               Salary = 100M
                           };
            var parameter = new QueryBuilderParameter<Employee>(employee, SqlKind.Insert, excludeNull: true);
            using var connection = Fixture.Connection;
            var affected = connection.ExecuteNonQueryByQueryBuilder(parameter, loggerAction: _output.WriteLine);
            affected.Is(1);
            //Expression<Func<Employee, bool>> predicate = (x) => x.Id == employee.Id && x.VersionNo == 1L;
            //var visitor = new PredicateVisitor();
            //var keyValues = visitor.GetKeyValues(predicate);
            //keyValues.Count.IsNot(0);
            //foreach (var keyValue in keyValues)
            //{
            //    _output.WriteLine($"{keyValue.Key}\t{keyValue.Value}");
            //}
            var instance = connection.ExecuteReaderByQueryBuilder(parameter, (x) => x.Id == employee.Id, _output.WriteLine);
            instance.Name.IsNull();
        }

    }

   
}
