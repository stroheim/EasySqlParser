using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EasySqlParser.Configurations;

namespace EasySqlParser.SqlGenerator
{
    public static class DbCommandHelper
    {
        public static int ConsumeScalar(object scalarValue, QueryBuilderParameter builderParameter)
        {
            if (scalarValue == null)
            {
                return 0;
            }

            var entityInfo = builderParameter.EntityTypeInfo;
            var instance = builderParameter.Entity;
            var config = builderParameter.Config;
            if (config.DbConnectionKind == DbConnectionKind.SQLite)
            {
                if (entityInfo.IdentityColumn != null && scalarValue is long longValue)
                {
                    var converted = Convert.ChangeType(longValue,
                        entityInfo.IdentityColumn.PropertyInfo.PropertyType);
                    entityInfo.IdentityColumn.PropertyInfo.SetValue(instance, converted);
                    if (!builderParameter.IsSameVersion())
                    {
                        return 0;
                    }
                    return 1;
                }
            }

            entityInfo.IdentityColumn?.PropertyInfo.SetValue(instance, scalarValue);


            if (!builderParameter.IsSameVersion())
            {
                return 0;
            }
            return 1;

        }

        public static int ConsumeReader(DbDataReader reader, QueryBuilderParameter builderParameter)
        {
            if (!reader.HasRows)
            {
                reader.Close();
                reader.Dispose();
                return 0;
            }

            var entityInfo = builderParameter.EntityTypeInfo;
            reader.Read();
            var instance = builderParameter.Entity;
            builderParameter.WriteLog("[Start] ConsumeReader");
            var config = builderParameter.Config;
            foreach (var columnInfo in entityInfo.Columns)
            {
                var col = reader.GetOrdinal(columnInfo.ColumnName);
                if (!reader.IsDBNull(col))
                {
                    var value = reader.GetValue(col);
                    builderParameter.WriteLog($"{columnInfo.PropertyInfo.Name}\t{value}");
                    if (config.DbConnectionKind == DbConnectionKind.SQLite)
                    {
                        if (value is long longValue)
                        {
                            var converted = Convert.ChangeType(longValue, columnInfo.PropertyInfo.PropertyType);
                            columnInfo.PropertyInfo.SetValue(instance, converted);
                            continue;
                        }

                        if (value is string stringValue)
                        {
                            if (columnInfo.PropertyInfo.PropertyType == typeof(DateTime) ||
                                columnInfo.PropertyInfo.PropertyType == typeof(DateTime?))
                            {
                                columnInfo.PropertyInfo.SetValue(instance,
                                    DateTime.ParseExact(stringValue, "yyyy-MM-dd HH:mm:ss.FFFFFFF",
                                        CultureInfo.InvariantCulture));
                                continue;
                            }
                            if (columnInfo.PropertyInfo.PropertyType == typeof(DateTimeOffset) ||
                                     columnInfo.PropertyInfo.PropertyType == typeof(DateTimeOffset?))
                            {
                                columnInfo.PropertyInfo.SetValue(instance,
                                    DateTimeOffset.ParseExact(stringValue, "yyyy-MM-dd HH:mm:ss.FFFFFFFzzz",
                                        CultureInfo.InvariantCulture));
                                continue;
                            }

                        }
                    }
                    columnInfo.PropertyInfo.SetValue(instance, value);
                }
            }
            reader.Close();
            reader.Dispose();
            builderParameter.WriteLog("[End] ConsumeReader");
            if (!builderParameter.IsSameVersion())
            {
                return 0;
            }
            return 1;

        }

        public static async Task<int> ConsumeReaderAsync(DbDataReader reader, QueryBuilderParameter builderParameter)
        {
            if (!reader.HasRows)
            {
                reader.Close();
                reader.Dispose();
                return 0;
            }

            var entityInfo = builderParameter.EntityTypeInfo;
            await reader.ReadAsync();
            var instance = builderParameter.Entity;
            builderParameter.WriteLog("[Start] ConsumeReader");
            var config = builderParameter.Config;
            foreach (var columnInfo in entityInfo.Columns)
            {
                var col = reader.GetOrdinal(columnInfo.ColumnName);
                if (!await reader.IsDBNullAsync(col))
                {
                    var value = reader.GetValue(col);
                    builderParameter.WriteLog($"{columnInfo.PropertyInfo.Name}\t{value}");
                    if (config.DbConnectionKind == DbConnectionKind.SQLite)
                    {
                        if (value is long longValue)
                        {
                            var converted = Convert.ChangeType(longValue, columnInfo.PropertyInfo.PropertyType);
                            columnInfo.PropertyInfo.SetValue(instance, converted);
                            continue;
                        }

                        if (value is string stringValue)
                        {
                            if (columnInfo.PropertyInfo.PropertyType == typeof(DateTime) ||
                                columnInfo.PropertyInfo.PropertyType == typeof(DateTime?))
                            {
                                columnInfo.PropertyInfo.SetValue(instance,
                                    DateTime.ParseExact(stringValue, "yyyy-MM-dd HH:mm:ss.FFFFFFF",
                                        CultureInfo.InvariantCulture));
                                continue;
                            }
                            if (columnInfo.PropertyInfo.PropertyType == typeof(DateTimeOffset) ||
                                     columnInfo.PropertyInfo.PropertyType == typeof(DateTimeOffset?))
                            {
                                columnInfo.PropertyInfo.SetValue(instance,
                                    DateTimeOffset.ParseExact(stringValue, "yyyy-MM-dd HH:mm:ss.FFFFFFFzzz",
                                        CultureInfo.InvariantCulture));
                                continue;
                            }

                        }
                    }
                    columnInfo.PropertyInfo.SetValue(instance, value);
                }
            }
            reader.Close();
            reader.Dispose();
            builderParameter.WriteLog("[End] ConsumeReader");
            if (!builderParameter.IsSameVersion())
            {
                return 0;
            }
            return 1;

        }
    }

