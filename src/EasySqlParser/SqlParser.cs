using System;
using System.Collections.Concurrent;
using System.IO;
using EasySqlParser.Configurations;
using EasySqlParser.Exceptions;
using EasySqlParser.Internals;
using EasySqlParser.Internals.Node;

namespace EasySqlParser
{
    /// <summary>
    /// "2 way sql" parser
    /// </summary>
    public sealed class SqlParser
    {
        private static readonly ConcurrentDictionary<string, SqlFileInfo> SqlCache =
            new ConcurrentDictionary<string, SqlFileInfo>();
        private readonly string _sqlFilePath;
        private readonly bool _isUserDefinedParameter;
        private readonly object _model;
        private readonly string _name;
        private readonly object _value;
        private readonly SqlParserConfig _config;
        private EasyExpressionEvaluator _evaluator;

        /// <summary>
        /// Create a new SqlParser instance <br/>
        /// Use this if you want to use a user-defined type
        /// </summary>
        /// <param name="sqlFilePath">file path for 2way sql</param>
        /// <param name="model">parameter object for 2way sql</param>
        /// <param name="config">
        /// configuration of SqlParser.<br/>
        /// The default value for this parameter is null.<br/>
        /// if this parameter is null, use default config.<br/>
        /// </param>
        public SqlParser(string sqlFilePath, object model, SqlParserConfig config = null)
        {
            _sqlFilePath = sqlFilePath;
            _model = model;
            _config = config ?? ConfigContainer.DefaultConfig;
            _isUserDefinedParameter = true;
        }


        /// <summary>
        /// Create a new SqlParser instance <br/>
        /// Use this if you don't want to use a user-defined type
        /// </summary>
        /// <param name="sqlFilePath">file path for 2way sql</param>
        /// <param name="name">parameter name</param>
        /// <param name="value">parameter value</param>
        /// <param name="config">
        /// configuration of SqlParser.<br/>
        /// The default value for this parameter is null.<br/>
        /// if this parameter is null, use default config.<br/>
        /// </param>
        public SqlParser(string sqlFilePath, string name, object value, SqlParserConfig config = null)
        {
            _sqlFilePath = sqlFilePath;
            _name = name;
            _value = value;
            _config = config ?? ConfigContainer.DefaultConfig;
            _isUserDefinedParameter = false;
        }

        internal ISqlNode PrepareParse()
        {

            if (!File.Exists(_sqlFilePath))
            {
                throw new InvalidSqlFileException(ExceptionMessageId.EspC001, _sqlFilePath);
            }

            var rawSql = File.ReadAllText(_sqlFilePath);
            if (string.IsNullOrWhiteSpace(rawSql))
            {
                throw new InvalidSqlFileException(ExceptionMessageId.EspC002, _sqlFilePath);
            }
            if (_config == null)
            {
                throw new InvalidSqlParserConfigException(ExceptionMessageId.EspD001);
            }

            if (_config.DbConnectionKind == DbConnectionKind.Unknown)
            {
                throw new InvalidSqlParserConfigException(ExceptionMessageId.EspD002);
            }

            if (_config.DataParameterCreator == null)
            {
                throw new InvalidSqlParserConfigException(ExceptionMessageId.EspD003);
            }

            _evaluator = new EasyExpressionEvaluator(_sqlFilePath);

            ISqlNode sqlNode = null;
            var hasCache = false;
            if (ConfigContainer.EnableCache)
            {
                if (SqlCache.TryGetValue(_sqlFilePath, out SqlFileInfo info))
                {
                    sqlNode = info.SqlNode;
                    hasCache = true;
                }
            }

            if (!hasCache)
            {
                var parser = new DomaSqlParser(rawSql);
                sqlNode = parser.Parse();
                if (ConfigContainer.EnableCache)
                {
                    var info = new SqlFileInfo
                               {
                                   FilePath = _sqlFilePath,
                                   RawSql = rawSql,
                                   SqlNode = sqlNode
                    };
                    SqlCache.TryAdd(_sqlFilePath, info);
                }
            }

            return sqlNode;
        }

        internal SqlFileInfo SqlFileInfo
        {
            get
            {
                if (SqlCache.TryGetValue(_sqlFilePath, out SqlFileInfo info))
                {
                    return info;
                }
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Parse the SQL statement.
        /// </summary>
        /// <returns>Result of parsing SQL statement</returns>
        public SqlParserResult Parse()
        {
            var sqlNode = PrepareParse();
            var builder = _isUserDefinedParameter
                ? new DomaSqlBuilder(sqlNode, _model, _config, _evaluator)
                : new DomaSqlBuilder(sqlNode, _name, _value, _config, _evaluator);
            return builder.Build();
        }

        /// <summary>
        /// Parse the SQL statement for "Paginated".
        /// </summary>
        /// <param name="skip">
        /// The number of records to skip.<br/>
        /// It's like LINQ.
        /// </param>
        /// <param name="take">
        /// The number of records to gets.<br/>
        /// It's like LINQ.
        /// </param>
        /// <param name="rowNumberColumn">
        /// If you want to force the use of the ROW_NUMBER function.<br/>
        /// Specify a virtual column name to receive the result of ROW_NUMBER function
        /// </param>
        /// <returns>Result of parsing SQL statement</returns>
        public SqlParserResultPaginated ParsePaginated(int skip, int take, string rowNumberColumn = null)
        {
            var result = new SqlParserResultPaginated();
            var sqlNode = PrepareParse();
            var nodeForCount = _config.Dialect.ToCountGettingSqlNode(sqlNode);
            var builderForCount = _isUserDefinedParameter
                ? new DomaSqlBuilder(nodeForCount, _model, _config, _evaluator)
                : new DomaSqlBuilder(nodeForCount, _name, _value, _config, _evaluator);
            result.CountResult = builderForCount.Build();
            var nodeForPaging = _config.Dialect.ToPagingSqlNode(sqlNode, skip, take, rowNumberColumn);
            var builderForPaging = _isUserDefinedParameter
                ? new DomaSqlBuilder(nodeForPaging, _model, _config, _evaluator)
                : new DomaSqlBuilder(nodeForPaging, _name, _value, _config, _evaluator);
            result.Result = builderForPaging.Build();
            return result;
        }

        /// <summary>
        /// Clear the cache.
        /// </summary>
        /// <param name="filePath"></param>
        public static void ClearCache(string filePath)
        {
            SqlCache.TryRemove(filePath, out _);
            EasyExpressionEvaluator.ClearCache(filePath);
        }

        /// <summary>
        /// Clear all caches.
        /// </summary>
        public static void ClearCacheAll()
        {
            SqlCache.Clear();
            EasyExpressionEvaluator.ClearCacheAll();
        }

    }
}
