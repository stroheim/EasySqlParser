using System.Collections.Generic;
using System.Data;

namespace EasySqlParser
{
    /// <summary>
    /// Object storing the result of parsing SQL statement for "Paginated".
    /// </summary>
    public sealed class SqlParserResultPaginated
    {
        /// <summary>
        /// <see cref="SqlParserResult"/> for gets data.
        /// </summary>
        public SqlParserResult Result { get; internal set; }

        /// <summary>
        /// <see cref="SqlParserResult"/> for gets count.
        /// </summary>
        public SqlParserResult CountResult { get; internal set; }
    }


    /// <summary>
    /// Object storing the result of parsing SQL statement.
    /// </summary>
    public sealed class SqlParserResult
    {
        /// <summary>
        /// Parsed SQL statement.
        /// Dynamic conditional expressions are replaced by SQL parameters <see cref="IDbDataParameter"/>. 
        /// </summary>
        public string ParsedSql { get; internal set; }

        /// <summary>
        /// Parsed SQL statement for debug.
        /// Dynamic conditional expressions are replaced by actual value.
        /// It can be used for logging.
        /// </summary>
        public string DebugSql { get; internal set; }

        /// <summary>
        /// SQL parameters generated from dynamic conditional expressions.
        /// </summary>
        public List<IDbDataParameter> DbDataParameters { get; internal set; }
    }
}
