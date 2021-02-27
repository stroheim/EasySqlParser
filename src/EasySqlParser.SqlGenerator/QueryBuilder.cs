using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EasySqlParser.Configurations;
using EasySqlParser.Extensions;

namespace EasySqlParser.SqlGenerator
{
    public class QueryBuilder<T>
    {
        public static QueryBuilderResult GetQueryBuilderResultFromSqlFile(QueryBuilderParameter<T> parameter)
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

        public static QueryBuilderResult GetQueryBuilderResult(QueryBuilderParameter<T> parameter)
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
            }

            return null;
        }


        internal static int ConsumeScalar(
            QueryBuilderParameter<T> builderParameter,
            DbCommand command)
        {
            var rawScalarValue = command.ExecuteScalar();
            if (rawScalarValue == null)
            {
                return 0;
            }
            var entityInfo = builderParameter.EntityTypeInfo;
            var instance = builderParameter.Entity;
            entityInfo.IdentityColumn?.PropertyInfo.SetValue(instance, rawScalarValue);


            return 1;
        }

        internal static int ConsumeNonQuery(
            QueryBuilderParameter<T> builderParameter,
            DbCommand command)

        {
            var affectedCount = command.ExecuteNonQuery();
            builderParameter.ApplyReturningColumns();
            return affectedCount;
        }


        internal static int ConsumeReader(
            QueryBuilderParameter<T> builderParameter,
            DbCommand command)
        {
            var reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                reader.Close();
                reader.Dispose();
                return 0;
            }

            var entityInfo = builderParameter.EntityTypeInfo;
            reader.Read();
            var instance = builderParameter.Entity;
            foreach (var columnInfo in entityInfo.Columns)
            {
                var col = reader.GetOrdinal(columnInfo.ColumnName);
                if (!reader.IsDBNull(col))
                {
                    columnInfo.PropertyInfo.SetValue(instance, reader.GetValue(col));
                }
            }
            reader.Close();
            reader.Dispose();
            return 1;
        }

        internal static async Task<int> ConsumeScalarAsync(
            QueryBuilderParameter<T> builderParameter,
            DbCommand command,
            CancellationToken cancellationToken = default)
        {
            var rawScalarValue = await command.ExecuteScalarAsync(cancellationToken);
            if (rawScalarValue == null)
            {
                return 0;
            }

            var entityInfo = builderParameter.EntityTypeInfo;
            var instance = builderParameter.Entity;
            entityInfo.IdentityColumn?.PropertyInfo.SetValue(instance, rawScalarValue);


            return 1;
        }

        internal static async Task<int> ConsumeNonQueryAsync(
            QueryBuilderParameter<T> builderParameter,
            DbCommand command,
            CancellationToken cancellationToken = default)

        {
            var affectedCount = await command.ExecuteNonQueryAsync(cancellationToken);
            builderParameter.ApplyReturningColumns();
            return affectedCount;
        }


        internal static async Task<int> ConsumeReaderAsync(
            QueryBuilderParameter<T> builderParameter,
            DbCommand command,
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
            foreach (var columnInfo in entityInfo.Columns)
            {
                var col = reader.GetOrdinal(columnInfo.ColumnName);
                if (!await reader.IsDBNullAsync(col, cancellationToken))
                {
                    columnInfo.PropertyInfo.SetValue(instance, reader.GetValue(col));
                }
            }
            reader.Close();
            reader.Dispose();
            return 1;
        }

        internal static void PreInsert(QueryBuilderParameter<T> parameter, DbConnection connection)
        {
            if (parameter.SqlKind != SqlKind.Insert) return;
            var entityInfo = parameter.EntityTypeInfo;
            var config = parameter.Config;
            if (!config.Dialect.SupportsSequence) return;
            if (entityInfo.SequenceColumns.Count == 0) return;
            foreach (var columnInfo in entityInfo.SequenceColumns)
            {
                if (columnInfo.PropertyInfo.PropertyType == typeof(int))
                {
                    if (connection.TryGenerateSequence(parameter, columnInfo.SequenceGeneratorAttribute,
                        out int intValue))
                    {
                        columnInfo.PropertyInfo.SetValue(parameter.Entity, intValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(long))
                {
                    if (connection.TryGenerateSequence(parameter, columnInfo.SequenceGeneratorAttribute,
                        out long longValue))
                    {
                        columnInfo.PropertyInfo.SetValue(parameter.Entity, longValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(decimal))
                {
                    if (connection.TryGenerateSequence(parameter, columnInfo.SequenceGeneratorAttribute,
                        out decimal decimalValue))
                    {
                        columnInfo.PropertyInfo.SetValue(parameter.Entity, decimalValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(string))
                {
                    if (connection.TryGenerateSequence(parameter, columnInfo.SequenceGeneratorAttribute,
                        out string stringValue))
                    {
                        columnInfo.PropertyInfo.SetValue(parameter.Entity, stringValue);
                        continue;
                    }
                }
            }
        }

        internal static async Task PreInsertAsync(QueryBuilderParameter<T> parameter, DbConnection connection)
        {
            if (parameter.SqlKind != SqlKind.Insert) return;
            var entityInfo = parameter.EntityTypeInfo;
            var config = parameter.Config;
            if (!config.Dialect.SupportsSequence) return;
            if (entityInfo.SequenceColumns.Count == 0) return;
            foreach (var columnInfo in entityInfo.SequenceColumns)
            {
                if (columnInfo.PropertyInfo.PropertyType == typeof(int))
                {
                    var (isSuccess, intValue) = await connection.TryGenerateSequenceAsync<T, int>(parameter,
                        columnInfo.SequenceGeneratorAttribute);

                    if (isSuccess)
                    {
                        columnInfo.PropertyInfo.SetValue(parameter.Entity, intValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(long))
                {
                    var (isSuccess, longValue) = await connection.TryGenerateSequenceAsync<T, long>(parameter,
                        columnInfo.SequenceGeneratorAttribute);
                    if (isSuccess)
                    {
                        columnInfo.PropertyInfo.SetValue(parameter.Entity, longValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(decimal))
                {
                    var (isSuccess, decimalValue) = await connection.TryGenerateSequenceAsync<T, decimal>(parameter,
                        columnInfo.SequenceGeneratorAttribute);
                    if (isSuccess)
                    {
                        columnInfo.PropertyInfo.SetValue(parameter.Entity, decimalValue);
                        continue;
                    }
                }

                if (columnInfo.PropertyInfo.PropertyType == typeof(string))
                {
                    var (isSuccess, stringValue) = await connection.TryGenerateSequenceAsync<T, string>(parameter,
                        columnInfo.SequenceGeneratorAttribute);
                    if (isSuccess)
                    {
                        columnInfo.PropertyInfo.SetValue(parameter.Entity, stringValue);
                        continue;
                    }
                }
            }

        }


        private static QueryBuilderResult GetInsertSql(QueryBuilderParameter<T> parameter)
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
            object identityValue = null;
            foreach (var columnInfo in entityInfo.Columns)
            {
                if (parameter.ExcludeNull)
                {
                    var propValue= columnInfo.PropertyInfo.GetValue(parameter.Entity);
                    if (propValue == null) continue;
                }

                if (columnInfo.IsIdentity)
                {
                    if (builder.TryAppendIdentity(parameter, entityInfo, counter, out identityValue))
                    {
                        counter++;
                    }
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
                    if (!builder.HasIdentityValue(entityInfo, identityValue))
                    {
                        continue;
                    }
                    builder.AppendIdentityValue(entityInfo, counter, identityValue);
                    counter++;
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

        private static QueryBuilderResult GetSoftDeleteSql(QueryBuilderParameter<T> parameter)
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
            foreach (var columnInfo in entityInfo.Columns)
            {
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
                builder.AppendLine();

                counter++;

            }

            AppendFinalTableSelectCommandTerminator(builder, parameter);
            AppendSelectAffectedCommandHeader(builder, entityInfo, parameter);
            AppendFromClause(builder, entityInfo, parameter);

            return builder.GetResult();

        }

        private static QueryBuilderResult GetUpdateSql(QueryBuilderParameter<T> parameter)
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
            foreach (var columnInfo in entityInfo.Columns)
            {
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
                builder.AppendLine();

                counter++;
            }
            AppendFinalTableSelectCommandTerminator(builder, parameter);
            AppendSelectAffectedCommandHeader(builder, entityInfo, parameter);
            AppendFromClause(builder, entityInfo, parameter);

            return builder.GetResult();
        }

        private static QueryBuilderResult GetDeleteSql(QueryBuilderParameter<T> parameter)
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

        internal static QueryBuilderResult GetCountSql(
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
            var builderParameter = new QueryBuilderParameter<T>();
            var entityInfo = builderParameter.EntityTypeInfo;
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

        internal static (QueryBuilderResult builderResult, EntityTypeInfo entityInfo) GetSelectSql(
            Expression<Func<T, bool>> predicate,
            string configName = null,
            bool writeIndented = false)
        {
            var visitor = new PredicateVisitor();
            var keyValues = visitor.GetKeyValues(predicate);
            var builderParameter = new QueryBuilderParameter<T>();
            var entityInfo = builderParameter.EntityTypeInfo;
            var config = configName == null
                ? ConfigContainer.DefaultConfig
                : ConfigContainer.AdditionalConfigs[configName];

            if (entityInfo.KeyColumns.Count == 0)
            {
                // pkがなければupdateできない
                throw new InvalidOperationException("");
            }

            var builder = new QueryStringBuilder(config, writeIndented);
            builder.AppendSql("SELECT ");
            var counter = 0;
            foreach (var columnInfo in entityInfo.Columns)
            {
                builder.AppendComma(counter);
                builder.AppendLine(config.Dialect.ApplyQuote(columnInfo.ColumnName));

                counter++;

            }

            builder.AppendLine(" FROM ");
            if (!string.IsNullOrEmpty(entityInfo.SchemaName))
            {
                builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.SchemaName));
                builder.AppendSql(".");
            }

            builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.TableName));
            builder.AppendLine(" WHERE ");
            counter = 0;
            foreach (var columnInfo in entityInfo.Columns)
            {
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

                builder.AppendLine();

                counter++;
            }

            return (builder.GetResult(), entityInfo);

        }

        private static void AppendSelectAffectedCommandHeader(
            QueryStringBuilder builder, EntityTypeInfo entityInfo,
            QueryBuilderParameter<T> parameter,
            bool callFromFinalTable = false)
        {
            if (parameter.QueryBehavior == QueryBehavior.None) return;
            if (parameter.SqlKind == SqlKind.Delete) return;
            var config = parameter.Config;
            if (config.Dialect.SupportsFinalTable && !callFromFinalTable) return;
            if (config.Dialect.SupportsReturning)
            {
                builder.AppendLine(" RETURNING ");
            }
            else
            {
                builder.AppendLine(config.Dialect.StatementTerminator);
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
                builder.AppendComma(0);
                builder.AppendLine(config.Dialect.ApplyQuote(entityInfo.IdentityColumn.ColumnName));
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
                builder.AppendLine(" ");

                counter++;

            }

            if (config.Dialect.SupportsReturning)
            {
                builder.AppendLine(" INTO ");
                counter = 0;
                foreach (var columnInfo in entityInfo.Columns)
                {
                    builder.AppendComma(counter);
                    builder.AppendReturnParameter(parameter, columnInfo);
                    builder.AppendLine();

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
            QueryStringBuilder builder, EntityTypeInfo entityInfo, QueryBuilderParameter<T> parameter)
        {
            if (parameter.QueryBehavior == QueryBehavior.None) return;
            if (parameter.SqlKind == SqlKind.Delete) return;
            var config = parameter.Config;
            if (!config.Dialect.SupportsFinalTable) return;
            AppendSelectAffectedCommandHeader(builder, entityInfo, parameter, true);
            builder.ApplyIndent(4);

        }

        private static void AppendFinalTableSelectCommandTerminator(
            QueryStringBuilder builder, QueryBuilderParameter<T> parameter)
        {
            if (parameter.QueryBehavior == QueryBehavior.None) return;
            if (parameter.SqlKind == SqlKind.Delete) return;
            var config = parameter.Config;
            if (!config.Dialect.SupportsFinalTable) return;
            builder.AppendLine(") AS t_");
            builder.RemoveIndent();

        }

        private static void AppendFromClause(
            QueryStringBuilder builder, EntityTypeInfo entityInfo, QueryBuilderParameter<T> parameter)
        {
            if (parameter.QueryBehavior == QueryBehavior.None) return;
            if (parameter.SqlKind == SqlKind.Delete || parameter.SqlKind == SqlKind.SoftDelete) return;
            var config = parameter.Config;
            if (config.Dialect.SupportsReturning)
            {
                return;
            }
            if (config.Dialect.SupportsFinalTable)
            {
                builder.AppendLine(" FROM FINAL TABLE (");
                return;
            }
            builder.AppendLine("FROM ");
            if (!string.IsNullOrEmpty(entityInfo.SchemaName))
            {
                builder.AppendIndent(3);
                builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.SchemaName));
                builder.AppendSql(".");
            }
            else
            {
                builder.AppendIndent(3);
            }

            builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.TableName));
            builder.AppendLine(" ");
            builder.AppendLine("WHERE ");
            var identityAppended = false;
            if (parameter.SqlKind == SqlKind.Insert)
            {
                if (entityInfo.IdentityColumn != null)
                {
                    builder.AppendIndent(3);
                    builder.AppendSql(config.Dialect.GetIdentityWhereClause(entityInfo.IdentityColumn.ColumnName));
                    identityAppended = true;
                }
            }

            if (parameter.SqlKind == SqlKind.Update || !identityAppended)
            {
                var counter = 0;
                foreach (var columnInfo in entityInfo.KeyColumns)
                {
                    builder.AppendAnd(counter);
                    var propValue = columnInfo.PropertyInfo.GetValue(parameter.Entity);
                    var columnName = config.Dialect.ApplyQuote(columnInfo.ColumnName);
                    builder.AppendSql(columnName);
                    builder.AppendSql(" = ");
                    builder.AppendParameter(columnInfo.PropertyInfo, propValue);
                    builder.AppendLine();
                    counter++;
                }
            }

            if (!config.Dialect.SupportsFinalTable && !config.Dialect.SupportsReturning)
            {
                builder.AppendLine(config.Dialect.StatementTerminator);
            }

        }

        // generic type cache
        //private static class Cache<TK>
        //{
        //    static Cache()
        //    {
        //        var type = typeof(TK);
        //        var entityInfo = new EntityTypeInfo
        //                         {
        //                             TableName = type.Name,
        //                             Columns = new List<EntityColumnInfo>(),
        //                             KeyColumns = new List<EntityColumnInfo>(),
        //                             SequenceColumns = new List<EntityColumnInfo>()
        //                         };
        //        var table = type.GetCustomAttribute<TableAttribute>();
        //        if (table != null)
        //        {
        //            entityInfo.TableName = table.Name;
        //            entityInfo.SchemaName = table.Schema;
        //        }
        //        var props = type.GetProperties();
        //        foreach (var propertyInfo in props)
        //        {
        //            var notMapped = propertyInfo.GetCustomAttribute<NotMappedAttribute>();
        //            if (notMapped != null)
        //            {
        //                continue;
        //            }

        //            var columnInfo = new EntityColumnInfo
        //                             {
        //                                 PropertyInfo = propertyInfo
        //                             };
                    

        //            var column = propertyInfo.GetCustomAttribute<ColumnAttribute>();
        //            columnInfo.ColumnName = propertyInfo.Name;
        //            if (column != null)
        //            {
        //                columnInfo.ColumnName = column.Name;
        //            }

        //            var identityAttr = propertyInfo.GetCustomAttribute<DatabaseGeneratedAttribute>();
        //            if (identityAttr != null && identityAttr.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)
        //            {
        //                columnInfo.IsIdentity = true;
        //            }

        //            var versionAttr = propertyInfo.GetCustomAttribute<VersionAttribute>();
        //            if (versionAttr != null)
        //            {
        //                columnInfo.IsVersion = true;
        //            }

        //            var currentTimestampAttr = propertyInfo.GetCustomAttribute<CurrentTimestampAttribute>();
        //            if (currentTimestampAttr != null)
        //            {
        //                columnInfo.CurrentTimestampAttribute = currentTimestampAttr;
        //            }

        //            var seqAttr = propertyInfo.GetCustomAttribute<SequenceGeneratorAttribute>();
        //            if (seqAttr != null)
        //            {
        //                columnInfo.IsSequence = true;
        //                columnInfo.SequenceGeneratorAttribute = seqAttr;
        //                entityInfo.SequenceColumns.Add(columnInfo.Clone());
        //            }

        //            var keyAttr = propertyInfo.GetCustomAttribute<KeyAttribute>();
        //            if (keyAttr != null)
        //            {
        //                columnInfo.IsPrimaryKey = true;
        //                entityInfo.KeyColumns.Add(columnInfo.Clone());
        //            }

        //            if (columnInfo.IsPrimaryKey && columnInfo.IsIdentity)
        //            {
        //                entityInfo.IdentityColumn = columnInfo.Clone();
        //            }

        //            entityInfo.Columns.Add(columnInfo);
        //        }

        //        EntityTypeInfo = entityInfo;
        //    }

        //    // ReSharper disable once StaticMemberInGenericType
        //    internal static EntityTypeInfo EntityTypeInfo { get; }

        //}
    }
}
