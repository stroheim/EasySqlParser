using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace EasySqlParser.EntityFrameworkCore.Extensions
{
    public static class DatabaseFacadeExtension
    {
        // https://devadjust.exblog.jp/28241806/
        /// <summary>
        /// 1つの値を取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T ExecuteScalarSqlRaw<T>(this DatabaseFacade database, string sql, params IDbDataParameter[] parameters)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }
            var concurrentDetector = facadeDependencies.ConcurrencyDetector;
            var logger = facadeDependencies.CommandLogger;
            using (concurrentDetector.EnterCriticalSection())
            {
                var rawSqlCommand = facadeDependencies.RawSqlCommandBuilder
                    .Build(sql, parameters);
                var rawScalar = rawSqlCommand.RelationalCommand
                    .ExecuteScalar(new RelationalCommandParameterObject(
                        facadeDependencies.RelationalConnection,
                        rawSqlCommand.ParameterValues,
                        null,
                        facade.Context,
                        logger));
                if (rawScalar is T scalar)
                {
                    return scalar;
                }
                // TODO: type error
                throw new InvalidOperationException("");
            }
        }

        /// <summary>
        /// 1つの値を取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <param name="sql"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<T> ExecuteScalarSqlRawAsync<T>(this DatabaseFacade database, string sql, CancellationToken cancellationToken = default, params IDbDataParameter[] parameters)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }
            var concurrentDetector = facadeDependencies.ConcurrencyDetector;
            var logger = facadeDependencies.CommandLogger;
            using (concurrentDetector.EnterCriticalSection())
            {
                var rawSqlCommand = facadeDependencies.RawSqlCommandBuilder
                    .Build(sql, parameters);
                var rawScalar = await rawSqlCommand.RelationalCommand
                    .ExecuteScalarAsync(new RelationalCommandParameterObject(
                            facadeDependencies.RelationalConnection,
                            rawSqlCommand.ParameterValues,
                            null,
                            facade.Context,
                            logger),
                        cancellationToken);
                if (rawScalar is T scalar)
                {
                    return scalar;
                }
                // TODO: type error
                throw new InvalidOperationException("");
            }

        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="database"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static int ExecuteNonQuerySqlRaw(this DatabaseFacade database, string sql,
            params IDbDataParameter[] parameters)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }
            var concurrentDetector = facadeDependencies.ConcurrencyDetector;
            var logger = facadeDependencies.CommandLogger;
            using (concurrentDetector.EnterCriticalSection())
            {
                var rawSqlCommand = facadeDependencies.RawSqlCommandBuilder
                    .Build(sql, parameters);
                return rawSqlCommand.RelationalCommand
                    .ExecuteNonQuery(new RelationalCommandParameterObject(
                        facadeDependencies.RelationalConnection,
                        rawSqlCommand.ParameterValues,
                        null,
                        facade.Context,
                        logger));
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="database"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<int> ExecuteNonQuerySqlRawAsync(this DatabaseFacade database, string sql,
            params IDbDataParameter[] parameters)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }
            var concurrentDetector = facadeDependencies.ConcurrencyDetector;
            var logger = facadeDependencies.CommandLogger;
            using (concurrentDetector.EnterCriticalSection())
            {
                var rawSqlCommand = facadeDependencies.RawSqlCommandBuilder
                    .Build(sql, parameters);
                return await rawSqlCommand.RelationalCommand
                    .ExecuteNonQueryAsync(new RelationalCommandParameterObject(
                        facadeDependencies.RelationalConnection,
                        rawSqlCommand.ParameterValues,
                        null,
                        facade.Context,
                        logger));
            }

        }


        /// <summary>
        /// データ取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static IEnumerable<T> ExecuteReaderSqlRaw<T>(this DatabaseFacade database, string sql,
            params IDbDataParameter[] parameters)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                yield break;
            }
            var concurrentDetector = facadeDependencies.ConcurrencyDetector;
            var logger = facadeDependencies.CommandLogger;
            var maps = Cache<T>.ColumnMaps;

            using (concurrentDetector.EnterCriticalSection())
            {
                var rawSqlCommand = facadeDependencies.RawSqlCommandBuilder
                    .Build(sql, parameters);
                using (var relationalDataReader = rawSqlCommand.RelationalCommand
                    .ExecuteReader(new RelationalCommandParameterObject(
                        facadeDependencies.RelationalConnection,
                        rawSqlCommand.ParameterValues,
                        null,
                        facade.Context,
                        logger
                    )))
                {
                    var reader = relationalDataReader.DbDataReader;
                    if (reader.HasRows)
                    {
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
            }
        }

        /// <summary>
        /// データ取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<T> ExecuteReaderSqlRawAsync<T>(this DatabaseFacade database, string sql,
            params IDbDataParameter[] parameters)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                yield break;
            }
            var concurrentDetector = facadeDependencies.ConcurrencyDetector;
            var logger = facadeDependencies.CommandLogger;
            var maps = Cache<T>.ColumnMaps;

            using (concurrentDetector.EnterCriticalSection())
            {
                var rawSqlCommand = facadeDependencies.RawSqlCommandBuilder
                    .Build(sql, parameters);
                using (var relationalDataReader = await rawSqlCommand.RelationalCommand
                    .ExecuteReaderAsync(new RelationalCommandParameterObject(
                        facadeDependencies.RelationalConnection,
                        rawSqlCommand.ParameterValues,
                        null,
                        facade.Context,
                        logger
                    )))
                {
                    var reader = relationalDataReader.DbDataReader;
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            var instance = Activator.CreateInstance<T>();
                            foreach (var map in maps)
                            {
                                var col = reader.GetOrdinal(map.Value.columnName);
                                if (!await reader.IsDBNullAsync(col))
                                {
                                    map.Value.property.SetValue(instance, reader.GetValue(col));
                                }
                            }

                            yield return instance;

                        }
                    }
                }
            }
        }

        /// <summary>
        /// 1件データを取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T ExecuteReaderSingleSqlRaw<T>(this DatabaseFacade database, string sql,
            params IDbDataParameter[] parameters)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }
            var concurrentDetector = facadeDependencies.ConcurrencyDetector;
            var logger = facadeDependencies.CommandLogger;
            var maps = Cache<T>.ColumnMaps;
            using (concurrentDetector.EnterCriticalSection())
            {
                var rawSqlCommand = facadeDependencies.RawSqlCommandBuilder
                    .Build(sql, parameters);
                using (var relationalDataReader = rawSqlCommand.RelationalCommand
                    .ExecuteReader(new RelationalCommandParameterObject(
                        facadeDependencies.RelationalConnection,
                        rawSqlCommand.ParameterValues,
                        null,
                        facade.Context,
                        logger
                    )))
                {
                    var reader = relationalDataReader.DbDataReader;
                    if (reader.HasRows)
                    {
                        reader.Read();
                        var instance = Activator.CreateInstance<T>();
                        foreach (var map in maps)
                        {
                            var col = reader.GetOrdinal(map.Value.columnName);
                            if (!reader.IsDBNull(col))
                            {
                                map.Value.property.SetValue(instance, reader.GetValue(col));
                            }
                        }

                        return instance;
                    }
                }
                return default;
            }
        }

        /// <summary>
        /// 1件データを取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<T> ExecuteReaderSingleSqlRawAsync<T>(this DatabaseFacade database, string sql,
            params IDbDataParameter[] parameters)
        {
            var facade = database as IDatabaseFacadeDependenciesAccessor;
            var facadeDependencies = facade.Dependencies as IRelationalDatabaseFacadeDependencies;
            if (facadeDependencies == null)
            {
                return default;
            }
            var concurrentDetector = facadeDependencies.ConcurrencyDetector;
            var logger = facadeDependencies.CommandLogger;
            var maps = Cache<T>.ColumnMaps;
            using (concurrentDetector.EnterCriticalSection())
            {
                var rawSqlCommand = facadeDependencies.RawSqlCommandBuilder
                    .Build(sql, parameters);
                using (var relationalDataReader = await rawSqlCommand.RelationalCommand
                    .ExecuteReaderAsync(new RelationalCommandParameterObject(
                        facadeDependencies.RelationalConnection,
                        rawSqlCommand.ParameterValues,
                        null,
                        facade.Context,
                        logger
                    )))
                {
                    var reader = relationalDataReader.DbDataReader;
                    if (reader.HasRows)
                    {
                        reader.Read();
                        var instance = Activator.CreateInstance<T>();
                        foreach (var map in maps)
                        {
                            var col = reader.GetOrdinal(map.Value.columnName);
                            if (!reader.IsDBNull(col))
                            {
                                map.Value.property.SetValue(instance, reader.GetValue(col));
                            }
                        }

                        return instance;
                    }

                }
            }
            return default;
        }

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
