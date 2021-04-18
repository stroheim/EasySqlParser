using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace EasySqlParser.EntityFrameworkCore.Tests.Oracle
{
    public class DbContextFixture : IDisposable
    {
        private static readonly object _lock = new object();
        private static bool _initialized;
        private const string ConnectionString = "Data Source=localhost:51521/oracle19c;User Id=JOJO;Password=Uryyymudamuda19";

        public DbConnection Connection { get; private set; }

        public DbContextFixture()
        {
            Connection = new OracleConnection(ConnectionString);
            Seed();
            Connection.Open();
        }

        public OracleContext CreateContext(DbTransaction transaction = null)
        {
            var options = new DbContextOptionsBuilder<OracleContext>()
                .UseOracle(Connection)
                .Options;
            var context = new OracleContext(options);
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
                if (_initialized) return;

                using (var context = CreateContext())
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    context.Employees.Add(CreateEmployee(1, "Jim Rodman", 0, 1L));
                    context.Employees.Add(CreateEmployee(2, "Rob Walters", 1, 1L));
                    context.Employees.Add(CreateEmployee(3, "Gail Erickson", 2, 1L));
                    context.Employees.Add(CreateEmployee(4, "Jossef Goldberg", 3, 1L));
                    context.Employees.Add(CreateEmployee(5, "Dylan Miller", 4, 1L));
                    context.Employees.Add(CreateEmployee(6, "Diane Margheim", 5, 1L));
                    context.Employees.Add(CreateEmployee(7, "Gigi Matthew", 6, 1L));
                    context.Employees.Add(CreateEmployee(8, "Michael Raheem", 7, 1L));
                    context.Employees.Add(CreateEmployee(9, "Ovidiu Cracium", 8, 1L));
                    context.Employees.Add(CreateEmployee(10, "Janice Galvin", 9, 1L));

                    context.SaveChanges();
                }

                var localConnection = new OracleConnection(ConnectionString);
                localConnection.Open();

                ExecuteCommand(localConnection, @"
DECLARE
   c NUMBER;
BEGIN
   SELECT COUNT(*) INTO c FROM USER_SEQUENCES WHERE SEQUENCE_NAME = 'EF_METAL_GEAR_SERIES_SEQ';
   IF c = 1 THEN
      EXECUTE IMMEDIATE 'DROP SEQUENCE ""EF_METAL_GEAR_SERIES_SEQ""';
   END IF;
END;
");

                ExecuteCommand(localConnection, @"
CREATE SEQUENCE EF_METAL_GEAR_SERIES_SEQ
    INCREMENT BY 1
    START WITH 1
    MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NOCACHE
    ORDER
");


                ExecuteCommand(localConnection, @"
DECLARE
   c NUMBER;
BEGIN
   SELECT COUNT(*) INTO c FROM USER_SEQUENCES WHERE SEQUENCE_NAME = 'EF_LONG_SEQ';
   IF c = 1 THEN
      EXECUTE IMMEDIATE 'DROP SEQUENCE ""EF_LONG_SEQ""';
   END IF;
END;
");

                ExecuteCommand(localConnection, @"
DECLARE
   c NUMBER;
BEGIN
   SELECT COUNT(*) INTO c FROM USER_SEQUENCES WHERE SEQUENCE_NAME = 'EF_STRING_SEQ';
   IF c = 1 THEN
      EXECUTE IMMEDIATE 'DROP SEQUENCE ""EF_STRING_SEQ""';
   END IF;
END;
");

                ExecuteCommand(localConnection, @"
CREATE SEQUENCE EF_LONG_SEQ
    INCREMENT BY 1
    START WITH 1
    MAXVALUE 9999999999999999999
    MINVALUE 1
    CYCLE 
    NOCACHE
    ORDER
");

                ExecuteCommand(localConnection, @"
CREATE SEQUENCE EF_STRING_SEQ
    INCREMENT BY 1
    START WITH 1
    MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NOCACHE
    ORDER
");


                _initialized = true;
            }

        }

        private static Employee CreateEmployee(int id, string name, decimal salary, long versionNo)
        {
            return new Employee
                   {
                       Id = id,
                       Name = name,
                       Salary = salary,
                       VersionNo = versionNo
                   };
        }

        protected void ExecuteCommand(DbConnection connection, string sql)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }


        public void Dispose() => Connection.Dispose();

    }
}
