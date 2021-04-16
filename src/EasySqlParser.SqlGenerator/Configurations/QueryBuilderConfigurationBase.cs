using System;
using System.Collections.Generic;
using System.Reflection;
using EasySqlParser.SqlGenerator.Enums;
using EasySqlParser.SqlGenerator.Metadata;

namespace EasySqlParser.SqlGenerator.Configurations
{
    public abstract class QueryBuilderConfigurationBase : IQueryBuilderConfiguration
    {
        public int CommandTimeout { get; }
        public bool WriteIndented { get; }
        public QueryBehavior QueryBehavior { get; }
        public ExcludeNullBehavior ExcludeNullBehavior { get; }
        public Action<string> LoggerAction { get; set; }
        public virtual EntityTypeInfo GetEntityTypeInfo(Type type)
        {
            return _hashDictionary.Get(type);
        }
        private readonly IEnumerable<Assembly> _assemblies;
        private static TypeHashDictionary<EntityTypeInfo> _hashDictionary;

        protected QueryBuilderConfigurationBase(
            IEnumerable<Assembly> entityAssemblies,
            int commandTimeout = 30,
            bool writeIndented = true,
            QueryBehavior queryBehavior = QueryBehavior.None,
            ExcludeNullBehavior excludeNullBehavior = ExcludeNullBehavior.NullOnly,
            Action<string> loggerAction = null
        )
        {
            _assemblies = entityAssemblies;
            CommandTimeout = commandTimeout;
            WriteIndented = writeIndented;
            QueryBehavior = queryBehavior;
            ExcludeNullBehavior = excludeNullBehavior;
            LoggerAction = loggerAction;
            BuildCache();
        }

        private void BuildCache()
        {
            if (_assemblies == null) return;
            InternalBuildCache();
        }

        protected virtual void InternalBuildCache()
        {
            var prepare = EntityTypeInfoBuilder.Build(_assemblies);
            _hashDictionary = TypeHashDictionary<EntityTypeInfo>.Create(prepare);
        }

    }
}
