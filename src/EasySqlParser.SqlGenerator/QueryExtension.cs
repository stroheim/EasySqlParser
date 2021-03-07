using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EasySqlParser.Extensions;

namespace EasySqlParser.SqlGenerator
{
    public static class QueryExtension
    {

        public static bool TryGenerateSequence<T, TResult>(this DbConnection connection,
            QueryBuilderParameter<T> builderParameter,
            SequenceGeneratorAttribute attribute,
            out TResult sequenceValue,
            Func<decimal, TResult> converter = null)
        {
            var config = builderParameter.Config;
            if (!config.Dialect.SupportsSequence)
            {
                sequenceValue = default;
                return false;
            }

            var sql = attribute.GetSequenceGeneratorSql(config);
            builderParameter.WriteLog(sql);
            object rawResult;
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                rawResult = command.ExecuteScalar();
                if ((config.DbConnectionKind == DbConnectionKind.Oracle ||
                     config.DbConnectionKind == DbConnectionKind.OracleLegacy) &&
                    converter != null)
                {
                    sequenceValue = converter.Invoke((decimal) rawResult);
                    return true;
                }

                if (config.DbConnectionKind == DbConnectionKind.PostgreSql &&
                    converter != null)
                {
                    sequenceValue = converter.Invoke((long) rawResult);
                    return true;
                }

                if (rawResult is TResult result)
                {
                    sequenceValue = result;
                    return true;
                }
            }

            // error
            // not match TResult
            var message = $"戻されたシーケンスの型が期待されたものではありませんでした。期待された型：{typeof(TResult).Name} 実際の型：{rawResult.GetType().Name}";
            throw new InvalidOperationException(message);
        }

        public static async Task<(bool isSuccess, TResult sequenceValue)> TryGenerateSequenceAsync<T, TResult>(
            this DbConnection connection,
            QueryBuilderParameter<T> builderParameter,
            SequenceGeneratorAttribute attribute,
            CancellationToken cancellationToken = default,
            Func<decimal, TResult> converter = null)
        {
            var config = builderParameter.Config;
            if (!config.Dialect.SupportsSequence)
            {
                return (false, default);
            }

            var sql = attribute.GetSequenceGeneratorSql(config);
            builderParameter.WriteLog(sql);
            object rawResult;
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                rawResult = await command.ExecuteScalarAsync(cancellationToken);
                if ((config.DbConnectionKind == DbConnectionKind.Oracle ||
                     config.DbConnectionKind == DbConnectionKind.OracleLegacy) &&
                    converter != null)
                {
                    return (true, converter.Invoke((decimal)rawResult));
                }

                if (config.DbConnectionKind == DbConnectionKind.PostgreSql &&
                    converter != null)
                {
                    return (true, converter.Invoke((long) rawResult));
                }

                if (rawResult is TResult result)
                {
                    return (true, result);
                }
            }

