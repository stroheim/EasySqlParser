using System;
using EasySqlParser.SqlGenerator.Enums;
using EasySqlParser.SqlGenerator.Metadata;

namespace EasySqlParser.SqlGenerator.Configurations
{
    /// <summary>
    ///     Abstract implementation of <see cref="IQueryBuilderConfiguration"/>.
    /// </summary>
    public abstract class QueryBuilderConfigurationBase : IQueryBuilderConfiguration
    {
        /// <inheritdoc />
        public int CommandTimeout { get; protected set; }

        /// <inheritdoc />
        public bool WriteIndented { get; protected set; }

        /// <inheritdoc />
        public QueryBehavior QueryBehavior { get; protected set; }

        /// <inheritdoc />
        public ExcludeNullBehavior ExcludeNullBehavior { get; protected set; }

        /// <inheritdoc />
        public Action<string> LoggerAction { get; protected set; }

        /// <inheritdoc />
        public abstract EntityTypeInfo GetEntityTypeInfo(Type type);

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryBuilderConfigurationBase"/> class.
        /// </summary>
        /// <param name="options">The <see cref="QueryBuilderConfigurationOptions"/></param>
        /// <param name="loggerAction">Action delegate for logging.</param>
        protected QueryBuilderConfigurationBase(
            QueryBuilderConfigurationOptions options = null,
            Action<string> loggerAction = null
        )
        {
            CommandTimeout = options?.CommandTimeout ?? 30;
            WriteIndented = options?.WriteIndented ?? true;
            QueryBehavior = options?.QueryBehavior ?? QueryBehavior.None;
            ExcludeNullBehavior = options?.ExcludeNullBehavior ?? ExcludeNullBehavior.NullOnly;
            LoggerAction = loggerAction;
        }

        //protected QueryBuilderConfigurationBase(
        //    IEnumerable<Assembly> entityAssemblies,
        //    int commandTimeout = 30,
        //    bool writeIndented = true,
        //    QueryBehavior queryBehavior = QueryBehavior.None,
        //    ExcludeNullBehavior excludeNullBehavior = ExcludeNullBehavior.NullOnly,
        //    Action<string> loggerAction = null
        //)
        //{
        //    _assemblies = entityAssemblies;
        //    CommandTimeout = commandTimeout;
        //    WriteIndented = writeIndented;
        //    QueryBehavior = queryBehavior;
        //    ExcludeNullBehavior = excludeNullBehavior;
        //    LoggerAction = loggerAction;
        //    BuildCache();
        //}
    }
}
