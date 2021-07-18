using System;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasySqlParser.EntityFrameworkCore.Extensions
{
    /// <summary>
    ///     Extension methods for setting up EasySqlParser SqlGenerator related services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class EspEfServiceCollectionExtension
    {

        /// <summary>
        ///     Registers the EasySqlParser services in the <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddQueryBuilderConfiguration(
            this IServiceCollection services)
        {
            services.TryAddSingleton<IQueryBuilderConfiguration, EfCoreQueryBuilderConfiguration>();
            //services.TryAddScoped<ISqlContext, EfCoreSqlContext>();
            services.TryAddScoped(typeof(ISqlContext<>), typeof(EfCoreSqlContext<>));
            return services;
        }

        /// <summary>
        ///     Registers the EasySqlParser services in the <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddQueryBuilderConfiguration(
            this IServiceCollection services,
            Action<QueryBuilderConfigurationOptions> options)
        {
            services.Configure(options);
            services.TryAddSingleton<IQueryBuilderConfiguration, EfCoreQueryBuilderConfiguration>();
            //services.TryAddScoped<ISqlContext, EfCoreSqlContext>();
            services.TryAddScoped(typeof(ISqlContext<>), typeof(EfCoreSqlContext<>));
            return services;
        }
    }
}
