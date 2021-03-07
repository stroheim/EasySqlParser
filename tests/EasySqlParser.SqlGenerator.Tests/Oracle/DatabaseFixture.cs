using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Oracle.ManagedDataAccess.Client;

namespace EasySqlParser.SqlGenerator.Tests.Oracle
{
    public class DatabaseFixture : IDisposable
    {
        private const string ConnectionString = "Data Source=localhost:51521/oracle19c;User Id=JOJO;Password=Uryyymudamuda19";

        public DbConnection Connection { get; }

        private static readonly object _lock = new object();
        private static bool _initialized;


        public DatabaseFixture()
        {
            Connection = new OracleConnection(ConnectionString);
            Seed();
            Connection.Open();
        }

        private void Seed()
        {
            lock (_lock)
            {
                if (_initialized) return;
                var localConnection = new OracleConnection(ConnectionString);
                localConnection.Open();

                #region EMP
                ExecuteCommand(localConnection, @"
DECLARE
   c NUMBER;
BEGIN
   SELECT COUNT(*) INTO c FROM USER_TABLES WHERE TABLE_NAME = 'EMP';
   IF c = 1 THEN
      EXECUTE IMMEDIATE 'DROP TABLE ""EMP""';
   END IF;
END;
");

                ExecuteCommand(localConnection, @"
CREATE TABLE ""EMP""(
    ""ID"" NUMBER(10, 0) NOT NULL,
    ""NAME"" VARCHAR2(30),
    ""SALARY"" NUMBER(18, 2) NOT NULL,
    ""VERSION"" NUMBER(19, 0) NOT NULL,
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
DECLARE
   c NUMBER;
BEGIN
   SELECT COUNT(*) INTO c FROM USER_TABLES WHERE TABLE_NAME = 'MetalGearCharacters';
   IF c = 1 THEN
      EXECUTE IMMEDIATE 'DROP TABLE ""MetalGearCharacters""';
   END IF;
END;
");

                ExecuteCommand(localConnection, @"
CREATE TABLE ""MetalGearCharacters""(
    ""ID"" NUMBER(10, 0) GENERATED ALWAYS AS IDENTITY,
    ""NAME"" VARCHAR2(30),
    ""HEIGHT"" NUMBER(18, 2),
    ""CREATE_DATE"" DATE DEFAULT SYSDATE NOT NULL,
    ""VERSION"" NUMBER(19, 0) NOT NULL,
    PRIMARY KEY(""ID"")
)
");

                ExecuteCommand(localConnection, @"
INSERT INTO ""MetalGearCharacters""(""NAME"", ""HEIGHT"", ""VERSION"")VALUES('Solid Snake',182,1)
");

                #endregion

                #region MetalGearSeries

                ExecuteCommand(localConnection, @"
DECLARE
   c NUMBER;
BEGIN
   SELECT COUNT(*) INTO c FROM USER_TABLES WHERE TABLE_NAME = 'MetalGearSeries';
   IF c = 1 THEN
      EXECUTE IMMEDIATE 'DROP TABLE ""MetalGearSeries""';
   END IF;
END;
");

                ExecuteCommand(localConnection, @"
CREATE TABLE ""MetalGearSeries""(
    ""ID"" NUMBER(10, 0),
    ""NAME"" VARCHAR2(50) NOT NULL,
    ""RELEASE_DATE"" DATE NOT NULL,
    ""PLATFORM"" VARCHAR2(60) NOT NULL,
    ""CREATE_DATE"" DATE DEFAULT SYSDATE NOT NULL,
    ""VERSION"" NUMBER(19, 0) NOT NULL,
    PRIMARY KEY(""ID"")
)
");

                ExecuteCommand(localConnection, @"
DECLARE
   c NUMBER;
BEGIN
   SELECT COUNT(*) INTO c FROM USER_SEQUENCES WHERE SEQUENCE_NAME = 'METAL_GEAR_SERIES_SEQ';
   IF c = 1 THEN
      EXECUTE IMMEDIATE 'DROP SEQUENCE ""METAL_GEAR_SERIES_SEQ""';
   END IF;
END;
");

                ExecuteCommand(localConnection, @"
CREATE SEQUENCE METAL_GEAR_SERIES_SEQ
    INCREMENT BY 1
    START WITH 1
    MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NOCACHE
    ORDER
");

                ExecuteCommand(localConnection, @"
INSERT INTO ""MetalGearSeries""(""ID"", ""NAME"", ""RELEASE_DATE"", ""PLATFORM"", ""VERSION"")VALUES
(METAL_GEAR_SERIES_SEQ.nextval, 'METAL GEAR', DATE '1987-07-13', 'MSX2', 1)
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
