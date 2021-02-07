using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EasySqlParser.Extensions;
using EasySqlParser.SqlGenerator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace EasySqlParser.EntityFrameworkCore.Extensions
{
    // RelationalDatabaseFacadeExtensions

    // TODO: DOC
    /// <summary>
    /// Extension for <see cref="DatabaseFacade"/>
    /// </summary>
    public static class DatabaseFacadeExtension
    {
        // https://devadjust.exblog.jp/28241806/

        #region ExecuteScalarSqlRaw


        public static TResult ExecuteScalarBySqlRaw<TResult>(
            this DatabaseFacade database,
            string sql,
            IEnumerable<object> parameters = null,
            DbTransaction transaction = null,
            Action<string> loggerAction = null)
        {
            return InternalExecuteScalar<TResult, TResult>(
                database, 
                null, 
                sql, 
                parameters, 
                transaction,
                loggerAction);
        }

        public static TResult ExecuteScalarByQueryBuilder<TResult, T>(
            this DatabaseFacade database,
            QueryBuilderParameter<T> builderParameter,
            DbTransaction transaction = null,
            Action<string> loggerAction = null)
        {
            return InternalExecuteScalar<TResult, T>(
                database, 
                builderParameter, 
                null, 
                null, 
                transaction,
                loggerAction);
        }

        private static TResult InternalExecuteScalar<TResult, T>(
            DatabaseFacade database,
            QueryBuilderParameter<T> builderParameter = null,
            string sql = null,
            IEnumerable<object> parameters = null,
            DbTransaction transaction = null,
            Action<string> loggerAction = null)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }

            if (builderParameter != null)
            {
                database.SetCommandTimeout(builderParameter.CommandTimeout);

            }

            var concurrentDetector = facadeDependencies.ConcurrencyDetector;

            IDbContextTransaction dbContextTransaction = null;
            if (transaction == null)
            {
                dbContextTransaction = database.BeginTransaction();
            }

            if (transaction != null)
            {
                database.UseTransaction(transaction);
            }

            using (concurrentDetector.EnterCriticalSection())
            {
                var (command, parameterObject, debugSql) = CreateRelationalParameterObject(
                    database, facadeDependencies, facade,
                    builderParameter, sql, parameters, loggerAction);
                var rawScalar = command
                    .ExecuteScalar(parameterObject);
                if (rawScalar is TResult scalar)
                {
                    dbContextTransaction?.Commit();
                    return scalar;
                }

                // TODO: type error
                throw new InvalidOperationException("");
            }


        }


        public static async Task<TResult> ExecuteScalarBySqlRawAsync<TResult>(
            this DatabaseFacade database,
            string sql,
            IEnumerable<object> parameters = null,
            DbTransaction transaction = null,
            Action<string> loggerAction = null,
            CancellationToken cancellationToken = default)
        {
            return await InternalExecuteScalarAsync<TResult, TResult>(database, null, sql, parameters, transaction,
                loggerAction,
                cancellationToken);
        }

        public static async Task<TResult> ExecuteScalarByQueryBuilderAsync<TResult, T>(
            this DatabaseFacade database,
            QueryBuilderParameter<T> builderParameter,
            DbTransaction transaction = null,
            Action<string> loggerAction = null,
            CancellationToken cancellationToken = default)
        {
            return await InternalExecuteScalarAsync<TResult, T>(database, builderParameter, null, null, transaction,
                loggerAction,
                cancellationToken);
        }

        private static async Task<TResult> InternalExecuteScalarAsync<TResult, T>(
            DatabaseFacade database,
            QueryBuilderParameter<T> builderParameter = null,
            string sql = null,
            IEnumerable<object> parameters = null,
            DbTransaction transaction = null,
            Action<string> loggerAction = null,
            CancellationToken cancellationToken = default)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }

            if (builderParameter != null)
            {
                database.SetCommandTimeout(builderParameter.CommandTimeout);
            }
            var concurrentDetector = facadeDependencies.ConcurrencyDetector;

            IDbContextTransaction dbContextTransaction = null;
            if (transaction == null)
            {
                dbContextTransaction = await database.BeginTransactionAsync(cancellationToken);
            }

            if (transaction != null)
            {
                await database.UseTransactionAsync(transaction, cancellationToken);
            }

            using (concurrentDetector.EnterCriticalSection())
            {
                var (command, parameterObject, debugSql) = CreateRelationalParameterObject(database, facadeDependencies, facade,
                    builderParameter, sql, parameters, loggerAction);
                var rawScalar = await command
                    .ExecuteScalarAsync(parameterObject,
                        cancellationToken);
                if (rawScalar is TResult scalar)
                {
                    if (dbContextTransaction != null)
                    {
                        await dbContextTransaction.CommitAsync(cancellationToken);
                    }
                    return scalar;
                }
                // TODO: type error
                throw new InvalidOperationException("");

            }

        }
        #endregion

        public static QueryBuilderResult GetQueryBuilderResult<T>(this DatabaseFacade database,
            QueryBuilderParameter<T> parameter)
        {
            if (!string.IsNullOrEmpty(parameter.SqlFile))
            {
                return QueryBuilder<T>.GetQueryBuilderResultFromSqlFile(parameter);
            }
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            switch (parameter.SqlKind)
            {
                case SqlKind.Insert:
                    return GetInsertSql(facade, parameter);
                case SqlKind.Update:
                    return GetUpdateSql(facade, parameter);
                case SqlKind.Delete:
                    return GetDeleteSql(facade, parameter);
                default:
                    // TODO: error
                    throw new InvalidOperationException("");
            }
        }

        private static (IRelationalCommand command, RelationalCommandParameterObject parameterObject, string debugSql)
            CreateRelationalParameterObject<T>(
                DatabaseFacade database,
                IRelationalDatabaseFacadeDependencies facadeDependencies,
                IDatabaseFacadeDependenciesAccessor facade,
                QueryBuilderParameter<T> builderParameter = null,
                string sql = null,
                IEnumerable<object> parameters = null,
                Action<string> loggerAction = null)
        {
            IRelationalCommand command = null;
            IReadOnlyDictionary<string, object> parameterValues = null;
            string debugSql = null;
            // null check
            if (builderParameter == null && sql == null)
            {
                //TODO:
                throw new InvalidOperationException("");
            }

            if (sql != null)
            {
                if (parameters == null)
                {
                    command = facadeDependencies.RawSqlCommandBuilder.Build(sql);
                }
                else
                {
                    var rawSqlCommand = facadeDependencies
                        .RawSqlCommandBuilder
                        .Build(sql, parameters);
                    command = rawSqlCommand.RelationalCommand;
                    parameterValues = rawSqlCommand.ParameterValues;
                }

                loggerAction?.Invoke(sql);
                facadeDependencies.CommandLogger.Logger.LogDebug(sql);
                if (parameterValues != null)
                {
                    foreach (var parameterValue in parameterValues)
                    {
                        loggerAction?.Invoke($"{parameterValue.Key}:{parameterValue.Value}");
                        facadeDependencies.CommandLogger.Logger.LogDebug(
                            $"{parameterValue.Key}:{parameterValue.Value}");
                    }
                }
            }

            if (builderParameter != null)
            {
                var builderResult = GetQueryBuilderResult(database, builderParameter);
                if (builderResult.DbDataParameters.Count == 0)
                {
                    command = facadeDependencies.RawSqlCommandBuilder.Build(builderResult.ParsedSql);
                }
                else
                {
                    var rawSqlCommand = facadeDependencies
                        .RawSqlCommandBuilder
                        .Build(builderResult.ParsedSql, builderResult.DbDataParameters.Cast<object>().ToArray());
                    command = rawSqlCommand.RelationalCommand;
                    parameterValues = rawSqlCommand.ParameterValues;
                }

                debugSql = builderResult.DebugSql;
                loggerAction?.Invoke(builderResult.DebugSql);
                facadeDependencies.CommandLogger.Logger.LogDebug(builderResult.DebugSql);
            }

            return (command, new RelationalCommandParameterObject(
                facadeDependencies.RelationalConnection,
                parameterValues,
                null,
                facade.Context,
                facadeDependencies.CommandLogger), debugSql);

        }

        private static void ThrowIfOptimisticLockException<T>(
            QueryBuilderParameter<T> parameter,
            int affectedCount,
            string parsedSql,
            string debugSql,
            DbTransaction transaction = null,
            IDbContextTransaction dbContextTransaction = null)
        {
            if ((parameter.SqlKind == SqlKind.Update || parameter.SqlKind == SqlKind.Delete) &&
                parameter.UseVersion && !parameter.SuppressOptimisticLockException
                && affectedCount == 0)
            {
                transaction?.Rollback();
                dbContextTransaction?.Rollback();
                //throw new DbUpdateConcurrencyException(RelationalStrings.UpdateConcurrencyException(1, 0));
                throw new OptimisticLockException(parsedSql, debugSql, parameter.SqlFile);
            }
        }



        #region ExecuteNonQuerySqlRaw


        public static int ExecuteNonQueryBySqlRaw(
            this DatabaseFacade database,
            string sql,
            IEnumerable<object> parameters = null,
            DbTransaction transaction = null,
            Action<string> loggerAction = null)
        {
            return InternalExecuteNonQuery<object>(database, null, sql, parameters, transaction, loggerAction);
        }

        public static int ExecuteNonQueryByQueryBuilder<T>(this DatabaseFacade database,
            QueryBuilderParameter<T> builderParameter,
            DbTransaction transaction = null,
            Action<string> loggerAction = null)
        {
            return InternalExecuteNonQuery(database, builderParameter, null, null, transaction, loggerAction);
        }

        private static int InternalExecuteNonQuery<T>(
            DatabaseFacade database,
            QueryBuilderParameter<T> builderParameter = null,
            string sql = null,
            IEnumerable<object> parameters = null,
            DbTransaction transaction = null,
            Action<string> loggerAction = null)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }

            if (builderParameter != null)
            {
                database.SetCommandTimeout(builderParameter.CommandTimeout);
            }

            var concurrentDetector = facadeDependencies.ConcurrencyDetector;

            IDbContextTransaction dbContextTransaction = null;
            if (transaction == null)
            {
                dbContextTransaction = database.BeginTransaction();
            }
            
            if (transaction != null)
            {
                database.UseTransaction(transaction);
            }

            using (concurrentDetector.EnterCriticalSection())
            {
                var (command, parameterObject, debugSql) = CreateRelationalParameterObject(database, facadeDependencies, facade, builderParameter, sql,
                    parameters, loggerAction);
                var affectedCount = command
                    .ExecuteNonQuery(parameterObject);
                if (builderParameter != null)
                {
                    ThrowIfOptimisticLockException(builderParameter, affectedCount, command.CommandText, debugSql,
                        transaction, dbContextTransaction);
                }

                dbContextTransaction?.Commit();

                if (builderParameter != null && builderParameter.SqlKind == SqlKind.Update)
                {
                    builderParameter.IncrementVersion();
                }
                return affectedCount;

            }
        }

        public static async Task<int> ExecuteNonQuerySqlRawAsync(
            this DatabaseFacade database,
            string sql = null,
            IEnumerable<object> parameters = null,
            DbTransaction transaction = null,
            Action<string> loggerAction = null,
            CancellationToken cancellationToken = default)
        {
            return await InternalExecuteNonQueryAsync<object>(database, null, sql, parameters, transaction,
                loggerAction,
                cancellationToken);
        }

        public static async Task<int> ExecuteNonQueryByQueryBuilderAsync<T>(
            this DatabaseFacade database,
            QueryBuilderParameter<T> builderParameter,
            DbTransaction transaction = null,
            Action<string> loggerAction = null,
            CancellationToken cancellationToken = default)
        {
            return await InternalExecuteNonQueryAsync(database, builderParameter, null, null, transaction, loggerAction,
                cancellationToken);

        }

        private static async Task<int> InternalExecuteNonQueryAsync<T>(
            DatabaseFacade database,
            QueryBuilderParameter<T> builderParameter = null,
            string sql = null,
            IEnumerable<object> parameters = null,
            DbTransaction transaction = null,
            Action<string> loggerAction = null,
            CancellationToken cancellationToken = default)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }

            if (builderParameter != null)
            {
                database.SetCommandTimeout(builderParameter.CommandTimeout);
            }

            var concurrentDetector = facadeDependencies.ConcurrencyDetector;

            IDbContextTransaction dbContextTransaction = null;
            if (transaction == null)
            {
                dbContextTransaction = await database.BeginTransactionAsync(cancellationToken);
            }

            if (transaction != null)
            {
                await database.UseTransactionAsync(transaction, cancellationToken);
            }

            using (concurrentDetector.EnterCriticalSection())
            {
                var (command, parameterObject, debugSql) = CreateRelationalParameterObject(database, facadeDependencies, facade, builderParameter, sql,
                    parameters, loggerAction);
                var affectedCount = await command
                    .ExecuteNonQueryAsync(parameterObject, cancellationToken);
                if (builderParameter != null)
                {
                    ThrowIfOptimisticLockException(builderParameter, affectedCount, command.CommandText, debugSql,
                        transaction, dbContextTransaction);
                }

                if (dbContextTransaction != null)
                {
                    await dbContextTransaction.CommitAsync(cancellationToken);
                }

                if (builderParameter != null && builderParameter.SqlKind == SqlKind.Update)
                {
                    builderParameter.IncrementVersion();
                }
                return affectedCount;
            }

        }

        #endregion

        #region ExecuteReaderSqlRaw

        /// <summary>
        /// データ取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static IEnumerable<T> ExecuteReaderSqlRaw<T>(
            this DatabaseFacade database,
            string sql,
            IEnumerable<object> parameters = null,
            DbTransaction transaction = null)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                yield break;
            }

            var concurrentDetector = facadeDependencies.ConcurrencyDetector;
            if (transaction != null)
            {
                database.UseTransaction(transaction);
            }

            var maps = Cache<T>.ColumnMaps;

            using (concurrentDetector.EnterCriticalSection())
            {
                var (command, parameterObject, debugSql) = CreateRelationalParameterObject<object>(database, facadeDependencies,
                    facade, null, sql,
                    parameters);
                using var relationalDataReader = command
                    .ExecuteReader(parameterObject);
                var reader = relationalDataReader.DbDataReader;
                if (!reader.HasRows) yield break;
                while (reader.Read())
                {
                    var instance = Activator.CreateInstance<T>();
                    foreach (var map in maps)
                    {
                        var col = reader.GetOrdinal(map.Value.columnName);
                        if (!reader.IsDBNull(col))
                        {
                            map.Value.property.SetValue(instance, reader.GetValue(col));
                        }
                    }

                    yield return instance;
                }
            }
        }

        public static async IAsyncEnumerable<T> ExecuteReaderSqlRawAsync<T>(
            this DatabaseFacade database, 
            string sql,
            IEnumerable<object> parameters = null,
            DbTransaction transaction = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                yield break;
            }
            var concurrentDetector = facadeDependencies.ConcurrencyDetector;
            if (transaction != null)
            {
                await database.UseTransactionAsync(transaction, cancellationToken);
            }
            var maps = Cache<T>.ColumnMaps;

            using (concurrentDetector.EnterCriticalSection())
            {
                var (command, parameterObject, debugSql) = CreateRelationalParameterObject<object>(database, facadeDependencies, facade, null, sql,
                    parameters);
                using (var relationalDataReader =
                    await command.ExecuteReaderAsync(parameterObject, cancellationToken))
                {
                    var reader = relationalDataReader.DbDataReader;
                    if (!reader.HasRows) yield break;
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var instance = Activator.CreateInstance<T>();
                        foreach (var map in maps)
                        {
                            var col = reader.GetOrdinal(map.Value.columnName);
                            if (!await reader.IsDBNullAsync(col, cancellationToken))
                            {
                                map.Value.property.SetValue(instance, reader.GetValue(col));
                            }
                        }

                        yield return instance;

                    }

                }
            }
        }
        #endregion

        #region ExecuteReaderSingleSqlRaw

        /// <summary>
        /// 1件データを取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T ExecuteReaderSingleSqlRaw<T>(
            this DatabaseFacade database,
            string sql,
            IEnumerable<object> parameters = null,
            DbTransaction transaction = null)
        {
            return ExecuteReaderSqlRaw<T>(database, sql, parameters, transaction).FirstOrDefault();
        }

        /// <summary>
        /// 1件データを取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<T> ExecuteReaderSingleSqlRawAsync<T>(
            this DatabaseFacade database, 
            string sql,
            IEnumerable<object> parameters=null,
            DbTransaction transaction = null,
            CancellationToken cancellationToken = default)
        {
            //var results = ExecuteReaderSqlRawAsync<T>(database, sql, parameters, transaction, cancellationToken);
            //await foreach (var result in results.WithCancellation(cancellationToken))
            //{
            //    return result;
            //}

            //return default;

            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }
            var concurrentDetector = facadeDependencies.ConcurrencyDetector;
            if (transaction != null)
            {
                await database.UseTransactionAsync(transaction, cancellationToken);
            }
            var maps = Cache<T>.ColumnMaps;
            using (concurrentDetector.EnterCriticalSection())
            {
                var (command, parameterObject, debugSql) = CreateRelationalParameterObject<object>(database, facadeDependencies, facade, null, sql,
                    parameters);
                using (var relationalDataReader = await command
                    .ExecuteReaderAsync(parameterObject, cancellationToken))
                {
                    var reader = relationalDataReader.DbDataReader;
                    if (!reader.HasRows) return default;
                    await reader.ReadAsync(cancellationToken);
                    var instance = Activator.CreateInstance<T>();
                    foreach (var map in maps)
                    {
                        var col = reader.GetOrdinal(map.Value.columnName);
                        if (!await reader.IsDBNullAsync(col, cancellationToken))
                        {
                            map.Value.property.SetValue(instance, reader.GetValue(col));
                        }
                    }

                    return instance;

                }
            }
        }
        #endregion

        #region GetInsertSql

        private static QueryBuilderResult GetInsertSql<T>(
            IDatabaseFacadeDependenciesAccessor facade,
            QueryBuilderParameter<T> parameter)
        {
            var config = parameter.Config;
            var entityType = facade.Context.Model.FindEntityType(typeof(T));
            var properties = entityType.GetProperties().ToList();
            var tableName = entityType.GetTableName();
            var schemaName = entityType.GetSchema();
            var tableId = StoreObjectIdentifier.Table(tableName, schemaName);
            var builder = new QueryStringBuilder(config, parameter.WriteIndented);
            builder.AppendSql("INSERT INTO ");
            if (!string.IsNullOrEmpty(schemaName))
            {
                builder.AppendSql(config.GetQuotedName(schemaName));
                builder.AppendSql(".");
            }
            builder.AppendSql(config.GetQuotedName(tableName));
            builder.AppendLine(" (");
            var counter = 0;
            foreach (var property in properties)
            {
                if (parameter.ExcludeNull)
                {
                    var propValue = property.PropertyInfo.GetValue(parameter.Entity);
                    if (propValue == null) continue;
                }

                if (property.ValueGenerated == ValueGenerated.OnAdd) continue;

                builder.AppendComma(counter);

                builder.AppendLine(config.GetQuotedName(property.GetColumnName(tableId)));

                counter++;
            }
            builder.AppendLine(") VALUES (");
            counter = 0;
            foreach (var property in properties)
            {
                var propValue = property.PropertyInfo.GetValue(parameter.Entity);
                if (parameter.ExcludeNull && propValue == null) continue;
                if (property.ValueGenerated == ValueGenerated.OnAdd) continue;
                builder.AppendComma(counter);
                if (property.PropertyInfo.GetCustomAttribute<VersionAttribute>() != null)
                {
                    propValue = builder.GetDefaultVersionNo(propValue, property.PropertyInfo.PropertyType);
                }
                builder.AppendParameter(config.GetParameterName(property.PropertyInfo.Name), propValue);
                builder.AppendLine();
                counter++;
            }
            builder.AppendLine(")");

            return builder.GetResult();
        }
        #endregion

        #region GetUpdateSql


        private static QueryBuilderResult GetUpdateSql<T>(
            IDatabaseFacadeDependenciesAccessor facade,
            QueryBuilderParameter<T> parameter)
        {
            var config = parameter.Config;
            var entityType = facade.Context.Model.FindEntityType(typeof(T));
            var properties = entityType.GetProperties().ToList();
            var tableName = entityType.GetTableName();
            var schemaName = entityType.GetSchema();
            var tableId = StoreObjectIdentifier.Table(tableName, schemaName);

            var hasPrimaryKey = properties.Any(x => x.IsPrimaryKey());
            if (!hasPrimaryKey)
            {
                // pkがなければupdateできない
                throw new InvalidOperationException("");
            }

            var builder = new QueryStringBuilder(config, parameter.WriteIndented);
            builder.AppendSql("UPDATE ");
            if (!string.IsNullOrEmpty(schemaName))
            {
                builder.AppendSql(config.GetQuotedName(schemaName));
                builder.AppendSql(".");
            }
            builder.AppendSql(config.GetQuotedName(tableName));
            builder.AppendLine(" SET ");
            var counter = 0;
            foreach (var property in properties)
            {
                if (property.IsPrimaryKey()) continue;
                var propValue = property.PropertyInfo.GetValue(parameter.Entity);
                if (parameter.ExcludeNull && propValue == null)
                {
                    continue;
                }

                builder.AppendComma(counter);
                var columnName = config.GetQuotedName(property.GetColumnName(tableId));
                builder.AppendSql(columnName);
                builder.AppendSql(" = ");
                builder.AppendParameter(config.GetParameterName(property.PropertyInfo.Name), propValue);
                builder.AppendVersion(parameter, property.PropertyInfo);

                builder.AppendLine();

                counter++;
            }

            builder.AppendLine("WHERE ");
            counter = 0;

            foreach (var property in properties)
            {
                var versionAttr = property.PropertyInfo.GetCustomAttribute<VersionAttribute>();
                if (parameter.IgnoreVersion && versionAttr != null) continue;
                if (!property.IsPrimaryKey() && versionAttr == null) continue;
                builder.AppendAnd(counter);
                var propValue = property.PropertyInfo.GetValue(parameter.Entity);
                var columnName = config.GetQuotedName(property.GetColumnName(tableId));
                builder.AppendSql(columnName);
                if (propValue == null)
                {
                    builder.AppendSql(" IS NULL ");
                }
                else
                {
                    builder.AppendSql(" = ");
                    builder.AppendParameter(config.GetParameterName(property.PropertyInfo.Name), propValue);
                }

                builder.AppendLine();

                counter++;
            }

            return builder.GetResult();
        }

        #endregion

        #region GetDeleteSql

        private static QueryBuilderResult GetDeleteSql<T>(
            IDatabaseFacadeDependenciesAccessor facade, QueryBuilderParameter<T> parameter)
        {
            var config = parameter.Config;
            var entityType = facade.Context.Model.FindEntityType(typeof(T));
            var properties = entityType.GetProperties().ToList();
            var tableName = entityType.GetTableName();
            var schemaName = entityType.GetSchema();
            var tableId = StoreObjectIdentifier.Table(tableName, schemaName);

            var hasPrimaryKey = properties.Any(x => x.IsPrimaryKey());
            if (!hasPrimaryKey)
            {
                // pkがなければupdateできない
                throw new InvalidOperationException("");
            }

            var builder = new QueryStringBuilder(config, parameter.WriteIndented);
            builder.AppendSql("DELETE FROM ");
            if (!string.IsNullOrEmpty(schemaName))
            {
                builder.AppendSql(config.GetQuotedName(schemaName));
                builder.AppendSql(".");
            }
            builder.AppendSql(config.GetQuotedName(tableName));
            builder.AppendLine(" WHERE ");
            var counter = 0;
            foreach (var property in properties)
            {
                var versionAttr = property.PropertyInfo.GetCustomAttribute<VersionAttribute>();
                if (parameter.IgnoreVersion && versionAttr != null) continue;
                if (!property.IsPrimaryKey() && versionAttr == null) continue;
                builder.AppendAnd(counter);
                var propValue = property.PropertyInfo.GetValue(parameter.Entity);
                var columnName = config.GetQuotedName(property.GetColumnName(tableId));
                builder.AppendSql(columnName);
                builder.AppendSql(" = ");
                builder.AppendParameter(config.GetParameterName(property.PropertyInfo.Name), propValue);
                builder.AppendLine();


                counter++;
            }

            return builder.GetResult();

        }

        #endregion

        //private static QueryBuilderResult GetSelectSql<T>(
        //    IDatabaseFacadeDependenciesAccessor facade, QueryBuilderParameter<T> parameter)
        //{
        //    var config = parameter.Config;
        //    var entityType = facade.Context.Model.FindEntityType(typeof(T));
        //    var properties = entityType.GetProperties().ToList();
        //    var tableName = entityType.GetTableName();
        //    var schemaName = entityType.GetSchema();
        //    var tableId = StoreObjectIdentifier.Table(tableName, schemaName);
        //    var builder = new QueryStringBuilder(config);
        //    builder.AppendLine("SELECT ");
        //    var counter = 0;
        //    foreach (var property in properties)
        //    {
        //        builder.AppendSql(counter == 0 ? "   " : " , ");

        //        builder.AppendLine(config.GetQuotedName(property.GetColumnName(tableId)));

        //        counter++;
        //    }
        //    if (!string.IsNullOrEmpty(schemaName))
        //    {
        //        builder.AppendSql(config.GetQuotedName(schemaName));
        //        builder.AppendSql(".");
        //    }
        //    builder.AppendSql(config.GetQuotedName(tableName));
        //    var hasPrimaryKey = properties.Any(x => x.IsPrimaryKey());
        //    if (!hasPrimaryKey)
        //    {
        //        return builder.GetResult();
        //    }
        //    builder.AppendLine("WHERE ");
        //    counter = 0;
        //    foreach (var property in properties)
        //    {
        //        builder.AppendSql(counter == 0 ? "     " : " AND ");
        //        if (!property.IsPrimaryKey()) continue;

        //        counter++;
        //    }

        //}

        private static class Cache<T>
        {
            static Cache()
            {
                var type = typeof(T);
                TableName = type.Name;
                var table = type.GetCustomAttribute<TableAttribute>();
                if (table != null)
                {
                    TableName = table.Name;
                }
                var props = type.GetProperties();
                ColumnMaps = new Dictionary<string, (string columnName, PropertyInfo property)>();
                foreach (var propertyInfo in props)
                {
                    var notMapped = propertyInfo.GetCustomAttribute<NotMappedAttribute>();
                    if (notMapped != null)
                    {
                        continue;
                    }

                    var column = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                    var columnName = propertyInfo.Name;
                    if (column != null)
                    {
                        columnName = column.Name;
                    }

                    ColumnMaps.Add(propertyInfo.Name, (columnName, propertyInfo));
                }

            }

            internal static string TableName { get; }

            internal static Dictionary<string, (string columnName,PropertyInfo property)> ColumnMaps { get; }


        }
    }
}
