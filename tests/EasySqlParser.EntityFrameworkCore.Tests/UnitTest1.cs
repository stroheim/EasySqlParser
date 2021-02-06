using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.EntityFrameworkCore.Tests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _output;
        public UnitTest1(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Test1()
        {
            var options = new DbContextOptionsBuilder<EfContext>()
                .UseInMemoryDatabase(databaseName: "foo_bar")
                .Options;

            using (var context = new EfContext(options))
            {
                var entity = new Employee
                             {
                                 Name = "John Doe",
                                 Salary = 10000,

                             };
                context.Employees.Add(entity);
                context.SaveChanges();
            }

            using (var context = new EfContext(options))
            {
                context.Employees.Count().Is(1);
                var result = context.Employees.First();
                _output.WriteLine($"{result.Id}");
                _output.WriteLine($"{result.Name}");
                _output.WriteLine($"{result.Salary}");
                _output.WriteLine($"{result.VersionNo}");
            }
        }
    }
}
