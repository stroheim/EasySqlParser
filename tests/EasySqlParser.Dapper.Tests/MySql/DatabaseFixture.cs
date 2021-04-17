using System;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace EasySqlParser.Dapper.Tests.MySql
{
    public class DatabaseFixture : IDisposable
    {
        private const string ConnectionString = @"server=localhost;port=53306;uid=user01;pwd=userpass;database=sample";
        public DbConnection Connection { get; }

        private static readonly object _lock = new object();
        private static bool _initialized;

        public DatabaseFixture()
        {
            Connection = new MySqlConnection(ConnectionString);
            Seed();
            Connection.Open();
        }

        private void Seed()
        {
            lock (_lock)
            {
                if (_initialized) return;

                using var localConnection = new MySqlConnection(ConnectionString);
                localConnection.Open();

                #region EMP

                ExecuteCommand(localConnection, @"
DROP TABLE IF EXISTS `EMP`
");

                ExecuteCommand(localConnection, @"
CREATE TABLE `EMP`(
    `ID` INT NOT NULL,
    `NAME` VARCHAR(30),
    `SALARY` NUMERIC(10, 0) NOT NULL,
    `VERSION` BIGINT NOT NULL,
    PRIMARY KEY(`ID`)
)
");

                ExecuteCommand(localConnection, @"
INSERT INTO `EMP`(
`ID`,
`NAME`,
`SALARY`,
`VERSION`
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
DROP TABLE IF EXISTS `MetalGearCharacters`
");

                ExecuteCommand(localConnection, @"
CREATE TABLE `MetalGearCharacters`(
    `ID` INT AUTO_INCREMENT NOT NULL,
    `NAME` VARCHAR(30),
    `HEIGHT` NUMERIC(10, 2),
    `CREATE_DATE` DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL,
    `VERSION` BIGINT NOT NULL,
    PRIMARY KEY(`ID`)
)
");

                ExecuteCommand(localConnection, @"
INSERT INTO `MetalGearCharacters`(`NAME`, `HEIGHT`, `VERSION`)VALUES('Solid Snake',182,1);
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
