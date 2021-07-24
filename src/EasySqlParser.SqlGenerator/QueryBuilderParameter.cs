using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using EasySqlParser.Configurations;
using EasySqlParser.SqlGenerator.Configurations;
using EasySqlParser.SqlGenerator.Enums;
using EasySqlParser.SqlGenerator.Metadata;

namespace EasySqlParser.SqlGenerator
{
    /// <summary>
    ///     A parameter for <see cref="QueryBuilder"/>.
    /// </summary>
    public class QueryBuilderParameter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryBuilderParameter"/> class.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="sqlKind"></param>
        /// <param name="builderConfiguration"></param>
        /// <param name="entityType"></param>
        /// <param name="excludeNull"></param>
        /// <param name="ignoreVersion"></param>
        /// <param name="useVersion"></param>
        /// <param name="suppressOptimisticLockException"></param>
        /// <param name="sqlFile"></param>
        /// <param name="configName"></param>
        public QueryBuilderParameter(
            object entity,
            SqlKind sqlKind,
            IQueryBuilderConfiguration builderConfiguration,
            Type entityType = null,
            bool excludeNull = false,
            bool ignoreVersion = false,
            bool useVersion = true,
            bool suppressOptimisticLockException = false,
            string sqlFile = null,
            string configName = null)
        {
            Entity = entity;
            SqlKind = sqlKind;
            ExcludeNull = excludeNull;
            IgnoreVersion = ignoreVersion;
            UseVersion = useVersion;
            SuppressOptimisticLockException = suppressOptimisticLockException;
            CommandTimeout = builderConfiguration.CommandTimeout;
            //UseDbSet = useDbSet;
            WriteIndented = builderConfiguration.WriteIndented;
            SqlFile = sqlFile;
            QueryBehavior = builderConfiguration.QueryBehavior;
            ExcludeNullBehavior = builderConfiguration.ExcludeNullBehavior;
            _loggerAction = builderConfiguration.LoggerAction;
            Config = configName == null
                ? ConfigContainer.DefaultConfig
                : ConfigContainer.AdditionalConfigs[configName];
            EntityType = entityType ?? entity.GetType();
            EntityTypeInfo = builderConfiguration.GetEntityTypeInfo(EntityType);
            VersionPropertyInfo = EntityTypeInfo.VersionColumn?.PropertyInfo;
        }

        /// <summary>
        ///     Gets the <see cref="EntityTypeInfo"/>.
        /// </summary>
        public EntityTypeInfo EntityTypeInfo { get; }

        /// <summary>
        ///     Gets the entity instance.
        /// </summary>
        public object Entity { get; }

        /// <summary>
        ///     Gets whether SQL NULL columns are excluded from SQL statements.
        /// </summary>
        public bool ExcludeNull { get; }

        /// <summary>
        ///     GEts whether the version property is ignored.
        /// </summary>
        /// <remarks>
        ///     VersionNo itself is updated with the set value.
        /// </remarks>
        public bool IgnoreVersion { get; }

        /// <summary>
        ///     Gets whether optimistic exclusion is performed using a general type
        ///     such as long, rather than a special type such as the RowVersion type that EfCore assumes.
        /// </summary>
        /// <remarks>
        ///     OptimisticLockException is thrown if the number of updates is 0
        /// </remarks>
        public bool UseVersion { get; }

        /// <summary>
        ///     Gets whether <see cref="OptimisticLockException"/> is suppressed.
        /// </summary>
        /// <remarks>
        ///     Version no is included in the update conditions.
        ///     OptimisticLockException is not thrown even if the number of updates is 0.
        /// </remarks>
        public bool SuppressOptimisticLockException { get; }

        /// <summary>
        ///     Gets the <see cref="SqlKind"/>.
        /// </summary>
        public SqlKind SqlKind { get; }

        /// <summary>
        ///     Gets the <see cref="SqlParserConfig"/>.
        /// </summary>
        public SqlParserConfig Config { get; }

        /// <summary>
        ///     Gets the command timeout (in seconds).
        /// </summary>
        public int CommandTimeout { get; }

