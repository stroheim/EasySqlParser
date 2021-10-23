using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using EasySqlParser.Configurations;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Configurations;
using EasySqlParser.SqlGenerator.Enums;
using EasySqlParser.SqlGenerator.Helpers;

namespace EasySqlParser.Dapper.Extensions
{
    internal static class FormattableStringHelper
    {
        internal static (string Sql, List<IDbDataParameter> Parameters) CreateParameters(FormattableString sqlTemplate, SqlParserConfig config)
        {
            if (sqlTemplate.ArgumentCount == 0)
            {
                return (sqlTemplate.Format, null);
            }

            var substitutions = new List<string>();
            //var parameters = new DynamicParameters();
            var parameters = new List<IDbDataParameter>();
            for (var i = 0; i < sqlTemplate.ArgumentCount; i++)
            {
                var parameterKey = $"p{i}";
                string parameterName;
                if (!config.Dialect.EnableNamedParameter)
                {
                    substitutions.Add(config.Dialect.ParameterPrefix);
                    parameterName = config.Dialect.ParameterPrefix;
                }
                else
                {
                    substitutions.Add(parameterKey);
                    parameterName = config.Dialect.ParameterPrefix + parameterKey;
                }
                var paramValue = sqlTemplate.GetArgument(i);
                var parameter = config.DataParameterCreator();
                parameter.ParameterName = parameterName;
                if (paramValue == null)
                {
                    //parameters.Add(parameterName, DBNull.Value);
                    parameter.Value = DBNull.Value;
                    parameters.Add(parameter);
                    continue;
                }
                //parameters.Add(parameterName, paramValue);
                parameter.Value = paramValue;
                parameters.Add(parameter);
            }

            // ReSharper disable once CoVariantArrayConversion
            return (string.Format(sqlTemplate.Format, substitutions.ToArray()), parameters);

        }
    }

    /// <summary>
    ///     Extension methods for Dapper.
    /// </summary>
    public static class DapperExtension
    {
        /// <summary>
        ///     Convert from <see cref="IDbDataParameter"/> to <see cref="DynamicParameters"/>.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this IReadOnlyList<IDbDataParameter> parameters)
        {
            var result = new DynamicParameters();
            foreach (var parameter in parameters)
            {

                result.Add(parameter.ParameterName,
                    parameter.Value,
                    parameter.DbType,
                    parameter.Direction,
                    parameter.Size,
                    parameter.Precision,
                    parameter.Scale);
            }

            return result;
        }

        #region ExecuteReader use SqlParserResult

        /// <summary>
        ///     Executes a query, and get the first record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="configuration"></param>
        /// <param name="parserResult"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static T ExecuteReaderFirst<T>(this DbConnection connection,
            IQueryBuilderConfiguration configuration,
            SqlParserResult parserResult,
            int commandTimeout = -1,
            DbTransaction transaction = null)
            where T : class
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            configuration.LoggerAction?.Invoke(parserResult.DebugSql);
            int? localCommandTimeout = null;
            if (configuration.CommandTimeout > -1)
            {
                localCommandTimeout = configuration.CommandTimeout;
            }
            if (commandTimeout > -1)
            {
                localCommandTimeout = commandTimeout;
            }