            // error
            // not match TResult
            var message = $"戻されたシーケンスの型が期待されたものではありませんでした。期待された型：{typeof(TResult).Name} 実際の型：{rawResult.GetType().Name}";
            throw new InvalidOperationException(message);
        }


        public static int ExecuteNonQueryByQueryBuilder<T>(this DbConnection connection,
            QueryBuilderParameter<T> builderParameter,
            DbTransaction transaction = null)
        {
            QueryBuilder<T>.PreInsert(builderParameter, connection);
            //DbTransaction localTransaction = null;
            //if (transaction == null)
            //{
            //    localTransaction = connection.BeginTransaction();
            //}

            var builderResult = QueryBuilder<T>.GetQueryBuilderResult(builderParameter);
            builderParameter.WriteLog(builderResult.DebugSql);

            using (var command = connection.CreateCommand())
            {
                command.CommandText = builderResult.ParsedSql;
                command.Parameters.Clear();
                command.Parameters.AddRange(builderResult.DbDataParameters.ToArray());
                if (transaction != null)
                {
                    command.Transaction = transaction;
                }
                //command.Transaction = transaction ?? localTransaction;

                command.CommandTimeout = builderParameter.CommandTimeout;
                int affectedCount;

                switch (builderParameter.CommandExecutionType)
                {
                    case CommandExecutionType.ExecuteNonQuery:
                        affectedCount = QueryBuilder<T>.ConsumeNonQuery(builderParameter, command);
                        break;
                    case CommandExecutionType.ExecuteReader:
                        affectedCount = QueryBuilder<T>.ConsumeReader(builderParameter, command);
                        break;
                    case CommandExecutionType.ExecuteScalar:
                        affectedCount = QueryBuilder<T>.ConsumeScalar(builderParameter, command);
                        break;
                    default:
                        // TODO: error
                        throw new InvalidOperationException("");
                }

                ThrowIfOptimisticLockException(builderParameter, affectedCount, builderResult, command.Transaction);
                //localTransaction?.Commit();
                if (builderParameter.SqlKind == SqlKind.Update)
                {
                    builderParameter.IncrementVersion();
                }
                return affectedCount;
            }

        }


        public static async Task<int> ExecuteNonQueryByQueryBuilderAsync<T>(this DbConnection connection,
            QueryBuilderParameter<T> builderParameter,
            DbTransaction transaction = null,
            CancellationToken cancellationToken = default)
        {
            await QueryBuilder<T>.PreInsertAsync(builderParameter, connection);

            //DbTransaction localTransaction = null;
            //if (transaction == null)
            //{
            //    localTransaction = connection.BeginTransaction();
            //}
            var builderResult = QueryBuilder<T>.GetQueryBuilderResult(builderParameter);
            builderParameter.WriteLog(builderResult.DebugSql);

            using (var command = connection.CreateCommand())
            {
                command.CommandText = builderResult.ParsedSql;
                command.Parameters.Clear();
                command.Parameters.AddRange(builderResult.DbDataParameters.ToArray());
                if (transaction != null)
                {
                    command.Transaction = transaction;
                }
                //command.Transaction = transaction ?? localTransaction;

                command.CommandTimeout = builderParameter.CommandTimeout;
                int affectedCount;
                switch (builderParameter.CommandExecutionType)
                {
                    case CommandExecutionType.ExecuteNonQuery:
                        affectedCount =
                            await QueryBuilder<T>.ConsumeNonQueryAsync(builderParameter, command, cancellationToken);
                        break;
                    case CommandExecutionType.ExecuteReader:
                        affectedCount =
                            await QueryBuilder<T>.ConsumeReaderAsync(builderParameter, command, cancellationToken);
                        break;
                    case CommandExecutionType.ExecuteScalar:
                        affectedCount =
                            await QueryBuilder<T>.ConsumeScalarAsync(builderParameter, command, cancellationToken);
                        break;
                    default:
                        // TODO: error
                        throw new InvalidOperationException("");
                }
                ThrowIfOptimisticLockException(builderParameter, affectedCount, builderResult, command.Transaction);
                //localTransaction?.Commit();
                if (builderParameter.SqlKind == SqlKind.Update)
                {
                    builderParameter.IncrementVersion();
                }
                return affectedCount;
            }

        }


        private static void ThrowIfOptimisticLockException<T>(
            QueryBuilderParameter<T> parameter,
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

        internal static TResult GetCount<TEntity, TResult>(this DbConnection connection,
            IQueryBuilderConfiguration builderConfiguration,
            Expression<Func<TEntity, bool>> predicate = null,
            string configName = null,
            DbTransaction transaction = null)
        {
            var builderResult = QueryBuilder<TEntity>.GetCountSql(predicate, configName, builderConfiguration.WriteIndented);
            builderConfiguration.LoggerAction?.Invoke(builderResult.DebugSql);
            DbTransaction localTransaction = null;
            if (transaction == null)
            {
                localTransaction = connection.BeginTransaction();
            }
            using (var command = connection.CreateCommand())
            {
                command.CommandText = builderResult.ParsedSql;
                command.Parameters.Clear();
                command.Parameters.AddRange(builderResult.DbDataParameters.ToArray());
                command.Transaction = localTransaction ?? transaction;

                command.CommandTimeout = builderConfiguration.CommandTimeout;
                var scalar = command.ExecuteScalar();
                localTransaction?.Commit();
                if (scalar is TResult result)
                {
                    return result;
                }

                return default;
            }
        }

        //internal static async Task<IEnumerable<T>> ExecuteReaderByQueryBuilderAsync<T>(this DbConnection connection,
        //    Expression<Func<T, bool>> predicate,
        //    string configName = null,
        //    DbTransaction transaction = null,
        //    bool writeIndented = false,
        //    int timeout = 30,
        //    Action<string> loggerAction = null,
        //    CancellationToken cancellationToken = default)
        //{
        //    var tasks = new List<Task<T>>();

        //    var (builderResult, entityInfo) = QueryBuilder<T>.GetSelectSql(predicate, configName, writeIndented);
        //    loggerAction?.Invoke(builderResult.DebugSql);
        //    DbTransaction localTransaction = null;
        //    if (transaction == null)
        //    {
        //        localTransaction = connection.BeginTransaction();
        //    }

        //    using (var command = connection.CreateCommand())
        //    {
        //        command.CommandText = builderResult.ParsedSql;
        //        command.Parameters.Clear();
        //        command.Parameters.AddRange(builderResult.DbDataParameters.ToArray());
        //        command.Transaction = localTransaction ?? transaction;

        //        command.CommandTimeout = timeout;
        //        var reader = await command.ExecuteReaderAsync(cancellationToken);
        //        if (!reader.HasRows)
        //        {
        //            reader.Close();
        //            reader.Dispose();
        //            return Enumerable.Empty<T>();
        //        }

        //        while (await reader.ReadAsync(cancellationToken))
        //        {
        //            var instance = Activator.CreateInstance<T>();
        //            foreach (var columnInfo in entityInfo.Columns)
        //            {
        //                var col = reader.GetOrdinal(columnInfo.ColumnName);
        //                if (!await reader.IsDBNullAsync(col, cancellationToken))
        //                {
        //                    columnInfo.PropertyInfo.SetValue(instance, reader.GetValue(col));
        //                }
        //            }

        //            tasks.Add(Task.FromResult(instance));
        //        }

        //        return await Task.WhenAll(tasks);

        //    }

        //}


        internal static IEnumerable<T> ExecuteReaderByQueryBuilder<T>(this DbConnection connection,
            Expression<Func<T, bool>> predicate,
            IQueryBuilderConfiguration builderConfiguration,
            string configName = null,
            DbTransaction transaction = null)
        {
            var (builderResult, entityInfo) = QueryBuilder<T>.GetSelectSql(predicate, configName, builderConfiguration.WriteIndented);
            builderConfiguration.LoggerAction?.Invoke(builderResult.DebugSql);
            //DbTransaction localTransaction = null;
            //if (transaction == null)
            //{
            //    localTransaction = connection.BeginTransaction();
            //}
            using (var command = connection.CreateCommand())
            {
                command.CommandText = builderResult.ParsedSql;
                command.Parameters.Clear();
                command.Parameters.AddRange(builderResult.DbDataParameters.ToArray());
                if (transaction != null)
                {
                    command.Transaction = transaction;
                }

                //command.Transaction = transaction ?? localTransaction;

                command.CommandTimeout = builderConfiguration.CommandTimeout;
                var reader = command.ExecuteReader();
                if (!reader.HasRows)
                {
                    reader.Close();
                    reader.Dispose();
                    yield break;
                }

                while (reader.Read())
                {
                    var instance = Activator.CreateInstance<T>();
                    foreach (var columnInfo in entityInfo.Columns)
                    {
                        var col = reader.GetOrdinal(columnInfo.ColumnName);
                        if (!reader.IsDBNull(col))
                        {
                            columnInfo.PropertyInfo.SetValue(instance, reader.GetValue(col));
                        }
                    }

                    yield return instance;
                }
                reader.Close();
                reader.Dispose();
                //localTransaction?.Commit();
            }


        }

    }
}
