// Creation time : 2021/01/16 21:09:42.877
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using EasySqlParser;
using EasySqlParser.EntityFrameworkCore;
using EasySqlParser.EntityFrameworkCore.Extensions;
using EasySqlParser.SourceGenerator;
using EasySqlParser.SourceGenerator.Attributes;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlKind = EasySqlParser.SqlGenerator.Enums.SqlKind;

#nullable disable

// ReSharper disable once CheckNamespace
namespace EasySqlParser.SourceGeneratorSandbox.Interfaces
{
    public interface IEmployeeDaoFuga : ISqlContext<EfContext>
    {
        List<Employee> GetEmployees(SqlCondition condition);

        Employee GetEmployee(int id);

    }

    public class EmployeeDaoFuga:EfCoreSqlContext<EfContext>,IEmployeeDaoFuga
    {
        private readonly IQueryBuilderConfiguration _configuration;
        private readonly ILogger<EmployeeDaoFuga> _logger;
        public EmployeeDaoFuga(EfContext context, IQueryBuilderConfiguration configuration, ILogger<EmployeeDaoFuga> logger):base(context, configuration)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public List<Employee> GetEmployees(SqlCondition condition)
        {
            var parser = new SqlParser(@"SqlResources\EasySqlParser\SourceGeneratorSandbox\Interfaces\GetEmployees.sql", condition);
            var parserResult = parser.Parse();
            _logger.LogDebug(parserResult.DebugSql);
            var results = Context.Database.ExecuteReader<Employee>(_configuration, parserResult);
            return Context.Set<Employee>().FromSqlRaw(
                parserResult.ParsedSql,
                parserResult.DbDataParameters.Cast<object>().ToArray()
            )
            .ToList();
        }
        public Employee GetEmployee(int id)
        {
            var parser = new SqlParser(@"aaaa.sql", "id", id);
            var parserResult = parser.Parse();
            _logger.LogDebug(parserResult.DebugSql);
            return Context.Set<Employee>().FromSqlRaw(
                parserResult.ParsedSql,
                parserResult.DbDataParameters.Cast<object>().ToArray()
            )
            .SingleOrDefault();
        }

        private void InsertEmployee(Employee employee)
        {
            var parameter = new QueryBuilderParameter(employee, SqlKind.Insert, _configuration);
            Add(parameter);
        }

        private void UpdateEmployee(Employee employee)
        {
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _configuration);
            Add(parameter);
        }

        private int Update(Employee employee)
        {
            var parameter = new QueryBuilderParameter(employee, SqlKind.Update, _configuration);
            return Context.Database.ExecuteNonQuery(parameter);
        }

    }
}
