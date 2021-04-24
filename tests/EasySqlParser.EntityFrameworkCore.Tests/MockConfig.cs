using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySqlParser.EntityFrameworkCore.Tests.SqlServer;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Configurations;
using EasySqlParser.SqlGenerator.Enums;
using EasySqlParser.SqlGenerator.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasySqlParser.EntityFrameworkCore.Tests
{
    internal class MockConfig : IQueryBuilderConfiguration
    {
        public MockConfig(QueryBehavior queryBehavior, Action<string> loggerAction)
        {
            CommandTimeout = 30;
            WriteIndented = true;
            QueryBehavior = queryBehavior;
            LoggerAction = loggerAction;
            //BuildCache();
        }

        public int CommandTimeout { get; }
        public bool WriteIndented { get; }
        public QueryBehavior QueryBehavior { get; }
        public ExcludeNullBehavior ExcludeNullBehavior { get; internal set; }
        public Action<string> LoggerAction { get; }
        public void BuildCache()
        {
            var assembly = typeof(MockConfig).Assembly;
            var types = assembly.GetTypes()
                .Where(t => t.BaseType != null && t.BaseType == typeof(DbContext));
            foreach (var type in types)
            {
                if (type == typeof(EfContext))
                {
                    // uuum
                    var options = new DbContextOptionsBuilder<EfContext>()
                        .UseInMemoryDatabase(databaseName: "EfContext_BuildCache")
                        .Options;
                    using var dbContext = new EfContext(options);
                    var values = EfCoreEntityTypeInfoBuilder.Build(dbContext);
                    foreach (var pair in values)
                    {
                        Cache.GetOrAdd(pair.Key, pair.Value);
                    }
                }
                
                continue;
            }
        }

        private static readonly ConcurrentDictionary<Type, EntityTypeInfo> Cache =
            new ConcurrentDictionary<Type, EntityTypeInfo>();

        internal void AddCache(DbContext dbContext)
        {
            var values = EfCoreEntityTypeInfoBuilder.Build(dbContext);
            foreach (var pair in values)
            {
                Cache.GetOrAdd(pair.Key, pair.Value);
            }

        }

        public EntityTypeInfo GetEntityTypeInfo(Type type)
        {
            return Cache[type];
        }
    }


}
