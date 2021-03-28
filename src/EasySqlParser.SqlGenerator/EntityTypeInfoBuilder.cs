using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using EasySqlParser.Extensions;

namespace EasySqlParser.SqlGenerator
{
    public static class EntityTypeInfoBuilder
    {
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

                if (!propertyInfo.PropertyType.IsEspKnownType()) continue;

                if (propertyInfo.PropertyType == typeof(DateTime) ||
                    propertyInfo.PropertyType == typeof(DateTime?) ||
                    propertyInfo.PropertyType == typeof(DateTimeOffset) ||
                    propertyInfo.PropertyType == typeof(DateTimeOffset?))
                {
                    columnInfo.IsDateTime = true;
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

                var currentUserAttr = propertyInfo.GetCustomAttribute<CurrentUserAttribute>();
                if (currentUserAttr != null)
                {
                    columnInfo.CurrentUserAttribute = currentUserAttr;
                    columnInfo.IsCurrentUser = true;
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
            return entityInfo;
        }
    }
}