            return connection.QueryFirst<T>(parserResult.ParsedSql,
                parserResult.DbDataParameters.ToDynamicParameters(), transaction, localCommandTimeout);

        }

        /// <summary>
        ///     Asynchronously executes a query, and get the first record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="configuration"></param>
        /// <param name="parserResult"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static Task<T> ExecuteReaderFirstAsync<T>(this DbConnection connection,
            IQueryBuilderConfiguration configuration,
            SqlParserResult parserResult,
            int commandTimeout = -1,
            DbTransaction transaction = null)
            where T : class
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            configuration.LoggerAction?.Invoke(parserResult.DebugSql);
            int? localCommandTimeout = null;
            if (configuration.CommandTimeout > -1)
            {
                localCommandTimeout = configuration.CommandTimeout;
            }
            if (commandTimeout > -1)
            {
                localCommandTimeout = commandTimeout;
            }

            return connection.QueryFirstAsync<T>(parserResult.ParsedSql,
                parserResult.DbDataParameters.ToDynamicParameters(), transaction, localCommandTimeout);
        }

        /// <summary>
        ///     Executes a query, and get the all records.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="configuration"></param>
        /// <param name="parserResult"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static IEnumerable<T> ExecuteReader<T>(this DbConnection connection,
            IQueryBuilderConfiguration configuration,
            SqlParserResult parserResult,
            int commandTimeout = -1,
            DbTransaction transaction = null)
            where T : class
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            configuration.LoggerAction?.Invoke(parserResult.DebugSql);
            int? localCommandTimeout = null;
            if (configuration.CommandTimeout > -1)
            {
                localCommandTimeout = configuration.CommandTimeout;
            }
            if (commandTimeout > -1)
            {
                localCommandTimeout = commandTimeout;
            }

            return connection.Query<T>(parserResult.ParsedSql, parserResult.DbDataParameters.ToDynamicParameters(),
                transaction, commandTimeout: localCommandTimeout);
        }

        /// <summary>
        ///     Asynchronously executes a query, and get the all records.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="configuration"></param>
        /// <param name="parserResult"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static Task<IEnumerable<T>> ExecuteReaderAsync<T>(this DbConnection connection,
            IQueryBuilderConfiguration configuration,
            SqlParserResult parserResult,
            int commandTimeout = -1,
            DbTransaction transaction = null)
            where T : class
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            configuration.LoggerAction?.Invoke(parserResult.DebugSql);
            int? localCommandTimeout = null;
            if (configuration.CommandTimeout > -1)
            {
                localCommandTimeout = configuration.CommandTimeout;
            }
            if (commandTimeout > -1)
            {
                localCommandTimeout = commandTimeout;
            }

            return connection.QueryAsync<T>(parserResult.ParsedSql, parserResult.DbDataParameters.ToDynamicParameters(),
                transaction, commandTimeout: localCommandTimeout);
        }
        #endregion

        #region ExecuteReader use Query Expression
        /// <summary>
        ///     Executes a query, and get the first record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="configuration"></param>
        /// <param name="predicate"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static T ExecuteReaderFirst<T>(this DbConnection connection,
            IQueryBuilderConfiguration configuration,
            Expression<Func<T, bool>> predicate,
            int commandTimeout = -1,
            DbTransaction transaction = null)
            where T : class
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            var builderResult = QueryBuilder.GetSelectSql(configuration, predicate);
            configuration.LoggerAction?.Invoke(builderResult.DebugSql);

            int? localCommandTimeout = null;
            if (configuration.CommandTimeout > -1)
            {
                localCommandTimeout = configuration.CommandTimeout;
            }
            if (commandTimeout > -1)
            {
                localCommandTimeout = commandTimeout;
            }


            return connection.QueryFirst<T>(builderResult.ParsedSql,
                builderResult.DbDataParameters.ToDynamicParameters(), transaction, localCommandTimeout);
        }

        /// <summary>
        ///     Asynchronously executes a query, and get the first record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="configuration"></param>
        /// <param name="predicate"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static Task<T> ExecuteReaderFirstAsync<T>(this DbConnection connection,
            IQueryBuilderConfiguration configuration,
            Expression<Func<T, bool>> predicate,
            int commandTimeout = -1,
            DbTransaction transaction = null)
            where T : class
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            var builderResult = QueryBuilder.GetSelectSql(configuration, predicate);
            configuration.LoggerAction?.Invoke(builderResult.DebugSql);

            int? localCommandTimeout = null;
            if (configuration.CommandTimeout > -1)
            {
                localCommandTimeout = configuration.CommandTimeout;
            }
            if (commandTimeout > -1)
            {
                localCommandTimeout = commandTimeout;
            }
            return connection.QueryFirstAsync<T>(builderResult.ParsedSql,
                builderResult.DbDataParameters.ToDynamicParameters(), transaction, localCommandTimeout);
        }

        /// <summary>
        ///     Executes a query, and get the all records.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="configuration"></param>
        /// <param name="predicate"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static IEnumerable<T> ExecuteReader<T>(this DbConnection connection,
            IQueryBuilderConfiguration configuration,
            Expression<Func<T, bool>> predicate,
            int commandTimeout = -1,
            DbTransaction transaction = null)
            where T : class
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            var builderResult = QueryBuilder.GetSelectSql(configuration, predicate);
            configuration.LoggerAction?.Invoke(builderResult.DebugSql);
            int? localCommandTimeout = null;
            if (configuration.CommandTimeout > -1)
            {
                localCommandTimeout = configuration.CommandTimeout;
            }
            if (commandTimeout > -1)
            {
                localCommandTimeout = commandTimeout;
            }

            return connection.Query<T>(builderResult.ParsedSql,
                builderResult.DbDataParameters.ToDynamicParameters(), transaction, commandTimeout: localCommandTimeout);
        }

        /// <summary>
        ///     Asynchronously executes a query, and get the all records.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="configuration"></param>
        /// <param name="predicate"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static Task<IEnumerable<T>> ExecuteReaderAsync<T>(this DbConnection connection,
            IQueryBuilderConfiguration configuration,
            Expression<Func<T, bool>> predicate,
            int commandTimeout = -1,
            DbTransaction transaction = null)
            where T : class
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            var builderResult = QueryBuilder.GetSelectSql(configuration, predicate);
            configuration.LoggerAction?.Invoke(builderResult.DebugSql);
            int? localCommandTimeout = null;
            if (configuration.CommandTimeout > -1)
            {
                localCommandTimeout = configuration.CommandTimeout;
            }
            if (commandTimeout > -1)
            {
                localCommandTimeout = commandTimeout;
            }

            return connection.QueryAsync<T>(builderResult.ParsedSql,
                builderResult.DbDataParameters.ToDynamicParameters(), transaction, localCommandTimeout);
        }
        #endregion

        /// <summary>
        ///     Gets a record count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="configuration"></param>
        /// <param name="predicate"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static int GetCount<T>(this DbConnection connection,
            IQueryBuilderConfiguration configuration,
            Expression<Func<T, bool>> predicate,
            int commandTimeout = -1,
            DbTransaction transaction = null)
            where T : class
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            var builderResult = QueryBuilder.GetCountSql(predicate, configuration);
            configuration.LoggerAction?.Invoke(builderResult.DebugSql);
            int? localCommandTimeout = null;
            if (configuration.CommandTimeout > -1)
            {
                localCommandTimeout = configuration.CommandTimeout;
            }
            if (commandTimeout > -1)
            {
                localCommandTimeout = commandTimeout;
            }

            return connection.ExecuteScalar<int>(builderResult.ParsedSql,
                builderResult.DbDataParameters.ToDynamicParameters(), transaction, localCommandTimeout);
        }

        /// <summary>
        ///     Asynchronously gets a record count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="configuration"></param>
        /// <param name="predicate"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static Task<int> GetCountAsync<T>(this DbConnection connection,
            IQueryBuilderConfiguration configuration,
            Expression<Func<T, bool>> predicate,
            int commandTimeout = -1,
            DbTransaction transaction = null)
            where T : class
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            var builderResult = QueryBuilder.GetCountSql(predicate, configuration);
            configuration.LoggerAction?.Invoke(builderResult.DebugSql);
            int? localCommandTimeout = null;
            if (configuration.CommandTimeout > -1)
            {
                localCommandTimeout = configuration.CommandTimeout;
            }
            if (commandTimeout > -1)
            {
                localCommandTimeout = commandTimeout;
            }

            return connection.ExecuteScalarAsync<int>(builderResult.ParsedSql,
                builderResult.DbDataParameters.ToDynamicParameters(), transaction, localCommandTimeout);
        }

        /// <summary>
        ///     Executes a query with no results.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="parserResult"></param>
        /// <param name="merge">Set true for SQL Server merge statements, otherwise set false.</param>
        /// <param name="commandTimeout"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(
            this DbConnection connection,
            SqlParserResult parserResult,
            bool merge = false,
            int commandTimeout = -1,
            DbTransaction transaction = null)
        {
            var localTransaction = transaction;
            var beganTransaction = false;
            if (localTransaction == null)
            {
                localTransaction = connection.BeginTransaction();
                beganTransaction = true;
            }
            int affectedCount;

            try
            {
                using var command =
                    CommandHelper.BuildCommand(connection, parserResult,
                        merge, commandTimeout, localTransaction);
                affectedCount = command.ExecuteNonQuery();
                if (beganTransaction)
                {
                    localTransaction.Commit();
                }
            }
            finally
            {
                if (beganTransaction)
                {
                    localTransaction.Rollback();
                }
            }

            return affectedCount;
        }

        /// <summary>
        ///     Asynchronously executes a query with no results.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="parserResult"></param>
        /// <param name="merge">Set true for SQL Server merge statements, otherwise set false.</param>
        /// <param name="commandTimeout"></param>
        /// <param name="transaction"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<int> ExecuteNonQueryAsync(
            this DbConnection connection,
            SqlParserResult parserResult,
            bool merge = false,
            int commandTimeout = -1,
            DbTransaction transaction = null,
            CancellationToken cancellationToken = default)
        {
            var localTransaction = transaction;
            var beganTransaction = false;
            if (localTransaction == null)
            {
                localTransaction = connection.BeginTransaction();
                beganTransaction = true;
            }
            int affectedCount;

            try
            {
                using var command =
                    CommandHelper.BuildCommand(connection, parserResult,
                        merge, commandTimeout, localTransaction);
                affectedCount = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                if (beganTransaction)
                {
                    localTransaction.Commit();
                }
            }
            finally
            {
                if (beganTransaction)
                {
                    localTransaction.Rollback();
                }
            }

            return affectedCount;

        }

        /// <summary>
        ///     Executes a query with no results.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="sqlTemplate"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="configName"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(
            this DbConnection connection,
            FormattableString sqlTemplate,
            int commandTimeout = -1,
            string configName = null,
            DbTransaction transaction = null)
        {
            if (sqlTemplate == null)
            {
                throw new ArgumentNullException(nameof(sqlTemplate));
            }
            var config = configName == null
                ? ConfigContainer.DefaultConfig
                : ConfigContainer.AdditionalConfigs[configName];

            var localTransaction = transaction;
            var beganTransaction = false;
            if (localTransaction == null)
            {
                localTransaction = connection.BeginTransaction();
                beganTransaction = true;
            }
            int affectedCount;
            try
            {
                using var command =
                    CommandHelper.BuildCommand(connection, sqlTemplate, config, commandTimeout, localTransaction);
                affectedCount = command.ExecuteNonQuery();
                if (beganTransaction)
                {
                    localTransaction.Commit();
                }
            }
            finally
            {
                if (beganTransaction)
                {
                    localTransaction.Rollback();
                }
            }

            return affectedCount;
        }

        /// <summary>
        ///     Asynchronously executes a query with no results.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="sqlTemplate"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="configName"></param>
        /// <param name="transaction"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<int> ExecuteNonQueryAsync(
            this DbConnection connection,
            FormattableString sqlTemplate,
            int commandTimeout = -1,
            string configName = null,
            DbTransaction transaction = null,
            CancellationToken cancellationToken = default)
        {
            if (sqlTemplate == null)
            {
                throw new ArgumentNullException(nameof(sqlTemplate));
            }
            var config = configName == null
                ? ConfigContainer.DefaultConfig
                : ConfigContainer.AdditionalConfigs[configName];

            var localTransaction = transaction;
            var beganTransaction = false;
            if (localTransaction == null)
            {
                localTransaction = connection.BeginTransaction();
                beganTransaction = true;
            }

            int affectedCount;
            try
            {
                using var command =
                    CommandHelper.BuildCommand(connection, sqlTemplate, config, commandTimeout, localTransaction);
                affectedCount = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                if (beganTransaction)
                {
                    localTransaction.Commit();
                }

            }
            finally
            {
                if (beganTransaction)
                {
                    localTransaction.Rollback();
                }
            }

            return affectedCount;
        }

        /// <summary>
        ///     Executes a query with no results.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="builderParameter"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static int Execute(this DbConnection connection,
            QueryBuilderParameter builderParameter,
            DbTransaction transaction = null)
        {
            if (builderParameter == null)
            {
                throw new ArgumentNullException(nameof(builderParameter));
            }
            SequenceHelper.Generate(connection, builderParameter);
            var builderResult = QueryBuilder.GetQueryBuilderResult(builderParameter);
            builderParameter.WriteLog(builderResult.DebugSql);

            int affectedCount;
            builderParameter.SaveExpectedVersion();

            switch (builderParameter.CommandExecutionType)
            {
                case CommandExecutionType.ExecuteNonQuery:
                    affectedCount =
                        ConsumeHelper.ConsumeNonQuery(connection, builderParameter, builderResult, transaction);
                    break;
                case CommandExecutionType.ExecuteReader:
                    affectedCount =
                        ConsumeHelper.ConsumeReader(connection, builderParameter, builderResult, transaction);
                    break;
                case CommandExecutionType.ExecuteScalar:
                    affectedCount =
                        ConsumeHelper.ConsumeScalar(connection, builderParameter, builderResult, transaction);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown CommandExecutionType:{builderParameter.CommandExecutionType}");
            }

            ThrowIfOptimisticLockException(builderParameter, affectedCount, builderResult, transaction);
            builderParameter.IncrementVersion();
            return affectedCount;

        }

        /// <summary>
        ///     Asynchronously executes a query with no results.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="builderParameter"></param>
        /// <param name="transaction"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<int> ExecuteAsync(this DbConnection connection,
            QueryBuilderParameter builderParameter,
            DbTransaction transaction = null,
            CancellationToken cancellationToken = default)
        {
            if (builderParameter == null)
            {
                throw new ArgumentNullException(nameof(builderParameter));
            }
            await SequenceHelper.GenerateAsync(connection, builderParameter, cancellationToken).ConfigureAwait(false);
            var builderResult = QueryBuilder.GetQueryBuilderResult(builderParameter);
            builderParameter.WriteLog(builderResult.DebugSql);
            int affectedCount;
            builderParameter.SaveExpectedVersion();
            switch (builderParameter.CommandExecutionType)
            {
                case CommandExecutionType.ExecuteNonQuery:
                    affectedCount =
                        await ConsumeHelper.ConsumeNonQueryAsync(connection, builderParameter, builderResult, transaction, cancellationToken)
                            .ConfigureAwait(false);
                    break;
                case CommandExecutionType.ExecuteReader:
                    affectedCount =
                        await ConsumeHelper.ConsumeReaderAsync(connection, builderParameter, builderResult, transaction)
                            .ConfigureAwait(false);
                    break;
                case CommandExecutionType.ExecuteScalar:
                    affectedCount =
                        await ConsumeHelper.ConsumeScalarAsync(connection, builderParameter, builderResult, transaction)
                            .ConfigureAwait(false);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown CommandExecutionType:{builderParameter.CommandExecutionType}");
            }

            ThrowIfOptimisticLockException(builderParameter, affectedCount, builderResult, transaction);
            builderParameter.IncrementVersion();
            return affectedCount;


        }

        private static void ThrowIfOptimisticLockException(
            QueryBuilderParameter parameter,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            int affectedCount,
            QueryBuilderResult builderResult,
            DbTransaction transaction)
        {
            if (parameter.ThrowableOptimisticLockException(affectedCount))
            {
                transaction?.Rollback();
                throw new OptimisticLockException(builderResult.ParsedSql, builderResult.DebugSql, parameter.SqlFile);
            }
        }
    }

    internal static class CommandHelper
    {
        internal static DbCommand BuildCommand(
            DbConnection connection,
            QueryBuilderParameter builderParameter,
            QueryBuilderResult builderResult,
            DbTransaction transaction)
        {
            var command = connection.CreateCommand();
            command.CommandText = builderResult.ParsedSql;
            command.Parameters.AddRange(builderResult.DbDataParameters.ToArray());
            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            if (builderParameter.CommandTimeout > -1)
            {
                command.CommandTimeout = builderParameter.CommandTimeout;
            }
            return command;
        }

        internal static DbCommand BuildCommand(
            DbConnection connection,
            FormattableString sqlTemplate,
            SqlParserConfig config,
            int commandTimeout,
            DbTransaction transaction)
        {
            var command = connection.CreateCommand();
            var (sql, parameters) = FormattableStringHelper.CreateParameters(sqlTemplate, config);
            command.CommandText = sql;
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters.ToArray());
            }
            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            if (commandTimeout > -1)
            {
                command.CommandTimeout = commandTimeout;
            }

            return command;
        }

        internal static DbCommand BuildCommand(
            DbConnection connection,
            SqlParserResult parserResult,
            bool merge,
            int commandTimeout,
            DbTransaction transaction)
        {
            var command = connection.CreateCommand();
            var sql = merge ? parserResult.ParsedSql + ";;" : parserResult.ParsedSql;
            command.CommandText = sql;
            command.Parameters.AddRange(parserResult.DbDataParameters.ToArray());
            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            if (commandTimeout > -1)
            {
                command.CommandTimeout = commandTimeout;
            }

            return command;
        }
    }

    internal static class ConsumeHelper
    {

        internal static int ConsumeNonQuery(DbConnection connection,
            QueryBuilderParameter builderParameter,
            QueryBuilderResult builderResult,
            DbTransaction transaction)
        {
            // returning value not working in dapper
            //var affectedCount = connection.Execute(
            //    builderResult.ParsedSql,
            //    builderResult.DbDataParameters.ToDynamicParameters(),
            //    transaction,
            //    builderParameter.CommandTimeout
            //);

            ////builderParameter.ApplyReturningColumns();
            //return affectedCount;
            using var command = CommandHelper.BuildCommand(connection, builderParameter, builderResult, transaction);
            var affectedCount = command.ExecuteNonQuery();
            builderParameter.ApplyReturningColumns();
            return affectedCount;
        }

        internal static async Task<int> ConsumeNonQueryAsync(DbConnection connection,
            QueryBuilderParameter builderParameter,
            QueryBuilderResult builderResult,
            DbTransaction transaction,
            CancellationToken cancellationToken)
        {
            //var affectedCount = await connection.ExecuteAsync(
            //    builderResult.ParsedSql,
            //    builderResult.DbDataParameters.ToDynamicParameters(),
            //    transaction,
            //    builderParameter.CommandTimeout)
            //    .ConfigureAwait(false);
            //return affectedCount;
            using var command = CommandHelper.BuildCommand(connection, builderParameter, builderResult, transaction);
            var affectedCount = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            builderParameter.ApplyReturningColumns();
            return affectedCount;
        }

        internal static int ConsumeReader(IDbConnection connection,
            QueryBuilderParameter builderParameter,
            QueryBuilderResult builderResult,
            DbTransaction transaction)
        {
            var entity = connection.QuerySingleOrDefault(builderParameter.Entity.GetType(),
                builderResult.ParsedSql,
                builderResult.DbDataParameters.ToDynamicParameters(),
                transaction,
                builderParameter.CommandTimeout);
            if (entity == null) return 0;
            builderParameter.ResetEntity(entity);
            if (!builderParameter.IsSameVersion())
            {
                return 0;
            }
            return 1;

        }

        internal static async Task<int> ConsumeReaderAsync(IDbConnection connection,
            QueryBuilderParameter builderParameter,
            QueryBuilderResult builderResult,
            DbTransaction transaction)
        {
            var entity = await connection.QuerySingleOrDefaultAsync(builderParameter.EntityType,
                builderResult.ParsedSql,
                builderResult.DbDataParameters.ToDynamicParameters(),
                transaction,
                builderParameter.CommandTimeout)
                .ConfigureAwait(false);
            if (entity == null) return 0;
            builderParameter.ResetEntity(entity);
            if (!builderParameter.IsSameVersion())
            {
                return 0;
            }

            return 1;
        }

        internal static int ConsumeScalar(IDbConnection connection,
            QueryBuilderParameter builderParameter,
            QueryBuilderResult builderResult,
            DbTransaction transaction)
        {
            var rawScalarValue = connection.ExecuteScalar(
                builderResult.ParsedSql,
                builderResult.DbDataParameters.ToDynamicParameters(),
                transaction,
                builderParameter.CommandTimeout);
            return DbCommandHelper.ConsumeScalar(rawScalarValue, builderParameter);
        }

        internal static async Task<int> ConsumeScalarAsync(IDbConnection connection,
            QueryBuilderParameter builderParameter,
            QueryBuilderResult builderResult,
            DbTransaction transaction)
        {
            var rawScalarValue = await connection.ExecuteScalarAsync(
                builderResult.ParsedSql,
                builderResult.DbDataParameters.ToDynamicParameters(),
                transaction,
                builderParameter.CommandTimeout)
                .ConfigureAwait(false);

            return DbCommandHelper.ConsumeScalar(rawScalarValue, builderParameter);
        }
    }

}
