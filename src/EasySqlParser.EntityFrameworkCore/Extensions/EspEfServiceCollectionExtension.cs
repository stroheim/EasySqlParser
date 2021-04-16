using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Configurations;
using EasySqlParser.SqlGenerator.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasySqlParser.EntityFrameworkCore.Extensions
{
    public static class EspEfServiceCollectionExtension
    {
        public static IServiceCollection AddQueryBuilderConfiguration(
            this IServiceCollection services,
            int commandTimeout = 30,
            bool writeIndented = true,
            QueryBehavior queryBehavior = QueryBehavior.None,
            ExcludeNullBehavior excludeNullBehavior = ExcludeNullBehavior.NullOnly,
            IEnumerable<Assembly> additionalAssemblies = null)
        {
            services
                .AddSingleton<IQueryBuilderConfiguration>(s =>
                                                          {
                                                              var dbContext = s.GetRequiredService<DbContext>();
                                                              var logger =
                                                                  s.GetRequiredService<
                                                                      ILogger<EfCoreQueryBuilderConfiguration>>();
                                                              return new EfCoreQueryBuilderConfiguration(dbContext,
                                                                  logger,
                                                                  commandTimeout,
                                                                  writeIndented,
                                                                  queryBehavior,
                                                                  excludeNullBehavior,
                                                                  additionalAssemblies);
                                                          });
            return services;
        }
    }
}