        /// <summary>
        ///     Gets a value that defines whether SQL should use pretty printing. 
        /// </summary>
        public bool WriteIndented { get; }

        /// <summary>
        ///     Gets the sql file.
        /// </summary>
        public string SqlFile { get; }

        /// <summary>
        ///     Gets the <see cref="Enums.QueryBehavior"/>.
        /// </summary>
        public QueryBehavior QueryBehavior { get; }

        /// <summary>
        ///     Gets the <see cref="Enums.ExcludeNullBehavior"/>.
        /// </summary>
        public ExcludeNullBehavior ExcludeNullBehavior { get; }

        /// <summary>
        ///     Gets the entity type.
        /// </summary>
        public Type EntityType { get; private set; }

        /// <summary>
        ///     Rewrites the state of the internally allocated entity in the specified entity.
        /// </summary>
        /// <param name="entity"></param>
        public void ResetEntity(object entity)
        {
            foreach (var columnInfo in EntityTypeInfo.Columns)
            {
                var value = columnInfo.PropertyInfo.GetValue(entity);
                columnInfo.PropertyInfo.SetValue(Entity, value);
            }
            //Entity = entity;
            EntityType = entity.GetType();
        }

        internal Dictionary<string, (EntityColumnInfo columnInfo, IDbDataParameter dataParameter)>
            ReturningColumns
        { get; set; } =
            new Dictionary<string, (EntityColumnInfo columnInfo, IDbDataParameter dataParameter)>();

        internal PropertyInfo VersionPropertyInfo { get; }

        private readonly Action<string> _loggerAction;

        private object _expectedVersionNo;

        private enum VersionType
        {
            None,
            Short,
            Integer,
            Long,
            Decimal,
        }

        private VersionType _versionType = VersionType.None;

        /// <summary>
        ///     Save the expected version no.
        /// </summary>
        public void SaveExpectedVersion()
        {
            if (VersionPropertyInfo == null) return;
            if ((SqlKind == SqlKind.Update || SqlKind == SqlKind.Delete || SqlKind == SqlKind.SoftDelete) &&
                CommandExecutionType != CommandExecutionType.ExecuteNonQuery)
            {
                _expectedVersionNo = AddVersion(VersionPropertyInfo.GetValue(Entity));
            }

        }

        /// <summary>
        ///     Determines if the entity's version no matches the expected version no.
        /// </summary>
        /// <returns></returns>
        public bool IsSameVersion()
        {
            if (VersionPropertyInfo == null) return true;
            if (_expectedVersionNo == null) return true;
            if (IgnoreVersion) return true;
            var currentVersion = VersionPropertyInfo.GetValue(Entity);
            switch (_versionType)
            {
                case VersionType.Short:
                    return (short) currentVersion == (short) _expectedVersionNo;
                case VersionType.Integer:
                    return (int)currentVersion == (int)_expectedVersionNo;
                case VersionType.Long:
                    return (long)currentVersion == (long)_expectedVersionNo;
                case VersionType.Decimal:
                    return (decimal)currentVersion == (decimal)_expectedVersionNo;
                default:
                    throw new InvalidOperationException($"unsupported version no data type:{_versionType}");
            }

        }

        private object AddVersion(object versionValue)
        {
            switch (versionValue)
            {
                case short shortValue:
                    _versionType = VersionType.Short;
                    return shortValue + 1;
                case int intValue:
                    _versionType = VersionType.Integer;
                    return intValue + 1;
                case long longValue:
                    _versionType = VersionType.Long;
                    return longValue + 1L;
                case decimal decimalValue:
                    _versionType = VersionType.Decimal;
                    return decimalValue + 1M;
                default:
                    throw new InvalidOperationException($"unsupported version no data type:{versionValue.GetType()}");
            }
        }

