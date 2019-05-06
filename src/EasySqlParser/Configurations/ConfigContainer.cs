using System;
using System.Collections.Generic;
using System.Data;
using EasySqlParser.Exceptions;
using EasySqlParser.Internals.Dialect;

namespace EasySqlParser.Configurations
{
    /// <summary>
    /// Container of configuration
    /// </summary>
    public static class ConfigContainer
    {
        static ConfigContainer()
        {
        }

        /// <summary>
        /// Gets "additional" sql parser configurations
        /// </summary>
        public static Dictionary<string, SqlParserConfig> AdditionalConfigs { get; } =
            new Dictionary<string, SqlParserConfig>();

        /// <summary>
        /// Gets "default" sql parser configuration
        /// </summary>
        public static SqlParserConfig DefaultConfig { get; private set; }

        /// <summary>
        /// Gets or sets whether to cache sql nodes
        /// </summary>
        public static bool EnableCache { get; set; } = true;

        // for unit test
        internal static SqlParserConfig CreateConfigForTest(DbConnectionKind dbConnectionKind, string configName)
        {
            return CreateConfig(dbConnectionKind, null);
        }

        /// <summary>
        /// Add "default" sql parser configuration
        /// </summary>
        /// <param name="dbConnectionKind">A kind of DB connection</param>
        /// <param name="dbParameterCreator">Delegate for create <see cref="IDbDataParameter"/> instance.</param>
        public static void AddDefault(DbConnectionKind dbConnectionKind, Func<IDbDataParameter> dbParameterCreator)
        {
            ValidateParameter(dbConnectionKind, dbParameterCreator);

            if (DefaultConfig == null)
            {
                DefaultConfig = CreateConfig(dbConnectionKind, dbParameterCreator);
            }

        }

        /// <summary>
        /// Add "additional" sql parser configuration
        /// </summary>
        /// <param name="dbConnectionKind">A kind of DB connection</param>
        /// <param name="dbParameterCreator">Delegate for create <see cref="IDbDataParameter"/> instance.</param>
        /// <param name="configName">A name of configuration</param>
        public static void AddAdditional(DbConnectionKind dbConnectionKind, Func<IDbDataParameter> dbParameterCreator,
            string configName)
        {
            ValidateParameter(dbConnectionKind, dbParameterCreator);
            if (string.IsNullOrEmpty(configName))
            {
                throw new InvalidSqlParserConfigException(ExceptionMessageId.EspD004);
            }

            if (!AdditionalConfigs.ContainsKey(configName))
            {
                AdditionalConfigs.Add(configName, CreateConfig(dbConnectionKind, dbParameterCreator));
            }
        }

        private static void ValidateParameter(DbConnectionKind dbConnectionKind,
            Func<IDbDataParameter> dbParameterCreator)
        {
            if (dbConnectionKind == DbConnectionKind.Unknown)
            {
                throw new InvalidSqlParserConfigException(ExceptionMessageId.EspD002);
            }

            if (dbParameterCreator == null)
            {
                throw new InvalidSqlParserConfigException(ExceptionMessageId.EspD003);
            }
        }

        private static SqlParserConfig CreateConfig(DbConnectionKind dbConnectionKind,
            Func<IDbDataParameter> dbParameterCreator)
        {
            var config = new SqlParserConfig
                         {
                             DbConnectionKind = dbConnectionKind,
                             DataParameterCreator = dbParameterCreator
                         };
            switch (dbConnectionKind)
            {
                case DbConnectionKind.AS400:
                case DbConnectionKind.DB2:
                    config.Dialect = new Db2Dialect();
                    break;
                case DbConnectionKind.MySql:
                    config.Dialect = new MysqlDialect();
                    break;
                case DbConnectionKind.OracleLegacy:
                    config.Dialect = new Oracle11Dialect();
                    break;
                case DbConnectionKind.Oracle:
                    config.Dialect = new OracleDialect();
                    break;
                case DbConnectionKind.PostgreSql:
                    config.Dialect = new PostgresDialect();
                    break;
                case DbConnectionKind.SQLite:
                    config.Dialect = new SqliteDialect();
                    break;
                case DbConnectionKind.SqlServerLegacy:
                    config.Dialect = new Mssql2008Dialect();
                    break;
                case DbConnectionKind.SqlServer:
                    config.Dialect = new MssqlDialect();
                    break;
                case DbConnectionKind.Odbc:
                case DbConnectionKind.OleDb:
                    config.Dialect = new StandardDialect();
                    config.Dialect.UseOdbcDateFormat = true;
                    break;
                default:
                    config.Dialect = new StandardDialect();
                    break;
            }

            return config;
        }

    }

}