    internal static class CommandExtension
    {
        internal static int ConsumeScalar(this DbCommand command, QueryBuilderParameter builderParameter)
        {
            var rawScalarValue = command.ExecuteScalar();
            return DbCommandHelper.ConsumeScalar(rawScalarValue, builderParameter);
        }

        internal static int ConsumeNonQuery(this DbCommand command, QueryBuilderParameter builderParameter)
        {
            var affectedCount = command.ExecuteNonQuery();
            builderParameter.ApplyReturningColumns();
            return affectedCount;

        }

        internal static int ConsumeReader(this DbCommand command, QueryBuilderParameter builderParameter)
        {
            var reader = command.ExecuteReader();
            return DbCommandHelper.ConsumeReader(reader, builderParameter);
        }


        internal static async Task<int> ConsumeScalarAsync(
            this DbCommand command,
            QueryBuilderParameter builderParameter,
            CancellationToken cancellationToken = default)
        {
            var rawScalarValue = await command.ExecuteScalarAsync(cancellationToken);
            if (rawScalarValue == null)
            {
                return 0;
            }

            var entityInfo = builderParameter.EntityTypeInfo;
            var instance = builderParameter.Entity;
            var config = builderParameter.Config;
            if (config.DbConnectionKind == DbConnectionKind.SQLite)
            {
                if (entityInfo.IdentityColumn != null && rawScalarValue is long longValue)
                {
                    var converted = Convert.ChangeType(longValue,
                        entityInfo.IdentityColumn.PropertyInfo.PropertyType);
                    entityInfo.IdentityColumn.PropertyInfo.SetValue(instance, converted);
                    return 1;

                }
            }
            entityInfo.IdentityColumn?.PropertyInfo.SetValue(instance, rawScalarValue);


            return 1;
        }

        internal static async Task<int> ConsumeNonQueryAsync(
            this DbCommand command,
            QueryBuilderParameter builderParameter,
            CancellationToken cancellationToken = default)

        {
            var affectedCount = await command.ExecuteNonQueryAsync(cancellationToken);
            builderParameter.ApplyReturningColumns();
            return affectedCount;
        }

        internal static async Task<int> ConsumeReaderAsync(
            this DbCommand command,
            QueryBuilderParameter builderParameter,
            CancellationToken cancellationToken = default)
        {
            var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!reader.HasRows)
            {
                reader.Close();
                reader.Dispose();
                return 0;
            }

            var entityInfo = builderParameter.EntityTypeInfo;
            await reader.ReadAsync(cancellationToken);
            var instance = builderParameter.Entity;
            builderParameter.WriteLog("[Start] ConsumeReader");
            var config = builderParameter.Config;
            foreach (var columnInfo in entityInfo.Columns)
            {
                var col = reader.GetOrdinal(columnInfo.ColumnName);
                if (await reader.IsDBNullAsync(col, cancellationToken)) continue;
                var value = reader.GetValue(col);
                builderParameter.WriteLog($"{columnInfo.PropertyInfo.Name}\t{value}");
                if (config.DbConnectionKind == DbConnectionKind.SQLite)
                {
                    if (value is long longValue)
                    {
                        var converted = Convert.ChangeType(longValue, columnInfo.PropertyInfo.PropertyType);
                        columnInfo.PropertyInfo.SetValue(instance, converted);
                        continue;
                    }

                    if (value is string stringValue)
                    {
                        if (columnInfo.PropertyInfo.PropertyType == typeof(DateTime) ||
                            columnInfo.PropertyInfo.PropertyType == typeof(DateTime?))
                        {
                            columnInfo.PropertyInfo.SetValue(instance,
                                DateTime.ParseExact(stringValue, "yyyy-MM-dd HH:mm:ss.FFFFFFF",
                                    CultureInfo.InvariantCulture));
                            continue;
                        }

                        if (columnInfo.PropertyInfo.PropertyType == typeof(DateTimeOffset) ||
                            columnInfo.PropertyInfo.PropertyType == typeof(DateTimeOffset?))
                        {
                            columnInfo.PropertyInfo.SetValue(instance,
                                DateTimeOffset.ParseExact(stringValue, "yyyy-MM-dd HH:mm:ss.FFFFFFFzzz",
                                    CultureInfo.InvariantCulture));
                            continue;
                        }

                    }
                }

                columnInfo.PropertyInfo.SetValue(instance, value);
            }

