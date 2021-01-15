using System;
using System.Collections.Generic;
using System.Linq;
using EasySqlParser;
using EasySqlParser.SourceGenerator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#nullable disable

// ReSharper disable once CheckNamespace
namespace EasySqlParser.SourceGeneratorSandbox.Interfaces
{
    public class EmployeeDao : IEmployeeDao
    {
        private readonly ILogger<EmployeeDao> _logger;
        private readonly DbContext _context;
        public EmployeeDao(DbContext context, ILogger<EmployeeDao> logger)
        {
            _context = context;
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
    }
}
