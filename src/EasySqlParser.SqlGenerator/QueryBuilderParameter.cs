using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using EasySqlParser.Configurations;

namespace EasySqlParser.SqlGenerator
{
    public interface IQueryBuilderConfiguration
    {
        int CommandTimeout { get; }
        bool WriteIndented { get; }
        QueryBehavior QueryBehavior { get; }
        Action<string> LoggerAction { get; }

        void BuildCache();

        EntityTypeInfo GetEntityTypeInfo(Type type);


    }

    public class QueryBuilderConfiguration : IQueryBuilderConfiguration
    {
        private readonly IEnumerable<Assembly> _assemblies;
        private TypeHashDictionary<EntityTypeInfo> _hashDictionary;
        public QueryBuilderConfiguration(
            IEnumerable<Assembly> entityAssemblies,
            int commandTimeout = 30,
            bool writeIndented = true,
            QueryBehavior queryBehavior = QueryBehavior.None,
            Action<string> loggerAction = null
        )
        {
            _assemblies = entityAssemblies;
            CommandTimeout = commandTimeout;
            WriteIndented = writeIndented;
            QueryBehavior = queryBehavior;
            LoggerAction = loggerAction;
            BuildCache();
        }

        public int CommandTimeout { get; }
        public bool WriteIndented { get; }
        public QueryBehavior QueryBehavior { get; }
        public Action<string> LoggerAction { get; }
        public void BuildCache()
        {
            var prepare = EntityTypeInfoBuilder.Build(_assemblies);
            _hashDictionary = TypeHashDictionary<EntityTypeInfo>.Create(prepare);
        }

        public EntityTypeInfo GetEntityTypeInfo(Type type)
        {
            return _hashDictionary.Get(type);
        }
    }

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


        public object Entity { get; }

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

        public string CurrentUser { get; }

        internal Type EntityType { get; }

        internal Dictionary<string, (EntityColumnInfo columnInfo, IDbDataParameter dataParameter)>
            ReturningColumns
        { get; set; } =
            new Dictionary<string, (EntityColumnInfo columnInfo, IDbDataParameter dataParameter)>();

        internal PropertyInfo VersionPropertyInfo { get; }

        private readonly Action<string> _loggerAction;

        private object _beforeChangeVersion;

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
                _beforeChangeVersion = AddVersion(VersionPropertyInfo.GetValue(Entity));
            }

        }

        public bool IsSameVersion()
        {
            if (VersionPropertyInfo == null) return true;
            if (_beforeChangeVersion == null) return true;
            if (IgnoreVersion) return true;
            var currentVersion = VersionPropertyInfo.GetValue(Entity);
            switch (_versionType)
            {
                case VersionType.Short:
                    return (short) currentVersion == (short) _beforeChangeVersion;
                case VersionType.Integer:
                    return (int)currentVersion == (int)_beforeChangeVersion;
                case VersionType.Long:
                    return (long)currentVersion == (long)_beforeChangeVersion;
                case VersionType.Decimal:
                    return (decimal)currentVersion == (decimal)_beforeChangeVersion;
            }

            return false;
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
            if (QueryBehavior == QueryBehavior.AllColumns) return;
            if (QueryBehavior == QueryBehavior.IdentityOrAllColumns &&
                EntityTypeInfo.IdentityColumn == null) return;
            var versionValue = VersionPropertyInfo.GetValue(Entity);
            if (versionValue == null) return;
            VersionPropertyInfo.SetValue(Entity, AddVersion(versionValue));
        }

        public void ApplyReturningColumns()
        {
            if (ReturningColumns.Count == 0) return;
            _loggerAction?.Invoke("[START] returning value read");
            foreach (var kvp in ReturningColumns)
            {
                var returningValue = kvp.Value.dataParameter.Value;
                _loggerAction?.Invoke($"{kvp.Value.dataParameter.ParameterName}\t{returningValue}");
                kvp.Value.columnInfo.PropertyInfo.SetValue(Entity, returningValue);
            }
            _loggerAction?.Invoke("[END] returning value read");
        }


        public void WriteLog(string message)
        {
            _loggerAction?.Invoke(message);
        }

        public CommandExecutionType CommandExecutionType
        {
            get
            {
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

        public bool ThrowableOptimisticLockException(int affectedCount)
        {
            //if ((SqlKind == SqlKind.Update || SqlKind == SqlKind.Delete || SqlKind == SqlKind.SoftDelete) &&
            //    UseVersion && !SuppressOptimisticLockException)
            //{
            //    if (affectedCount == 0) return true;
            //    if (!IsSameVersion()) return true;
            //}
            //return false;
            return (SqlKind == SqlKind.Update || SqlKind == SqlKind.Delete || SqlKind == SqlKind.SoftDelete) &&
                   UseVersion && !SuppressOptimisticLockException
                   && affectedCount == 0;
        }

    }


    public enum SqlKind
    {
        Insert,
        Update,
        Delete,
        SoftDelete
    }

    /// <summary>
    /// INSERT または UPDATE 時の動作
    /// </summary>
    public enum QueryBehavior
    {
        /// <summary>
        /// 無し
        /// エンティティに変更結果は戻されない
        /// </summary>
        None,
        /// <summary>
        /// 自動採番列の値のみエンティティに戻される
        /// </summary>
        IdentityOnly,
        /// <summary>
        /// 全ての値がエンティティに戻される
        /// </summary>
        AllColumns,
        /// <summary>
        /// 自動採番列があればその値のみを
        /// そうでない場合はすべての列がエンティティに戻される
        /// </summary>
        IdentityOrAllColumns
    }

    public enum CommandExecutionType
    {
        ExecuteReader,
        ExecuteNonQuery,
        ExecuteScalar
    }
}
