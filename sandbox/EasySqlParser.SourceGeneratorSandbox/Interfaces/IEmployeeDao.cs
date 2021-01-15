using System.Collections.Generic;
using EasySqlParser.SourceGenerator;

namespace EasySqlParser.SourceGeneratorSandbox.Interfaces
{
    [Dao()]
    //[Dao(nameof(EfContext), GenerationType.EntityFrameworkCore,"bar")]
    //[Dao("EfContext", GenerationType.EntityFrameworkCore, "bar")]
    //[Dao(GenerationType.Dapper,"foo")]
    public interface IEmployeeDao
    {

        [Query]
        List<Employee> GetEmployees(SqlCondition condition);

        [Query(FilePath = "aaaa.sql")]
        Employee GetEmployee(int id);
    }
}
