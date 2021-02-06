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


        public static string GetQuotedName(this SqlParserConfig config, string name)
        {
            return config.Dialect.ApplyQuote(name);
        }
    }
}
