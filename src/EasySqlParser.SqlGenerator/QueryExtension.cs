using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EasySqlParser.SqlGenerator
{
    public static class QueryExtension
    {
        public static int ExecuteNonQueryByQueryBuilder<T>(this DbConnection connection,
            QueryBuilderParameter<T> builderParameter,
            DbTransaction transaction = null)
        {
            DbTransaction localTransaction = null;
            if (transaction == null)
            {
                localTransaction = connection.BeginTransaction();
            }

            var builderResult = QueryBuilder<T>.GetQueryBuilderResult(builderParameter);
            builderParameter.WriteLog(builderResult.DebugSql);

            using (var command = connection.CreateCommand())
            {
                command.CommandText = builderResult.ParsedSql;
                command.Parameters.Clear();
                command.Parameters.AddRange(builderResult.DbDataParameters.ToArray());
                command.Transaction = transaction ?? localTransaction;

                command.CommandTimeout = builderParameter.CommandTimeout;
                int affectedCount;
                if (builderParameter.QueryBehavior == QueryBehavior.None ||
                    builderParameter.SqlKind == SqlKind.Delete)
                {
                    affectedCount = command.ExecuteNonQuery();
                }
                else
                {
                    affectedCount = Consume(builderParameter, command);
                }

                ThrowIfOptimisticLockException(builderParameter, affectedCount, builderResult, command.Transaction);
                localTransaction?.Commit();
                if (builderParameter.SqlKind == SqlKind.Update)
                {
                    builderParameter.IncrementVersion();
                }
                return affectedCount;
            }

        }

        private static int Consume<T>(
            QueryBuilderParameter<T> builderParameter,
            DbCommand command)
        {
            var reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                reader.Close();
                reader.Dispose();
                return 0;
            }

            var entityInfo = QueryBuilder<T>.GetEntityTypeInfo();
            reader.Read();
            var instance = builderParameter.Entity;
            foreach (var columnInfo in entityInfo.Columns)
            {
                var col = reader.GetOrdinal(columnInfo.ColumnName);
                if (!reader.IsDBNull(col))
                {
                    columnInfo.PropertyInfo.SetValue(instance, reader.GetValue(col));
                }
            }
            reader.Close();
            reader.Dispose();
            return 1;
        }

        public static async Task<int> ExecuteNonQueryByQueryBuilderAsync<T>(this DbConnection connection,
            QueryBuilderParameter<T> builderParameter,
            DbTransaction transaction = null,
            CancellationToken cancellationToken = default)
        {
            DbTransaction localTransaction = null;
            if (transaction == null)
            {
                localTransaction = connection.BeginTransaction();
            }
            var builderResult = QueryBuilder<T>.GetQueryBuilderResult(builderParameter);
            builderParameter.WriteLog(builderResult.DebugSql);

            using (var command = connection.CreateCommand())
            {
                command.CommandText = builderResult.ParsedSql;
                command.Parameters.Clear();
                command.Parameters.AddRange(builderResult.DbDataParameters.ToArray());
                command.Transaction = transaction ?? localTransaction;

                command.CommandTimeout = builderParameter.CommandTimeout;
                int affectedCount;
                if (builderParameter.QueryBehavior == QueryBehavior.None ||
                    builderParameter.SqlKind == SqlKind.Delete)
                {
                    affectedCount = await command.ExecuteNonQueryAsync(cancellationToken);
                }
                else
                {
                    affectedCount = await ConsumeAsync(builderParameter, command, cancellationToken);
                }
                ThrowIfOptimisticLockException(builderParameter, affectedCount, builderResult, command.Transaction);
                localTransaction?.Commit();
                if (builderParameter.SqlKind == SqlKind.Update)
                {
                    builderParameter.IncrementVersion();
                }
                return affectedCount;
            }

        }

        private static async Task<int> ConsumeAsync<T>(
            QueryBuilderParameter<T> builderParameter,
            DbCommand command,
            CancellationToken cancellationToken = default)
        {
            var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!reader.HasRows)
            {
                reader.Close();
                reader.Dispose();
                return 0;
            }

            var entityInfo = QueryBuilder<T>.GetEntityTypeInfo();
            await reader.ReadAsync(cancellationToken);
            var instance = builderParameter.Entity;
            foreach (var columnInfo in entityInfo.Columns)
            {
                var col = reader.GetOrdinal(columnInfo.ColumnName);
                if (!await reader.IsDBNullAsync(col, cancellationToken))
                {
                    columnInfo.PropertyInfo.SetValue(instance, reader.GetValue(col));
                }
            }
            reader.Close();
            reader.Dispose();
            return 1;
        }

        private static void ThrowIfOptimisticLockException<T>(
            QueryBuilderParameter<T> parameter,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            int affectedCount,
            QueryBuilderResult builderResult,
            DbTransaction transaction)
        {
            if ((parameter.SqlKind == SqlKind.Update || parameter.SqlKind == SqlKind.Delete) &&
                parameter.UseVersion && !parameter.SuppressOptimisticLockException
                && affectedCount == 0)
            {
                transaction.Rollback();
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
                localTransaction?.Commit();
            }


        }

    }
}
