using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using EasySqlParser.Extensions;
using EasySqlParser.SqlGenerator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace EasySqlParser.EntityFrameworkCore
{
    public class EfCoreQueryBuilderConfiguration : IQueryBuilderConfiguration
    {
        private readonly ILogger<EfCoreQueryBuilderConfiguration> _logger;
        private readonly DbContext _dbContext;
        private readonly IEnumerable<Assembly> _assemblies;
        private TypeHashDictionary<EntityTypeInfo> _hashDictionary;

        public int CommandTimeout { get; }
        public bool WriteIndented { get; }
        public QueryBehavior QueryBehavior { get; }
        public Action<string> LoggerAction { get; }
        public void BuildCache()
        {
            var prepare = EfCoreEntityTypeInfoBuilder.Build(_dbContext, _assemblies);
            _hashDictionary = TypeHashDictionary<EntityTypeInfo>.Create(prepare);
        }

        public EntityTypeInfo GetEntityTypeInfo(Type type)
        {
            return _hashDictionary.Get(type);
        }

        public void WriteLog(string message)
        {
            _logger.LogDebug(message);
        }

        public EfCoreQueryBuilderConfiguration(
            DbContext dbContext, 
            ILogger<EfCoreQueryBuilderConfiguration> logger,
            int commandTimeout = 30,
            bool writeIndented = true,
            QueryBehavior queryBehavior = QueryBehavior.None,
            IEnumerable<Assembly> additionalAssemblies = null)
        {
            _dbContext = dbContext;
            _logger = logger;
            LoggerAction = WriteLog;
            CommandTimeout = commandTimeout;
            WriteIndented = writeIndented;
            QueryBehavior = queryBehavior;
            _assemblies = additionalAssemblies;
            BuildCache();
        }
    }

    internal static class EfCoreEntityTypeInfoBuilder
    {
        internal static KeyValuePair<Type, EntityTypeInfo>[] Build(DbContext dbContext,
            IEnumerable<Assembly> assemblies)
        {
            var values = Build(dbContext);
            if (assemblies == null) return values;
            return values.Concat(EntityTypeInfoBuilder.Build(assemblies)).ToArray();
        }

        internal static KeyValuePair<Type, EntityTypeInfo>[] Build(DbContext dbContext)
        {
            return dbContext.Model.GetEntityTypes()
                .Select(item =>
                        {
                            return new KeyValuePair<Type, EntityTypeInfo>(item.ClrType,
                                EfCoreEntityTypeInfoBuilder.Build(item));
                        }).ToArray();
        }

        internal static EntityTypeInfo Build(IEntityType entityType)
        {
            var entityInfo = new EntityTypeInfo();
            var properties = entityType.GetProperties().ToList();
            var tableName = entityType.GetTableName();
            var schemaName = entityType.GetSchema();
            entityInfo.TableName = tableName;
            entityInfo.SchemaName = schemaName;
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


                columnInfo.ColumnName = property.GetColumnName(tableId);
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
