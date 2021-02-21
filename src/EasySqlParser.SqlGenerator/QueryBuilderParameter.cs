using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EasySqlParser.Configurations;

namespace EasySqlParser.SqlGenerator
{
    public interface IQueryBuilderConfiguration
    {
        int CommandTimeout { get; }
        bool WriteIndented { get; }
        QueryBehavior QueryBehavior { get; }
        Action<string> LoggerAction { get; }
    }

    public class GlobalQueryBuilderConfiguration : IQueryBuilderConfiguration
    {
        public GlobalQueryBuilderConfiguration(
            int commandTimeout = 30,
            bool writeIndented = true,
            QueryBehavior queryBehavior = QueryBehavior.None,
            Action<string> loggerAction = null
        )
        {
            CommandTimeout = commandTimeout;
            WriteIndented = writeIndented;
            QueryBehavior = queryBehavior;
            LoggerAction = loggerAction;
        }

        public int CommandTimeout { get; }
        public bool WriteIndented { get; }
        public QueryBehavior QueryBehavior { get; }
        public Action<string> LoggerAction { get; }
    }

    public class QueryBuilderParameter<T>
    {
        public QueryBuilderParameter(
            T entity,
            SqlKind sqlKind,
            IQueryBuilderConfiguration builderConfiguration,
            bool excludeNull = false,
            bool ignoreVersion = false,
            bool useVersion = true,
            bool suppressOptimisticLockException = false,
            //bool useDbSet = true,
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
            Config = configName == null
                ? ConfigContainer.DefaultConfig
                : ConfigContainer.AdditionalConfigs[configName];
        }

        public T Entity { get; }

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

        internal PropertyInfo VersionPropertyInfo { get; set; }

        private readonly Action<string> _loggerAction;

        public void IncrementVersion()
        {
            if (VersionPropertyInfo == null) return;
            var versionValue = VersionPropertyInfo.GetValue(Entity);
            if (versionValue == null) return;
            if (versionValue is int intValue)
            {
                VersionPropertyInfo.SetValue(Entity, intValue + 1);
            }else if (versionValue is long longValue)
            {
                VersionPropertyInfo.SetValue(Entity, longValue + 1L);
            }else if (versionValue is decimal decimalValue)
            {
                VersionPropertyInfo.SetValue(Entity, decimalValue + 1M);
            }
            else
            {
                // TODO:
                throw new InvalidOperationException("unsupported data type");
            }
        }


        public void WriteLog(string message)
        {
            _loggerAction?.Invoke(message);
        }

    }

    public enum SqlKind
    {
        Insert,
        Update,
        Delete
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
}
