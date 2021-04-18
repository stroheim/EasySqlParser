using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Configurations;
using EasySqlParser.SqlGenerator.Enums;
using EasySqlParser.SqlGenerator.Helpers;
using EasySqlParser.SqlGenerator.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace EasySqlParser.EntityFrameworkCore.Extensions
{

    internal static class RelationalCommandExtension
    {
        internal static int ConsumeScalar(this IRelationalCommand command,
            RelationalCommandParameterObject parameterObject,
            QueryBuilderParameter builderParameter)
        {
            var scalarValue = command.ExecuteScalar(parameterObject);
            return DbCommandHelper.ConsumeScalar(scalarValue, builderParameter);
        }

        internal static int ConsumeNonQuery(this IRelationalCommand command,
            RelationalCommandParameterObject parameterObject,
            QueryBuilderParameter builderParameter)
        {
            var affectedCount = command.ExecuteNonQuery(parameterObject);
            builderParameter.ApplyReturningColumns();
            return affectedCount;
        }

        internal static int ConsumeReader(this IRelationalCommand command, 
            RelationalCommandParameterObject parameterObject,
            QueryBuilderParameter builderParameter)
        {
            using var relationalDataReader = command.ExecuteReader(parameterObject);
            var reader = relationalDataReader.DbDataReader;
            return ConsumeReader(reader, builderParameter);
        }

        private static int ConsumeReader(DbDataReader reader, QueryBuilderParameter builderParameter)
        {
            if (!reader.HasRows)
            {
                reader.Close();
                reader.Dispose();
                return 0;
            }

            var entityInfo = builderParameter.EntityTypeInfo;
            reader.Read();
            var instance = builderParameter.Entity;
            builderParameter.WriteLog("[Start] ConsumeReader");
            DataReaderHelper.ReadRow(instance, reader, entityInfo, builderParameter.WriteLog);
            reader.Close();
            reader.Dispose();
            builderParameter.WriteLog("[End] ConsumeReader");
            if (!builderParameter.IsSameVersion())
            {
                return 0;
            }
            return 1;

        }

        internal static async Task<int> ConsumeScalarAsync(this IRelationalCommand command,
            RelationalCommandParameterObject parameterObject,
            QueryBuilderParameter builderParameter)
        {
            var scalarValue = await command.ExecuteScalarAsync(parameterObject).ConfigureAwait(false);
            return DbCommandHelper.ConsumeScalar(scalarValue, builderParameter);
        }

        internal static async Task<int> ConsumeNonQueryAsync(this IRelationalCommand command,
            RelationalCommandParameterObject parameterObject,
            QueryBuilderParameter builderParameter)
        {
            var affectedCount = await command.ExecuteNonQueryAsync(parameterObject).ConfigureAwait(false);
            builderParameter.ApplyReturningColumns();
            return affectedCount;
        }

        internal static async Task<int> ConsumeReaderAsync(this IRelationalCommand command,
            RelationalCommandParameterObject parameterObject,
            QueryBuilderParameter builderParameter,
            CancellationToken cancellationToken = default)
        {
            await using var relationalDataReader = await command.ExecuteReaderAsync(parameterObject, cancellationToken).ConfigureAwait(false);
            var reader = relationalDataReader.DbDataReader;
            return await ConsumeReaderAsync(reader, builderParameter, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<int> ConsumeReaderAsync(DbDataReader reader, 
            QueryBuilderParameter builderParameter,
            CancellationToken cancellationToken = default)
        {
            if (!reader.HasRows)
            {
                await reader.CloseAsync().ConfigureAwait(false);
                await reader.DisposeAsync().ConfigureAwait(false);
                return 0;
            }

            var entityInfo = builderParameter.EntityTypeInfo;
            await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            var instance = builderParameter.Entity;
            builderParameter.WriteLog("[Start] ConsumeReader");
            await DataReaderHelper.ReadRowAsync(instance, reader, entityInfo, builderParameter.WriteLog,
                cancellationToken);
            await reader.CloseAsync().ConfigureAwait(false);
            await reader.DisposeAsync().ConfigureAwait(false);
            builderParameter.WriteLog("[End] ConsumeReader");
            if (!builderParameter.IsSameVersion())
            {
                return 0;
            }
            return 1;
        }
    }

    internal static class DataReaderHelper
    {
        internal static void ReadRow<T>(T instance, DbDataReader reader,
            EntityTypeInfo entityInfo,
            Action<string> loggerAction = null)
            where T : class
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (reader.IsDBNull(i)) continue;
                ReadField(instance, reader, entityInfo, i, loggerAction);
            }
        }


        internal static async Task ReadRowAsync<T>(T instance, DbDataReader reader, 
            EntityTypeInfo entityInfo,
            Action<string> loggerAction = null,
            CancellationToken cancellationToken = default)
            where T : class
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(false)) continue;
                ReadField(instance, reader, entityInfo, i, loggerAction);
            }
        }

        private static void ReadField(object instance, DbDataReader reader,
            EntityTypeInfo entityInfo,
            int index,
            Action<string> loggerAction = null)
        {
            var value = reader.GetValue(index);
            var columnName = reader.GetName(index);
            var columnInfo = entityInfo.ColumnNameKeyDictionary[columnName];
            loggerAction?.Invoke($"{columnInfo.PropertyInfo.Name}\t{value}");
            if (columnInfo.ConvertFromProvider != null)
            {
                var converted = columnInfo.ConvertFromProvider(value);
                columnInfo.PropertyInfo.SetValue(instance, converted);
            }
            else
            {
                var type = value.GetType();
                var propertyType = columnInfo.PropertyInfo.PropertyType;
                if (columnInfo.NullableUnderlyingType != null)
                {
                    propertyType = columnInfo.NullableUnderlyingType;
                }
                if (propertyType == type)
                {
                    columnInfo.PropertyInfo.SetValue(instance, value);
                }
                else
                {
                    var converted = Convert.ChangeType(value, propertyType);
                    columnInfo.PropertyInfo.SetValue(instance, converted);
                }
            }

        }


    }

    // RelationalDatabaseFacadeExtensions

    // TODO: DOC
    /// <summary>
    /// Extension for <see cref="DatabaseFacade"/>
    /// </summary>
    public static class DatabaseFacadeExtension
    {
        // https://devadjust.exblog.jp/28241806/

        #region ExecuteScalar

        public static TResult ExecuteScalar<TResult>(
            this DatabaseFacade database,
            SqlParserResult parserResult,
            DbTransaction transaction = null)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }


            var concurrentDetector = facadeDependencies.ConcurrencyDetector;

            if (transaction != null)
            {
                database.UseTransaction(transaction);
            }


            using (concurrentDetector.EnterCriticalSection())
            {
                var (command, parameterObject) = CreateRelationalParameterObject(
                    facadeDependencies, facade, parserResult);
                var rawScalar = command.ExecuteScalar(parameterObject);
                if (rawScalar is TResult scalar)
                {
                    return scalar;
                }

                return (TResult) Convert.ChangeType(rawScalar, typeof(TResult));
            }
        }

        public static async Task<TResult> ExecuteScalarAsync<TResult>(
            this DatabaseFacade database,
            SqlParserResult parserResult,
            DbTransaction transaction = null,
            CancellationToken cancellationToken = default)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }

            var concurrentDetector = facadeDependencies.ConcurrencyDetector;

            if (transaction != null)
            {
                await database.UseTransactionAsync(transaction, cancellationToken).ConfigureAwait(false);
            }

            using (concurrentDetector.EnterCriticalSection())
            {
                var (command, parameterObject) = CreateRelationalParameterObject(
                    facadeDependencies, facade, parserResult);
                var rawScalar = await command.ExecuteScalarAsync(parameterObject, cancellationToken).ConfigureAwait(false);
                if (rawScalar is TResult scalar)
                {
                    return scalar;
                }

                return (TResult)Convert.ChangeType(rawScalar, typeof(TResult));
            }
        }

        public static TResult ExecuteScalar<TResult>(
            this DatabaseFacade database,
            FormattableString sqlTemplate,
            DbTransaction transaction = null)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }


            var concurrentDetector = facadeDependencies.ConcurrencyDetector;

            if (transaction != null)
            {
                database.UseTransaction(transaction);
            }

            using (concurrentDetector.EnterCriticalSection())
            {
                var command = facadeDependencies.RawSqlCommandBuilder
                    .Build(sqlTemplate.Format, sqlTemplate.GetArguments());
                var relationalCommand = command.RelationalCommand;
                var parameterObject = new RelationalCommandParameterObject(
                    facadeDependencies.RelationalConnection,
                    command.ParameterValues,
                    null,
                    facade.Context,
                    facadeDependencies.CommandLogger
                );
                var rawScalar = relationalCommand
                    .ExecuteScalar(parameterObject);
                if (rawScalar is TResult scalar)
                {
                    return scalar;
                }

                return (TResult) Convert.ChangeType(rawScalar, typeof(TResult));
            }


        }

        public static async Task<TResult> ExecuteScalarAsync<TResult>(
            this DatabaseFacade database,
            FormattableString sqlTemplate,
            DbTransaction transaction = null,
            CancellationToken cancellationToken = default)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }

            var concurrentDetector = facadeDependencies.ConcurrencyDetector;

            if (transaction != null)
            {
                await database.UseTransactionAsync(transaction, cancellationToken).ConfigureAwait(false);
            }

            using (concurrentDetector.EnterCriticalSection())
            {
                var command = facadeDependencies.RawSqlCommandBuilder
                    .Build(sqlTemplate.Format, sqlTemplate.GetArguments());
                var relationalCommand = command.RelationalCommand;
                var parameterObject = new RelationalCommandParameterObject(
                    facadeDependencies.RelationalConnection,
                    command.ParameterValues,
                    null,
                    facade.Context,
                    facadeDependencies.CommandLogger
                );
                var rawScalar = await relationalCommand
                    .ExecuteScalarAsync(parameterObject, cancellationToken)
                    .ConfigureAwait(false);
                if (rawScalar is TResult scalar)
                {
                    return scalar;
                }

                return (TResult)Convert.ChangeType(rawScalar, typeof(TResult));
            }

        }

        #endregion


        public static int ExecuteNonQuery(
            this DatabaseFacade database,
            QueryBuilderParameter builderParameter,
            DbTransaction transaction = null)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }

            database.SetCommandTimeout(builderParameter.CommandTimeout);

            var concurrentDetector = facadeDependencies.ConcurrencyDetector;

            if (transaction != null)
            {
                database.UseTransaction(transaction);
            }

            var dbContextTransaction = database.CurrentTransaction;
            var beganTransaction = false;
            if (dbContextTransaction == null)
            {
                dbContextTransaction = database.BeginTransaction();
                beganTransaction = true;
            }

            if (builderParameter.ThrowableOptimisticLockException() && dbContextTransaction == null)
            {
                throw new InvalidOperationException("トランザクションが開始されていないため楽観排他制御ができません");
            }

            using (concurrentDetector.EnterCriticalSection())
            {
                int affectedCount;
                try
                {
                    var connection = facadeDependencies.RelationalConnection.DbConnection;
                    SequenceHelper.Generate(connection, builderParameter);
                    var (command, parameterObject, debugSql) = CreateRelationalParameterObject(
                        facadeDependencies, facade, builderParameter);
                    builderParameter.SaveExpectedVersion();
                    switch (builderParameter.CommandExecutionType)
                    {
                        case CommandExecutionType.ExecuteNonQuery:
                            affectedCount = command.ConsumeNonQuery(parameterObject, builderParameter);
                            break;
                        case CommandExecutionType.ExecuteReader:
                            affectedCount = command.ConsumeReader(parameterObject, builderParameter);
                            break;
                        case CommandExecutionType.ExecuteScalar:
                            affectedCount = command.ConsumeScalar(parameterObject, builderParameter);
                            break;
                        default:
                            // TODO: error
                            throw new InvalidOperationException("");
                    }

                    ThrowIfOptimisticLockException(builderParameter, affectedCount, command.CommandText, debugSql,
                        dbContextTransaction);

                    if (builderParameter.SqlKind == SqlKind.Update)
                    {
                        builderParameter.IncrementVersion();
                    }

                    if (beganTransaction)
                    {
                        dbContextTransaction.Commit();
                    }
                }
                finally
                {
                    if (beganTransaction)
                    {
                        dbContextTransaction.Dispose();
                    }
                }
                return affectedCount;

            }
        }


        public static async Task<int> ExecuteNonQueryAsync(
            this DatabaseFacade database,
            QueryBuilderParameter builderParameter,
            DbTransaction transaction = null,
            CancellationToken cancellationToken = default)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }

            database.SetCommandTimeout(builderParameter.CommandTimeout);

            var concurrentDetector = facadeDependencies.ConcurrencyDetector;

            if (transaction != null)
            {
                await database.UseTransactionAsync(transaction, cancellationToken).ConfigureAwait(false);
            }

            var dbContextTransaction = database.CurrentTransaction;
            var beganTransaction = false;
            if (dbContextTransaction == null)
            {
                dbContextTransaction = await database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                beganTransaction = true;
            }

            if (builderParameter.ThrowableOptimisticLockException() && dbContextTransaction == null)
            {
                throw new InvalidOperationException("トランザクションが開始されていないため楽観排他制御ができません");
            }

            using (concurrentDetector.EnterCriticalSection())
            {
                int affectedCount;
                try
                {
                    var connection = facadeDependencies.RelationalConnection.DbConnection;
                    await SequenceHelper.GenerateAsync(connection, builderParameter).ConfigureAwait(false);
                    var (command, parameterObject, debugSql) = CreateRelationalParameterObject(
                        facadeDependencies, facade, builderParameter);
                    builderParameter.SaveExpectedVersion();
                    switch (builderParameter.CommandExecutionType)
                    {
                        case CommandExecutionType.ExecuteNonQuery:
                            affectedCount = await command.ConsumeNonQueryAsync(parameterObject, builderParameter)
                                .ConfigureAwait(false);
                            break;
                        case CommandExecutionType.ExecuteReader:
                            affectedCount = await command.ConsumeReaderAsync(parameterObject, builderParameter, cancellationToken)
                                .ConfigureAwait(false);
                            break;
                        case CommandExecutionType.ExecuteScalar:
                            affectedCount = await command.ConsumeScalarAsync(parameterObject, builderParameter)
                                .ConfigureAwait(false);
                            break;
                        default:
                            // TODO: error
                            throw new InvalidOperationException("");
                    }

                    await ThrowIfOptimisticLockExceptionAsync(builderParameter, affectedCount, command.CommandText, debugSql,
                        dbContextTransaction, cancellationToken).ConfigureAwait(false);

                    if (builderParameter.SqlKind == SqlKind.Update)
                    {
                        builderParameter.IncrementVersion();
                    }

                    if (beganTransaction)
                    {
                        await dbContextTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    if (beganTransaction)
                    {
                        await dbContextTransaction.DisposeAsync().ConfigureAwait(false);
                    }
                }
                return affectedCount;
            }
        }


        #region ExecuteReader use SqlParserResult

        public static T ExecuteReaderFirst<T>(
            this DatabaseFacade database,
            IQueryBuilderConfiguration configuration,
            SqlParserResult parserResult,
            DbTransaction transaction = null)
            where T : class
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }

            database.SetCommandTimeout(configuration.CommandTimeout);

            var concurrentDetector = facadeDependencies.ConcurrencyDetector;

            if (transaction != null)
            {
                database.UseTransaction(transaction);
            }

            using (concurrentDetector.EnterCriticalSection())
            {
                var entityInfo = configuration.GetEntityTypeInfo(typeof(T));
                var (command, parameterObject) = CreateRelationalParameterObject(
                    facadeDependencies, facade, parserResult);
                return InternalExecuteReaderFirst<T>(command, parameterObject, entityInfo);
            }
        }

        public static async Task<T> ExecuteReaderFirstAsync<T>(
            this DatabaseFacade database,
            IQueryBuilderConfiguration configuration,
            SqlParserResult parserResult,
            DbTransaction transaction = null,
            CancellationToken cancellationToken = default)
            where T : class
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }

            database.SetCommandTimeout(configuration.CommandTimeout);

            var concurrentDetector = facadeDependencies.ConcurrencyDetector;

            if (transaction != null)
            {
                await database.UseTransactionAsync(transaction, cancellationToken).ConfigureAwait(false);
            }

            using (concurrentDetector.EnterCriticalSection())
            {
                var entityInfo = configuration.GetEntityTypeInfo(typeof(T));
                var (command, parameterObject) = CreateRelationalParameterObject(
                    facadeDependencies, facade, parserResult);
                return await InternalExecuteReaderFirstAsync<T>(command, parameterObject, entityInfo,
                    cancellationToken);
            }

        }


        public static List<T> ExecuteReader<T>(
            this DatabaseFacade database,
            IQueryBuilderConfiguration configuration,
            SqlParserResult parserResult,
            DbTransaction transaction = null)
            where T : class
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return new List<T>();
            }

            database.SetCommandTimeout(configuration.CommandTimeout);

            var concurrentDetector = facadeDependencies.ConcurrencyDetector;

            if (transaction != null)
            {
                database.UseTransaction(transaction);
            }


            using (concurrentDetector.EnterCriticalSection())
            {
                var entityInfo = configuration.GetEntityTypeInfo(typeof(T));
                var (command, parameterObject) = CreateRelationalParameterObject(
                    facadeDependencies, facade, parserResult);
                return InternalExecuteReader<T>(command, parameterObject, entityInfo);
            }
        }

        public static async Task<List<T>> ExecuteReaderAsync<T>(
            this DatabaseFacade database,
            IQueryBuilderConfiguration configuration,
            SqlParserResult parserResult,
            DbTransaction transaction = null,
            CancellationToken cancellationToken = default)
            where T : class
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return new List<T>();
            }

            database.SetCommandTimeout(configuration.CommandTimeout);

            var concurrentDetector = facadeDependencies.ConcurrencyDetector;

            if (transaction != null)
            {
                await database.UseTransactionAsync(transaction, cancellationToken).ConfigureAwait(false);
            }


            using (concurrentDetector.EnterCriticalSection())
            {
                var entityInfo = configuration.GetEntityTypeInfo(typeof(T));
                var (command, parameterObject) = CreateRelationalParameterObject(
                    facadeDependencies, facade, parserResult);
                return await InternalExecuteReaderAsync<T>(command, parameterObject, entityInfo, cancellationToken);
            }
        }
        #endregion

        #region ExecuteReader use Query Expression

        public static T ExecuteReaderFirst<T>(
            this DatabaseFacade database,
            IQueryBuilderConfiguration configuration,
            Expression<Func<T, bool>> predicate,
            DbTransaction transaction = null,
            string configName = null)
            where T : class
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }

            database.SetCommandTimeout(configuration.CommandTimeout);

            var concurrentDetector = facadeDependencies.ConcurrencyDetector;

            if (transaction != null)
            {
                database.UseTransaction(transaction);
            }


            var builderResult = QueryBuilder.GetSelectSql(configuration, predicate, configName);

            using (concurrentDetector.EnterCriticalSection())
            {
                var entityInfo = configuration.GetEntityTypeInfo(typeof(T));
                var (command, parameterObject, _) = CreateRelationalParameterObject(
                    facadeDependencies, facade, null, builderResult, configuration.LoggerAction);
                return InternalExecuteReaderFirst<T>(command, parameterObject, entityInfo);
            }

        }

        public static async Task<T> ExecuteReaderFirstAsync<T>(
            this DatabaseFacade database,
            IQueryBuilderConfiguration configuration,
            Expression<Func<T, bool>> predicate,
            DbTransaction transaction = null,
            string configName = null,
            CancellationToken cancellationToken = default)
            where T : class
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }

            database.SetCommandTimeout(configuration.CommandTimeout);

            var concurrentDetector = facadeDependencies.ConcurrencyDetector;

            if (transaction != null)
            {
                await database.UseTransactionAsync(transaction, cancellationToken).ConfigureAwait(false);
            }

            var builderResult = QueryBuilder.GetSelectSql(configuration, predicate, configName);

            using (concurrentDetector.EnterCriticalSection())
            {
                var entityInfo = configuration.GetEntityTypeInfo(typeof(T));
                var (command, parameterObject, _) = CreateRelationalParameterObject(
                    facadeDependencies, facade, null, builderResult, configuration.LoggerAction);
                return await InternalExecuteReaderFirstAsync<T>(command, parameterObject, entityInfo,
                    cancellationToken);
            }
        }


        public static List<T> ExecuteReader<T>(
            this DatabaseFacade database,
            IQueryBuilderConfiguration configuration,
            Expression<Func<T, bool>> predicate,
            DbTransaction transaction = null,
            string configName = null)
            where T : class
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return new List<T>();
            }

            database.SetCommandTimeout(configuration.CommandTimeout);

            var concurrentDetector = facadeDependencies.ConcurrencyDetector;

            if (transaction != null)
            {
                database.UseTransaction(transaction);
            }


            var builderResult = QueryBuilder.GetSelectSql(configuration, predicate, configName);

            using (concurrentDetector.EnterCriticalSection())
            {
                var entityInfo = configuration.GetEntityTypeInfo(typeof(T));
                var (command, parameterObject, _) = CreateRelationalParameterObject(
                    facadeDependencies, facade, null, builderResult, configuration.LoggerAction);
                return InternalExecuteReader<T>(command, parameterObject, entityInfo);
            }

        }

        public static async Task<List<T>> ExecuteReaderAsync<T>(
            this DatabaseFacade database,
            IQueryBuilderConfiguration configuration,
            Expression<Func<T, bool>> predicate,
            DbTransaction transaction = null,
            string configName = null,
            CancellationToken cancellationToken = default)
            where T : class
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return new List<T>();
            }

            database.SetCommandTimeout(configuration.CommandTimeout);

            var concurrentDetector = facadeDependencies.ConcurrencyDetector;

            if (transaction != null)
            {
                await database.UseTransactionAsync(transaction, cancellationToken).ConfigureAwait(false);
            }

            var builderResult = QueryBuilder.GetSelectSql(configuration, predicate, configName);


            using (concurrentDetector.EnterCriticalSection())
            {
                var entityInfo = configuration.GetEntityTypeInfo(typeof(T));
                var (command, parameterObject, _) = CreateRelationalParameterObject(
                    facadeDependencies, facade, null, builderResult, configuration.LoggerAction);
                return await InternalExecuteReaderAsync<T>(command, parameterObject, entityInfo, cancellationToken);
            }
        }
        #endregion


        #region InternalExecuteReader

        private static T InternalExecuteReaderFirst<T>(IRelationalCommand command,
            RelationalCommandParameterObject parameterObject,
            EntityTypeInfo entityInfo)
            where T : class
        {
            var relationalDataReader = command.ExecuteReader(parameterObject);
            var reader = relationalDataReader.DbDataReader;
            if (!reader.HasRows)
            {
                reader.Close();
                reader.Dispose();
                return default;
            }

            reader.Read();
            var instance = Activator.CreateInstance<T>();
            DataReaderHelper.ReadRow(instance, reader, entityInfo);
            reader.Close();
            reader.Dispose();
            return instance;
        }

        private static async Task<T> InternalExecuteReaderFirstAsync<T>(IRelationalCommand command,
            RelationalCommandParameterObject parameterObject,
            EntityTypeInfo entityInfo,
            CancellationToken cancellationToken = default)
            where T : class
        {
            var relationalDataReader = await command.ExecuteReaderAsync(parameterObject, cancellationToken).ConfigureAwait(false);
            var reader = relationalDataReader.DbDataReader;
            if (!reader.HasRows)
            {
                await reader.CloseAsync().ConfigureAwait(false);
                await reader.DisposeAsync().ConfigureAwait(false);
                return default;
            }

            await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            var instance = Activator.CreateInstance<T>();
            await DataReaderHelper.ReadRowAsync(instance, reader, entityInfo, null, cancellationToken);
            await reader.CloseAsync().ConfigureAwait(false);
            await reader.DisposeAsync().ConfigureAwait(false);
            return instance;
        }

        private static List<T> InternalExecuteReader<T>(IRelationalCommand command,
            RelationalCommandParameterObject parameterObject,
            EntityTypeInfo entityInfo)
            where T : class
        {
            var results = new List<T>();
            var relationalDataReader = command.ExecuteReader(parameterObject);
            var reader = relationalDataReader.DbDataReader;
            if (!reader.HasRows)
            {
                reader.Close();
                reader.Dispose();
                return results;
            }

            while (reader.Read())
            {
                var instance = Activator.CreateInstance<T>();
                DataReaderHelper.ReadRow(instance, reader, entityInfo);
                results.Add(instance);
            }

            reader.Close();
            reader.Dispose();
            return results;

        }

        private static async Task<List<T>> InternalExecuteReaderAsync<T>(IRelationalCommand command,
            RelationalCommandParameterObject parameterObject,
            EntityTypeInfo entityInfo,
            CancellationToken cancellationToken = default)
            where T : class
        {
            var results = new List<T>();
            var relationalDataReader = await command.ExecuteReaderAsync(parameterObject, cancellationToken).ConfigureAwait(false);
            var reader = relationalDataReader.DbDataReader;
            if (!reader.HasRows)
            {
                await reader.CloseAsync().ConfigureAwait(false);
                await reader.DisposeAsync().ConfigureAwait(false);
                return results;
            }

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var instance = Activator.CreateInstance<T>();
                await DataReaderHelper.ReadRowAsync(instance, reader, entityInfo, null, cancellationToken);
                results.Add(instance);
            }

            await reader.CloseAsync().ConfigureAwait(false);
            await reader.DisposeAsync().ConfigureAwait(false);
            return results;
        }
        #endregion


        #region CreateRelationalParameterObject

        private static (IRelationalCommand command, RelationalCommandParameterObject parameterObject)
            CreateRelationalParameterObject(
                IRelationalDatabaseFacadeDependencies facadeDependencies,
                IDatabaseFacadeDependenciesAccessor facade,
                SqlParserResult parserResult,
                Action<string> loggerAction = null)
        {
            IRelationalCommand command;
            IReadOnlyDictionary<string, object> parameterValues = null;

            // null check
            if (parserResult == null)
            {
                throw new InvalidOperationException("");
            }

            if (parserResult.DbDataParameters.Count == 0)
            {
                command = facadeDependencies.RawSqlCommandBuilder.Build(parserResult.ParsedSql);
            }
            else
            {
                var rawSqlCommand = facadeDependencies
                    .RawSqlCommandBuilder
                    .Build(parserResult.ParsedSql, parserResult.DbDataParameters.Cast<object>().ToArray());
                command = rawSqlCommand.RelationalCommand;
                parameterValues = rawSqlCommand.ParameterValues;
            }

            var debugSql = parserResult.DebugSql;
            loggerAction?.Invoke(debugSql);

            return (command, new RelationalCommandParameterObject(
                facadeDependencies.RelationalConnection,
                parameterValues,
                null,
                facade.Context,
                facadeDependencies.CommandLogger));
        }

        private static (IRelationalCommand command, RelationalCommandParameterObject parameterObject, string debugSql)
            CreateRelationalParameterObject(
                IRelationalDatabaseFacadeDependencies facadeDependencies,
                IDatabaseFacadeDependenciesAccessor facade,
                QueryBuilderParameter builderParameter,
                QueryBuilderResult builderResult = null,
                Action<string> loggerAction = null)
        {
            IRelationalCommand command;
            IReadOnlyDictionary<string, object> parameterValues = null;


            var localBuilderResult = builderParameter == null
                ? builderResult
                : QueryBuilder.GetQueryBuilderResult(builderParameter);
            // null check
            if (localBuilderResult == null)
            {
                throw new InvalidOperationException("");
            }

            if (localBuilderResult.DbDataParameters.Count == 0)
            {
                command = facadeDependencies.RawSqlCommandBuilder.Build(localBuilderResult.ParsedSql);
            }
            else
            {
                var rawSqlCommand = facadeDependencies
                    .RawSqlCommandBuilder
                    .Build(localBuilderResult.ParsedSql, localBuilderResult.DbDataParameters.Cast<object>().ToArray());
                command = rawSqlCommand.RelationalCommand;
                parameterValues = rawSqlCommand.ParameterValues;
            }

            var debugSql = localBuilderResult.DebugSql;
            if (builderParameter != null)
            {
                builderParameter.WriteLog(debugSql);
            }
            else
            {
                loggerAction?.Invoke(debugSql);
            }

            return (command, new RelationalCommandParameterObject(
                facadeDependencies.RelationalConnection,
                parameterValues,
                null,
                facade.Context,
                facadeDependencies.CommandLogger), debugSql);

        }
        #endregion


        #region ThrowIfOptimisticLockException

        private static void ThrowIfOptimisticLockException(
            QueryBuilderParameter parameter,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            int affectedCount,
            string parsedSql,
            string debugSql,
            IDbContextTransaction dbContextTransaction)
        {
            if (parameter.ThrowableOptimisticLockException(affectedCount))
            {
                dbContextTransaction.Rollback();
                //throw new DbUpdateConcurrencyException(RelationalStrings.UpdateConcurrencyException(1, 0));
                throw new OptimisticLockException(parsedSql, debugSql, parameter.SqlFile);
            }
        }

        private static async Task ThrowIfOptimisticLockExceptionAsync(
            QueryBuilderParameter parameter,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            int affectedCount,
            string parsedSql,
            string debugSql,
            IDbContextTransaction dbContextTransaction,
            CancellationToken cancellationToken = default)
        {
            if (parameter.ThrowableOptimisticLockException(affectedCount))
            {
                await dbContextTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw new OptimisticLockException(parsedSql, debugSql, parameter.SqlFile);
            }
        }
        #endregion


    }


}
