using System;
using EasySqlParser.SqlGenerator.Enums;
using EasySqlParser.SqlGenerator.Metadata;

namespace EasySqlParser.SqlGenerator.Configurations
{
    /// <summary>
    ///     Configuration of query builder.
    /// </summary>
    public interface IQueryBuilderConfiguration
    {
        /// <summary>
        ///     Gets the command timeout (in seconds).
        /// </summary>
        int CommandTimeout { get; }

        /// <summary>
        ///     Gets a value that defines whether SQL should use pretty printing. 
        /// </summary>
        bool WriteIndented { get; }

        /// <summary>
        ///     Gets the <see cref="QueryBehavior"/> .
        /// </summary>
        QueryBehavior QueryBehavior { get; }

        /// <summary>
        ///     Gets the <see cref="ExcludeNullBehavior"/> .
        /// </summary>
        ExcludeNullBehavior ExcludeNullBehavior { get; }

        /// <summary>
        ///     Gets a action delegate for logging.
        /// </summary>
        Action<string> LoggerAction { get; }

        /// <summary>
        ///     Gets a <see cref="EntityTypeInfo"/> by entity type.
        /// </summary>
        /// <param name="type">type of entity</param>
        /// <returns></returns>
        EntityTypeInfo GetEntityTypeInfo(Type type);


    }
}
