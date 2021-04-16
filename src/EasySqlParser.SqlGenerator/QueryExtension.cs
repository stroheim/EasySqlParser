using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EasySqlParser.SqlGenerator.Configurations;
using EasySqlParser.SqlGenerator.Enums;
using EasySqlParser.SqlGenerator.Helpers;

namespace EasySqlParser.SqlGenerator
{
    public static class QueryExtension
    {
        // non generic

        public static int ExecuteNonQueryByQueryBuilder(this DbConnection connection,
            QueryBuilderParameter builderParameter,
            DbTransaction transaction = null)
        {
            SequenceHelper.Generate(connection, builderParameter);

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
                if (builderParameter.SqlKind == SqlKind.Update || builderParameter.SqlKind == SqlKind.SoftDelete)
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
            await SequenceHelper.GenerateAsync(connection, builderParameter).ConfigureAwait(false);

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
                            await command.ConsumeNonQueryAsync(builderParameter, cancellationToken)
                                .ConfigureAwait(false);
                        break;
                    case CommandExecutionType.ExecuteReader:
                        affectedCount =
                            await command.ConsumeReaderAsync(builderParameter, cancellationToken)
                                .ConfigureAwait(false);
                        break;
                    case CommandExecutionType.ExecuteScalar:
                        affectedCount =
                            await command.ConsumeScalarAsync(builderParameter, cancellationToken)
                                .ConfigureAwait(false);
                        break;
                    default:
                        // TODO: error
                        throw new InvalidOperationException("");
                }
                ThrowIfOptimisticLockException(builderParameter, affectedCount, builderResult, command.Transaction);
                if (builderParameter.SqlKind == SqlKind.Update || builderParameter.SqlKind == SqlKind.SoftDelete)
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
            var (builderResult, entityInfo) = QueryBuilder.InternalGetSelectSql(predicate, configName, builderConfiguration.WriteIndented);
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
