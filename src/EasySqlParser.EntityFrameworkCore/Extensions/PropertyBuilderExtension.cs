using EasySqlParser.SqlGenerator.Attributes;
using EasySqlParser.SqlGenerator.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasySqlParser.EntityFrameworkCore.Extensions
{
    public static class PropertyBuilderExtension
    {
        public static PropertyBuilder HasCurrentTimestamp(
            this PropertyBuilder propertyBuilder,
            string sql,
            GenerationStrategy strategy = GenerationStrategy.Always)
        {
            propertyBuilder.Metadata.AddAnnotation(EspAnnotationNames.CurrentTimestamp,
                new CurrentTimestampAttribute(sql, strategy));
            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> HasCurrentTimestamp<TProperty>(
            this PropertyBuilder<TProperty> propertyBuilder,
            string sql,
            GenerationStrategy strategy = GenerationStrategy.Always)
            => (PropertyBuilder<TProperty>) HasCurrentTimestamp((PropertyBuilder) propertyBuilder, sql, strategy);

        public static PropertyBuilder HasSoftDeleteKey(
            this PropertyBuilder propertyBuilder)
        {
            propertyBuilder.Metadata.AddAnnotation(EspAnnotationNames.SoftDeleteKey,
                new SoftDeleteKeyAttribute());
            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> HasSoftDeleteKey<TProperty>(
            this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>) HasSoftDeleteKey((PropertyBuilder) propertyBuilder);

        public static PropertyBuilder HasVersion(
            this PropertyBuilder propertyBuilder)
        {
            propertyBuilder.Metadata.AddAnnotation(EspAnnotationNames.Version,
                new VersionAttribute());
            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> HasVersion<TProperty>(
            this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>) HasVersion((PropertyBuilder) propertyBuilder);

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
