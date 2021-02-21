using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Microsoft.Data.SqlClient;

namespace EasySqlParser.SqlGenerator.Tests
{
    // code base from
    // https://www.jvandertil.nl/posts/2020-04-02_sqlserverintegrationtesting/
    public class DatabaseFixture : IDisposable
    {
        private static readonly object _lock = new object();
        private static bool _initialized;
        private const string DbName = "EasySqlParserSqlGeneratorTests";
        private const string BaseConnectionString = @"Server=(localdb)\mssqllocaldb;ConnectRetryCount=0";

        private static readonly string _connectionString =
            $@"Server=(localdb)\mssqllocaldb;Database={DbName};ConnectRetryCount=0";

        public DatabaseFixture()
        {
            Seed();
            _connection = new SqlConnection(_connectionString);


            _connection.Open();

        }

        private DbConnection _connection;

        public DbConnection Connection
        {
            get
            {
                if (_connection.State == ConnectionState.Open)
                {
                    return _connection;
                }

                _connection = new SqlConnection(_connectionString);
                _connection.Open();
                return _connection;
            }

        }

        private void Seed()
        {
            lock (_lock)
            {
                if (!_initialized)
                {
                    using var localConnection = new SqlConnection(BaseConnectionString);
                    localConnection.Open();
                    ExecuteCommand(localConnection, $"DROP DATABASE IF EXISTS [{DbName}]");
                    ExecuteCommand(localConnection, $"CREATE DATABASE [{DbName}]");
                    ExecuteCommand(localConnection, $"USE [{DbName}]");
                    ExecuteCommand(localConnection, @"CREATE TABLE [EMP](
[ID] int not null primary key,
[NAME] varchar(30),
[SALARY] numeric(10, 0) not null,
[VERSION] bigint not null
)");
                    ExecuteCommand(localConnection, @"INSERT INTO [EMP](
[ID],
[NAME],
[SALARY],
[VERSION]
)VALUES
(1,'John Doe',0,1),
(2,'Rob Walters',1,1),
(3,'Gail Erickson',2,1),
(4,'Jossef Goldberg',3,1),
(5,'Dylan Miller',4,1),
(6,'Diane Margheim',5,1),
(7,'Gigi Matthew',6,1),
(8,'Michael Raheem',7,1),
(9,'Ovidiu Cracium',8,1),
(10,'Janice Galvin',9,1);
");
                    _initialized = true;
                }
            }
        }

        private static void ExecuteCommand(DbConnection connection, string sql)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        public void Dispose()
        {
            _connection.Dispose();
            using var localConnection = new SqlConnection(BaseConnectionString);
            localConnection.Open();
            ExecuteCommand(localConnection, $"EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'{DbName}'");
            ExecuteCommand(localConnection, "USE [master]");
            ExecuteCommand(localConnection, $"ALTER DATABASE [{DbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
            ExecuteCommand(localConnection, "USE [master]");
            ExecuteCommand(localConnection, $"DROP DATABASE [{DbName}]");
        }

    }
}
