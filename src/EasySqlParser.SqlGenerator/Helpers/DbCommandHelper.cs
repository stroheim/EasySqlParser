using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace EasySqlParser.SqlGenerator.Helpers
{
    public static class DbCommandHelper
    {
        // used by dapper and ef
        public static int ConsumeScalar(object scalarValue, QueryBuilderParameter builderParameter)
        {
            if (scalarValue == null || scalarValue is DBNull)
            {
                return 0;
            }

            var entityInfo = builderParameter.EntityTypeInfo;
            var instance = builderParameter.Entity;
            //var config = builderParameter.Config;
            if (entityInfo.IdentityColumn != null)
            {
                var changed = Convert.ChangeType(scalarValue, entityInfo.IdentityColumn.PropertyInfo.PropertyType);
                entityInfo.IdentityColumn.PropertyInfo.SetValue(instance, changed);
                if (!builderParameter.IsSameVersion())
                {
                    return 0;
                }
                return 1;
            }
            //if (config.DbConnectionKind == DbConnectionKind.SQLite)
            //{
            //    if (entityInfo.IdentityColumn != null && scalarValue is long longValue)
            //    {
            //        var converted = Convert.ChangeType(longValue,
            //            entityInfo.IdentityColumn.PropertyInfo.PropertyType);
            //        entityInfo.IdentityColumn.PropertyInfo.SetValue(instance, converted);
            //        if (!builderParameter.IsSameVersion())
            //        {
            //            return 0;
            //        }
            //        return 1;
            //    }
            //}

            //entityInfo.IdentityColumn?.PropertyInfo.SetValue(instance, scalarValue);


            //if (!builderParameter.IsSameVersion())
            //{
            //    return 0;
            //}
            return 1;

        }

        // used by ef
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

        // used by ef
        public static async Task<int> ConsumeReaderAsync(DbDataReader reader, QueryBuilderParameter builderParameter)
        {
            if (!reader.HasRows)
            {
                reader.Close();
                reader.Dispose();
                return 0;
            }

            var entityInfo = builderParameter.EntityTypeInfo;
            await reader.ReadAsync().ConfigureAwait(false);
            var instance = builderParameter.Entity;
            builderParameter.WriteLog("[Start] ConsumeReader");
            var config = builderParameter.Config;
            foreach (var columnInfo in entityInfo.Columns)
            {
                var col = reader.GetOrdinal(columnInfo.ColumnName);
                if (!await reader.IsDBNullAsync(col).ConfigureAwait(false))
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

}
