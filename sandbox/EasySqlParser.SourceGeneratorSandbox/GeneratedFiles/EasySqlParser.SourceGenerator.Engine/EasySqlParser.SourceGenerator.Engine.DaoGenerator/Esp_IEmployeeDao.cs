// Creation time : 2021/01/16 21:09:42.877
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using EasySqlParser;
using EasySqlParser.EntityFrameworkCore.Extensions;
using EasySqlParser.SourceGenerator;
using EasySqlParser.SqlGenerator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlKind = EasySqlParser.SqlGenerator.SqlKind;

#nullable disable

// ReSharper disable once CheckNamespace
namespace EasySqlParser.SourceGeneratorSandbox.Interfaces
{
    public interface IEmployeeDaoFuga
    {
        List<Employee> GetEmployees(SqlCondition condition);

        Employee GetEmployee(int id);

        void Foo(Employee employee, DbTransaction transaction);
    }

    public class EmployeeDaoFuga:IEmployeeDaoFuga
    {
        private readonly DbContext _context;
        private readonly IQueryBuilderConfiguration _configuration;
        private readonly ILogger<EmployeeDaoFuga> _logger;
        public EmployeeDaoFuga(DbContext context, IQueryBuilderConfiguration configuration, ILogger<EmployeeDaoFuga> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public List<Employee> GetEmployees(SqlCondition condition)
        {
            var parser = new SqlParser(@"SqlResources\EasySqlParser\SourceGeneratorSandbox\Interfaces\GetEmployees.sql", condition);
            var parserResult = parser.Parse();
            _logger.LogDebug(parserResult.DebugSql);
            return _context.Set<Employee>().FromSqlRaw(
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
            return _context.Set<Employee>().FromSqlRaw(
                parserResult.ParsedSql,
                parserResult.DbDataParameters.Cast<object>().ToArray()
            )
            .SingleOrDefault();
        }

        public void Foo(Employee employee, DbTransaction transaction)
        {

        }
    }
}
