using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EasySqlParser.EntityFrameworkCore.Tests.SqlServer
{

    public class FacadeFixture : FixtureBase
    {
        private static readonly object _lock = new object();
        private static bool _initialized;
        public FacadeFixture() : base(nameof(FacadeFixture))
        {
        }

        protected override void Seed()
        {
            lock (_lock)
            {
                if (_initialized) return;
                using var localConnection = new SqlConnection(BaseConnectionString);
                localConnection.Open();
                ExecuteCommand(localConnection, $"DROP DATABASE IF EXISTS [{DbName}]");
                ExecuteCommand(localConnection, $"CREATE DATABASE [{DbName}]");
                ExecuteCommand(localConnection, $"USE [{DbName}]");
                using (var context = CreateContext())
                {
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

                    context.EmployeeWithDateAndUsers.Add(CreateEmployeeWithDateAndUser(1, "John Doe", 0));
                    context.EmployeeWithDateAndUsers.Add(CreateEmployeeWithDateAndUser(2, "Rob Walters", 1));
                    context.EmployeeWithDateAndUsers.Add(CreateEmployeeWithDateAndUser(3, "Gail Erickson", 2));
                    context.EmployeeWithDateAndUsers.Add(CreateEmployeeWithDateAndUser(4, "Jossef Goldberg", 3));
                    context.EmployeeWithDateAndUsers.Add(CreateEmployeeWithDateAndUser(5, "Dylan Miller", 4));
                    context.EmployeeWithDateAndUsers.Add(CreateEmployeeWithDateAndUser(6, "Diane Margheim", 5));
                    context.EmployeeWithDateAndUsers.Add(CreateEmployeeWithDateAndUser(7, "Gigi Matthew", 6));
                    context.EmployeeWithDateAndUsers.Add(CreateEmployeeWithDateAndUser(8, "Michael Raheem", 7));
                    context.EmployeeWithDateAndUsers.Add(CreateEmployeeWithDateAndUser(9, "Ovidiu Cracium", 8));
                    context.EmployeeWithDateAndUsers.Add(CreateEmployeeWithDateAndUser(10, "Janice Galvin", 9));

                    context.SaveChanges();

                }

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

        private static EmployeeWithDateAndUser CreateEmployeeWithDateAndUser(int id, string name, decimal salary)
        {
            return new EmployeeWithDateAndUser
                   {
                       Id = id,
                       Name = name,
                       Salary = salary,
                       CreateDateTime = DateTime.Now,
                       CreateUser = "admin",
                       VersionNo = 1
                   };
        }

    }

    public class IdentityFixture : FixtureBase
    {
        private static readonly object _lock = new object();
        private static bool _initialized;


        public IdentityFixture() : base(nameof(IdentityFixture))
        {
        }

        protected override void Seed()
        {
            lock (_lock)
            {
                if (_initialized) return;
                using var localConnection = new SqlConnection(BaseConnectionString);
                localConnection.Open();
                ExecuteCommand(localConnection, $"DROP DATABASE IF EXISTS [{DbName}]");
                ExecuteCommand(localConnection, $"CREATE DATABASE [{DbName}]");
                ExecuteCommand(localConnection, $"USE [{DbName}]");
                using (var context = CreateContext())
                {
                    context.Database.EnsureCreated();
                    context.Characters.Add(new Characters
                                           {
                                               Name = "Solid Snake",
                                               Height = 182,
                                               VersionNo = 1
                                           });
                    context.SaveChanges();
                }
                _initialized = true;
            }
        }
    }

    public abstract class FixtureBase : IDisposable
    {
        protected readonly string DbName;
        protected readonly string BaseConnectionString = @"Server=localhost,51433;User Id=sa;Password=Mitsuhide@1582;ConnectRetryCount=0";
        private readonly string _connectionString;

        protected FixtureBase(string dbName)
        {
            DbName = dbName;
            _connectionString= $@"Server=localhost,51433;Database={DbName};User Id=sa;Password=Mitsuhide@1582;ConnectRetryCount=0";
            Connection = new SqlConnection(_connectionString);

            Seed();

            Connection.Open();

        }

        protected abstract void Seed();

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

        protected void ExecuteCommand(DbConnection connection, string sql)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }


        public void Dispose()
        {
            Connection?.Dispose();
            //using var localConnection = new SqlConnection(BaseConnectionString);
            //localConnection.Open();
            //ExecuteCommand(localConnection, $"EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'{DbName}'");
            //ExecuteCommand(localConnection, "USE [master]");
            //ExecuteCommand(localConnection, $"ALTER DATABASE [{DbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
            //ExecuteCommand(localConnection, "USE [master]");
            //ExecuteCommand(localConnection, $"DROP DATABASE [{DbName}]");
        }

    }

    // https://docs.microsoft.com/ja-jp/ef/core/testing/sharing-databases
    public class DbContextFixture : IDisposable
    {
        private static readonly object _lock = new object();
        private static bool _initialized;
        //private const string ConnectionString = @"Server=(localdb)\mssqllocaldb;Database=EasySqlParserEntityFrameworkTests;ConnectRetryCount=0";
        private const string BaseConnectionString = @"Server=localhost,51433;User Id=sa;Password=Mitsuhide@1582;ConnectRetryCount=0";
        private const string DbName = "EasySqlParserEntityFrameworkTests";
        private static readonly string ConnectionString =
            $@"Server=localhost,51433;Database={DbName};User Id=sa;Password=Mitsuhide@1582;ConnectRetryCount=0";

        public DbContextFixture()
        {
            Connection = new SqlConnection(ConnectionString);

            Seed();

            Connection.Open();
        }

        public DbConnection Connection { get; private set; }

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
                if (_initialized) return;
                using var localConnection = new SqlConnection(BaseConnectionString);
                localConnection.Open();
                ExecuteCommand(localConnection, $"DROP DATABASE IF EXISTS [{DbName}]");
                ExecuteCommand(localConnection, $"CREATE DATABASE [{DbName}]");
                ExecuteCommand(localConnection, $"USE [{DbName}]");
                //Connection = new SqlConnection(ConnectionString);

                //Connection.Open();

                using (var context = CreateContext())
                {
                    //context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    context.Add(CreateEmployee(1, "Jim Rodman", 0, 1L));
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

        protected void ExecuteCommand(DbConnection connection, string sql)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        public void Dispose() => Connection.Dispose();
    }
}
