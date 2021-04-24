using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using EasySqlParser.Extensions;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Attributes;
using EasySqlParser.SqlGenerator.Configurations;
using EasySqlParser.SqlGenerator.Enums;
using EasySqlParser.SqlGenerator.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace EasySqlParser.EntityFrameworkCore
{
    public class EfCoreQueryBuilderConfiguration : QueryBuilderConfigurationBase
    {
        private readonly ILogger<EfCoreQueryBuilderConfiguration> _logger;
        private readonly DbContext _dbContext;
        private readonly IEnumerable<Assembly> _assemblies;
        private static TypeHashDictionary<EntityTypeInfo> _hashDictionary;


        protected override void InternalBuildCache()
        {
            var prepare = EfCoreEntityTypeInfoBuilder.Build(_dbContext, _assemblies);
            _hashDictionary = TypeHashDictionary<EntityTypeInfo>.Create(prepare);
        }

        public override EntityTypeInfo GetEntityTypeInfo(Type type)
        {
            return _hashDictionary.Get(type);
        }

        public virtual void WriteLog(string message)
        {
            _logger.LogDebug(message);
        }

        public EfCoreQueryBuilderConfiguration(
            DbContext dbContext,
            ILogger<EfCoreQueryBuilderConfiguration> logger,
            int commandTimeout = 30,
            bool writeIndented = true,
            QueryBehavior queryBehavior = QueryBehavior.None,
            ExcludeNullBehavior excludeNullBehavior = ExcludeNullBehavior.NullOnly,
            IEnumerable<Assembly> additionalAssemblies = null) : base(
            null,
            commandTimeout,
            writeIndented,
            queryBehavior,
            excludeNullBehavior
        )
        {
            _dbContext = dbContext;
            _logger = logger;
            LoggerAction = WriteLog;
            _assemblies = additionalAssemblies;
            BuildCache();
        }

        private void BuildCache()
        {
            if (_dbContext == null)
            {
                throw new InvalidOperationException("dbContext is null");
            }
            InternalBuildCache();
        }
    }


    internal static class EfCoreEntityTypeInfoBuilder
    {
        internal static KeyValuePair<Type, EntityTypeInfo>[] Build(DbContext dbContext,
            IEnumerable<Assembly> assemblies)
        {
            var values = Build(dbContext);
            if (assemblies == null) return values;
            var results = new List<KeyValuePair<Type, EntityTypeInfo>>();
            results.AddRange(values);
            var additionalList = EntityTypeInfoBuilder.Build(assemblies);
            foreach (var pair in additionalList)
            {
                if (results.Exists(x => x.Key == pair.Key)) continue;
                results.Add(pair);
            }

            return results.ToArray();
            //return values.Concat(EntityTypeInfoBuilder.Build(assemblies)).ToArray();
        }

        internal static KeyValuePair<Type, EntityTypeInfo>[] Build(DbContext dbContext)
        {
            return dbContext.Model.GetEntityTypes()
                .Select(item => new KeyValuePair<Type, EntityTypeInfo>(item.ClrType,
                            Build(item))).ToArray();
        }


        internal static EntityTypeInfo Build(IEntityType entityType)
        {
            var entityInfo = new EntityTypeInfo();
            var properties = entityType.GetProperties().ToList();
            var tableName = entityType.GetTableName();
            //var tableName = entityType.GetSchemaQualifiedTableName();
            var schemaName = entityType.GetSchema();
            entityInfo.TableName = tableName;
            entityInfo.SchemaName = schemaName;
            var type = entityType.ClrType;
            //var naming = Naming.None;
            //var entityAttr = type.GetCustomAttribute<EntityAttribute>();
            //if (entityAttr != null)
            //{
            //    naming = entityAttr.Naming;
            //}
            var table = type.GetCustomAttribute<TableAttribute>();
            if (table != null)
            {
                entityInfo.TableName = table.Name;
                entityInfo.SchemaName = table.Schema;
            }
            //else
            //{
            //    entityInfo.TableName = naming.Apply(type.Name);
            //}

            var tableId = StoreObjectIdentifier.Table(tableName, schemaName);
            var columns = new List<EntityColumnInfo>();
            var keyColumns = new List<EntityColumnInfo>();
            var sequenceColumns = new List<EntityColumnInfo>();
            var hasSoftDeleteKey = false;

            foreach (var property in properties)
            {
                var propertyInfo = property.PropertyInfo;
                var notMapped = propertyInfo.GetCustomAttribute<NotMappedAttribute>();
                if (notMapped != null)
                {
                    continue;
                }

                var mapping = property.FindRelationalTypeMapping(tableId);
                var columnInfo = new EntityColumnInfo
                {
                    PropertyInfo = propertyInfo
                };
                var supportedType = false;
                if (propertyInfo.PropertyType.IsEspKnownType())
                {
                    supportedType = true;
                }else if (propertyInfo.PropertyType.BaseType != null && propertyInfo.PropertyType.BaseType == typeof(Enum))
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

                columnInfo.ColumnName = property.GetColumnName(tableId);
                var column = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                if (column != null)
                {
                    columnInfo.ColumnName = column.Name;
                    columnInfo.TypeName = column.TypeName;
                }
                //else
                //{
                //    columnInfo.ColumnName = naming.Apply(propertyInfo.Name);
                //}

                //columnInfo.TypeName = property.GetColumnType(tableId);
                columnInfo.DbType = propertyInfo.PropertyType.ResolveDbType();

                var converter = property.GetValueConverter();
                if (converter != null)
                {
                    columnInfo.ConvertFromProvider = converter.ConvertFromProvider;
                    columnInfo.ConvertToProvider = converter.ConvertToProvider;
                }

                if (property.ValueGenerated == ValueGenerated.OnAdd)
                {
                    columnInfo.IsIdentity = true;
                }
                //var identityAttr = propertyInfo.GetCustomAttribute<DatabaseGeneratedAttribute>();
                //if (identityAttr != null && identityAttr.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)
                //{
                //    columnInfo.IsIdentity = true;
                //}

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

                var maxLength = property.GetMaxLength(tableId);
                if (maxLength.HasValue)
                {
                    columnInfo.StringMaxLength = maxLength.Value;
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

                if (property.IsPrimaryKey())
                {
                    columnInfo.IsPrimaryKey = true;
                }
                //var keyAttr = propertyInfo.GetCustomAttribute<KeyAttribute>();
                //if (keyAttr != null)
                //{
                //    columnInfo.IsPrimaryKey = true;
                //}

                if (columnInfo.IsPrimaryKey)
                {
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
