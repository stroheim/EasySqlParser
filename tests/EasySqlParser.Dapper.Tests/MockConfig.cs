using System;
using System.Collections.Concurrent;
using System.Reflection;
using EasySqlParser.SqlGenerator.Configurations;
using EasySqlParser.SqlGenerator.Enums;
using EasySqlParser.SqlGenerator.Metadata;

namespace EasySqlParser.Dapper.Tests
{
    internal class MockConfig : DapperQueryBuilderConfiguration
    {
        public MockConfig(QueryBehavior queryBehavior, Action<string> loggerAction)
            : base(new Assembly[] {typeof(MockConfig).Assembly}, queryBehavior: queryBehavior,
                loggerAction: loggerAction)
        {
            //CommandTimeout = 30;
            //WriteIndented = false;
            //QueryBehavior = queryBehavior;
            //LoggerAction = loggerAction;
            //BuildCache();
        }

        //public int CommandTimeout { get; }
        //public bool WriteIndented { get; internal set; }
        //public QueryBehavior QueryBehavior { get; internal set; }
        //public ExcludeNullBehavior ExcludeNullBehavior { get; internal set; }
        //public Action<string> LoggerAction { get; }

        //private static readonly ConcurrentDictionary<Type, EntityTypeInfo> Cache =
        //    new ConcurrentDictionary<Type, EntityTypeInfo>();

        //public void BuildCache()
        //{
        //    var assembly = typeof(MockConfig).Assembly;
        //    var values = EntityTypeInfoBuilder.Build(new[] {assembly});
        //    foreach (var pair in values)
        //    {
        //        Cache.GetOrAdd(pair.Key, pair.Value);
        //    }
        //}

        //public EntityTypeInfo GetEntityTypeInfo(Type type)
        //{
        //    return Cache[type];
        //}
    }

}
