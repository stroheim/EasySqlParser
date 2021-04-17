using System;
using System.Data.Common;
using IBM.Data.DB2.Core;

namespace EasySqlParser.Dapper.Tests.Db2
{
    public class DatabaseFixture : IDisposable
    {
        private const string ConnectionString =
            "server=localhost:50010;database=sample;user id=db2inst1;password=sample_pass;";
        public DbConnection Connection { get; }

        public DatabaseFixture()
        {
            Connection = new DB2Connection(ConnectionString);
            Seed();
            Connection.Open();
        }

        private static readonly object _lock = new object();
        private static bool _initialized;
        private void Seed()
        {
            lock (_lock)
            {
                if (_initialized) return;

                using var localConnection = new DB2Connection(ConnectionString);
                localConnection.Open();

                //var sql = File.ReadAllText(@"Db2\db2_inst.sql");
                //ExecuteCommand(localConnection, sql);
                //_initialized = true;
                //return;

                #region EMP

                ExecuteCommand(localConnection, @"
BEGIN
    IF EXISTS(SELECT TABNAME FROM SYSCAT.TABLES WHERE TABSCHEMA='DB2INST1' AND TABNAME='EMP') THEN
        PREPARE stmt FROM 'DROP TABLE DB2INST1.EMP';
        EXECUTE stmt;
    END IF;
END
                ");

                ExecuteCommand(localConnection, @"
CREATE TABLE ""EMP""(
    ""ID"" INT NOT NULL,
    ""NAME"" VARCHAR(30),
    ""SALARY"" DECIMAL(18, 2) NOT NULL,
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

                #endregion

                #region MetalGearCharacters

                ExecuteCommand(localConnection, @"
BEGIN
    IF EXISTS(SELECT TABNAME FROM SYSCAT.TABLES WHERE TABSCHEMA='DB2INST1' AND TABNAME='MetalGearCharacters') THEN
        PREPARE stmt FROM 'DROP TABLE DB2INST1.""MetalGearCharacters""';
        EXECUTE stmt;
    END IF;
END
");

                ExecuteCommand(localConnection, @"
CREATE TABLE ""MetalGearCharacters""(
    ""ID"" INT GENERATED ALWAYS AS IDENTITY NOT NULL,
    ""NAME"" VARCHAR(30),
    ""HEIGHT"" DECIMAL(18, 2),
    ""CREATE_DATE"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    ""VERSION"" BIGINT NOT NULL,
    PRIMARY KEY(""ID"")
)
");
                ExecuteCommand(localConnection, @"
INSERT INTO ""MetalGearCharacters""(""NAME"", ""HEIGHT"", ""VERSION"")VALUES('Solid Snake',182,1);
");

                #endregion

                #region MetalGearSeries


                ExecuteCommand(localConnection, @"
BEGIN
    IF EXISTS(SELECT TABNAME FROM SYSCAT.TABLES WHERE TABSCHEMA='DB2INST1' AND TABNAME='MetalGearSeries') THEN
        PREPARE stmt FROM 'DROP TABLE DB2INST1.""MetalGearSeries""';
        EXECUTE stmt;
    END IF;
END
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
BEGIN
    IF EXISTS(SELECT SEQNAME FROM SYSCAT.SEQUENCES WHERE SEQSCHEMA='DB2INST1' AND SEQNAME='METAL_GEAR_SERIES_SEQ') THEN
        PREPARE stmt FROM 'DROP SEQUENCE DB2INST1.METAL_GEAR_SERIES_SEQ';
        EXECUTE stmt;
    END IF;
END
");
                ExecuteCommand(localConnection, @"
CREATE SEQUENCE METAL_GEAR_SERIES_SEQ
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE
    NOCACHE
    ORDER 
");
                ExecuteCommand(localConnection, @"
INSERT INTO ""MetalGearSeries""(""ID"", ""NAME"", ""RELEASE_DATE"", ""PLATFORM"", ""VERSION"")VALUES
(NEXT VALUE FOR METAL_GEAR_SERIES_SEQ, 'METAL GEAR', TIMESTAMP('1987-07-13'), 'MSX2', 1)
");

                #endregion

                #region EMP_SEQ


                ExecuteCommand(localConnection, @"
BEGIN
    IF EXISTS(SELECT TABNAME FROM SYSCAT.TABLES WHERE TABSCHEMA='DB2INST1' AND TABNAME='EMP_SEQ') THEN
        PREPARE stmt FROM 'DROP TABLE DB2INST1.""EMP_SEQ""';
        EXECUTE stmt;
    END IF;
END
                ");


                ExecuteCommand(localConnection, @"
CREATE TABLE ""EMP_SEQ""(
    ""ID"" INT GENERATED ALWAYS AS IDENTITY NOT NULL,
    ""NAME"" VARCHAR(50) NOT NULL,
    ""SHORT_COL"" SMALLINT NOT NULL,
    ""INT_COL"" INTEGER NOT NULL,
    ""LONG_COL"" BIGINT NOT NULL,
    ""DECIMAL_COL"" DECIMAL NOT NULL,
    ""STRING_COL"" VARCHAR(10) NOT NULL,
    ""VERSION"" BIGINT NOT NULL,
    PRIMARY KEY(""ID"")
)
");

                ExecuteCommand(localConnection, @"
BEGIN
    IF EXISTS(SELECT SEQNAME FROM SYSCAT.SEQUENCES WHERE SEQSCHEMA='DB2INST1' AND SEQNAME='SHORT_SEQ') THEN
        PREPARE stmt FROM 'DROP SEQUENCE DB2INST1.SHORT_SEQ';
        EXECUTE stmt;
    END IF;
END
");

                ExecuteCommand(localConnection, @"
CREATE SEQUENCE SHORT_SEQ AS SMALLINT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE
    NOCACHE
    ORDER 
");

                ExecuteCommand(localConnection, @"
BEGIN
    IF EXISTS(SELECT SEQNAME FROM SYSCAT.SEQUENCES WHERE SEQSCHEMA='DB2INST1' AND SEQNAME='INT_SEQ') THEN
        PREPARE stmt FROM 'DROP SEQUENCE DB2INST1.INT_SEQ';
        EXECUTE stmt;
    END IF;
END
");

                ExecuteCommand(localConnection, @"
CREATE SEQUENCE INT_SEQ AS INTEGER
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE
    NOCACHE
    ORDER 
");

                ExecuteCommand(localConnection, @"
BEGIN
    IF EXISTS(SELECT SEQNAME FROM SYSCAT.SEQUENCES WHERE SEQSCHEMA='DB2INST1' AND SEQNAME='LONG_SEQ') THEN
        PREPARE stmt FROM 'DROP SEQUENCE DB2INST1.LONG_SEQ';
        EXECUTE stmt;
    END IF;
END
");

                ExecuteCommand(localConnection, @"
CREATE SEQUENCE LONG_SEQ AS BIGINT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE
    NOCACHE
    ORDER 
");

                ExecuteCommand(localConnection, @"
BEGIN
    IF EXISTS(SELECT SEQNAME FROM SYSCAT.SEQUENCES WHERE SEQSCHEMA='DB2INST1' AND SEQNAME='DECIMAL_SEQ') THEN
        PREPARE stmt FROM 'DROP SEQUENCE DB2INST1.DECIMAL_SEQ';
        EXECUTE stmt;
    END IF;
END
");

                ExecuteCommand(localConnection, @"
CREATE SEQUENCE DECIMAL_SEQ AS DECIMAL
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE
    NOCACHE
    ORDER 
");

                ExecuteCommand(localConnection, @"
BEGIN
    IF EXISTS(SELECT SEQNAME FROM SYSCAT.SEQUENCES WHERE SEQSCHEMA='DB2INST1' AND SEQNAME='STRING_SEQ') THEN
        PREPARE stmt FROM 'DROP SEQUENCE DB2INST1.STRING_SEQ';
        EXECUTE stmt;
    END IF;
END
");

                ExecuteCommand(localConnection, @"
CREATE SEQUENCE STRING_SEQ AS SMALLINT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE
    NOCACHE
    ORDER 
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
