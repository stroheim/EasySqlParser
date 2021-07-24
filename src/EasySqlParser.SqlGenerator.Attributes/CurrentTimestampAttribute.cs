using System;
using EasySqlParser.SqlGenerator.Enums;

namespace EasySqlParser.SqlGenerator.Attributes
{
    /// <summary>
    ///     Attribute for generating the current timestamp as an SQL statement.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CurrentTimestampAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrentTimestampAttribute"/> class.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="strategy"></param>
        public CurrentTimestampAttribute(
            string sql,
            GenerationStrategy strategy = GenerationStrategy.Always
        )
        {
            Strategy = strategy;
            Sql = sql;
        }

        /// <summary>
        ///     Gets the <see cref="GenerationStrategy"/>.
        /// </summary>
        public GenerationStrategy Strategy { get; }

        /// <summary>
        ///     Gets the current timestamp as an SQL statement.
        /// </summary>
        public string Sql { get; }

    }
}
