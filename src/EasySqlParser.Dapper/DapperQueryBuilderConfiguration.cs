using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using EasySqlParser.SqlGenerator;

namespace EasySqlParser.Dapper
{
    public class DapperQueryBuilderConfiguration : QueryBuilderConfigurationBase
    {
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
            DapperMapBuilder.CreateMap(entityAssemblies);
        }
    }

    internal static class DapperMapBuilder
    {
        internal static void CreateMap(IEnumerable<Assembly> assemblies)
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

        }
    }
}
