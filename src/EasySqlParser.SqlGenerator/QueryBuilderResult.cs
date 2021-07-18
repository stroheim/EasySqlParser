using System.Collections.Generic;
using System.Data;

namespace EasySqlParser.SqlGenerator
{
    /// <summary>
    ///     Object that stores the generation result of SQL statement.
    /// </summary>
    public sealed class QueryBuilderResult
    {
        /// <summary>
        ///     Parsed SQL statement.
        ///     Dynamic conditional expressions are replaced by SQL parameters <see cref="IDbDataParameter"/>. 
        /// </summary>
        public string ParsedSql { get; internal set; }

        /// <summary>
        ///     Parsed SQL statement for debug.
        ///     Dynamic conditional expressions are replaced by actual value.
        ///     It can be used for logging.
        /// </summary>
        public string DebugSql { get; internal set; }

        /// <summary>
        ///     SQL parameters generated from dynamic conditional expressions.
        /// </summary>
        public IReadOnlyList<IDbDataParameter> DbDataParameters { get; internal set; }

    }
}
