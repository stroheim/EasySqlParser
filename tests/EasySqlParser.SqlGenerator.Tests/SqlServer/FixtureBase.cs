using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Microsoft.Data.SqlClient;

namespace EasySqlParser.SqlGenerator.Tests.SqlServer
{
    public abstract class FixtureBase : IDisposable
    {
        protected readonly string DbName;
        protected readonly string BaseConnectionString = @"Server=(localdb)\mssqllocaldb;ConnectRetryCount=0";
        //protected readonly string BaseConnectionString = @"Server=localhost,51433;User Id=sa;Password=Mitsuhide@1582;ConnectRetryCount=0";
        private readonly string _connectionString;

        protected FixtureBase(string dbName)
        {
            DbName = dbName;
            Seed();
            _connectionString = $@"Server=(localdb)\mssqllocaldb;Database={DbName};ConnectRetryCount=0";
            //_connectionString = $@"Server=localhost,51433;Database={DbName};User Id=sa;Password=Mitsuhide@1582;ConnectRetryCount=0";

            _connection = new SqlConnection(_connectionString);
            _connection.Open();

        }

        protected abstract void Seed();

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

        protected void ExecuteCommand(DbConnection connection, string sql)
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
