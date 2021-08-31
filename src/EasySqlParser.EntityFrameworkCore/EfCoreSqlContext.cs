using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasySqlParser.EntityFrameworkCore.Extensions;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Configurations;
using EasySqlParser.SqlGenerator.Enums;
using EasySqlParser.SqlGenerator.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EasySqlParser.EntityFrameworkCore
{
    /// <summary>
    ///     <see cref="ISqlContext"/> implementation for EntityFrameworkCore.
    /// </summary>
    public class EfCoreSqlContext : ISqlContext
    {
        private readonly DbContext _context;
        private readonly IDatabaseFacadeDependenciesAccessor _facade;
        private readonly IRelationalDatabaseFacadeDependencies _facadeDependencies;
        private readonly List<SqlItem> _sqlItems;
        private readonly List<SqlItem> _prioritySqlItems;
        private readonly IQueryBuilderConfiguration _configuration;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EfCoreSqlContext"/> class.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public EfCoreSqlContext(DbContext context, IQueryBuilderConfiguration configuration)
        {
            _context = context;
            _facade = context.Database;
            _facadeDependencies = _facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            _sqlItems = new List<SqlItem>();
            _prioritySqlItems = new List<SqlItem>();
            _configuration = configuration;
        }


        /// <inheritdoc />
        public SaveChangesBehavior SaveChangesBehavior { get; internal set; }

        /// <inheritdoc />
        public void Add(FormattableString sqlTemplate, bool forceFirst = false)
        {
            IRelationalCommand command;
            IReadOnlyDictionary<string, object> parameterValues = null;
            if (sqlTemplate.ArgumentCount == 0)
            {
                command = _facadeDependencies.RawSqlCommandBuilder.Build(sqlTemplate.Format);
            }
            else
            {
                var rawSqlCommand = _facadeDependencies
                    .RawSqlCommandBuilder.Build(sqlTemplate.Format, sqlTemplate.GetArguments());
                command = rawSqlCommand.RelationalCommand;
                parameterValues = rawSqlCommand.ParameterValues;
            }
            

            var sqlItem = new SqlItem(command, new RelationalCommandParameterObject(
                _facadeDependencies.RelationalConnection,
                parameterValues,
                null,
                _facade.Context,
                _facadeDependencies.CommandLogger),
                null,
                null);
            if (forceFirst)
            {
                _prioritySqlItems.Add(sqlItem);
            }
            else
            {
                _sqlItems.Add(sqlItem);
            }
        }

        /// <inheritdoc />
        public void Add(SqlParserResult parserResult, bool merge = false, bool forceFirst = false)
        {
            IRelationalCommand command;
            IReadOnlyDictionary<string, object> parameterValues = null;
            if (parserResult == null)
            {
                throw new ArgumentNullException(nameof(parserResult));
            }

            if (parserResult.DbDataParameters.Count == 0)
            {
                var sql = parserResult.ParsedSql;
                if (merge)
                {
                    sql += ";;";
                }
                command = _facadeDependencies.RawSqlCommandBuilder.Build(sql);

            }
            else
            {
                var sql = parserResult.ParsedSql;
                if (merge)
                {
                    sql += ";;";
                }
                var rawSqlCommand = _facadeDependencies
                    .RawSqlCommandBuilder
                    .Build(sql, parserResult.DbDataParameters.Cast<object>().ToArray());
                command = rawSqlCommand.RelationalCommand;
                parameterValues = rawSqlCommand.ParameterValues;
            }
            var sqlItem = new SqlItem(command, new RelationalCommandParameterObject(
                _facadeDependencies.RelationalConnection,
                parameterValues,
                null,
                _facade.Context,
                _facadeDependencies.CommandLogger),
                null,
                null);
            if (forceFirst)
            {
                _prioritySqlItems.Add(sqlItem);
            }
            else
            {
                _sqlItems.Add(sqlItem);
            }

        }

        /// <inheritdoc />
        public void Add(QueryBuilderParameter builderParameter, bool forceFirst = false)
        {
            var connection = _facadeDependencies.RelationalConnection.DbConnection;
            SequenceHelper.Generate(connection, builderParameter);

            IRelationalCommand command;
            IReadOnlyDictionary<string, object> parameterValues = null;
            var builderResult = QueryBuilder.GetQueryBuilderResult(builderParameter);
            if (builderParameter.CommandTimeout > -1)
            {
                _facadeDependencies.RelationalConnection.CommandTimeout = builderParameter.CommandTimeout;
            }
            if (builderResult.DbDataParameters.Count == 0)
            {
                command = _facadeDependencies.RawSqlCommandBuilder.Build(builderResult.ParsedSql);

            }
            else
            {
                var rawSqlCommand = _facadeDependencies
                    .RawSqlCommandBuilder
                    .Build(builderResult.ParsedSql, builderResult.DbDataParameters.Cast<object>().ToArray());
                command = rawSqlCommand.RelationalCommand;
                parameterValues = rawSqlCommand.ParameterValues;
            }
            var sqlItem = new SqlItem(command, new RelationalCommandParameterObject(
                _facadeDependencies.RelationalConnection,
                parameterValues,
                null,
                _facade.Context,
                _facadeDependencies.CommandLogger),
                builderParameter,
                builderResult);
            if (forceFirst)
            {
                _prioritySqlItems.Add(sqlItem);
            }
            else
            {
                _sqlItems.Add(sqlItem);
            }
        }

        /// <inheritdoc />
        public int SaveChanges()
        {
            var concurrentDetector = _facadeDependencies.ConcurrencyDetector;
            var dbContextTransaction = _context.Database.CurrentTransaction;
            var beganTransaction = false;
            if (dbContextTransaction == null)
            {
                dbContextTransaction = _context.Database.BeginTransaction();
                beganTransaction = true;
            }

            using (concurrentDetector.EnterCriticalSection())
            {
                var affectedCount = 0;
                try
                {
                    foreach (var sqlItem in _prioritySqlItems)
                    {
                        affectedCount += Save(sqlItem);
                    }

                    if (SaveChangesBehavior == SaveChangesBehavior.DbContextFirst)
                    {
                        affectedCount += _context.SaveChanges();
                    }

                    foreach (var sqlItem in _sqlItems)
                    {
                        affectedCount += Save(sqlItem);
                    }

                    if (SaveChangesBehavior == SaveChangesBehavior.SqlContextFirst)
                    {
                        affectedCount += _context.SaveChanges();
                    }

                    if (beganTransaction)
                    {
                        dbContextTransaction?.Commit();
                    }

                }

                finally
                {
                    _prioritySqlItems.Clear();
                    _sqlItems.Clear();
                    if (beganTransaction)
                    {
                        dbContextTransaction?.Dispose();
                    }
                }

                return affectedCount;
            }
        }

        /// <inheritdoc />
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var concurrentDetector = _facadeDependencies.ConcurrencyDetector;

            var dbContextTransaction = _context.Database.CurrentTransaction;
            var beganTransaction = false;
            if (dbContextTransaction == null)
            {
                dbContextTransaction = await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                beganTransaction = true;
            }

            using (concurrentDetector.EnterCriticalSection())
            {
                var affectedCount = 0;
                try
                {
                    foreach (var sqlItem in _prioritySqlItems)
                    {
                        affectedCount += await SaveAsync(sqlItem, cancellationToken);
                    }

                    if (SaveChangesBehavior == SaveChangesBehavior.DbContextFirst)
                    {
                        affectedCount += await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }

                    foreach (var sqlItem in _sqlItems)
                    {
                        affectedCount += await SaveAsync(sqlItem, cancellationToken);
                    }

                    if (SaveChangesBehavior == SaveChangesBehavior.SqlContextFirst)
                    {
                        affectedCount += await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }

                    if (beganTransaction && dbContextTransaction != null)
                    {
                        await dbContextTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                    }

                }
                finally
                {
                    _prioritySqlItems.Clear();
                    _sqlItems.Clear();
                    if (beganTransaction && dbContextTransaction != null)
                    {
                        await dbContextTransaction.DisposeAsync().ConfigureAwait(false);
                    }

                }

                return affectedCount;
            }
        }

        private static int Save(SqlItem sqlItem)
        {
            if (sqlItem.BuilderParameter == null)
            {
                return sqlItem.Command.ExecuteNonQuery(sqlItem.ParameterObject);
            }

            var builderParameter = sqlItem.BuilderParameter;
            builderParameter.SaveExpectedVersion();
            int affectedCount;
            var command = sqlItem.Command;
            var parameterObject = sqlItem.ParameterObject;
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
                    throw new InvalidOperationException($"Unknown CommandExecutionType:{builderParameter.CommandExecutionType}");
            }

            ThrowIfOptimisticLockException(builderParameter, affectedCount, sqlItem.BuilderResult);
            builderParameter.IncrementVersion();
            return affectedCount;
        }

        private static async Task<int> SaveAsync(SqlItem sqlItem, CancellationToken cancellationToken = default)
        {
            if (sqlItem.BuilderParameter == null)
            {
                return await sqlItem.Command.ExecuteNonQueryAsync(sqlItem.ParameterObject, cancellationToken).ConfigureAwait(false);
            }

            var builderParameter = sqlItem.BuilderParameter;
            builderParameter.SaveExpectedVersion();
            int affectedCount;
            var command = sqlItem.Command;
            var parameterObject = sqlItem.ParameterObject;
            
            switch (builderParameter.CommandExecutionType)
            {
                case CommandExecutionType.ExecuteNonQuery:
                    affectedCount = await command.ConsumeNonQueryAsync(parameterObject, builderParameter, cancellationToken).ConfigureAwait(false);
                    break;
                case CommandExecutionType.ExecuteReader:
                    affectedCount = await command.ConsumeReaderAsync(parameterObject, builderParameter, cancellationToken).ConfigureAwait(false);
                    break;
                case CommandExecutionType.ExecuteScalar:
                    affectedCount = await command.ConsumeScalarAsync(parameterObject, builderParameter, cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown CommandExecutionType:{builderParameter.CommandExecutionType}");
            }

            ThrowIfOptimisticLockException(builderParameter, affectedCount, sqlItem.BuilderResult);
            builderParameter.IncrementVersion();
            return affectedCount;

        }

        private static void ThrowIfOptimisticLockException(
            QueryBuilderParameter parameter,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            int affectedCount,
            QueryBuilderResult builderResult)
        {
            if (parameter.ThrowableOptimisticLockException(affectedCount))
            {
                throw new OptimisticLockException(builderResult.ParsedSql, builderResult.DebugSql, parameter.SqlFile);
            }
        }


        private class SqlItem
        {
            internal readonly IRelationalCommand Command;
            internal readonly RelationalCommandParameterObject ParameterObject;
            internal readonly QueryBuilderParameter BuilderParameter;
            internal readonly QueryBuilderResult BuilderResult;

            internal SqlItem(IRelationalCommand command, RelationalCommandParameterObject parameterObject,
                QueryBuilderParameter builderParameter,
                QueryBuilderResult builderResult)
            {
                Command = command;
                ParameterObject = parameterObject;
                BuilderParameter = builderParameter;
                BuilderResult = builderResult;
            }

        }
    }
}
