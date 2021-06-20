using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using EasySqlParser.Configurations;
using EasySqlParser.Dapper.Extensions;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Enums;
using EasySqlParser.SqlGenerator.Helpers;

namespace EasySqlParser.Dapper
{
    public class DapperSqlContext : ISqlContext
    {
        private readonly DbConnection _connection;
        private readonly List<SqlItem> _sqlItems;
        private readonly List<SqlItem> _prioritySqlItems;
        private readonly SqlParserConfig _config;

        public DapperSqlContext(DbConnection connection, string configName = null)
        {
            _connection = connection;
            _sqlItems = new List<SqlItem>();
            _prioritySqlItems = new List<SqlItem>();
            _config = configName == null
                ? ConfigContainer.DefaultConfig
                : ConfigContainer.AdditionalConfigs[configName];
        }

        public SaveChangesBehavior SaveChangesBehavior { get; } = SaveChangesBehavior.SqlContextOnly;

        public void Add(FormattableString sqlTemplate, bool forceFirst = false)
        {
            SqlItem sqlItem;
            if (sqlTemplate.ArgumentCount == 0)
            {
                sqlItem = new SqlItem(sqlTemplate.Format, null, null, null);
            }
            else
            {
                var substitutions = new List<string>();
                var parameters = new DynamicParameters();
                for (var i = 0; i < sqlTemplate.ArgumentCount; i++)
                {
                    var parameterKey = $"p{i}";
                    string parameterName;
                    if (!_config.Dialect.EnableNamedParameter)
                    {
                        substitutions.Add(_config.Dialect.ParameterPrefix);
                        parameterName = _config.Dialect.ParameterPrefix;
                    }
                    else
                    {
                        substitutions.Add(parameterKey);
                        parameterName = _config.Dialect.ParameterPrefix + parameterKey;
                    }
                    var paramValue = sqlTemplate.GetArgument(i);
                    if (paramValue == null)
                    {
                        parameters.Add(parameterName, DBNull.Value);
                        continue;
                    }
                    parameters.Add(parameterName, paramValue);
                }

                // ReSharper disable once CoVariantArrayConversion
                sqlItem = new SqlItem(
                    string.Format(sqlTemplate.Format, substitutions.ToArray()), 
                    parameters,
                    null,
                    null);
            }

            if (forceFirst)
            {
                _prioritySqlItems.Add(sqlItem);
            }
            else
            {
                _sqlItems.Add(sqlItem);
            }
        }

        public void Add(SqlParserResult parserResult, bool merge = false, bool forceFirst = false)
        {
            var sql = merge ? parserResult.ParsedSql + ";;" : parserResult.ParsedSql;
            var sqlItem = new SqlItem(sql, 
                parserResult.DbDataParameters.ToDynamicParameters(),
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

        public void Add(QueryBuilderParameter builderParameter, bool forceFirst = false)
        {
            SequenceHelper.Generate(_connection, builderParameter);
            var builderResult = QueryBuilder.GetQueryBuilderResult(builderParameter);
            var sqlItem = new SqlItem(builderResult.ParsedSql,
                builderResult.DbDataParameters.ToDynamicParameters(),
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

        public int SaveChanges()
        {
            DbTransaction transaction = null;
            var affectedCount = 0;
            try
            {
                transaction = _connection.BeginTransaction();
                foreach (var sqlItem in _prioritySqlItems)
                {
                    affectedCount += Save(sqlItem, _connection, transaction);
                }

                foreach (var sqlItem in _sqlItems)
                {
                    affectedCount += Save(sqlItem, _connection, transaction);
                }

                transaction.Commit();
            }
            finally
            {
                _prioritySqlItems.Clear();
                _sqlItems.Clear();
                transaction?.Dispose();
            }

            return affectedCount;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            DbTransaction transaction = null;
            var affectedCount = 0;
            try
            {
                transaction = _connection.BeginTransaction();
                foreach (var sqlItem in _prioritySqlItems)
                {
                    affectedCount += await SaveAsync(sqlItem, _connection, transaction, cancellationToken);
                }

                foreach (var sqlItem in _sqlItems)
                {
                    affectedCount += await SaveAsync(sqlItem, _connection, transaction, cancellationToken);
                }

                transaction.Commit();
            }
            finally
            {
                _prioritySqlItems.Clear();
                _sqlItems.Clear();
                transaction?.Dispose();
            }

            return affectedCount;
        }

        private static int Save(SqlItem sqlItem, DbConnection connection, DbTransaction transaction)
        {
            if (sqlItem.BuilderParameter == null)
            {
                return connection.Execute(sqlItem.Sql, sqlItem.Parameters, transaction);
            }
            var builderParameter = sqlItem.BuilderParameter;
            var builderResult = sqlItem.BuilderResult;
            builderParameter.SaveExpectedVersion();
            int affectedCount;

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

            ThrowIfOptimisticLockException(builderParameter, affectedCount, builderResult);
            builderParameter.IncrementVersion();
            return affectedCount;
        }

        private static async Task<int> SaveAsync(SqlItem sqlItem, 
            DbConnection connection, DbTransaction transaction, 
            CancellationToken cancellationToken = default)
        {
            if (sqlItem.BuilderParameter == null)
            {
                return await connection.ExecuteAsync(sqlItem.Sql, sqlItem.Parameters, transaction);
            }
            var builderParameter = sqlItem.BuilderParameter;
            var builderResult = sqlItem.BuilderResult;
            builderParameter.SaveExpectedVersion();
            int affectedCount;

            switch (builderParameter.CommandExecutionType)
            {
                case CommandExecutionType.ExecuteNonQuery:
                    affectedCount =
                        await ConsumeHelper.ConsumeNonQueryAsync(connection, builderParameter, builderResult, transaction, cancellationToken);
                    break;
                case CommandExecutionType.ExecuteReader:
                    affectedCount =
                        await ConsumeHelper.ConsumeReaderAsync(connection, builderParameter, builderResult, transaction);
                    break;
                case CommandExecutionType.ExecuteScalar:
                    affectedCount =
                        await ConsumeHelper.ConsumeScalarAsync(connection, builderParameter, builderResult, transaction);
                    break;
                default:
                    // TODO: error
                    throw new InvalidOperationException("");
            }

            ThrowIfOptimisticLockException(builderParameter, affectedCount, builderResult);
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
            internal readonly string Sql;
            internal readonly DynamicParameters Parameters;
            internal readonly QueryBuilderParameter BuilderParameter;
            internal readonly QueryBuilderResult BuilderResult;

            internal SqlItem(string sql, DynamicParameters parameters,
                    QueryBuilderParameter builderParameter,
                    QueryBuilderResult builderResult)
            {
                Sql = sql;
                Parameters = parameters;
                BuilderParameter = builderParameter;
                BuilderResult = builderResult;
            }
        }

    }
}
