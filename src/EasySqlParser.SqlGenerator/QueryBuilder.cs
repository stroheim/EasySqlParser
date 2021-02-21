using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
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
                case SqlKind.Delete:
                    return GetDeleteSql(parameter);
            }

            return null;
        }

        internal static EntityTypeInfo GetEntityTypeInfo()
        {
            return Cache<T>.EntityTypeInfo;
        }


        private static QueryBuilderResult GetInsertSql(QueryBuilderParameter<T> parameter)
        {
            var entityInfo = Cache<T>.EntityTypeInfo;
            var config = parameter.Config;
            var builder = new QueryStringBuilder(config, parameter.WriteIndented);
            AppendFinalTableSelectCommandHeader(builder, entityInfo, parameter);
            builder.AppendSql("INSERT INTO ");
            if (!string.IsNullOrEmpty(entityInfo.SchemaName))
            {
                builder.AppendSql(config.GetQuotedName(entityInfo.SchemaName));
                builder.AppendSql(".");
            }
            builder.AppendSql(config.GetQuotedName(entityInfo.TableName));
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
                builder.AppendComma(counter);

                builder.AppendLine(config.GetQuotedName(columnInfo.ColumnName));

                counter++;
            }
            builder.AppendLine(") VALUES (");
            counter = 0;
            foreach (var columnInfo in entityInfo.Columns)
            {
                if (columnInfo.IsIdentity)
                {
                    if (builder.TryAppendIdentityValue(parameter, entityInfo, counter, identityValue))
                    {
                        counter++;
                    }
                    continue;
                }
                var propValue = columnInfo.PropertyInfo.GetValue(parameter.Entity);
                if (parameter.ExcludeNull && propValue == null) continue;
                builder.AppendComma(counter);
                if (columnInfo.IsVersion)
                {
                    propValue = builder.GetDefaultVersionNo(propValue, columnInfo.PropertyInfo.PropertyType);
                }
                builder.AppendParameter(config.GetParameterName(columnInfo.PropertyInfo.Name), propValue);
                builder.AppendLine();
                counter++;
            }
            builder.AppendLine(")");
            AppendFinalTableSelectCommandTerminator(builder, parameter);
            AppendSelectAffectedCommandHeader(builder, entityInfo, parameter);
            AppendFromClause(builder, entityInfo, parameter);
            return builder.GetResult();

        }


        private static QueryBuilderResult GetUpdateSql(QueryBuilderParameter<T> parameter)
        {
            var entityInfo = Cache<T>.EntityTypeInfo;
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
                builder.AppendSql(config.GetQuotedName(entityInfo.SchemaName));
                builder.AppendSql(".");
            }
            builder.AppendSql(config.GetQuotedName(entityInfo.TableName));
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
                builder.AppendComma(counter);

                var columnName = config.GetQuotedName(columnInfo.ColumnName);
                builder.AppendSql(columnName);
                builder.AppendSql(" = ");
                builder.AppendParameter(config.GetParameterName(columnInfo.PropertyInfo.Name), propValue);
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
                var columnName = config.GetQuotedName(columnInfo.ColumnName);
                builder.AppendSql(columnName);
                if (propValue == null)
                {
                    builder.AppendSql(" IS NULL ");
                }
                else
                {
                    builder.AppendSql(" = ");
                    builder.AppendParameter(config.GetParameterName(columnInfo.PropertyInfo.Name), propValue);
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
            var entityInfo = Cache<T>.EntityTypeInfo;
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
                builder.AppendSql(config.GetQuotedName(entityInfo.SchemaName));
                builder.AppendSql(".");
            }
            builder.AppendSql(config.GetQuotedName(entityInfo.TableName));
            builder.AppendLine(" WHERE ");
            var counter = 0;
            foreach (var columnInfo in entityInfo.Columns)
            {
                if (parameter.IgnoreVersion && columnInfo.IsVersion) continue;
                if (!columnInfo.IsPrimaryKey && !columnInfo.IsVersion) continue;
                builder.AppendAnd(counter);
                var propValue = columnInfo.PropertyInfo.GetValue(parameter.Entity);
                var columnName = config.GetQuotedName(columnInfo.ColumnName);
                builder.AppendSql(columnName);
                builder.AppendSql(" = ");
                builder.AppendParameter(config.GetParameterName(columnInfo.PropertyInfo.Name), propValue);
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
            var entityInfo = Cache<T>.EntityTypeInfo;
            var config = configName == null
                ? ConfigContainer.DefaultConfig
                : ConfigContainer.AdditionalConfigs[configName];
            var builder = new QueryStringBuilder(config, writeIndented);
            builder.AppendSql("SELECT COUNT(*) CNT FROM ");
            if (!string.IsNullOrEmpty(entityInfo.SchemaName))
            {
                builder.AppendSql(config.GetQuotedName(entityInfo.SchemaName));
                builder.AppendSql(".");
            }

            builder.AppendSql(config.GetQuotedName(entityInfo.TableName));
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

                    var columnName = config.GetQuotedName(columnInfo.ColumnName);
                    builder.AppendSql(columnName);
                    if (propValue == null)
                    {
                        builder.AppendSql(" IS NULL ");
                    }
                    else
                    {
                        builder.AppendSql(" = ");
                        builder.AppendParameter(config.GetParameterName(columnInfo.PropertyInfo.Name), propValue);
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
            var entityInfo = Cache<T>.EntityTypeInfo;
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
                builder.AppendLine(config.GetQuotedName(columnInfo.ColumnName));

                counter++;

            }

            builder.AppendLine(" FROM ");
            if (!string.IsNullOrEmpty(entityInfo.SchemaName))
            {
                builder.AppendSql(config.GetQuotedName(entityInfo.SchemaName));
                builder.AppendSql(".");
            }

            builder.AppendSql(config.GetQuotedName(entityInfo.TableName));
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

                var columnName = config.GetQuotedName(columnInfo.ColumnName);
                builder.AppendSql(columnName);
                if (propValue == null)
                {
                    builder.AppendSql(" IS NULL ");
                }
                else
                {
                    builder.AppendSql(" = ");
                    builder.AppendParameter(config.GetParameterName(columnInfo.PropertyInfo.Name), propValue);
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
            if (config.SupportsFinalTable() && !callFromFinalTable) return;
            if (config.SupportsReturning())
            {
                builder.AppendSql(" RETURNING ");
            }
            else
            {
                builder.AppendLine(config.GetStatementTerminator());
                builder.AppendSql(" SELECT ");
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
                builder.AppendLine(config.GetQuotedName(entityInfo.IdentityColumn.ColumnName));
                return;
            }

            var counter = 0;
            foreach (var columnInfo in entityInfo.Columns)
            {
                builder.AppendComma(counter);
                if (config.SupportsFinalTable())
                {
                    builder.AppendSql("t_.");
                }

                builder.AppendLine(config.GetQuotedName(columnInfo.ColumnName));

                counter++;

            }

            if (!config.SupportsFinalTable() && !config.SupportsReturning())
            {
                builder.AppendLine(config.GetStatementTerminator());
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
            if (!config.SupportsFinalTable()) return;
            AppendSelectAffectedCommandHeader(builder, entityInfo, parameter, true);
            builder.ApplyIndent(4);

        }

        private static void AppendFinalTableSelectCommandTerminator(
            QueryStringBuilder builder, QueryBuilderParameter<T> parameter)
        {
            if (parameter.QueryBehavior == QueryBehavior.None) return;
            if (parameter.SqlKind == SqlKind.Delete) return;
            var config = parameter.Config;
            if (!config.SupportsFinalTable()) return;
            builder.AppendLine(") AS t_");
            builder.RemoveIndent();

        }

        private static void AppendFromClause(
            QueryStringBuilder builder, EntityTypeInfo entityInfo, QueryBuilderParameter<T> parameter)
        {
            if (parameter.QueryBehavior == QueryBehavior.None) return;
            if (parameter.SqlKind == SqlKind.Delete) return;
            var config = parameter.Config;
            if (config.SupportsReturning())
            {
                return;
            }
            if (config.SupportsFinalTable())
            {
                builder.AppendLine(" FROM FINAL TABLE (");
                return;
            }
            builder.AppendLine(" FROM ");
            if (!string.IsNullOrEmpty(entityInfo.SchemaName))
            {
                builder.AppendSql(config.GetQuotedName(entityInfo.SchemaName));
                builder.AppendSql(".");
            }

            builder.AppendSql(config.GetQuotedName(entityInfo.TableName));
            builder.AppendLine(" WHERE ");
            if (parameter.SqlKind == SqlKind.Insert)
            {
                builder.AppendSql(config.GetIdentityWhereClause(entityInfo.IdentityColumn.ColumnName));
            }

            if (parameter.SqlKind == SqlKind.Update)
            {
                var counter = 0;
                foreach (var columnInfo in entityInfo.KeyColumns)
                {
                    builder.AppendAnd(counter);
                    var propValue = columnInfo.PropertyInfo.GetValue(parameter.Entity);
                    var columnName = config.GetQuotedName(columnInfo.ColumnName);
                    builder.AppendSql(columnName);
                    builder.AppendSql(" = ");
                    builder.AppendParameter(config.GetParameterName(columnInfo.PropertyInfo.Name), propValue);
                    builder.AppendLine();
                    counter++;
                }
            }
            if (!config.SupportsFinalTable() && !config.SupportsReturning())
            {
                builder.AppendLine(config.GetStatementTerminator());
            }

        }

        // generic type cache
        private static class Cache<TK>
        {
            static Cache()
            {
                var type = typeof(TK);
                var entityInfo = new EntityTypeInfo
                                 {
                                     TableName = type.Name,
                                     Columns = new List<EntityColumnInfo>(),
                                     KeyColumns = new List<EntityColumnInfo>()
                                 };
                var table = type.GetCustomAttribute<TableAttribute>();
                if (table != null)
                {
                    entityInfo.TableName = table.Name;
                    entityInfo.SchemaName = table.Schema;
                }
                var props = type.GetProperties();
                foreach (var propertyInfo in props)
                {
                    var notMapped = propertyInfo.GetCustomAttribute<NotMappedAttribute>();
                    if (notMapped != null)
                    {
                        continue;
                    }

                    var columnInfo = new EntityColumnInfo
                                     {
                                         PropertyInfo = propertyInfo
                                     };
                    

                    var column = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                    columnInfo.ColumnName = propertyInfo.Name;
                    if (column != null)
                    {
                        columnInfo.ColumnName = column.Name;
                    }

                    var identityAttr = propertyInfo.GetCustomAttribute<DatabaseGeneratedAttribute>();
                    if (identityAttr != null && identityAttr.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)
                    {
                        columnInfo.IsIdentity = true;
                    }

                    var versionAttr = propertyInfo.GetCustomAttribute<VersionAttribute>();
                    if (versionAttr != null)
                    {
                        columnInfo.IsVersion = true;
                    }

                    var keyAttr = propertyInfo.GetCustomAttribute<KeyAttribute>();
                    if (keyAttr != null)
                    {
                        columnInfo.IsPrimaryKey = true;
                        entityInfo.KeyColumns.Add(columnInfo.Clone());
                    }

                    if (columnInfo.IsPrimaryKey && columnInfo.IsIdentity)
                    {
                        entityInfo.IdentityColumn = columnInfo.Clone();
                    }

                    entityInfo.Columns.Add(columnInfo);
                }

                EntityTypeInfo = entityInfo;
            }

            // ReSharper disable once StaticMemberInGenericType
            internal static EntityTypeInfo EntityTypeInfo { get; }

        }
    }
}
