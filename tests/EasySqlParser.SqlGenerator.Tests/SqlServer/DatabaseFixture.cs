using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace EasySqlParser.SqlGenerator.Tests.SqlServer
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

                    #region EMP

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
                    #endregion

                    #region EMP_WITH_DATE

                    ExecuteCommand(localConnection, @"CREATE TABLE [EMP_WITH_DATE](
[ID] int not null primary key,
[NAME] varchar(30),
[SALARY] numeric(10, 0) not null,
[DELETE_FLAG] bit DEFAULT 0 not null,
[CREATE_DATETIME] datetime2 DEFAULT CURRENT_TIMESTAMP not null,
[UPDATE_DATETIME] datetime2 ,
[DELETE_DATETIME] datetime2 ,
[VERSION] bigint not null
)");

                    ExecuteCommand(localConnection, @"INSERT INTO [EMP_WITH_DATE](
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
                    #endregion

                    #region EMP_WITH_DATE_USER

                    ExecuteCommand(localConnection, @"CREATE TABLE [EMP_WITH_DATE_USER](
[ID] int not null primary key,
[NAME] varchar(30),
[SALARY] numeric(10, 0) not null,
[DELETE_FLAG] bit DEFAULT 0 not null,
[CREATE_DATETIME] datetime2 DEFAULT CURRENT_TIMESTAMP not null,
[CREATE_USER] varchar(30),
[UPDATE_DATETIME] datetime2 ,
[UPDATE_USER] varchar(30),
[DELETE_DATETIME] datetime2 ,
[DELETE_USER] varchar(30),
[VERSION] bigint not null
)");


                    ExecuteCommand(localConnection, @"INSERT INTO [EMP_WITH_DATE_USER](
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
                    #endregion

                    #region MetalGearCharacters

                    ExecuteCommand(localConnection, @"CREATE TABLE [MetalGearCharacters](
    [ID] INT IDENTITY NOT NULL,
    [NAME] VARCHAR(30),
    [HEIGHT] NUMERIC(18, 2),
    [CREATE_DATE] DATETIME2 DEFAULT CURRENT_TIMESTAMP NOT NULL,
    [VERSION] BIGINT NOT NULL,
    PRIMARY KEY([ID])
);
");
                    ExecuteCommand(localConnection,
                        @"INSERT INTO [MetalGearCharacters]([NAME], [HEIGHT], [VERSION])VALUES('Solid Snake',182,1);");
                    #endregion

                    #region MetalGearSeries

                    ExecuteCommand(localConnection, @"
CREATE TABLE [MetalGearSeries](
    [ID] INT NOT NULL,
    [NAME] VARCHAR(50) NOT NULL,
    [RELEASE_DATE] DATETIME2 NOT NULL,
    [PLATFORM] VARCHAR(60) NOT NULL,
    [CREATE_DATE] DATETIME2 DEFAULT CURRENT_TIMESTAMP NOT NULL,
    [VERSION] BIGINT NOT NULL,
    PRIMARY KEY([ID])
);
CREATE SEQUENCE METAL_GEAR_SERIES_SEQ AS INT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;

");

                    ExecuteCommand(localConnection, @"
INSERT INTO [MetalGearSeries]([ID], [NAME], [RELEASE_DATE], [PLATFORM], [VERSION])VALUES
(NEXT VALUE FOR METAL_GEAR_SERIES_SEQ, 'METAL GEAR', '1987-07-13', 'MSX2', 1);

");
                    #endregion

                    #region EMP_SEQ
                    ExecuteCommand(localConnection, @"
CREATE TABLE [EMP_SEQ](
    [ID] INT IDENTITY NOT NULL,
    [NAME] VARCHAR(50) NOT NULL,
    [BYTE_COL] tinyint NOT NULL,
    [SHORT_COL] smallint NOT NULL,
    [INT_COL] int NOT NULL,
    [LONG_COL] bigint NOT NULL,
    [DECIMAL_COL] numeric(26) NOT NULL,
    [STRING_COL] VARCHAR(10) NOT NULL,
    [VERSION] BIGINT NOT NULL,
    PRIMARY KEY([ID])
);
CREATE SEQUENCE BYTE_SEQ AS TINYINT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;
CREATE SEQUENCE SHORT_SEQ AS SMALLINT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;
CREATE SEQUENCE INT_SEQ AS INT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;
CREATE SEQUENCE LONG_SEQ AS BIGINT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;
CREATE SEQUENCE DECIMAL_SEQ AS NUMERIC(26)
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;
CREATE SEQUENCE STRING_SEQ AS SMALLINT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;

");



                    #endregion

                    #region EMP_MULTIPLE_KEY


                    ExecuteCommand(localConnection, @"
CREATE TABLE [MetalGearSeries](
    [KEY_COL1] VARCHAR(10) NOT NULL,
    [KEY_COL2] VARCHAR(10) NOT NULL,
    [NAME] VARCHAR(50) NOT NULL,
    PRIMARY KEY([KEY_COL1],[KEY_COL2])
);
");

                    #endregion

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

    public class QueryExtensionFixture : FixtureBase
    {
        private static readonly object _lock = new object();
        private static bool _initialized;
        public QueryExtensionFixture() : base("QueryExtensionFixture")
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

                #region EMP

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
                #endregion

                ExecuteCommand(localConnection, @"CREATE TABLE [EMP_WITH_DATE_USER](
    [ID] INT NOT NULL,
    [NAME] VARCHAR(30),
    [SALARY] NUMERIC(10, 0) NOT NULL,
    [DELETE_FLAG] BIT DEFAULT 0 NOT NULL,
    [CREATE_DATETIME] DATETIME2 NOT NULL,
    [CREATE_USER] VARCHAR(30) NOT NULL,
    [UPDATE_DATETIME] DATETIME2,
    [UPDATE_USER] VARCHAR(30),
    [DELETE_DATETIME] DATETIME2,
    [DELETE_USER] VARCHAR(30),
    [VERSION] BIGINT NOT NULL,
    PRIMARY KEY([ID])
)");

                ExecuteCommand(localConnection, @"INSERT INTO [EMP_WITH_DATE_USER](
[ID],
[NAME],
[SALARY],
[CREATE_DATETIME],
[CREATE_USER],
[VERSION]
)VALUES
(1,'John Doe',0,CURRENT_TIMESTAMP,'admin',1),
(2,'Rob Walters',1,CURRENT_TIMESTAMP,'admin',1),
(3,'Gail Erickson',2,CURRENT_TIMESTAMP,'admin',1),
(4,'Jossef Goldberg',3,CURRENT_TIMESTAMP,'admin',1),
(5,'Dylan Miller',4,CURRENT_TIMESTAMP,'admin',1),
(6,'Diane Margheim',5,CURRENT_TIMESTAMP,'admin',1),
(7,'Gigi Matthew',6,CURRENT_TIMESTAMP,'admin',1),
(8,'Michael Raheem',7,CURRENT_TIMESTAMP,'admin',1),
(9,'Ovidiu Cracium',8,CURRENT_TIMESTAMP,'admin',1),
(10,'Janice Galvin',9,CURRENT_TIMESTAMP,'admin',1);
");

                _initialized = true;
            }
        }
    }

    public class IdentityFixture : FixtureBase
    {
        private static readonly object _lock = new object();
        private static bool _initialized;

        public IdentityFixture() : base("IdentityFixture")
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
                #region MetalGearCharacters

                ExecuteCommand(localConnection, @"CREATE TABLE [MetalGearCharacters](
    [ID] INT IDENTITY NOT NULL,
    [NAME] VARCHAR(30),
    [HEIGHT] NUMERIC(18, 2),
    [CREATE_DATE] DATETIME2 DEFAULT CURRENT_TIMESTAMP NOT NULL,
    [VERSION] BIGINT NOT NULL,
    PRIMARY KEY([ID])
);
");
                ExecuteCommand(localConnection,
                    @"INSERT INTO [MetalGearCharacters]([NAME], [HEIGHT], [VERSION])VALUES('Solid Snake',182,1);");
                #endregion

                _initialized = true;
            }
        }
    }

    public class SequenceFixture : FixtureBase
    {
        private static readonly object _lock = new object();
        private static bool _initialized;

        public SequenceFixture() : base("SequenceFixture")
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

                #region EMP_SEQ
                ExecuteCommand(localConnection, @"
CREATE TABLE [EMP_SEQ](
    [ID] INT IDENTITY NOT NULL,
    [NAME] VARCHAR(50) NOT NULL,
    [BYTE_COL] tinyint NOT NULL,
    [SHORT_COL] smallint NOT NULL,
    [INT_COL] int NOT NULL,
    [LONG_COL] bigint NOT NULL,
    [DECIMAL_COL] numeric(26) NOT NULL,
    [STRING_COL] VARCHAR(10) NOT NULL,
    [VERSION] BIGINT NOT NULL,
    PRIMARY KEY([ID])
);
CREATE SEQUENCE BYTE_SEQ AS TINYINT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;
CREATE SEQUENCE SHORT_SEQ AS SMALLINT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;
CREATE SEQUENCE INT_SEQ AS INT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;
CREATE SEQUENCE LONG_SEQ AS BIGINT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;
CREATE SEQUENCE DECIMAL_SEQ AS NUMERIC(26)
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;
CREATE SEQUENCE STRING_SEQ AS SMALLINT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;

");



                #endregion

                #region EMP_SEQ_FOR_ASYNC
                ExecuteCommand(localConnection, @"
CREATE TABLE [EMP_SEQ_FOR_ASYNC](
    [ID] INT IDENTITY NOT NULL,
    [NAME] VARCHAR(50) NOT NULL,
    [BYTE_COL] tinyint NOT NULL,
    [SHORT_COL] smallint NOT NULL,
    [INT_COL] int NOT NULL,
    [LONG_COL] bigint NOT NULL,
    [DECIMAL_COL] numeric(26) NOT NULL,
    [STRING_COL] VARCHAR(10) NOT NULL,
    [VERSION] BIGINT NOT NULL,
    PRIMARY KEY([ID])
);
CREATE SEQUENCE BYTE_SEQ_FOR_ASYNC AS TINYINT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;
CREATE SEQUENCE SHORT_SEQ_FOR_ASYNC AS SMALLINT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;
CREATE SEQUENCE INT_SEQ_FOR_ASYNC AS INT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;
CREATE SEQUENCE LONG_SEQ_FOR_ASYNC AS BIGINT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;
CREATE SEQUENCE DECIMAL_SEQ_FOR_ASYNC AS NUMERIC(26)
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;
CREATE SEQUENCE STRING_SEQ_FOR_ASYNC AS SMALLINT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;

");



                #endregion

                #region MetalGearSeries

                ExecuteCommand(localConnection, @"
CREATE TABLE [MetalGearSeries](
    [ID] INT NOT NULL,
    [NAME] VARCHAR(50) NOT NULL,
    [RELEASE_DATE] DATETIME2 NOT NULL,
    [PLATFORM] VARCHAR(60) NOT NULL,
    [CREATE_DATE] DATETIME2 DEFAULT CURRENT_TIMESTAMP NOT NULL,
    [VERSION] BIGINT NOT NULL,
    PRIMARY KEY([ID])
);
CREATE SEQUENCE METAL_GEAR_SERIES_SEQ AS INT
    INCREMENT BY 1
    START WITH 1
    --MAXVALUE 9999999999
    MINVALUE 1
    CYCLE 
    NO CACHE;

");

                ExecuteCommand(localConnection, @"
INSERT INTO [MetalGearSeries]([ID], [NAME], [RELEASE_DATE], [PLATFORM], [VERSION])VALUES
(NEXT VALUE FOR METAL_GEAR_SERIES_SEQ, 'METAL GEAR', '1987-07-13', 'MSX2', 1);

");
                #endregion


                _initialized = true;
            }
        }
    }
}
