using EasySqlParser.SqlGenerator.Attributes;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EasySqlParser.EntityFrameworkCore.Extensions
{
    /// <summary>
    ///     Extension methods for <see cref="IProperty" /> for EasySqlParser.
    /// </summary>
    public static class PropertyExtension
    {
        /// <summary>
        ///     Gets the <see cref="CurrentTimestampAttribute"/>.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static CurrentTimestampAttribute GetCurrentTimestampAttribute(
            this IProperty property)
        {
            var annotation = property.FindAnnotation(EspAnnotationNames.CurrentTimestamp);
            return (CurrentTimestampAttribute) annotation?.Value;
        }

        /// <summary>
        ///     Gets the <see cref="SoftDeleteKeyAttribute"/>.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static SoftDeleteKeyAttribute GetSoftDeleteKeyAttribute(
            this IProperty property)
        {
            var annotation = property.FindAnnotation(EspAnnotationNames.SoftDeleteKey);
            return (SoftDeleteKeyAttribute) annotation?.Value;
        }

        /// <summary>
        ///     Gets the <see cref="VersionAttribute"/>.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static VersionAttribute GetVersionAttribute(
            this IProperty property)
        {
            var annotation = property.FindAnnotation(EspAnnotationNames.Version);
            return (VersionAttribute) annotation?.Value;
        }

        /// <summary>
        ///     Gets the <see cref="SequenceGeneratorAttribute"/>.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static SequenceGeneratorAttribute GetSequenceGeneratorAttribute(
            this IProperty property)
        {
            var annotation = property.FindAnnotation(EspAnnotationNames.SequenceGenerator);
            return (SequenceGeneratorAttribute) annotation?.Value;
        }

    }
}
