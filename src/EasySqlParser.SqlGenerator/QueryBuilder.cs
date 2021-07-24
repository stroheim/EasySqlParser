using System;
using System.Linq.Expressions;
using EasySqlParser.Configurations;
using EasySqlParser.SqlGenerator.Configurations;
using EasySqlParser.SqlGenerator.Enums;
using EasySqlParser.SqlGenerator.Metadata;

namespace EasySqlParser.SqlGenerator
{
    /// <summary>
    ///     A class for query builder.
    /// </summary>

    public class QueryBuilder
    {
        /// <summary>
        ///     Gets the <see cref="QueryBuilderResult"/> from sql file.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
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

        /// <summary>
        ///     Gets the <see cref="QueryBuilderResult"/>.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static QueryBuilderResult GetQueryBuilderResult(
            QueryBuilderParameter parameter)
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
                    throw new InvalidOperationException($"Unknown sql kind:{parameter.SqlKind}");
            }

        }

        /// <summary>
        ///     Generate count SQL statements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="configuration"></param>
        /// <param name="configName"></param>
        /// <returns></returns>
        public static QueryBuilderResult GetCountSql<T>(
            Expression<Func<T, bool>> predicate = null,
            IQueryBuilderConfiguration configuration = null,
            string configName = null)
            where T : class
        {
            var entityInfo = configuration == null
                ? EntityTypeInfoBuilder.Build(typeof(T))
                : configuration.GetEntityTypeInfo(typeof(T));
            var config = configName == null
                ? ConfigContainer.DefaultConfig
                : ConfigContainer.AdditionalConfigs[configName];
            var writeIndented = configuration?.WriteIndented ?? false;
            var builder = new QueryStringBuilder(config, writeIndented);
            builder.AppendSql("SELECT COUNT(*) CNT FROM ");
            if (!string.IsNullOrEmpty(entityInfo.SchemaName))
            {
                builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.SchemaName));
                builder.AppendSql(".");
            }

            builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.TableName));
            if (predicate != null)
            {
                var visitor = new PredicateVisitor(builder, entityInfo);
                visitor.BuildPredicate(predicate);
            }
            return builder.GetResult();

        }

        /// <summary>
        ///     Generate delete SQL statements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <param name="predicate"></param>
        /// <param name="configName"></param>
        /// <returns></returns>
        public static QueryBuilderResult GetDeleteSql<T>(
            IQueryBuilderConfiguration configuration,
            Expression<Func<T, bool>> predicate,
            string configName = null)
            where T : class
        {
            var config = configName == null
                ? ConfigContainer.DefaultConfig
                : ConfigContainer.AdditionalConfigs[configName];
            var builder = new QueryStringBuilder(config, configuration.WriteIndented);
            var entityInfo = configuration.GetEntityTypeInfo(typeof(T));
            builder.AppendSql("DELETE FROM ");
            if (!string.IsNullOrEmpty(entityInfo.SchemaName))
            {
                builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.SchemaName));
                builder.AppendSql(".");
            }
            builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.TableName));
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(predicate);
            return builder.GetResult();
        }


        /// <summary>
        ///     Generate select SQL statements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <param name="predicate"></param>
        /// <param name="configName"></param>
        /// <returns></returns>
        public static QueryBuilderResult GetSelectSql<T>(
            IQueryBuilderConfiguration configuration,
            Expression<Func<T, bool>> predicate,
            string configName = null)
            where T : class
        {
            var config = configName == null
                ? ConfigContainer.DefaultConfig
                : ConfigContainer.AdditionalConfigs[configName];
            var builder = new QueryStringBuilder(config, configuration.WriteIndented);
            var entityInfo = configuration.GetEntityTypeInfo(typeof(T));
            builder.AppendLine("SELECT ");
            var counter = 0;
            foreach (var columnInfo in entityInfo.Columns)
            {
                builder.AppendComma(counter);
                builder.AppendLine(config.Dialect.ApplyQuote(columnInfo.ColumnName));

                counter++;

            }

            if (!configuration.WriteIndented)
            {
                builder.AppendSql(" ");
            }

            builder.AppendLine("FROM ");
            if (configuration.WriteIndented)
            {
                builder.AppendIndent(3);
            }

            if (!string.IsNullOrEmpty(entityInfo.SchemaName))
            {
                builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.SchemaName));
                builder.AppendSql(".");
            }

            builder.AppendSql(config.Dialect.ApplyQuote(entityInfo.TableName));
            if (predicate == null)
            {
                return builder.GetResult();
            }
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(predicate);
            return builder.GetResult();
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
                var propValue = columnInfo.PropertyInfo.GetValue(parameter.Entity);
                if (builder.IsNull(parameter, propValue))
                {
                    continue;
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

                var propValue = columnInfo.PropertyInfo.GetValue(parameter.Entity);
                if (builder.IsNull(parameter, propValue))
                {
                    continue;
                }
                builder.AppendComma(counter);
                if (columnInfo.IsVersion)
                {
                    propValue = builder.GetDefaultVersionNo(propValue, columnInfo.PropertyInfo.PropertyType);
                }
                builder.AppendParameter(columnInfo, propValue);
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

                if (columnInfo.IsVersion)
                {
                    builder.AppendComma(counter);
                    var propValue = columnInfo.PropertyInfo.GetValue(parameter.Entity);

                    var columnName = config.Dialect.ApplyQuote(columnInfo.ColumnName);
                    builder.AppendSql(columnName);
                    builder.AppendSql(" = ");
                    builder.AppendParameter(columnInfo, propValue);
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
                    builder.AppendParameter(columnInfo, propValue);
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

                if (builder.IsNull(parameter, propValue))
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

                builder.AppendComma(counter);

                var columnName = config.Dialect.ApplyQuote(columnInfo.ColumnName);
                builder.AppendSql(columnName);
                builder.AppendSql(" = ");
                builder.AppendParameter(columnInfo, propValue);
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
                    builder.AppendParameter(columnInfo, propValue);
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
                builder.AppendParameter(columnInfo, propValue);
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
            if (parameter.SqlKind == SqlKind.Delete) return;
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

            if (parameter.SqlKind == SqlKind.Update || parameter.SqlKind == SqlKind.SoftDelete || !identityAppended)
            {
                var counter = 0;
                for (var i = 0; i < entityInfo.KeyColumns.Count; i++)
                {
                    var columnInfo = entityInfo.KeyColumns[i];
                    builder.AppendAnd(counter);
                    var propValue = columnInfo.PropertyInfo.GetValue(parameter.Entity);
                    var columnName = config.Dialect.ApplyQuote(columnInfo.ColumnName);
                    builder.AppendSql(columnName);
                    builder.AppendSql(" = ");
                    builder.AppendParameter(columnInfo, propValue);
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
