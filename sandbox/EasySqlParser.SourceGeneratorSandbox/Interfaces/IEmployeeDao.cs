using System.Collections.Generic;
using EasySqlParser.SourceGenerator.Attributes;

namespace EasySqlParser.SourceGeneratorSandbox.Interfaces
{
    [Dao()]
    //[Dao(nameof(EfContext), GenerationType.EntityFrameworkCore,"bar")]
    //[Dao("EfContext", GenerationType.EntityFrameworkCore, "bar")]
    //[Dao(GenerationType.Dapper,"foo")]
    public interface IEmployeeDao
    {

        [Select]
        List<Employee> GetEmployees(SqlCondition condition);

        [Select(FilePath = "aaaa.sql")]
        Employee GetEmployee(int id);

        [Update()]
        int UpdateEmployee(Employee entity);
    }
}
