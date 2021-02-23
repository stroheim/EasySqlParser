using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using EasySqlParser.Configurations;
using EasySqlParser.Internals;

namespace EasySqlParser.Extensions
{
    // TODO: DOC
    /// <summary>
    /// 
    /// </summary>
    public static class SqlParserConfigExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ConvertToLogFormat(this SqlParserConfig config, object value)
        {
            return ValueObject.ToLogFormat(config.Dialect, value);
        }

        public static string GetParameterName(this SqlParserConfig config, string propertyName)
        {
            return config.Dialect.ParameterPrefix + propertyName;
        }

        public static (string parameterName, IDbDataParameter parameter) CreateDbParameter(this SqlParserConfig config, string parameterKey, object parameterValue)
        {
            var param = config.DataParameterCreator()
                .AddName(parameterKey)
                .AddValue(parameterValue ?? DBNull.Value);
            // parameterKey is sql parameter name
            var localParameterKey = parameterKey;
            if (!config.Dialect.EnableNamedParameter)
            {
                localParameterKey = config.Dialect.ParameterPrefix;
            }

            return (localParameterKey, param);

        }

        public static (string parameterName, IDbDataParameter parameter) CreateDbReturnParameter(this SqlParserConfig config,
            string parameterKey)
        {
            var param = config.DataParameterCreator()
                .AddName(parameterKey);
            param.Direction = ParameterDirection.ReturnValue;

            var localParameterKey = parameterKey;
            if (!config.Dialect.EnableNamedParameter)
            {
                localParameterKey = config.Dialect.ParameterPrefix;
            }

            return (localParameterKey, param);
        }


        public static string GetQuotedName(this SqlParserConfig config, string name)
        {
            return config.Dialect.ApplyQuote(name);
        }

        public static bool SupportsIdentity(this SqlParserConfig config)
        {
            return config.Dialect.SupportsIdentity;
        }

        public static bool SupportsSequence(this SqlParserConfig config)
        {
            return config.Dialect.SupportsSequence;
        }

        public static bool SupportsFinalTable(this SqlParserConfig config)
        {
            return config.Dialect.SupportsFinalTable;
        }

        public static bool SupportsReturning(this SqlParserConfig config)
        {
            return config.Dialect.SupportsReturning;
        }

        public static string GetIdentityWhereClause(this SqlParserConfig config, string columnName)
        {
            return config.Dialect.GetIdentityWhereClause(columnName);
        }

        public static string GetStatementTerminator(this SqlParserConfig config)
        {
            return config.Dialect.StatementTerminator;
        }

        public static bool UseSqlite(this SqlParserConfig config)
        {
            return config.Dialect.UseSqlite;
        }


        public static string GetNextSequenceSql(this SqlParserConfig config, string sequenceName, string schema)
        {
            return config.Dialect.GetNextSequenceSql(sequenceName, schema);
        }

        public static string GetNextSequenceSqlZeroPadding(this SqlParserConfig config, string sequenceName,
            string schema, int length, string prefix = null)
        {
            return config.Dialect.GetNextSequenceSqlZeroPadding(sequenceName, schema, length, prefix);
        }
    }
}
