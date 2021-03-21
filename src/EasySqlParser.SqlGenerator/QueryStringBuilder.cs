using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using EasySqlParser.Configurations;
using EasySqlParser.Extensions;

namespace EasySqlParser.SqlGenerator
{
    internal class QueryStringBuilder
    {
        private readonly StringBuilder _rawSqlBuilder = new StringBuilder(200);
        private readonly StringBuilder _formattedSqlBuilder = new StringBuilder(200);

        private readonly SqlParserConfig _config;
        private readonly bool _writeIndented;
        private readonly Dictionary<string, IDbDataParameter> _sqlParameters =
            new Dictionary<string, IDbDataParameter>();

        private string _indent = "";
        private bool _firstWord;

        internal QueryStringBuilder(SqlParserConfig config, bool writeIndented)
        {
            _config = config;
            _writeIndented = writeIndented;
            _firstWord = true;
        }


        internal void ApplyIndent(int length)
        {
            if (_config.Dialect.SupportsFinalTable)
            {
                _indent = "".PadLeft(length, ' ');
            }
        }

        internal void RemoveIndent()
        {
            _indent = "";
        }

        internal void AppendIndent(int length)
        {
            var indent = "".PadLeft(length, ' ');
            _rawSqlBuilder.Append(indent);
            _formattedSqlBuilder.Append(indent);
        }

        internal void AppendSql(string sql)
        {
            if (_writeIndented && _firstWord)
            {
                _rawSqlBuilder.Append(_indent);
                _formattedSqlBuilder.Append(_indent);
                _firstWord = false;
            }
            _rawSqlBuilder.Append(sql);
            _formattedSqlBuilder.Append(sql);
        }

        internal void AppendLine()
        {
            if (!_writeIndented) return;
            _firstWord = true;
            _rawSqlBuilder.AppendLine();
            _formattedSqlBuilder.AppendLine();
        }

        internal void AppendLine(string sql)
        {
            if (!_writeIndented)
            {
                _rawSqlBuilder.Append(sql);
                _formattedSqlBuilder.Append(sql);
                return;
            }
            if (_firstWord)
            {
                _rawSqlBuilder.Append(_indent);
                _formattedSqlBuilder.Append(_indent);
            }

            _firstWord = true;
            _rawSqlBuilder.AppendLine(sql);
            _formattedSqlBuilder.AppendLine(sql);
        }

        internal void ForceAppendLine()
        {
            _firstWord = true;
            _rawSqlBuilder.AppendLine();
            _formattedSqlBuilder.AppendLine();
        }

        internal void ForceAppendLine(string sql)
        {
            if (_firstWord)
            {
                _rawSqlBuilder.Append(_indent);
                _formattedSqlBuilder.Append(_indent);
            }
            _firstWord = true;
            _rawSqlBuilder.AppendLine(sql);
            _formattedSqlBuilder.AppendLine(sql);
        }

        internal void AppendReturnParameter(QueryBuilderParameter builderParameter, EntityColumnInfo columnInfo)
        {
            var propertyName = $"{_config.Dialect.ParameterPrefix}p_{columnInfo.PropertyInfo.Name}";
            var (parameterName, parameter) = CreateDbParameter(propertyName, direction: ParameterDirection.ReturnValue);
            if (columnInfo.StringMaxLength.HasValue)
            {
                parameter.Size = columnInfo.StringMaxLength.Value;
            }

            if (_config.DbConnectionKind == DbConnectionKind.Oracle ||
                _config.DbConnectionKind == DbConnectionKind.OracleLegacy)
            {
                parameter.DbType = columnInfo.DbType;
            }
            if (!_sqlParameters.ContainsKey(propertyName))
            {
                _sqlParameters.Add(propertyName, parameter);
                builderParameter.ReturningColumns.Add(propertyName, (columnInfo, parameter));
            }
            _rawSqlBuilder.Append(parameterName);
            _formattedSqlBuilder.Append(parameterName);
        }


        internal void AppendParameter(PropertyInfo propertyInfo, object propertyValue)
        {
            var propertyName = _config.Dialect.ParameterPrefix + propertyInfo.Name;
            var (parameterName, parameter) = CreateDbParameter(propertyName, propertyValue);
            if (!_sqlParameters.ContainsKey(propertyName))
            {
                _sqlParameters.Add(propertyName, parameter);
            }
            _rawSqlBuilder.Append(parameterName);
            _formattedSqlBuilder.Append(_config.Dialect.ConvertToLogFormat(propertyValue));
        }

        private (string parameterName, IDbDataParameter parameter) CreateDbParameter(
            string parameterKey, object parameterValue = null,
            ParameterDirection direction = ParameterDirection.Input)
        {
            var param = _config.DataParameterCreator()
                .AddName(parameterKey);
            if (direction == ParameterDirection.Input || 
                direction == ParameterDirection.InputOutput)
            {
                param.AddValue(parameterValue ?? DBNull.Value);
            }
            param.Direction = direction;
            // parameterKey is sql parameter name
            var localParameterKey = parameterKey;
            if (!_config.Dialect.EnableNamedParameter)
            {
                localParameterKey = _config.Dialect.ParameterPrefix;
            }

            return (localParameterKey, param);

        }

