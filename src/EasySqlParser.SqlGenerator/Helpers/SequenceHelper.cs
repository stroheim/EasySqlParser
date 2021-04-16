using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using EasySqlParser.SqlGenerator.Attributes;
using EasySqlParser.SqlGenerator.Enums;

namespace EasySqlParser.SqlGenerator.Helpers
{
    public static class SequenceHelper
    {
        public static void Generate(DbConnection connection, QueryBuilderParameter builderParameter)
        {
            if (builderParameter.SqlKind != SqlKind.Insert) return;
            var entityInfo = builderParameter.EntityTypeInfo;
            var config = builderParameter.Config;
            if (!config.Dialect.SupportsSequence) return;
            if (entityInfo.SequenceColumns.Count == 0) return;
            builderParameter.WriteLog("[Start] Sequence Generate");
            foreach (var columnInfo in entityInfo.SequenceColumns)
            {
                if (columnInfo.PropertyInfo.PropertyType == typeof(byte))
                {
                    if (TryGenerateSequence(connection, builderParameter, columnInfo.SequenceGeneratorAttribute,
                        out byte byteValue, Convert.ToByte))
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, byteValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(short))
                {
                    if (TryGenerateSequence(connection, builderParameter, columnInfo.SequenceGeneratorAttribute,
                        out short shortValue, Convert.ToInt16))
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, shortValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(int))
                {
                    if (TryGenerateSequence(connection, builderParameter, columnInfo.SequenceGeneratorAttribute,
                        out int intValue, Convert.ToInt32))
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, intValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(long))
                {
                    if (TryGenerateSequence(connection, builderParameter, columnInfo.SequenceGeneratorAttribute,
                        out long longValue, Convert.ToInt64))
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, longValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(decimal))
                {
                    if (TryGenerateSequence(connection, builderParameter, columnInfo.SequenceGeneratorAttribute,
                        out decimal decimalValue))
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, decimalValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(string))
                {
                    if (TryGenerateSequence(connection, builderParameter, columnInfo.SequenceGeneratorAttribute,
                        out string stringValue))
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, stringValue);
                        //continue;
                    }
                }
            }
            builderParameter.WriteLog("[End] Sequence Generate");
        }

        public static async Task GenerateAsync(DbConnection connection, QueryBuilderParameter builderParameter)
        {
            if (builderParameter.SqlKind != SqlKind.Insert) return;
            var entityInfo = builderParameter.EntityTypeInfo;
            var config = builderParameter.Config;
            if (!config.Dialect.SupportsSequence) return;
            if (entityInfo.SequenceColumns.Count == 0) return;
            builderParameter.WriteLog("[Start] Sequence GenerateAsync");
            foreach (var columnInfo in entityInfo.SequenceColumns)
            {
                if (columnInfo.PropertyInfo.PropertyType == typeof(byte))
                {
                    var (isSuccess, byteValue) = await TryGenerateSequenceAsync(connection, builderParameter,
                        columnInfo.SequenceGeneratorAttribute, converter: Convert.ToByte)
                        .ConfigureAwait(false);
                    if (isSuccess)
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, byteValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(short))
                {
                    var (isSuccess, shortValue) = await TryGenerateSequenceAsync(connection, builderParameter,
                        columnInfo.SequenceGeneratorAttribute, converter: Convert.ToInt16)
                        .ConfigureAwait(false);
                    if (isSuccess)
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, shortValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(int))
                {
                    var (isSuccess, intValue) = await TryGenerateSequenceAsync(connection, builderParameter,
                        columnInfo.SequenceGeneratorAttribute, converter: Convert.ToInt32)
                        .ConfigureAwait(false);

                    if (isSuccess)
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, intValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(long))
                {
                    var (isSuccess, longValue) = await TryGenerateSequenceAsync(connection, builderParameter,
                        columnInfo.SequenceGeneratorAttribute, converter: Convert.ToInt64)
                        .ConfigureAwait(false);
                    if (isSuccess)
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, longValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(decimal))
                {
                    var (isSuccess, decimalValue) = await TryGenerateSequenceAsync<decimal>(connection, builderParameter,
                        columnInfo.SequenceGeneratorAttribute)
                        .ConfigureAwait(false);
                    if (isSuccess)
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, decimalValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(string))
                {
                    var (isSuccess, stringValue) = await TryGenerateSequenceAsync<string>(connection, builderParameter,
                        columnInfo.SequenceGeneratorAttribute)
                        .ConfigureAwait(false);
                    if (isSuccess)
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, stringValue);
                        //continue;
                    }
                }
            }
            builderParameter.WriteLog("[End] Sequence GenerateAsync");
        }


        private static bool TryGenerateSequence<TResult>(DbConnection connection,
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

        private static async Task<(bool isSuccess, TResult sequenceValue)> TryGenerateSequenceAsync<TResult>(
            DbConnection connection,
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
                rawResult = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
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
    }
}
