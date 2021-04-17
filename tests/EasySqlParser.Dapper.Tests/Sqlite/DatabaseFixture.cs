using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace EasySqlParser.Dapper.Tests.Sqlite
{
    public class DatabaseFixture : IDisposable
    {
        //private const string ConnectionString = @"Data Source=:memory:";
        private const string ConnectionString = @"Data Source=Sharable;Mode=Memory;Cache=Shared";

        public DbConnection Connection { get; }

        private static readonly object _lock = new object();
        private static bool _initialized;

        public DatabaseFixture()
        {
            Connection = new SqliteConnection(ConnectionString);
            SQLitePCL.raw.SetProvider(new SQLite3Provider_dynamic_cdecl());
            Connection.Open();
            Seed();
        }

        private void Seed()
        {
            lock (_lock)
            {
                if (_initialized) return;

                //using var localConnection = new SqliteConnection(ConnectionString);
                //SQLitePCL.raw.SetProvider(new SQLite3Provider_dynamic_cdecl());
                //localConnection.Open();

                #region EMP


                ExecuteCommand(Connection, @"
DROP TABLE IF EXISTS ""EMP""
");

                ExecuteCommand(Connection, @"
CREATE TABLE ""EMP""(
    ""ID"" INTEGER PRIMARY KEY,
    ""NAME"" TEXT,
    ""SALARY"" REAL NOT NULL,
    ""VERSION"" INTEGER NOT NULL
)
");

                ExecuteCommand(Connection, @"
INSERT INTO ""EMP""(
""NAME"",
""SALARY"",
""VERSION""
)VALUES(
'John Doe',
0,
1
);
");

                #endregion

                #region MetalGearCharacters

                ExecuteCommand(Connection, @"
DROP TABLE IF EXISTS ""MetalGearCharacters""
");

                ExecuteCommand(Connection, @"
CREATE TABLE ""MetalGearCharacters""(
    ""ID"" INTEGER PRIMARY KEY AUTOINCREMENT,
    ""NAME"" TEXT,
    ""HEIGHT"" REAL,
    ""CREATE_DATE"" TIMESTAMP DEFAULT (datetime(CURRENT_TIMESTAMP,'localtime')) NOT NULL,
    ""VERSION"" INTEGER NOT NULL
)
");

                ExecuteCommand(Connection, @"
INSERT INTO ""MetalGearCharacters""(""NAME"", ""HEIGHT"", ""VERSION"")VALUES('Solid Snake',182,1)
");

                #endregion
                _initialized = true;
            }
        }

        private static void ExecuteCommand(DbConnection connection, string sql)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        public void Dispose() => Connection.Dispose();


    }
}
