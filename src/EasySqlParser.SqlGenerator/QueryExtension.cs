using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EasySqlParser.SqlGenerator
{
    public static class QueryExtension
    {
        public static int ExecuteNonQueryByQueryBuilder<T>(this DbConnection connection,
            QueryBuilderParameter<T> builderParameter,
            DbTransaction transaction = null,
            Action<string> loggerAction = null)
        {
            DbTransaction localTransaction = null;
            if (transaction == null)
            {
                localTransaction = connection.BeginTransaction();
            }

            var builderResult = QueryBuilder<T>.GetQueryBuilderResult(builderParameter);
            loggerAction?.Invoke(builderResult.DebugSql);

            using (var command = connection.CreateCommand())
            {
                command.CommandText = builderResult.ParsedSql;
                command.Parameters.Clear();
                command.Parameters.AddRange(builderResult.DbDataParameters.ToArray());
                command.Transaction = transaction ?? localTransaction;

                command.CommandTimeout = builderParameter.CommandTimeout;
                var affectedCount = command.ExecuteNonQuery();
                ThrowIfOptimisticLockException(builderParameter, affectedCount, builderResult, command.Transaction);
                localTransaction?.Commit();
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
            Action<string> loggerAction = null,
            CancellationToken cancellationToken = default)
        {
            DbTransaction localTransaction = null;
            if (transaction == null)
            {
                localTransaction = connection.BeginTransaction();
            }
            var builderResult = QueryBuilder<T>.GetQueryBuilderResult(builderParameter);
            loggerAction?.Invoke(builderResult.DebugSql);

            using (var command = connection.CreateCommand())
            {
                command.CommandText = builderResult.ParsedSql;
                command.Parameters.Clear();
                command.Parameters.AddRange(builderResult.DbDataParameters.ToArray());
                command.Transaction = transaction ?? localTransaction;

                command.CommandTimeout = builderParameter.CommandTimeout;
                var affectedCount = await command.ExecuteNonQueryAsync(cancellationToken);
                ThrowIfOptimisticLockException(builderParameter, affectedCount, builderResult, command.Transaction);
                localTransaction?.Commit();
                if (builderParameter.SqlKind == SqlKind.Update)
                {
                    builderParameter.IncrementVersion();
                }
                return affectedCount;
            }

        }

        private static void ThrowIfOptimisticLockException<T>(
            QueryBuilderParameter<T> parameter,
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

        internal static T ExecuteReaderByQueryBuilder<T>(this DbConnection connection,
            QueryBuilderParameter<T> builderParameter,
            Expression<Func<T, bool>> predicate,
            Action<string> loggerAction = null)
        {
            var visitor = new PredicateVisitor();
            var keyValues = visitor.GetKeyValues(predicate);
            return ExecuteReaderByQueryBuilder(connection, builderParameter, keyValues, loggerAction: loggerAction);
        }

        internal static T ExecuteReaderByQueryBuilder<T>(this DbConnection connection,
            QueryBuilderParameter<T> builderParameter,
            Dictionary<string, object> keyValues,
            DbTransaction transaction = null,
            Action<string> loggerAction = null)
        {
            //DbTransaction localTransaction = null;
            //if (transaction == null)
            //{
            //    localTransaction = connection.BeginTransaction();
            //}

            var (builderResult, entityInfo) = QueryBuilder<T>.GetSelectSql(builderParameter, keyValues);
            loggerAction?.Invoke(builderResult.DebugSql);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = builderResult.ParsedSql;
                command.Parameters.Clear();
                command.Parameters.AddRange(builderResult.DbDataParameters.ToArray());
                if (transaction != null)
                {
                    command.Transaction = transaction;
                }

                command.CommandTimeout = builderParameter.CommandTimeout;
                var reader = command.ExecuteReader();
                reader.Read();
                var instance = Activator.CreateInstance<T>();
                foreach (var columnInfo in entityInfo.Columns)
                {
                    var col = reader.GetOrdinal(columnInfo.ColumnName);
                    if (!reader.IsDBNull(col))
                    {
                        columnInfo.PropertyInfo.SetValue(instance, reader.GetValue(col));
                    }
                }
                //localTransaction?.Commit();
                return instance;

            }

        }
    }
}
