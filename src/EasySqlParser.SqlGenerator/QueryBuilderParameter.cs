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

    public class QueryBuilderParameter
    {
        public QueryBuilderParameter(
            object entity,
            SqlKind sqlKind,
            IQueryBuilderConfiguration builderConfiguration,
            Type entityType=null,
            bool excludeNull = false,
            bool ignoreVersion = false,
            bool useVersion = true,
            bool suppressOptimisticLockException = false,
            //bool useDbSet = true,
            string currentUser = null,
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
            CurrentUser = currentUser;
            Config = configName == null
                ? ConfigContainer.DefaultConfig
                : ConfigContainer.AdditionalConfigs[configName];
            EntityType = entityType ?? entity.GetType();
            EntityTypeInfo = builderConfiguration.GetEntityTypeInfo(EntityType);
            VersionPropertyInfo = EntityTypeInfo.VersionColumn?.PropertyInfo;
        }

        internal EntityTypeInfo EntityTypeInfo { get; }


        public object Entity { get; private set; }

        public bool ExcludeNull { get; }

        /// <summary>
        /// VersionNoを更新条件に含めない
        /// VersionNo自体は設定された値で更新される
        /// </summary>
        public bool IgnoreVersion { get; }

        /// <summary>
        /// EfCoreが想定しているRowVersion型など特殊なものではなく、longなど一般的な型を使って楽観排他を行う
        /// 更新件数が0件の場合は `OptimisticLockException` をスローする
        /// </summary>
        public bool UseVersion { get; }

        /// <summary>
        /// VersionNoを更新条件に含める
        /// 更新件数0件でも `OptimisticLockException` をスローしない
        /// </summary>
        public bool SuppressOptimisticLockException { get; }

        public SqlKind SqlKind { get; }

        public SqlParserConfig Config { get; }

        public int CommandTimeout { get; }

        //public bool UseDbSet { get; }

        /// <summary>
        /// 改行およびインデントを行うかどうか
        /// </summary>
        public bool WriteIndented { get; }

        public string SqlFile { get; }

        public QueryBehavior QueryBehavior { get; }

        public ExcludeNullBehavior ExcludeNullBehavior { get; }

        public string CurrentUser { get; }

        public Type EntityType { get; private set; }

        public void ResetEntity(object entity)
        {
            Entity = entity;
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

        public void SaveExpectedVersion()
        {
            if (VersionPropertyInfo == null) return;
            if ((SqlKind == SqlKind.Update || SqlKind == SqlKind.Delete || SqlKind == SqlKind.SoftDelete) &&
                CommandExecutionType != CommandExecutionType.ExecuteNonQuery)
            {
                _expectedVersionNo = AddVersion(VersionPropertyInfo.GetValue(Entity));
            }

        }

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
                    // TODO:
                    throw new InvalidOperationException("unsupported data type");
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
                    // TODO:
                    throw new InvalidOperationException("unsupported data type");
            }
        }

        public void IncrementVersion()
        {
            if (VersionPropertyInfo == null) return;
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


        public void WriteLog(string message)
        {
            _loggerAction?.Invoke(message);
        }

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

        public bool ThrowableOptimisticLockException()
        {
            return (SqlKind == SqlKind.Update || 
                    SqlKind == SqlKind.Delete || 
                    SqlKind == SqlKind.SoftDelete) &&
                   UseVersion && !SuppressOptimisticLockException;
        }

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
