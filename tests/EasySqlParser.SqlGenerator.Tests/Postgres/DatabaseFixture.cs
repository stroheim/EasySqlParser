using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Npgsql;

namespace EasySqlParser.SqlGenerator.Tests.Postgres
{
    public class DatabaseFixture : IDisposable
    {
        private const string ConnectionString = "Host=localhost;Port=55432;Username=user01;Password=userpass;Database=sample";
        public DbConnection Connection { get; }

        private static readonly object _lock = new object();
        private static bool _initialized;

        public DatabaseFixture()
        {
            Connection = new NpgsqlConnection(ConnectionString);
            Seed();
            Connection.Open();
        }

        private void Seed()
        {
            lock (_lock)
            {
                if (_initialized) return;
                using var localConnection = new NpgsqlConnection(ConnectionString);
                localConnection.Open();

                #region EMP

                ExecuteCommand(localConnection, @"
DROP TABLE IF EXISTS ""EMP""
");

                ExecuteCommand(localConnection, @"
CREATE TABLE ""EMP""(
    ""ID"" INT NOT NULL,
    ""NAME"" VARCHAR(30),
    ""SALARY"" NUMERIC(10, 0) NOT NULL,
    ""VERSION"" BIGINT NOT NULL,
    PRIMARY KEY(""ID"")
)
");

                ExecuteCommand(localConnection, @"
INSERT INTO ""EMP""(
""ID"",
""NAME"",
""SALARY"",
""VERSION""
)VALUES(
1,
'John Doe',
0,
1
)
");

                #endregion

                #region MetalGearCharacters

                ExecuteCommand(localConnection, @"
DROP TABLE IF EXISTS ""MetalGearCharacters""
");

                ExecuteCommand(localConnection, @"
CREATE TABLE ""MetalGearCharacters""(
    ""ID"" SERIAL NOT NULL,
    ""NAME"" VARCHAR(30),
    ""HEIGHT"" NUMERIC(10, 2),
    ""CREATE_DATE"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    ""VERSION"" BIGINT NOT NULL,
    PRIMARY KEY(""ID"")
)
");

                ExecuteCommand(localConnection, @"
INSERT INTO ""MetalGearCharacters""(""NAME"", ""HEIGHT"", ""VERSION"")VALUES('Solid Snake',182,1)
");

                #endregion

                #region MetalGearSeries

                ExecuteCommand(localConnection, @"
DROP TABLE IF EXISTS ""MetalGearSeries""
");

                ExecuteCommand(localConnection, @"
CREATE TABLE ""MetalGearSeries""(
    ""ID"" INT NOT NULL,
    ""NAME"" VARCHAR(50) NOT NULL,
    ""RELEASE_DATE"" TIMESTAMP NOT NULL,
    ""PLATFORM"" VARCHAR(60) NOT NULL,
    ""CREATE_DATE"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    ""VERSION"" BIGINT NOT NULL,
    PRIMARY KEY(""ID"")
)
");

                ExecuteCommand(localConnection, @"
DROP SEQUENCE IF EXISTS ""METAL_GEAR_SERIES_SEQ""
");

                ExecuteCommand(localConnection, @"
CREATE SEQUENCE ""METAL_GEAR_SERIES_SEQ""
    INCREMENT BY 1
    START WITH 1
    MAXVALUE 9999999999
    MINVALUE 1
    CYCLE
");
                ExecuteCommand(localConnection, @"
INSERT INTO ""MetalGearSeries""(""ID"", ""NAME"", ""RELEASE_DATE"", ""PLATFORM"", ""VERSION"")VALUES
(nextval('""METAL_GEAR_SERIES_SEQ""'), 'METAL GEAR', TIMESTAMP '1987-07-13', 'MSX2', 1)
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
