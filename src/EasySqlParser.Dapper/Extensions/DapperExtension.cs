using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Helpers;

namespace EasySqlParser.Dapper.Extensions
{
    public static class DapperExtension
    {
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

        public static async Task<int> ExecuteAsync(this DbConnection connection,
            QueryBuilderParameter builderParameter,
            DbTransaction transaction = null)
        {
            var builderResult = QueryBuilder.GetQueryBuilderResult(builderParameter);
            builderParameter.WriteLog(builderResult.DebugSql);

            int affectedCount;
            await SequenceHelper.GenerateAsync(connection, builderParameter).ConfigureAwait(false);
            builderParameter.SaveExpectedVersion();
            switch (builderParameter.CommandExecutionType)
            {
                case CommandExecutionType.ExecuteNonQuery:
                    affectedCount =
                        await ConsumeHelper.ConsumeNonQueryAsync(connection, builderParameter, builderResult, transaction)
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
                    // TODO: error
                    throw new InvalidOperationException("");
            }

            ThrowIfOptimisticLockException(builderParameter, affectedCount, builderResult, transaction);
            if (builderParameter.SqlKind == SqlKind.Update || builderParameter.SqlKind == SqlKind.SoftDelete)
            {
                builderParameter.IncrementVersion();
            }
            return affectedCount;


        }

        public static int Execute(this DbConnection connection,
            QueryBuilderParameter builderParameter,
            DbTransaction transaction = null)
        {
            var builderResult = QueryBuilder.GetQueryBuilderResult(builderParameter);
            builderParameter.WriteLog(builderResult.DebugSql);

            int affectedCount;
            SequenceHelper.Generate(connection, builderParameter);
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
                    // TODO: error
                    throw new InvalidOperationException("");
            }

            ThrowIfOptimisticLockException(builderParameter, affectedCount, builderResult, transaction);
            if (builderParameter.SqlKind == SqlKind.Update || builderParameter.SqlKind == SqlKind.SoftDelete)
            {
                builderParameter.IncrementVersion();
            }
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

    internal static class ConsumeHelper
    {
        internal static int ConsumeNonQuery(IDbConnection connection,
            QueryBuilderParameter builderParameter,
            QueryBuilderResult builderResult,
            DbTransaction transaction = null)
        {
            var affectedCount = connection.Execute(
                builderResult.ParsedSql,
                builderResult.DbDataParameters.ToDynamicParameters(),
                transaction,
                builderParameter.CommandTimeout
            );
            //builderParameter.ApplyReturningColumns();
            return affectedCount;
        }

        internal static async Task<int> ConsumeNonQueryAsync(IDbConnection connection,
            QueryBuilderParameter builderParameter,
            QueryBuilderResult builderResult,
            DbTransaction transaction = null)
        {
            var affectedCount = await connection.ExecuteAsync(
                builderResult.ParsedSql,
                builderResult.DbDataParameters.ToDynamicParameters(),
                transaction,
                builderParameter.CommandTimeout)
                .ConfigureAwait(false);
            return affectedCount;
        }

        internal static int ConsumeReader(IDbConnection connection,
            QueryBuilderParameter builderParameter,
            QueryBuilderResult builderResult,
            DbTransaction transaction = null)
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
            DbTransaction transaction = null)
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
            DbTransaction transaction = null)
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
            DbTransaction transaction = null)
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