        /// <summary>
        ///     Add version no.
        /// </summary>
        public void IncrementVersion()
        {
            if (VersionPropertyInfo == null) return;
            if (!(SqlKind == SqlKind.Update || SqlKind == SqlKind.SoftDelete)) return;
            //if (SqlKind == SqlKind.Insert || SqlKind == SqlKind.Update)
            //{
                if (QueryBehavior == QueryBehavior.AllColumns) return;
                if (QueryBehavior == QueryBehavior.IdentityOrAllColumns &&
                    EntityTypeInfo.IdentityColumn == null) return;
            //}
            var versionValue = VersionPropertyInfo.GetValue(Entity);
            if (versionValue == null) return;
            VersionPropertyInfo.SetValue(Entity, AddVersion(versionValue));
        }

        /// <summary>
        ///     Writes back the value returned by the returning clause to the entity.
        /// </summary>
        public void ApplyReturningColumns()
        {
            if (ReturningColumns.Count == 0) return;
            WriteLog("[START] returning value read");
            foreach (var kvp in ReturningColumns)
            {
                var returningValue = kvp.Value.dataParameter.Value;
                WriteLog($"{kvp.Value.dataParameter.ParameterName}\t{returningValue}");
                kvp.Value.columnInfo.PropertyInfo.SetValue(Entity, returningValue);
            }
            WriteLog("[END] returning value read");
        }

        /// <summary>
        ///     Write log message.
        /// </summary>
        /// <param name="message"></param>
        public void WriteLog(string message)
        {
            _loggerAction?.Invoke(message);
        }

        /// <summary>
        ///     Gets the <see cref="Enums.CommandExecutionType"/>.
        /// </summary>
        public CommandExecutionType CommandExecutionType
        {
            get
            {
                if (!string.IsNullOrEmpty(SqlFile)) return CommandExecutionType.ExecuteNonQuery;
                if (SqlKind == SqlKind.Delete) return CommandExecutionType.ExecuteNonQuery;
                if (QueryBehavior == QueryBehavior.None) return CommandExecutionType.ExecuteNonQuery;
                if (QueryBehavior == QueryBehavior.IdentityOnly) return CommandExecutionType.ExecuteScalar;
                switch (Config.DbConnectionKind)
                {
                    case DbConnectionKind.AS400:
                    case DbConnectionKind.DB2:
                        return CommandExecutionType.ExecuteReader;
                    case DbConnectionKind.MySql:
                        return CommandExecutionType.ExecuteReader;
                    case DbConnectionKind.OracleLegacy:
                        return CommandExecutionType.ExecuteNonQuery;
                    case DbConnectionKind.Oracle:
                        return CommandExecutionType.ExecuteNonQuery;
                    case DbConnectionKind.PostgreSql:
                        return CommandExecutionType.ExecuteReader;
                    case DbConnectionKind.SQLite:
                        return CommandExecutionType.ExecuteReader;
                    case DbConnectionKind.SqlServerLegacy:
                        return CommandExecutionType.ExecuteReader;
                    case DbConnectionKind.SqlServer:
                        return CommandExecutionType.ExecuteReader;
                    case DbConnectionKind.Odbc:
                    case DbConnectionKind.OleDb:
                        return CommandExecutionType.ExecuteNonQuery;
                    default:
                        return CommandExecutionType.ExecuteNonQuery;
                }
            }
        }

        /// <summary>
        ///     Gets whether OptimisticLockException can be thrown.
        /// </summary>
        /// <returns></returns>
        public bool ThrowableOptimisticLockException()
        {
            return (SqlKind == SqlKind.Update || 
                    SqlKind == SqlKind.Delete || 
                    SqlKind == SqlKind.SoftDelete) &&
                   UseVersion && !SuppressOptimisticLockException;
        }

        /// <summary>
        ///     Gets whether OptimisticLockException can be thrown.
        /// </summary>
        /// <param name="affectedCount"></param>
        /// <returns></returns>
        public bool ThrowableOptimisticLockException(int affectedCount)
        {
            return (SqlKind == SqlKind.Update || 
                    SqlKind == SqlKind.Delete || 
                    SqlKind == SqlKind.SoftDelete) &&
                   UseVersion && !SuppressOptimisticLockException
                   && affectedCount == 0;
        }

    }





}
