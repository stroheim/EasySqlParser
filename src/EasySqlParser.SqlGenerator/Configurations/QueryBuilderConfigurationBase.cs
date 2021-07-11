using System;
using System.Collections.Generic;
using System.Reflection;
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
        public int CommandTimeout { get; }

        /// <inheritdoc />
        public bool WriteIndented { get; }

        /// <inheritdoc />
        public QueryBehavior QueryBehavior { get; }

        /// <inheritdoc />
        public ExcludeNullBehavior ExcludeNullBehavior { get; }

        /// <inheritdoc />
        public Action<string> LoggerAction { get; set; }

        /// <inheritdoc />
        public virtual EntityTypeInfo GetEntityTypeInfo(Type type)
        {
            return _hashDictionary.Get(type);
        }

        private readonly IEnumerable<Assembly> _assemblies;
        private static TypeHashDictionary<EntityTypeInfo> _hashDictionary;

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryBuilderConfigurationBase"/> class.
        /// </summary>
        /// <param name="entityAssemblies">Assemblies containing an entity.</param>
        /// <param name="options">The <see cref="QueryBuilderConfigurationOptions"/></param>
        /// <param name="loggerAction">Action delegate for logging.</param>
        protected QueryBuilderConfigurationBase(
            IEnumerable<Assembly> entityAssemblies,
            QueryBuilderConfigurationOptions options = null,
            Action<string> loggerAction = null
        )
        {
            _assemblies = entityAssemblies;
            CommandTimeout = options?.CommandTimeout ?? 30;
            WriteIndented = options?.WriteIndented ?? true;
            QueryBehavior = options?.QueryBehavior ?? QueryBehavior.None;
            ExcludeNullBehavior = options?.ExcludeNullBehavior ?? ExcludeNullBehavior.NullOnly;
            LoggerAction = loggerAction;
            BuildCache();
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


        private void BuildCache()
        {
            if (_assemblies == null) return;
            InternalBuildCache();
        }

        /// <summary>
        ///     Build caches for <see cref="EntityTypeInfo"/>.
        /// </summary>
        protected virtual void InternalBuildCache()
        {
            var prepare = EntityTypeInfoBuilder.Build(_assemblies);
            _hashDictionary = TypeHashDictionary<EntityTypeInfo>.Create(prepare);
        }

    }
}
