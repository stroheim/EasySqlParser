using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Configurations;
using EasySqlParser.SqlGenerator.Enums;
using EasySqlParser.SqlGenerator.Metadata;

namespace EasySqlParser.Dapper
{
    public class DapperQueryBuilderConfiguration : QueryBuilderConfigurationBase
    {
        private readonly IEnumerable<Assembly> _entityAssemblies;
        private static TypeHashDictionary<EntityTypeInfo> _hashDictionary;
        

        public DapperQueryBuilderConfiguration(
            IEnumerable<Assembly> entityAssemblies,
            int commandTimeout = 30,
            bool writeIndented = true,
            QueryBehavior queryBehavior = QueryBehavior.None,
            ExcludeNullBehavior excludeNullBehavior = ExcludeNullBehavior.NullOnly,
            Action<string> loggerAction = null) : base(
            null,
            commandTimeout,
            writeIndented,
            queryBehavior,
            excludeNullBehavior,
            loggerAction)
        {
            _entityAssemblies = entityAssemblies;
        }

        protected override void InternalBuildCache()
        {
            var prepare= DapperMapBuilder.CreateMap(_entityAssemblies);
            _hashDictionary = TypeHashDictionary<EntityTypeInfo>.Create(prepare);
        }

        public override EntityTypeInfo GetEntityTypeInfo(Type type)
        {
            return _hashDictionary.Get(type);
        }
    }

    internal static class DapperMapBuilder
    {
        internal static KeyValuePair<Type, EntityTypeInfo>[] CreateMap(IEnumerable<Assembly> assemblies)
        {
            var keyValuePairs = EntityTypeInfoBuilder.Build(assemblies);
            foreach (var pair in keyValuePairs)
            {
                var map = new CustomPropertyTypeMap(
                    pair.Key,
                    (type, columnName) =>
                    {
                        var columnInfo = pair.Value.Columns.SingleOrDefault(x => x.ColumnName == columnName);
                        return columnInfo?.PropertyInfo;
                    });
                SqlMapper.SetTypeMap(pair.Key, map);
            }

            return keyValuePairs;
        }
    }
}
