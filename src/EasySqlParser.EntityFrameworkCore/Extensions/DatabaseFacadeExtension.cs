using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Helpers;
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
            return DbCommandHelper.ConsumeReader(reader, builderParameter);
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
            QueryBuilderParameter builderParameter)
        {
            await using var relationalDataReader = await command.ExecuteReaderAsync(parameterObject).ConfigureAwait(false);
            var reader = relationalDataReader.DbDataReader;
            return await DbCommandHelper.ConsumeReaderAsync(reader, builderParameter).ConfigureAwait(false);
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

            var dbContextTransaction = database.CurrentTransaction;
            var beganTransaction = false;
            if (dbContextTransaction == null)
            {
                dbContextTransaction = database.BeginTransaction();
                beganTransaction = true;
            }

            using (concurrentDetector.EnterCriticalSection())
            {
                try
                {

                    var (command, parameterObject, debugSql) = CreateRelationalParameterObject(
                        facadeDependencies, facade, parserResult);
                    var rawScalar = command.ExecuteScalar(parameterObject);
                    if (rawScalar is TResult scalar)
                    {
                        return scalar;
                    }

                    return (TResult) Convert.ChangeType(rawScalar, typeof(TResult));
                }
                finally
                {
                    if (beganTransaction)
                    {
                        dbContextTransaction?.Dispose();
                    }
                }

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

            var dbContextTransaction = database.CurrentTransaction;
            var beganTransaction = false;
            if (dbContextTransaction == null)
            {
                dbContextTransaction = await database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                beganTransaction = true;
            }

            using (concurrentDetector.EnterCriticalSection())
            {
                try
                {
                    var (command, parameterObject, debugSql) = CreateRelationalParameterObject(
                        facadeDependencies, facade, parserResult);
                    var rawScalar = await command.ExecuteScalarAsync(parameterObject, cancellationToken).ConfigureAwait(false);
                    if (rawScalar is TResult scalar)
                    {
                        return scalar;
                    }

                    return (TResult)Convert.ChangeType(rawScalar, typeof(TResult));
                }
                finally
                {
                    if (beganTransaction && dbContextTransaction != null)
                    {
                        await dbContextTransaction.DisposeAsync();
                    }
                }

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

            var dbContextTransaction = database.CurrentTransaction;
            var beganTransaction = false;
            if (dbContextTransaction == null)
            {
                dbContextTransaction = database.BeginTransaction();
                beganTransaction = true;
            }

            using (concurrentDetector.EnterCriticalSection())
            {
                try
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
                finally
                {
                    if (beganTransaction)
                    {
                        dbContextTransaction?.Dispose();
                    }
                }
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

            var dbContextTransaction = database.CurrentTransaction;
            var beganTransaction = false;
            if (dbContextTransaction == null)
            {
                dbContextTransaction = await database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                beganTransaction = true;
            }

            using (concurrentDetector.EnterCriticalSection())
            {
                try
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
                finally
                {
                    if (beganTransaction && dbContextTransaction != null)
                    {
                        await dbContextTransaction.DisposeAsync();
                    }
                }

            }

        }

        #endregion

        private static int InternalExecuteNonQuery(
            DatabaseFacade database,
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
                var (command, parameterObject, debugSql) = CreateRelationalParameterObject(
                    facadeDependencies, facade, builderParameter);
                int affectedCount;
                try
                {
                    var connection = facadeDependencies.RelationalConnection.DbConnection;
                    SequenceHelper.Generate(connection, builderParameter);
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

        private static async Task<int> InternalExecuteNonQueryAsync(
            DatabaseFacade database,
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
                var (command, parameterObject, debugSql) = CreateRelationalParameterObject(
                    facadeDependencies, facade, builderParameter);
                int affectedCount;
                try
                {
                    var connection = facadeDependencies.RelationalConnection.DbConnection;
                    await SequenceHelper.GenerateAsync(connection, builderParameter).ConfigureAwait(false);
                    builderParameter.SaveExpectedVersion();
                    switch (builderParameter.CommandExecutionType)
                    {
                        case CommandExecutionType.ExecuteNonQuery:
                            affectedCount = await command.ConsumeNonQueryAsync(parameterObject, builderParameter)
                                .ConfigureAwait(false);
                            break;
                        case CommandExecutionType.ExecuteReader:
                            affectedCount = await command.ConsumeReaderAsync(parameterObject, builderParameter)
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

        public static int ExecuteNonQueryByQueryBuilder(
            this DatabaseFacade database,
            QueryBuilderParameter builderParameter,
            DbTransaction transaction = null)
        {
            return InternalExecuteNonQuery(database, builderParameter, transaction);
        }


        public static async Task<int> ExecuteNonQueryByQueryBuilderAsync(
            this DatabaseFacade database,
            QueryBuilderParameter builderParameter,
            DbTransaction transaction = null,
            CancellationToken cancellationToken = default)
        {
            return await InternalExecuteNonQueryAsync(database, builderParameter, transaction, cancellationToken)
                .ConfigureAwait(false);
        }


        //public static int Update<T>(
        //    this DatabaseFacade database,
        //    IQueryBuilderConfiguration configuration,
        //    T entity,
        //    Expression<Func<T, bool>> predicate,
        //    DbTransaction transaction = null,
        //    bool excludeNull = false,
        //    bool ignoreVersion = false,
        //    string configName = null)
        //    where T : class
        //{
        //    var builderParameter = new QueryBuilderParameter(entity, SqlKind.Update, configuration,
        //        excludeNull: excludeNull, ignoreVersion: ignoreVersion, configName: configName);

        //    return InternalExecuteNonQuery(database, builderParameter, predicate, transaction);

        //    //var facade = database as IDatabaseFacadeDependenciesAccessor;
        //    //var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
        //    //if (facadeDependencies == null)
        //    //{
        //    //    return default;
        //    //}

        //    //database.SetCommandTimeout(configuration.CommandTimeout);

        //    //var concurrentDetector = facadeDependencies.ConcurrencyDetector;

        //    //if (transaction != null)
        //    //{
        //    //    database.UseTransaction(transaction);
        //    //}

        //    //var builderResult = QueryBuilder.GetUpdateSql(configuration, entity, predicate, out var builderParameter,
        //    //    excludeNull, ignoreVersion, configName);

        //    //using (concurrentDetector.EnterCriticalSection())
        //    //{
        //    //    var (command, parameterObject, debugSql) = CreateRelationalParameterObject(
        //    //        facadeDependencies, facade, null, builderResult, configuration.LoggerAction);

        //    //}
        //}

        //public static async Task<int> UpdateAsync<T>(
        //    this DatabaseFacade database,
        //    IQueryBuilderConfiguration configuration,
        //    T entity,
        //    Expression<Func<T, bool>> predicate,
        //    DbTransaction transaction = null,
        //    bool excludeNull = false,
        //    bool ignoreVersion = false,
        //    string configName = null,
        //    CancellationToken cancellationToken = default)
        //    where T : class
        //{
        //    var builderParameter = new QueryBuilderParameter(entity, SqlKind.Update, configuration,
        //        excludeNull: excludeNull, ignoreVersion: ignoreVersion, configName: configName);
        //    return await InternalExecuteNonQueryAsync(database, builderParameter, predicate,
        //            transaction, cancellationToken)
        //        .ConfigureAwait(false);
        //}

        public static List<T> Select<T>(
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

            var dbContextTransaction = database.CurrentTransaction;
            var beganTransaction = false;
            if (dbContextTransaction == null)
            {
                dbContextTransaction = database.BeginTransaction();
                beganTransaction = true;
            }

            using (concurrentDetector.EnterCriticalSection())
            {
                var results = new List<T>();
                try
                {
                    var (command, parameterObject, debugSql) = CreateRelationalParameterObject(
                        facadeDependencies, facade, parserResult);
                    var relationalDataReader = command.ExecuteReader(parameterObject);
                    var entityInfo = configuration.GetEntityTypeInfo(typeof(T));
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
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            if (reader.IsDBNull(i)) continue;
                            var value = reader.GetValue(i);
                            var columnName = reader.GetName(i);
                            var columnInfo = entityInfo.ColumnNameKeyDictionary[columnName];
                            if (columnInfo.ConvertFromProvider != null)
                            {
                                var converted = columnInfo.ConvertFromProvider(value);
                                columnInfo.PropertyInfo.SetValue(instance, converted);
                            }
                            else
                            {
                                columnInfo.PropertyInfo.SetValue(instance, value);
                            }
                        }
                        //foreach (var columnInfo in entityInfo.Columns)
                        //{
                        //    var col = reader.GetOrdinal(columnInfo.ColumnName);
                        //    if (reader.IsDBNull(col)) continue;
                        //    var value = reader.GetValue(col);
                        //    if (columnInfo.ConvertFromProvider != null)
                        //    {
                        //        var converted = columnInfo.ConvertFromProvider(value);
                        //        columnInfo.PropertyInfo.SetValue(instance, converted);
                        //    }
                        //    else
                        //    {
                        //        columnInfo.PropertyInfo.SetValue(instance, value);
                        //    }
                        //}

                        results.Add(instance);
                    }

                    reader.Close();
                    reader.Dispose();
                }
                finally
                {
                    if (beganTransaction)
                    {
                        dbContextTransaction?.Dispose();
                    }
                }
                return results;
            }
        }

        public static async Task<List<T>> SelectAsync<T>(
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
            var dbContextTransaction = database.CurrentTransaction;
            var beganTransaction = false;
            if (dbContextTransaction == null)
            {
                dbContextTransaction = await database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                beganTransaction = true;
            }


            using (concurrentDetector.EnterCriticalSection())
            {
                var results = new List<T>();
                try
                {
                    var (command, parameterObject, debugSql) = CreateRelationalParameterObject(
                        facadeDependencies, facade, parserResult);
                    var relationalDataReader = await command.ExecuteReaderAsync(parameterObject, cancellationToken)
                        .ConfigureAwait(false);
                    var entityInfo = configuration.GetEntityTypeInfo(typeof(T));
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
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            if (await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(false)) continue;
                            var value = reader.GetValue(i);
                            var columnName = reader.GetName(i);
                            var columnInfo = entityInfo.ColumnNameKeyDictionary[columnName];
                            if (columnInfo.ConvertFromProvider != null)
                            {
                                var converted = columnInfo.ConvertFromProvider(value);
                                columnInfo.PropertyInfo.SetValue(instance, converted);
                            }
                            else
                            {
                                columnInfo.PropertyInfo.SetValue(instance, value);
                            }
                        }

                        //foreach (var columnInfo in entityInfo.Columns)
                        //{
                        //    var col = reader.GetOrdinal(columnInfo.ColumnName);
                        //    if (await reader.IsDBNullAsync(col, cancellationToken).ConfigureAwait(false)) continue;
                        //    var value = reader.GetValue(col);
                        //    if (columnInfo.ConvertFromProvider != null)
                        //    {
                        //        var converted = columnInfo.ConvertFromProvider(value);
                        //        columnInfo.PropertyInfo.SetValue(instance, converted);
                        //    }
                        //    else
                        //    {
                        //        columnInfo.PropertyInfo.SetValue(instance, value);
                        //    }

                        //}

                        results.Add(instance);
                    }

                    await reader.CloseAsync().ConfigureAwait(false);
                    await reader.DisposeAsync().ConfigureAwait(false);
                }
                finally
                {
                    if (beganTransaction && dbContextTransaction != null)
                    {
                        await dbContextTransaction.DisposeAsync().ConfigureAwait(false);
                    }
                }

                return results;
            }
        }

        public static List<T> Select<T>(
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

            var dbContextTransaction = database.CurrentTransaction;
            var beganTransaction = false;
            if (dbContextTransaction == null)
            {
                dbContextTransaction = database.BeginTransaction();
                beganTransaction = true;
            }

            var builderResult = QueryBuilder.GetSelectSql(configuration, predicate, configName);

            using (concurrentDetector.EnterCriticalSection())
            {
                var results = new List<T>();
                try
                {
                    var (command, parameterObject, debugSql) = CreateRelationalParameterObject(
                        facadeDependencies, facade, null, builderResult, configuration.LoggerAction);
                    var relationalDataReader = command.ExecuteReader(parameterObject);
                    var entityInfo = configuration.GetEntityTypeInfo(typeof(T));
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
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            if (reader.IsDBNull(i)) continue;
                            var value = reader.GetValue(i);
                            var columnName = reader.GetName(i);
                            var columnInfo = entityInfo.ColumnNameKeyDictionary[columnName];
                            if (columnInfo.ConvertFromProvider != null)
                            {
                                var converted = columnInfo.ConvertFromProvider(value);
                                columnInfo.PropertyInfo.SetValue(instance, converted);
                            }
                            else
                            {
                                columnInfo.PropertyInfo.SetValue(instance, value);
                            }
                        }
                        //foreach (var columnInfo in entityInfo.Columns)
                        //{
                        //    var col = reader.GetOrdinal(columnInfo.ColumnName);
                        //    if (reader.IsDBNull(col)) continue;
                        //    var value = reader.GetValue(col);
                        //    if (columnInfo.ConvertFromProvider != null)
                        //    {
                        //        var converted = columnInfo.ConvertFromProvider(value);
                        //        columnInfo.PropertyInfo.SetValue(instance, converted);
                        //    }
                        //    else
                        //    {
                        //        columnInfo.PropertyInfo.SetValue(instance, value);
                        //    }
                        //}

                        results.Add(instance);
                    }

                    reader.Close();
                    reader.Dispose();
                }
                finally
                {
                    if (beganTransaction)
                    {
                        dbContextTransaction?.Dispose();
                    }
                }
                return results;
            }

        }

        public static async Task<List<T>> SelectAsync<T>(
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
            var dbContextTransaction = database.CurrentTransaction;
            var beganTransaction = false;
            if (dbContextTransaction == null)
            {
                dbContextTransaction = await database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                beganTransaction = true;
            }


            var builderResult = QueryBuilder.GetSelectSql(configuration, predicate, configName);


            using (concurrentDetector.EnterCriticalSection())
            {
                var results = new List<T>();
                try
                {
                    var (command, parameterObject, debugSql) = CreateRelationalParameterObject(
                        facadeDependencies, facade, null, builderResult, configuration.LoggerAction);
                    var relationalDataReader = await command.ExecuteReaderAsync(parameterObject, cancellationToken)
                        .ConfigureAwait(false);
                    var entityInfo = configuration.GetEntityTypeInfo(typeof(T));
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
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            if (await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(false)) continue;
                            var value = reader.GetValue(i);
                            var columnName = reader.GetName(i);
                            var columnInfo = entityInfo.ColumnNameKeyDictionary[columnName];
                            if (columnInfo.ConvertFromProvider != null)
                            {
                                var converted = columnInfo.ConvertFromProvider(value);
                                columnInfo.PropertyInfo.SetValue(instance, converted);
                            }
                            else
                            {
                                columnInfo.PropertyInfo.SetValue(instance, value);
                            }
                        }
                        //foreach (var columnInfo in entityInfo.Columns)
                        //{
                        //    var col = reader.GetOrdinal(columnInfo.ColumnName);
                        //    if (await reader.IsDBNullAsync(col, cancellationToken).ConfigureAwait(false)) continue;
                        //    var value = reader.GetValue(col);
                        //    if (columnInfo.ConvertFromProvider != null)
                        //    {
                        //        var converted = columnInfo.ConvertFromProvider(value);
                        //        columnInfo.PropertyInfo.SetValue(instance, converted);
                        //    }
                        //    else
                        //    {
                        //        columnInfo.PropertyInfo.SetValue(instance, value);
                        //    }

                        //}

                        results.Add(instance);
                    }

                    await reader.CloseAsync().ConfigureAwait(false);
                    await reader.DisposeAsync().ConfigureAwait(false);
                }
                finally
                {
                    if (beganTransaction && dbContextTransaction != null)
                    {
                        await dbContextTransaction.DisposeAsync().ConfigureAwait(false);
                    }
                }

                return results;
            }
        }


        private static (IRelationalCommand command, RelationalCommandParameterObject parameterObject, string debugSql)
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
                facadeDependencies.CommandLogger), debugSql);
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


    }

    
}
