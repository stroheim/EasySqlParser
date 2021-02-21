using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EasySqlParser.EntityFrameworkCore.Tests
{
    public class DbContextFixture : IDisposable
    {
        private static readonly object _lock = new object();
        private static bool _initialized;

        public DbContextFixture()
        {
            Connection = new SqlConnection(@"Server=(localdb)\mssqllocaldb;Database=EasySqlParserEntityFrameworkTests;ConnectRetryCount=0");

            Seed();

            Connection.Open();
        }

        public DbConnection Connection { get; }

        public EfContext CreateContext(DbTransaction transaction = null)
        {
            var options = new DbContextOptionsBuilder<EfContext>()
                .UseSqlServer(Connection)
                .Options;
            var context = new EfContext(options);
            if (transaction != null)
            {
                context.Database.UseTransaction(transaction);
            }

            return context;
        }

        private void Seed()
        {
            lock (_lock)
            {
                if (!_initialized)
                {
                    using (var context = CreateContext())
                    {
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
                        context.Add(CreateEmployee(1, "John Doe", 0, 1L));
                        context.Add(CreateEmployee(2, "Rob Walters", 1, 1L));
                        context.Add(CreateEmployee(3, "Gail Erickson", 2, 1L));
                        context.Add(CreateEmployee(4, "Jossef Goldberg", 3, 1L));
                        context.Add(CreateEmployee(5, "Dylan Miller", 4, 1L));
                        context.Add(CreateEmployee(6, "Diane Margheim", 5, 1L));
                        context.Add(CreateEmployee(7, "Gigi Matthew", 6, 1L));
                        context.Add(CreateEmployee(8, "Michael Raheem", 7, 1L));
                        context.Add(CreateEmployee(9, "Ovidiu Cracium", 8, 1L));
                        context.Add(CreateEmployee(10, "Janice Galvin", 9, 1L));

                        context.SaveChanges();
                    }

                    _initialized = true;
                }
            }
        }

        private static Employee CreateEmployee(int id, string name,decimal salary, long versionNo)
        {
            return new Employee
                   {
                       Id = id,
                       Name = name,
                       Salary = salary,
                       VersionNo = versionNo
                   };
        }

        public void Dispose() => Connection.Dispose();
    }
}
