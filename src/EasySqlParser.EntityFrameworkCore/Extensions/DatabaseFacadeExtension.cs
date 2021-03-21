using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasySqlParser.SqlGenerator;
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
            var scalarValue = await command.ExecuteScalarAsync(parameterObject);
            return DbCommandHelper.ConsumeScalar(scalarValue, builderParameter);
        }

        internal static async Task<int> ConsumeNonQueryAsync(this IRelationalCommand command,
            RelationalCommandParameterObject parameterObject,
            QueryBuilderParameter builderParameter)
        {
            var affectedCount = await command.ExecuteNonQueryAsync(parameterObject);
            builderParameter.ApplyReturningColumns();
            return affectedCount;
        }

        internal static async Task<int> ConsumeReaderAsync(this IRelationalCommand command,
            RelationalCommandParameterObject parameterObject,
            QueryBuilderParameter builderParameter)
        {
            await using var relationalDataReader = await command.ExecuteReaderAsync(parameterObject);
            var reader = relationalDataReader.DbDataReader;
            return await DbCommandHelper.ConsumeReaderAsync(reader, builderParameter);
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


        public static TResult ExecuteScalarByQueryBuilder<TResult>(
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

                // TODO: type error
                throw new InvalidOperationException("");
            }


        }

        public static async Task<TResult> ExecuteScalarByQueryBuilderAsync<TResult>(
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
                await database.UseTransactionAsync(transaction, cancellationToken);
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
                    .ExecuteScalarAsync(parameterObject, cancellationToken);
                if (rawScalar is TResult scalar)
                {
                    return scalar;
                }
                // TODO: type error
                throw new InvalidOperationException("");

            }

        }

        #endregion

        public static int ExecuteNonQueryByQueryBuilder(
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
                    facadeDependencies, facade, builderParameter);
                int affectedCount;
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
                //affectedCount = command
                //    .ExecuteNonQuery(parameterObject);
                ThrowIfOptimisticLockException(builderParameter, affectedCount, command.CommandText, debugSql,
                    transaction, dbContextTransaction);

                dbContextTransaction?.Commit();

                if (builderParameter.SqlKind == SqlKind.Update)
                {
                    builderParameter.IncrementVersion();
                }
                return affectedCount;

            }
        }


        public static async Task<int> ExecuteNonQueryByQueryBuilderAsync(
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
                var (command, parameterObject, debugSql) = CreateRelationalParameterObject(
                    facadeDependencies, facade, builderParameter);
                //var affectedCount = await command
                //    .ExecuteNonQueryAsync(parameterObject, cancellationToken);
                int affectedCount;
                builderParameter.SaveExpectedVersion();
                switch (builderParameter.CommandExecutionType)
                {
                    case CommandExecutionType.ExecuteNonQuery:
                        affectedCount = await command.ConsumeNonQueryAsync(parameterObject, builderParameter);
                        break;
                    case CommandExecutionType.ExecuteReader:
                        affectedCount = await command.ConsumeReaderAsync(parameterObject, builderParameter);
                        break;
                    case CommandExecutionType.ExecuteScalar:
                        affectedCount = await command.ConsumeScalarAsync(parameterObject, builderParameter);
                        break;
                    default:
                        // TODO: error
                        throw new InvalidOperationException("");
                }
                ThrowIfOptimisticLockException(builderParameter, affectedCount, command.CommandText, debugSql,
                    transaction, dbContextTransaction);

                if (dbContextTransaction != null)
                {
                    await dbContextTransaction.CommitAsync(cancellationToken);
                }

                if (builderParameter.SqlKind == SqlKind.Update)
                {
                    builderParameter.IncrementVersion();
                }
                return affectedCount;
            }

        }


        private static (IRelationalCommand command, RelationalCommandParameterObject parameterObject, string debugSql)
            CreateRelationalParameterObject(
                IRelationalDatabaseFacadeDependencies facadeDependencies,
                IDatabaseFacadeDependenciesAccessor facade,
                QueryBuilderParameter builderParameter)
        {
            IRelationalCommand command;
            IReadOnlyDictionary<string, object> parameterValues = null;
            // null check
            if (builderParameter == null)
            {
                //TODO:
                throw new InvalidOperationException("");
            }

            var builderResult = QueryBuilder.GetQueryBuilderResult(builderParameter);
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

            var debugSql = builderResult.DebugSql;
            builderParameter.WriteLog(debugSql);

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
            DbTransaction transaction = null,
            IDbContextTransaction dbContextTransaction = null)
        {
            if (parameter.ThrowableOptimisticLockException(affectedCount))
            {
                transaction?.Rollback();
                dbContextTransaction?.Rollback();
                //throw new DbUpdateConcurrencyException(RelationalStrings.UpdateConcurrencyException(1, 0));
                throw new OptimisticLockException(parsedSql, debugSql, parameter.SqlFile);
            }
        }


    }

    
}
