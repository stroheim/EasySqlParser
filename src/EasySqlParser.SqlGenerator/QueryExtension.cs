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
        // non generic

        public static bool TryGenerateSequence<TResult>(this DbConnection connection,
            QueryBuilderParameter builderParameter,
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
                    sequenceValue = converter.Invoke((decimal)rawResult);
                    return true;
                }

                if (config.DbConnectionKind == DbConnectionKind.PostgreSql &&
                    converter != null)
                {
                    sequenceValue = converter.Invoke((long)rawResult);
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

        public static async Task<(bool isSuccess, TResult sequenceValue)> TryGenerateSequenceAsync<TResult>(
            this DbConnection connection,
            QueryBuilderParameter builderParameter,
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
                    return (true, converter.Invoke((long)rawResult));
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

        public static int ExecuteNonQueryByQueryBuilder(this DbConnection connection,
            QueryBuilderParameter builderParameter,
            DbTransaction transaction = null)
        {
            connection.PreInsert(builderParameter);

            var builderResult = QueryBuilder.GetQueryBuilderResult(builderParameter);
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

                command.CommandTimeout = builderParameter.CommandTimeout;
                int affectedCount;
                builderParameter.SaveExpectedVersion();

                switch (builderParameter.CommandExecutionType)
                {
                    case CommandExecutionType.ExecuteNonQuery:
                        affectedCount = command.ConsumeNonQuery(builderParameter);
                        break;
                    case CommandExecutionType.ExecuteReader:
                        affectedCount = command.ConsumeReader(builderParameter);
                        break;
                    case CommandExecutionType.ExecuteScalar:
                        affectedCount = command.ConsumeScalar(builderParameter);
                        break;
                    default:
                        // TODO: error
                        throw new InvalidOperationException("");
                }

                ThrowIfOptimisticLockException(builderParameter, affectedCount, builderResult, command.Transaction);
                if (builderParameter.SqlKind == SqlKind.Update)
                {
                    builderParameter.IncrementVersion();
                }
                return affectedCount;
            }

        }

        public static async Task<int> ExecuteNonQueryByQueryBuilderAsync(this DbConnection connection,
            QueryBuilderParameter builderParameter,
            DbTransaction transaction = null,
            CancellationToken cancellationToken = default)
        {
            await connection.PreInsertAsync(builderParameter);

            var builderResult = QueryBuilder.GetQueryBuilderResult(builderParameter);
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

                command.CommandTimeout = builderParameter.CommandTimeout;
                int affectedCount;
                builderParameter.SaveExpectedVersion();
                switch (builderParameter.CommandExecutionType)
                {
                    case CommandExecutionType.ExecuteNonQuery:
                        affectedCount =
                            await command.ConsumeNonQueryAsync(builderParameter, cancellationToken);
                        break;
                    case CommandExecutionType.ExecuteReader:
                        affectedCount =
                            await command.ConsumeReaderAsync(builderParameter, cancellationToken);
                        break;
                    case CommandExecutionType.ExecuteScalar:
                        affectedCount =
                            await command.ConsumeScalarAsync(builderParameter, cancellationToken);
                        break;
                    default:
                        // TODO: error
                        throw new InvalidOperationException("");
                }
                ThrowIfOptimisticLockException(builderParameter, affectedCount, builderResult, command.Transaction);
                if (builderParameter.SqlKind == SqlKind.Update)
                {
                    builderParameter.IncrementVersion();
                }
                return affectedCount;
            }

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


        //--------------------------------
        //--------------------------------
        //--------------------------------


        // for unit test
        internal static IEnumerable<T> ExecuteReaderByQueryBuilder<T>(this DbConnection connection,
            Expression<Func<T, bool>> predicate,
            IQueryBuilderConfiguration builderConfiguration,
            string configName = null,
            DbTransaction transaction = null)
        {
            var (builderResult, entityInfo) = QueryBuilder.GetSelectSql(predicate, configName, builderConfiguration.WriteIndented);
            builderConfiguration.LoggerAction?.Invoke(builderResult.DebugSql);

            using (var command = connection.CreateCommand())
            {
                command.CommandText = builderResult.ParsedSql;
                command.Parameters.Clear();
                command.Parameters.AddRange(builderResult.DbDataParameters.ToArray());
                if (transaction != null)
                {
                    command.Transaction = transaction;
                }


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
            }

        }

    }
}
