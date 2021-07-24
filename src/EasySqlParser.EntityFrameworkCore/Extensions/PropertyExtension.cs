using System;
using System.Collections.Generic;
using System.Text;
using EasySqlParser.SqlGenerator.Attributes;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EasySqlParser.EntityFrameworkCore.Extensions
{
    public static class PropertyExtension
    {
        public static CurrentTimestampAttribute GetCurrentTimestampAttribute(
            this IProperty property)
        {
            var annotation = property.GetAnnotation(EspAnnotationNames.CurrentTimestamp);
            return (CurrentTimestampAttribute) annotation?.Value;
        }

        public static SoftDeleteKeyAttribute GetSoftDeleteKeyAttribute(
            this IProperty property)
        {
            var annotation = property.GetAnnotation(EspAnnotationNames.SoftDeleteKey);
            return (SoftDeleteKeyAttribute) annotation?.Value;
        }

        public static VersionAttribute GetVersionAttribute(
            this IProperty property)
        {
            var annotation = property.GetAnnotation(EspAnnotationNames.Version);
            return (VersionAttribute) annotation?.Value;
        }

        public static SequenceGeneratorAttribute GetSequenceGeneratorAttribute(
            this IProperty property)
        {
            var annotation = property.GetAnnotation(EspAnnotationNames.SequenceGenerator);
            return (SequenceGeneratorAttribute) annotation?.Value;
        }
    }
}
