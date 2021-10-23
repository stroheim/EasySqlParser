﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using EasySqlParser.Extensions;
using EasySqlParser.SqlGenerator.Attributes;
using EasySqlParser.SqlGenerator.Attributes.Extensions;
using EasySqlParser.SqlGenerator.Enums;
using EasySqlParser.SqlGenerator.Metadata;

namespace EasySqlParser.SqlGenerator.Helpers
{
    /// <summary>
    ///     A class for building entity information.
    /// </summary>
    internal static class EntityTypeInfoBuilderHelper
    {
        /// <summary>
        ///     Build entity information from assembly.
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static KeyValuePair<Type, EntityTypeInfo>[] Build(IEnumerable<Assembly> assemblies)
        {
            var types = new List<Type>();
            foreach (var assembly in assemblies)
            {
                var targetTypes = assembly.GetTypes()
                    .Where(t => t.GetCustomAttribute<EntityAttribute>() != null);
                types.AddRange(targetTypes);

            }

            return types.Select(type =>
                                {
                                    var entityInfo = Build(type);
                                    return new KeyValuePair<Type, EntityTypeInfo>(type, entityInfo);
                                }).ToArray();

        }

        /// <summary>
        ///     Build entity information from the type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static EntityTypeInfo Build(Type type)
        {
            var entityInfo = new EntityTypeInfo
            {
                TableName = type.Name
            };
            var naming = Naming.None;
            var entityAttr = type.GetCustomAttribute<EntityAttribute>();
            if (entityAttr != null)
            {
                naming = entityAttr.Naming;
            }
            var table = type.GetCustomAttribute<TableAttribute>();
            if (table != null)
            {
                entityInfo.TableName = table.Name;
                entityInfo.SchemaName = table.Schema;
            }
            else
            {
                entityInfo.TableName = naming.Apply(type.Name);
            }

            var props = type.GetProperties();
            var columns = new List<EntityColumnInfo>();
            var keyColumns = new List<EntityColumnInfo>();
            var sequenceColumns = new List<EntityColumnInfo>();
            var hasSoftDeleteKey = false;
            foreach (var propertyInfo in props)
            {
                var notMapped = propertyInfo.GetCustomAttribute<NotMappedAttribute>();
                if (notMapped != null)
                {
                    continue;
                }

                var columnInfo = new EntityColumnInfo
                {
                    PropertyInfo = propertyInfo
                };

                var supportedType = false;
                if (propertyInfo.PropertyType.IsEspKnownType())
                {
                    supportedType = true;
                }
                else if (propertyInfo.PropertyType.BaseType != null && propertyInfo.PropertyType.BaseType == typeof(Enum))
                {
                    supportedType = true;
                }
                if (!supportedType) continue;

                if (propertyInfo.PropertyType == typeof(DateTime) ||
                    propertyInfo.PropertyType == typeof(DateTime?) ||
                    propertyInfo.PropertyType == typeof(DateTimeOffset) ||
                    propertyInfo.PropertyType == typeof(DateTimeOffset?))
                {
                    columnInfo.IsDateTime = true;
                }

                if (propertyInfo.PropertyType.IsNullable())
                {
                    columnInfo.NullableUnderlyingType = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
                }


                var column = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                columnInfo.ColumnName = propertyInfo.Name;
                columnInfo.DbType = propertyInfo.PropertyType.ResolveDbType();
                if (column != null)
                {
                    columnInfo.ColumnName = column.Name;
                    columnInfo.TypeName = column.TypeName;
                }
                else
                {
                    columnInfo.ColumnName = naming.Apply(propertyInfo.Name);
                }

                var identityAttr = propertyInfo.GetCustomAttribute<DatabaseGeneratedAttribute>();
                if (identityAttr != null && identityAttr.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)
                {
                    columnInfo.IsIdentity = true;
                }

                var versionAttr = propertyInfo.GetCustomAttribute<VersionAttribute>();
                if (versionAttr != null)
                {
                    columnInfo.IsVersion = true;
                }

                var currentTimestampAttr = propertyInfo.GetCustomAttribute<CurrentTimestampAttribute>();
                if (currentTimestampAttr != null)
                {
                    columnInfo.CurrentTimestampAttribute = currentTimestampAttr;
                    columnInfo.IsCurrentTimestamp = true;
                }

                var softDeleteKeyAttr = propertyInfo.GetCustomAttribute<SoftDeleteKeyAttribute>();
                if (softDeleteKeyAttr != null)
                {
                    columnInfo.IsSoftDeleteKey = true;
                    hasSoftDeleteKey = true;
                }

                var lengthAttr = propertyInfo.GetCustomAttribute<StringLengthAttribute>();
                if (lengthAttr != null)
                {
                    columnInfo.StringMaxLength = lengthAttr.MaximumLength;
                }

                var seqAttr = propertyInfo.GetCustomAttribute<SequenceGeneratorAttribute>();
                if (seqAttr != null)
                {
                    columnInfo.IsSequence = true;
                    columnInfo.SequenceGeneratorAttribute = seqAttr;
                    sequenceColumns.Add(columnInfo.Clone());
                }

                var keyAttr = propertyInfo.GetCustomAttribute<KeyAttribute>();
                if (keyAttr != null)
                {
                    columnInfo.IsPrimaryKey = true;
                    keyColumns.Add(columnInfo.Clone());
                }

                if (columnInfo.IsPrimaryKey && columnInfo.IsIdentity)
                {
                    entityInfo.IdentityColumn = columnInfo.Clone();
                }

                if (columnInfo.IsVersion)
                {
                    entityInfo.VersionColumn = columnInfo.Clone();
                }

                columns.Add(columnInfo);
            }

            entityInfo.HasSoftDeleteKey = hasSoftDeleteKey;
            entityInfo.Columns = columns.AsReadOnly();
            entityInfo.KeyColumns = keyColumns.AsReadOnly();
            entityInfo.SequenceColumns = sequenceColumns.AsReadOnly();
            entityInfo.ColumnNameKeyDictionary = columns.ToDictionary(x => x.ColumnName, x => x);
            return entityInfo;
        }
    }
}