        internal void AppendComma(int counter)
        {
            if (_writeIndented && _firstWord)
            {
                _rawSqlBuilder.Append(_indent);
                _formattedSqlBuilder.Append(_indent);
                _firstWord = false;
            }
            if (counter == 0)
            {
                if (_writeIndented)
                {
                    AppendSql("   ");
                }
            }
            else
            {
                AppendSql(_writeIndented ? " , " : ", ");
            }
        }

        internal void AppendAnd(int counter)
        {
            if (_writeIndented && _firstWord)
            {
                _rawSqlBuilder.Append(_indent);
                _formattedSqlBuilder.Append(_indent);
                _firstWord = false;
            }
            if (counter == 0)
            {
                if (_writeIndented)
                {
                    AppendSql("     ");
                }
            }
            else
            {
                AppendSql(" AND ");
            }
        }


        internal object GetDefaultVersionNo(object value, Type propertyType)
        {
            if (value == null)
            {
                if (propertyType == typeof(short))
                {
                    return (short) 1;
                }
                if (propertyType == typeof(int))
                {
                    return 1;
                }
                if (propertyType == typeof(long))
                {
                    return 1L;
                }

                if (propertyType == typeof(decimal))
                {
                    return 1M;
                }
            }
            else if (value is short shortValue)
            {
                if (shortValue <= 0)
                {
                    return (short) 1;
                }
            }
            else if (value is int intValue)
            {
                if (intValue <= 0)
                {
                    return 1;
                }
            }else if (value is long longValue)
            {
                if (longValue <= 0L)
                {
                    return 1L;
                }
            }else if (value is decimal decimalValue)
            {
                if (decimalValue <= 0M)
                {
                    return 1M;
                }
            }

            // TODO:
            throw new InvalidOperationException("");
        }

        internal void AppendVersion(QueryBuilderParameter parameter, EntityColumnInfo columnInfo)
        {
            if (!columnInfo.IsVersion) return;
            if (!parameter.IgnoreVersion)
            {
                AppendSql(" + 1 ");
            }
            else
            {
                AppendSql(" ");

            }

        }


        private bool UseStandardDialect => (_config.DbConnectionKind == DbConnectionKind.Odbc ||
                                            _config.DbConnectionKind == DbConnectionKind.OleDb);


        internal bool IncludeCurrentTimestampColumn(QueryBuilderParameter parameter,
            EntityColumnInfo columnInfo)
        {
            return !UseStandardDialect && columnInfo.CurrentTimestampAttribute.IsAvailable(parameter.SqlKind);
        }


        internal void AppendCurrentTimestamp(QueryBuilderParameter parameter,
            EntityColumnInfo columnInfo,
            int counter)
        {
            AppendComma(counter);
            if (parameter.SqlKind == SqlKind.Insert)
            {
                AppendLine(columnInfo.CurrentTimestampAttribute.Sql);
                return;
            }
            // update
            var columnName = parameter.Config.Dialect.ApplyQuote(columnInfo.ColumnName);
            AppendSql(columnName);
            AppendSql(" = ");
            AppendSql(columnInfo.CurrentTimestampAttribute.Sql);
            AppendLine();
        }


        internal bool IncludeCurrentUserColumn(QueryBuilderParameter parameter,
            EntityColumnInfo columnInfo)
        {
            return columnInfo.CurrentUserAttribute.IsAvailable(parameter.SqlKind);
        }


        internal void AppendCurrentUser(QueryBuilderParameter parameter,
            EntityColumnInfo columnInfo,
            int counter)
        {
            AppendComma(counter);
            if (parameter.SqlKind == SqlKind.Insert)
            {
                AppendParameter(columnInfo.PropertyInfo, parameter.CurrentUser);
                AppendLine();
                return;
            }

            // update
            var columnName = parameter.Config.Dialect.ApplyQuote(columnInfo.ColumnName);
            AppendSql(columnName);
            AppendSql(" = ");
            AppendParameter(columnInfo.PropertyInfo, parameter.CurrentUser);
            AppendLine();

        }


        internal void AppendSoftDeleteKey(QueryBuilderParameter parameter,
            EntityColumnInfo columnInfo,
            int counter)
        {
            AppendComma(counter);
            if (parameter.SqlKind != SqlKind.Insert)
            {
                var columnName = parameter.Config.Dialect.ApplyQuote(columnInfo.ColumnName);
                AppendSql(columnName);
                AppendSql(" = ");
            }
            var flgValue = parameter.SqlKind == SqlKind.SoftDelete ? "1" : "0";
            if (columnInfo.PropertyInfo.PropertyType == typeof(string))
            {
                AppendSql($"'{flgValue}'");
            }
            else
            {
                AppendSql(flgValue);
            }
            AppendLine();
        }


        internal QueryBuilderResult GetResult()
        {
            return new QueryBuilderResult
                   {
                       ParsedSql = _rawSqlBuilder.ToString(),
                       DebugSql = _formattedSqlBuilder.ToString(),
                       DbDataParameters = _sqlParameters.Values.ToList().AsReadOnly()
                   };
        }
    }


}
