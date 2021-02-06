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
                        var employee = new Employee
                                       {
                                           Id = 1,
                                           Name = "John Doe",
                                           VersionNo = 1L
                                       };
                        context.Add(employee);
                        context.SaveChanges();
                    }

                    _initialized = true;
                }
            }
        }

        public void Dispose() => Connection.Dispose();
    }
}
