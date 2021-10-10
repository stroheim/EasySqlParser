using System;
using System.Collections.Generic;
using System.Reflection;
using Dapper;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Configurations;
using EasySqlParser.SqlGenerator.Helpers;
using EasySqlParser.SqlGenerator.Metadata;

namespace EasySqlParser.Dapper
{
    /// <summary>
    ///     <see cref="IQueryBuilderConfiguration"/> implementation for Dapper.
    /// </summary>
    public class DapperQueryBuilderConfiguration : QueryBuilderConfigurationBase
    {
        private static TypeHashDictionary<EntityTypeInfo> _hashDictionary;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DapperQueryBuilderConfiguration"/> class.
        /// </summary>
        /// <param name="entityAssemblies"></param>
        /// <param name="options"></param>
        /// <param name="loggerAction"></param>
        public DapperQueryBuilderConfiguration(
            IEnumerable<Assembly> entityAssemblies,
            QueryBuilderConfigurationOptions options,
            Action<string> loggerAction = null) 
            : base(
            options,
            loggerAction)
        {
            var builder = new DapperEntityTYpeInfoBuilder();
            var prepare = builder.Build(entityAssemblies);
            _hashDictionary = TypeHashDictionary<EntityTypeInfo>.Create(prepare);
        }

        /// <inheritdoc />
        public override EntityTypeInfo GetEntityTypeInfo(Type type)
        {
            return _hashDictionary.Get(type);
        }
    }

    internal static class DapperMapBuilder
    {
        internal static KeyValuePair<Type, EntityTypeInfo>[] CreateMap(IEnumerable<Assembly> assemblies)
        {
            var keyValuePairs = EntityTypeInfoBuilderHelper.Build(assemblies);
            foreach (var pair in keyValuePairs)
            {
                var map = new CustomPropertyTypeMap(
                    pair.Key,
                    (type, columnName) => pair.Value.ColumnNameKeyDictionary[columnName].PropertyInfo);
                SqlMapper.SetTypeMap(pair.Key, map);
            }

            return keyValuePairs;
        }
    }
}
