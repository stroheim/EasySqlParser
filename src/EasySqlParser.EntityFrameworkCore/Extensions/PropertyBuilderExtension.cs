using EasySqlParser.SqlGenerator.Attributes;
using EasySqlParser.SqlGenerator.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasySqlParser.EntityFrameworkCore.Extensions
{
    /// <summary>
    ///     EasySqlParser specific extension methods for <see cref="PropertyBuilder" />.
    /// </summary>
    public static class PropertyBuilderExtension
    {
        /// <summary>
        ///     Configures the property to automatically set the current timestamp.
        /// </summary>
        /// <param name="propertyBuilder"></param>
        /// <param name="sql">current timestamp as an SQL statement</param>
        /// <param name="strategy">strategy for generating current timestamp</param>
        /// <returns></returns>
        public static PropertyBuilder HasCurrentTimestamp(
            this PropertyBuilder propertyBuilder,
            string sql,
            GenerationStrategy strategy = GenerationStrategy.Always)
        {
            propertyBuilder.Metadata.AddAnnotation(EspAnnotationNames.CurrentTimestamp,
                new CurrentTimestampAttribute(sql, strategy));
            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property to automatically set the current timestamp.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertyBuilder"></param>
        /// <param name="sql">current timestamp as an SQL statement</param>
        /// <param name="strategy">strategy for generating current timestamp</param>
        /// <returns></returns>
        public static PropertyBuilder<TProperty> HasCurrentTimestamp<TProperty>(
            this PropertyBuilder<TProperty> propertyBuilder,
            string sql,
            GenerationStrategy strategy = GenerationStrategy.Always)
            => (PropertyBuilder<TProperty>) HasCurrentTimestamp((PropertyBuilder) propertyBuilder, sql, strategy);


        /// <summary>
        ///     Configures the property for soft delete.
        /// </summary>
        /// <param name="propertyBuilder"></param>
        /// <returns></returns>
        public static PropertyBuilder HasSoftDeleteKey(
            this PropertyBuilder propertyBuilder)
        {
            propertyBuilder.Metadata.AddAnnotation(EspAnnotationNames.SoftDeleteKey,
                new SoftDeleteKeyAttribute());
            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property for soft delete.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertyBuilder"></param>
        /// <returns></returns>
        public static PropertyBuilder<TProperty> HasSoftDeleteKey<TProperty>(
            this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>) HasSoftDeleteKey((PropertyBuilder) propertyBuilder);


        /// <summary>
        ///     Configures the property for optimistic concurrency.
        /// </summary>
        /// <param name="propertyBuilder"></param>
        /// <returns></returns>
        public static PropertyBuilder HasVersion(
            this PropertyBuilder propertyBuilder)
        {
            propertyBuilder.Metadata.AddAnnotation(EspAnnotationNames.Version,
                new VersionAttribute());
            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property for optimistic concurrency.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertyBuilder"></param>
        /// <returns></returns>
        public static PropertyBuilder<TProperty> HasVersion<TProperty>(
            this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>) HasVersion((PropertyBuilder) propertyBuilder);

        /// <summary>
        ///     Configures the property to generate a value using a sequence.
        /// </summary>
        /// <param name="propertyBuilder"></param>
        /// <param name="sequenceName">the sequence name</param>
        /// <param name="schemaName">the schema name</param>
        /// <param name="prefix">prefix to the generated sequence</param>
        /// <param name="paddingLength">zero padding length to the generated sequence</param>
        /// <returns></returns>
        public static PropertyBuilder HasSequenceGenerator(
            this PropertyBuilder propertyBuilder,
            string sequenceName,
            string schemaName = null,
            string prefix = null,
            int paddingLength = 0)
        {
            propertyBuilder.Metadata.AddAnnotation(EspAnnotationNames.SequenceGenerator,
                new SequenceGeneratorAttribute(sequenceName)
                {
                    SchemaName = schemaName,
                    Prefix = prefix,
                    PaddingLength = paddingLength
                });
            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property to generate a value using a sequence.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertyBuilder"></param>
        /// <param name="sequenceName">the sequence name</param>
        /// <param name="schemaName">the schema name</param>
        /// <param name="prefix">prefix to the generated sequence</param>
        /// <param name="paddingLength">zero padding length to the generated sequence</param>
        /// <returns></returns>
        public static PropertyBuilder<TProperty> HasSequenceGenerator<TProperty>(
            this PropertyBuilder<TProperty> propertyBuilder,
            string sequenceName,
            string schemaName = null,
            string prefix = null,
            int paddingLength = 0)
            => (PropertyBuilder<TProperty>) HasSequenceGenerator((PropertyBuilder) propertyBuilder, sequenceName,
                schemaName, prefix, paddingLength);

    }
}
