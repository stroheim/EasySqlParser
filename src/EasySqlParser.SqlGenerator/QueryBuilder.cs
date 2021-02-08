using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
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


        private static QueryBuilderResult GetInsertSql(QueryBuilderParameter<T> parameter)
        {
            var entityInfo = Cache<T>.EntityTypeInfo;
            var config = parameter.Config;
            var builder = new QueryStringBuilder(config, parameter.WriteIndented);
            builder.AppendSql("INSERT INTO ");
            if (!string.IsNullOrEmpty(entityInfo.SchemaName))
            {
                builder.AppendSql(config.GetQuotedName(entityInfo.SchemaName));
                builder.AppendSql(".");
            }
            builder.AppendSql(config.GetQuotedName(entityInfo.TableName));
            builder.AppendLine(" (");
            var counter = 0;
            foreach (var columnInfo in entityInfo.Columns)
            {
                if (parameter.ExcludeNull)
                {
                    var propValue= columnInfo.PropertyInfo.GetValue(parameter.Entity);
                    if (propValue == null) continue;
                }

                if (columnInfo.IsIdentity) continue;
                builder.AppendComma(counter);

                builder.AppendLine(config.GetQuotedName(columnInfo.ColumnName));

                counter++;
            }
            builder.AppendLine(") VALUES (");
            counter = 0;
            foreach (var columnInfo in entityInfo.Columns)
            {
                if (columnInfo.IsIdentity) continue;
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


        internal static (QueryBuilderResult builderResult, EntityTypeInfo entityInfo) GetSelectSql(
            QueryBuilderParameter<T> parameter,
            Dictionary<string, object> keyValues)
        {
            var entityInfo = Cache<T>.EntityTypeInfo;
            var config = parameter.Config;
            if (entityInfo.KeyColumns.Count == 0)
            {
                // pkがなければupdateできない
                throw new InvalidOperationException("");
            }

            var builder = new QueryStringBuilder(config, parameter.WriteIndented);
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

                    entityInfo.Columns.Add(columnInfo);
                }

                EntityTypeInfo = entityInfo;
            }

            internal static EntityTypeInfo EntityTypeInfo { get; }

        }
    }
}