            reader.Close();
            reader.Dispose();
            builderParameter.WriteLog("[End] ConsumeReader");
            return 1;
        }

    }

    internal static class ConnectionExtension
    {
        internal static void PreInsert(this DbConnection connection, QueryBuilderParameter builderParameter)
        {
            if (builderParameter.SqlKind != SqlKind.Insert) return;
            var entityInfo = builderParameter.EntityTypeInfo;
            var config = builderParameter.Config;
            if (!config.Dialect.SupportsSequence) return;
            if (entityInfo.SequenceColumns.Count == 0) return;
            builderParameter.WriteLog("[Start] PreInsert");
            foreach (var columnInfo in entityInfo.SequenceColumns)
            {
                if (columnInfo.PropertyInfo.PropertyType == typeof(byte))
                {
                    if (connection.TryGenerateSequence(builderParameter, columnInfo.SequenceGeneratorAttribute,
                        out byte byteValue, Convert.ToByte))
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, byteValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(short))
                {
                    if (connection.TryGenerateSequence(builderParameter, columnInfo.SequenceGeneratorAttribute,
                        out short shortValue, Convert.ToInt16))
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, shortValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(int))
                {
                    if (connection.TryGenerateSequence(builderParameter, columnInfo.SequenceGeneratorAttribute,
                        out int intValue, Convert.ToInt32))
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, intValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(long))
                {
                    if (connection.TryGenerateSequence(builderParameter, columnInfo.SequenceGeneratorAttribute,
                        out long longValue, Convert.ToInt64))
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, longValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(decimal))
                {
                    if (connection.TryGenerateSequence(builderParameter, columnInfo.SequenceGeneratorAttribute,
                        out decimal decimalValue))
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, decimalValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(string))
                {
                    if (connection.TryGenerateSequence(builderParameter, columnInfo.SequenceGeneratorAttribute,
                        out string stringValue))
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, stringValue);
                        continue;
                    }
                }
            }
            builderParameter.WriteLog("[End] PreInsert");
        }

        internal static async Task PreInsertAsync(this DbConnection connection, QueryBuilderParameter builderParameter)
        {
            if (builderParameter.SqlKind != SqlKind.Insert) return;
            var entityInfo = builderParameter.EntityTypeInfo;
            var config = builderParameter.Config;
            if (!config.Dialect.SupportsSequence) return;
            if (entityInfo.SequenceColumns.Count == 0) return;
            builderParameter.WriteLog("[Start] PreInsertAsync");
            foreach (var columnInfo in entityInfo.SequenceColumns)
            {
                if (columnInfo.PropertyInfo.PropertyType == typeof(byte))
                {
                    var (isSuccess, byteValue) = await connection.TryGenerateSequenceAsync(builderParameter,
                        columnInfo.SequenceGeneratorAttribute, converter: Convert.ToByte);
                    if (isSuccess)
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, byteValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(short))
                {
                    var (isSuccess, shortValue) = await connection.TryGenerateSequenceAsync(builderParameter,
                        columnInfo.SequenceGeneratorAttribute, converter: Convert.ToInt16);
                    if (isSuccess)
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, shortValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(int))
                {
                    var (isSuccess, intValue) = await connection.TryGenerateSequenceAsync(builderParameter,
                        columnInfo.SequenceGeneratorAttribute, converter: Convert.ToInt32);

                    if (isSuccess)
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, intValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(long))
                {
                    var (isSuccess, longValue) = await connection.TryGenerateSequenceAsync(builderParameter,
                        columnInfo.SequenceGeneratorAttribute, converter: Convert.ToInt64);
                    if (isSuccess)
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, longValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(decimal))
                {
                    var (isSuccess, decimalValue) = await connection.TryGenerateSequenceAsync<decimal>(builderParameter,
                        columnInfo.SequenceGeneratorAttribute);
                    if (isSuccess)
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, decimalValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(string))
                {
                    var (isSuccess, stringValue) = await connection.TryGenerateSequenceAsync<string>(builderParameter,
                        columnInfo.SequenceGeneratorAttribute);
                    if (isSuccess)
                    {
                        columnInfo.PropertyInfo.SetValue(builderParameter.Entity, stringValue);
                        continue;
                    }
                }
            }
            builderParameter.WriteLog("[End] PreInsertAsync");

        }



    }

    public class QueryBuilder
    {
        #region for unit tests
        internal static QueryBuilderResult GetCountSql<T>(
            Expression<Func<T, bool>> predicate = null,
            string configName = null,
            bool writeIndented = false)
        {
            var keyValues = new Dictionary<string, object>();
            if (predicate != null)
            {
                var visitor = new PredicateVisitor();
                keyValues = visitor.GetKeyValues(predicate);
            }
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(T));
            var config = configName == null
                ? ConfigContainer.DefaultConfig
                : ConfigContainer.AdditionalConfigs[configName];
            var builder = new QueryStringBuilder(config, writeIndented);
            builder.AppendSql("SELECT COUNT(*) CNT FROM ");
            if (!string.IsNullOrEmpty(entityInfo.SchemaName))
            {
                builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.SchemaName));
                builder.AppendSql(".");
            }

            builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.TableName));
            if (keyValues.Count > 0)
            {
                builder.AppendLine(" WHERE ");

                var counter = 0;
                foreach (var columnInfo in entityInfo.Columns)
                {
                    builder.AppendComma(counter);
                    object propValue = null;
                    if (keyValues.ContainsKey(columnInfo.PropertyInfo.Name))
                    {
                        propValue = keyValues[columnInfo.PropertyInfo.Name];
                    }

                    var columnName = config.Dialect.ApplyQuote(columnInfo.ColumnName);
                    builder.AppendSql(columnName);
                    if (propValue == null)
                    {
                        builder.AppendSql(" IS NULL ");
                    }
                    else
                    {
                        builder.AppendSql(" = ");
                        builder.AppendParameter(columnInfo.PropertyInfo, propValue);
                    }

                    builder.AppendLine();

                    counter++;
                }

            }

            return builder.GetResult();
        }


        internal static (QueryBuilderResult builderResult, EntityTypeInfo entityInfo) GetSelectSql<T>(
            Expression<Func<T, bool>> predicate,
            string configName = null,
            bool writeIndented = false)
        {
            var visitor = new PredicateVisitor();
            var keyValues = visitor.GetKeyValues(predicate);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(T));
            var config = configName == null
                ? ConfigContainer.DefaultConfig
                : ConfigContainer.AdditionalConfigs[configName];

            if (entityInfo.KeyColumns.Count == 0)
            {
                // pkがなければupdateできない
                throw new InvalidOperationException("");
            }

            var builder = new QueryStringBuilder(config, writeIndented);
            builder.AppendLine("SELECT ");
            var counter = 0;
            foreach (var columnInfo in entityInfo.Columns)
            {
                builder.AppendComma(counter);
                builder.AppendLine(config.Dialect.ApplyQuote(columnInfo.ColumnName));

                counter++;

            }

            if (!writeIndented)
            {
                builder.AppendSql(" ");
            }
            builder.AppendLine("FROM ");
            if (writeIndented)
            {
                builder.AppendIndent(3);
            }
            if (!string.IsNullOrEmpty(entityInfo.SchemaName))
            {
                builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.SchemaName));
                builder.AppendSql(".");
            }

            builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.TableName));
            builder.AppendLine(" ");
            builder.AppendLine("WHERE ");
            counter = 0;
            for (int i = 0; i < entityInfo.Columns.Count; i++)
            {
                var columnInfo = entityInfo.Columns[i];
                if (!columnInfo.IsPrimaryKey) continue;
                builder.AppendComma(counter);
                object propValue = null;
                if (keyValues.ContainsKey(columnInfo.PropertyInfo.Name))
                {
                    propValue = keyValues[columnInfo.PropertyInfo.Name];
                }

                var columnName = config.Dialect.ApplyQuote(columnInfo.ColumnName);
                builder.AppendSql(columnName);
                if (propValue == null)
                {
                    builder.AppendSql(" IS NULL ");
                }
                else
                {
                    builder.AppendSql(" = ");
                    builder.AppendParameter(columnInfo.PropertyInfo, propValue);
                }

                if (i < entityInfo.Columns.Count - 1)
                {
                    builder.AppendLine();
                }

                counter++;
            }

            return (builder.GetResult(), entityInfo);

        }
        #endregion



        public static QueryBuilderResult GetQueryBuilderResultFromSqlFile(QueryBuilderParameter parameter)
        {
            var parser = new SqlParser(parameter.SqlFile, parameter.Entity, parameter.Config);
            var parserResult = parser.Parse();
            return new QueryBuilderResult
                   {
                       ParsedSql = parserResult.ParsedSql,
                       DebugSql = parserResult.DebugSql,
                       DbDataParameters = parserResult.DbDataParameters
                   };
        }

        public static QueryBuilderResult GetQueryBuilderResult(QueryBuilderParameter parameter)
        {
            if (!string.IsNullOrEmpty(parameter.SqlFile))
            {
                return GetQueryBuilderResultFromSqlFile(parameter);
            }
            switch (parameter.SqlKind)
            {
                case SqlKind.Insert:
                    return GetInsertSql(parameter);
                case SqlKind.Update:
                    return GetUpdateSql(parameter);
                case SqlKind.SoftDelete:
                    return GetSoftDeleteSql(parameter);
                case SqlKind.Delete:
                    return GetDeleteSql(parameter);
                default:
                    // TODO:
                    throw new InvalidOperationException("");
            }

        }

        private static QueryBuilderResult GetInsertSql(QueryBuilderParameter parameter)
        {
            var entityInfo = parameter.EntityTypeInfo;
            var config = parameter.Config;
            var builder = new QueryStringBuilder(config, parameter.WriteIndented);
            AppendFinalTableSelectCommandHeader(builder, entityInfo, parameter);
            builder.AppendSql("INSERT INTO ");
            if (!string.IsNullOrEmpty(entityInfo.SchemaName))
            {
                builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.SchemaName));
                builder.AppendSql(".");
            }
            builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.TableName));
            builder.AppendLine(" (");
            var counter = 0;
            foreach (var columnInfo in entityInfo.Columns)
            {
                if (parameter.ExcludeNull)
                {
                    var propValue = columnInfo.PropertyInfo.GetValue(parameter.Entity);
                    if (propValue == null) continue;
                }

                if (columnInfo.IsIdentity)
                {
                    continue;
                }

                if (columnInfo.IsCurrentTimestamp)
                {
                    if (!builder.IncludeCurrentTimestampColumn(parameter, columnInfo))
                    {
                        continue;
                    }
                }

                if (columnInfo.IsCurrentUser)
                {
                    if (!builder.IncludeCurrentUserColumn(parameter, columnInfo))
                    {
                        continue;
                    }
                }
                builder.AppendComma(counter);

                builder.AppendLine(config.Dialect.ApplyQuote(columnInfo.ColumnName));

                counter++;
            }
            builder.AppendLine(") VALUES (");
            counter = 0;
            foreach (var columnInfo in entityInfo.Columns)
            {
                if (columnInfo.IsIdentity)
                {
                    continue;
                }

                if (columnInfo.IsSoftDeleteKey)
                {
                    builder.AppendSoftDeleteKey(parameter, columnInfo, counter);
                    counter++;
                    continue;
                }

                if (columnInfo.IsCurrentTimestamp)
                {
                    if (!builder.IncludeCurrentTimestampColumn(parameter, columnInfo))
                    {
                        continue;
                    }

                    builder.AppendCurrentTimestamp(parameter, columnInfo, counter);
                    counter++;
                    continue;

                }

                if (columnInfo.IsCurrentUser)
                {
                    if (!builder.IncludeCurrentUserColumn(parameter, columnInfo))
                    {
                        continue;
                    }
                    builder.AppendCurrentUser(parameter, columnInfo, counter);
                    counter++;
                    continue;
                }
                var propValue = columnInfo.PropertyInfo.GetValue(parameter.Entity);
                if (parameter.ExcludeNull && propValue == null) continue;
                builder.AppendComma(counter);
                if (columnInfo.IsVersion)
                {
                    propValue = builder.GetDefaultVersionNo(propValue, columnInfo.PropertyInfo.PropertyType);
                }
                builder.AppendParameter(columnInfo.PropertyInfo, propValue);
                builder.AppendLine();
                counter++;
            }
            builder.AppendSql(")");
            AppendFinalTableSelectCommandTerminator(builder, parameter);
            AppendSelectAffectedCommandHeader(builder, entityInfo, parameter);
            AppendFromClause(builder, entityInfo, parameter);
            return builder.GetResult();

        }

        private static QueryBuilderResult GetSoftDeleteSql(QueryBuilderParameter parameter)
        {
            var entityInfo = parameter.EntityTypeInfo;
            if (!entityInfo.HasSoftDeleteKey)
            {
                return GetUpdateSql(parameter);
            }
            var config = parameter.Config;
            if (entityInfo.KeyColumns.Count == 0)
            {
                // pkがなければupdateできない
                throw new InvalidOperationException("");
            }
            var builder = new QueryStringBuilder(config, parameter.WriteIndented);
            AppendFinalTableSelectCommandHeader(builder, entityInfo, parameter);
            builder.AppendSql("UPDATE ");
            if (!string.IsNullOrEmpty(entityInfo.SchemaName))
            {
                builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.SchemaName));
                builder.AppendSql(".");
            }
            builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.TableName));
            builder.AppendLine(" SET ");
            var counter = 0;
            foreach (var columnInfo in entityInfo.Columns)
            {
                if (columnInfo.IsPrimaryKey) continue;
                if (columnInfo.IsSoftDeleteKey)
                {
                    builder.AppendSoftDeleteKey(parameter, columnInfo, counter);
                    counter++;
                    continue;
                }

                if (columnInfo.IsCurrentTimestamp)
                {
                    if (!builder.IncludeCurrentTimestampColumn(parameter, columnInfo))
                    {
                        continue;
                    }

                    builder.AppendCurrentTimestamp(parameter, columnInfo, counter);
                    counter++;
                    continue;
                }

                if (columnInfo.IsCurrentUser)
                {
                    if (!builder.IncludeCurrentUserColumn(parameter, columnInfo))
                    {
                        continue;
                    }
                    builder.AppendCurrentUser(parameter, columnInfo, counter);
                    counter++;
                    continue;
                }

                if (columnInfo.IsVersion)
                {
                    builder.AppendComma(counter);
                    var propValue = columnInfo.PropertyInfo.GetValue(parameter.Entity);

                    var columnName = config.Dialect.ApplyQuote(columnInfo.ColumnName);
                    builder.AppendSql(columnName);
                    builder.AppendSql(" = ");
                    builder.AppendParameter(columnInfo.PropertyInfo, propValue);
                    builder.AppendVersion(parameter, columnInfo);

                    builder.AppendLine();

                }
            }

            builder.AppendLine("WHERE ");
            counter = 0;
            for (int i = 0; i < entityInfo.Columns.Count; i++)
            {
                var columnInfo = entityInfo.Columns[i];
                if (parameter.IgnoreVersion && columnInfo.IsVersion) continue;
                if (!columnInfo.IsPrimaryKey && !columnInfo.IsVersion) continue;
                builder.AppendAnd(counter);
                var propValue = columnInfo.PropertyInfo.GetValue(parameter.Entity);
                var columnName = config.Dialect.ApplyQuote(columnInfo.ColumnName);
                builder.AppendSql(columnName);
                if (propValue == null)
                {
                    builder.AppendSql(" IS NULL ");
                }
                else
                {
                    builder.AppendSql(" = ");
                    builder.AppendParameter(columnInfo.PropertyInfo, propValue);
                }

                if (i < entityInfo.Columns.Count - 1)
                {
                    builder.AppendLine();
                }

                counter++;
            }


            AppendFinalTableSelectCommandTerminator(builder, parameter);
            AppendSelectAffectedCommandHeader(builder, entityInfo, parameter);
            AppendFromClause(builder, entityInfo, parameter);

            return builder.GetResult();

        }

        private static QueryBuilderResult GetUpdateSql(QueryBuilderParameter parameter)
        {
            var entityInfo = parameter.EntityTypeInfo;
            var config = parameter.Config;
            if (entityInfo.KeyColumns.Count == 0)
            {
                // pkがなければupdateできない
                throw new InvalidOperationException("");
            }

            var builder = new QueryStringBuilder(config, parameter.WriteIndented);
            AppendFinalTableSelectCommandHeader(builder, entityInfo, parameter);
            builder.AppendSql("UPDATE ");
            if (!string.IsNullOrEmpty(entityInfo.SchemaName))
            {
                builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.SchemaName));
                builder.AppendSql(".");
            }
            builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.TableName));
            builder.AppendLine(" SET ");
            var counter = 0;
            foreach (var columnInfo in entityInfo.Columns)
            {
                if (columnInfo.IsPrimaryKey) continue;
                var propValue = columnInfo.PropertyInfo.GetValue(parameter.Entity);
                if (parameter.ExcludeNull && propValue == null)
                {
                    continue;
                }

                if (columnInfo.IsSoftDeleteKey)
                {
                    builder.AppendSoftDeleteKey(parameter, columnInfo, counter);
                    counter++;
                    continue;
                }

                if (columnInfo.IsCurrentTimestamp)
                {
                    if (!builder.IncludeCurrentTimestampColumn(parameter, columnInfo))
                    {
                        continue;
                    }

                    builder.AppendCurrentTimestamp(parameter, columnInfo, counter);
                    counter++;
                    continue;
                }

                if (columnInfo.IsCurrentUser)
                {
                    if (!builder.IncludeCurrentUserColumn(parameter, columnInfo))
                    {
                        continue;
                    }
                    builder.AppendCurrentUser(parameter, columnInfo, counter);
                    counter++;
                    continue;
                }

                builder.AppendComma(counter);

                var columnName = config.Dialect.ApplyQuote(columnInfo.ColumnName);
                builder.AppendSql(columnName);
                builder.AppendSql(" = ");
                builder.AppendParameter(columnInfo.PropertyInfo, propValue);
                builder.AppendVersion(parameter, columnInfo);

                builder.AppendLine();
                counter++;
            }

            builder.AppendLine("WHERE ");
            counter = 0;
            for (int i = 0; i < entityInfo.Columns.Count; i++)
            {
                var columnInfo = entityInfo.Columns[i];
                if (parameter.IgnoreVersion && columnInfo.IsVersion) continue;
                if (!columnInfo.IsPrimaryKey && !columnInfo.IsVersion) continue;
                builder.AppendAnd(counter);
                var propValue = columnInfo.PropertyInfo.GetValue(parameter.Entity);
                var columnName = config.Dialect.ApplyQuote(columnInfo.ColumnName);
                builder.AppendSql(columnName);
                if (propValue == null)
                {
                    builder.AppendSql(" IS NULL ");
                }
                else
                {
                    builder.AppendSql(" = ");
                    builder.AppendParameter(columnInfo.PropertyInfo, propValue);
                }

                if (i < entityInfo.Columns.Count - 1)
                {
                    builder.AppendLine();
                }

                counter++;
            }

            AppendFinalTableSelectCommandTerminator(builder, parameter);
            AppendSelectAffectedCommandHeader(builder, entityInfo, parameter);
            AppendFromClause(builder, entityInfo, parameter);

            return builder.GetResult();
        }

        private static QueryBuilderResult GetDeleteSql(QueryBuilderParameter parameter)
        {
            var entityInfo = parameter.EntityTypeInfo;
            var config = parameter.Config;
            if (entityInfo.KeyColumns.Count == 0)
            {
                // pkがなければupdateできない
                throw new InvalidOperationException("");
            }

            var builder = new QueryStringBuilder(config, parameter.WriteIndented);
            builder.AppendSql("DELETE FROM ");
            if (!string.IsNullOrEmpty(entityInfo.SchemaName))
            {
                builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.SchemaName));
                builder.AppendSql(".");
            }
            builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.TableName));
            builder.AppendLine(" WHERE ");
            var counter = 0;
            foreach (var columnInfo in entityInfo.Columns)
            {
                if (parameter.IgnoreVersion && columnInfo.IsVersion) continue;
                if (!columnInfo.IsPrimaryKey && !columnInfo.IsVersion) continue;
                builder.AppendAnd(counter);
                var propValue = columnInfo.PropertyInfo.GetValue(parameter.Entity);
                var columnName = config.Dialect.ApplyQuote(columnInfo.ColumnName);
                builder.AppendSql(columnName);
                builder.AppendSql(" = ");
                builder.AppendParameter(columnInfo.PropertyInfo, propValue);
                builder.AppendLine();


                counter++;
            }

            return builder.GetResult();
        }


        private static void AppendSelectAffectedCommandHeader(
            QueryStringBuilder builder, EntityTypeInfo entityInfo,
            QueryBuilderParameter parameter,
            bool callFromFinalTable = false)
        {
            if (parameter.QueryBehavior == QueryBehavior.None) return;
            if (parameter.SqlKind == SqlKind.Delete) return;
            var config = parameter.Config;
            if (config.Dialect.SupportsFinalTable && !callFromFinalTable) return;
            if (config.Dialect.SupportsReturningInto || config.Dialect.SupportsReturning)
            {
                builder.AppendLine(" ");
                builder.AppendLine("RETURNING ");
            }
            else
            {
                if (!config.Dialect.SupportsFinalTable)
                {
                    builder.ForceAppendLine(config.Dialect.StatementTerminator);
                }
                builder.AppendLine("SELECT ");
            }

            if (parameter.QueryBehavior == QueryBehavior.IdentityOnly)
            {
                if (entityInfo.IdentityColumn == null)
                {
                    throw new InvalidOperationException("entity has no identity column");
                }
            }

            if ((parameter.QueryBehavior == QueryBehavior.IdentityOnly ||
                 parameter.QueryBehavior == QueryBehavior.IdentityOrAllColumns) &&
                entityInfo.IdentityColumn != null)
            {
                if (config.Dialect.SupportsReturningInto)
                {
                    builder.AppendComma(0);
                    builder.AppendLine(config.Dialect.ApplyQuote(entityInfo.IdentityColumn.ColumnName));
                    if (!parameter.WriteIndented)
                    {
                        builder.AppendSql(" ");
                    }
                    builder.AppendLine("INTO ");
                }

                builder.AppendComma(0);
                if (config.Dialect.SupportsFinalTable)
                {
                    builder.AppendSql("t_.");
                }

                if (config.Dialect.SupportsReturningInto)
                {
                    builder.AppendReturnParameter(parameter, entityInfo.IdentityColumn);
                }
                else
                {
                    builder.AppendLine(config.Dialect.ApplyQuote(entityInfo.IdentityColumn.ColumnName));
                }
                return;

            }

            var counter = 0;
            foreach (var columnInfo in entityInfo.Columns)
            {
                builder.AppendComma(counter);
                if (config.Dialect.SupportsFinalTable)
                {
                    builder.AppendSql("t_.");
                }

                builder.AppendSql(config.Dialect.ApplyQuote(columnInfo.ColumnName));
                if (parameter.WriteIndented)
                {
                    builder.AppendLine(" ");
                }

                counter++;

            }
            if (config.Dialect.SupportsReturningInto)
            {
                if (!parameter.WriteIndented)
                {
                    builder.AppendSql(" ");
                }
                builder.AppendLine("INTO ");
                counter = 0;
                for (var i = 0; i < entityInfo.Columns.Count; i++)
                {
                    var columnInfo = entityInfo.Columns[i];
                    builder.AppendComma(counter);
                    builder.AppendReturnParameter(parameter, columnInfo);
                    if (i < entityInfo.Columns.Count - 1)
                    {
                        builder.AppendLine();
                    }

                    counter++;
                }
            }

            // TODO: error
            if (counter == 0)
            {
                throw new InvalidOperationException("select column not found");
            }
        }

        private static void AppendFinalTableSelectCommandHeader(
            QueryStringBuilder builder, EntityTypeInfo entityInfo, QueryBuilderParameter parameter)
        {
            if (parameter.QueryBehavior == QueryBehavior.None) return;
            if (parameter.SqlKind == SqlKind.Delete) return;
            var config = parameter.Config;
            if (!config.Dialect.SupportsFinalTable) return;
            AppendSelectAffectedCommandHeader(builder, entityInfo, parameter, true);
            AppendFromClause(builder, entityInfo, parameter, true);
            builder.ApplyIndent(4);

        }

        private static void AppendFinalTableSelectCommandTerminator(
            QueryStringBuilder builder, QueryBuilderParameter parameter)
        {
            if (parameter.QueryBehavior == QueryBehavior.None) return;
            if (parameter.SqlKind == SqlKind.Delete) return;
            var config = parameter.Config;
            if (!config.Dialect.SupportsFinalTable) return;
            builder.RemoveIndent();
            builder.AppendLine();
            builder.AppendLine(") AS t_");

        }

        private static void AppendFromClause(
            QueryStringBuilder builder,
            EntityTypeInfo entityInfo,
            QueryBuilderParameter parameter,
            bool callFromFinalTable = false)
        {
            if (parameter.QueryBehavior == QueryBehavior.None) return;
            if (parameter.SqlKind == SqlKind.Delete || parameter.SqlKind == SqlKind.SoftDelete) return;
            var config = parameter.Config;
            if (config.Dialect.SupportsReturningInto || config.Dialect.SupportsReturning)
            {
                return;
            }
            if (config.Dialect.SupportsFinalTable)
            {
                if (callFromFinalTable)
                {
                    if (!parameter.WriteIndented)
                    {
                        builder.AppendSql(" ");
                    }
                    builder.AppendLine("FROM FINAL TABLE (");
                }
                return;
            }

            builder.AppendSql(parameter.WriteIndented ? "" : " ");
            builder.AppendLine("FROM ");
            if (parameter.WriteIndented)
            {
                builder.AppendIndent(3);
            }
            if (!string.IsNullOrEmpty(entityInfo.SchemaName))
            {
                builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.SchemaName));
                builder.AppendSql(".");
            }

            builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.TableName));
            builder.AppendLine(" ");
            builder.AppendLine("WHERE ");
            var identityAppended = false;
            if (parameter.SqlKind == SqlKind.Insert)
            {
                if (entityInfo.IdentityColumn != null)
                {
                    if (parameter.WriteIndented)
                    {
                        builder.AppendIndent(5);
                    }
                    builder.AppendSql(config.Dialect.GetIdentityWhereClause(entityInfo.IdentityColumn.ColumnName));
                    identityAppended = true;
                }
            }

            if (parameter.SqlKind == SqlKind.Update || !identityAppended)
            {
                var counter = 0;
                for (int i = 0; i < entityInfo.KeyColumns.Count; i++)
                {
                    var columnInfo = entityInfo.KeyColumns[i];
                    builder.AppendAnd(counter);
                    var propValue = columnInfo.PropertyInfo.GetValue(parameter.Entity);
                    var columnName = config.Dialect.ApplyQuote(columnInfo.ColumnName);
                    builder.AppendSql(columnName);
                    builder.AppendSql(" = ");
                    builder.AppendParameter(columnInfo.PropertyInfo, propValue);
                    if (i < entityInfo.KeyColumns.Count - 1)
                    {
                        builder.AppendLine();
                    }
                    counter++;

                }
            }

            if (!config.Dialect.SupportsFinalTable && !config.Dialect.SupportsReturningInto && !config.Dialect.SupportsReturning)
            {
                builder.ForceAppendLine(config.Dialect.StatementTerminator);
            }

        }

    }

}
