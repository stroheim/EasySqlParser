using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EasySqlParser.Configurations;

namespace EasySqlParser.SqlGenerator
{
    public class QueryBuilderParameter<T>
    {
        public QueryBuilderParameter(
            T entity,
            SqlKind sqlKind,
            bool excludeNull = false,
            bool ignoreVersion = false,
            bool useVersion = true,
            bool suppressDbUpdateConcurrencyException = false,
            int commandTimeout = 30,
            //bool useDbSet = true,
            bool writeIndented = false,
            string configName = null)
        {
            Entity = entity;
            SqlKind = sqlKind;
            ExcludeNull = excludeNull;
            IgnoreVersion = ignoreVersion;
            UseVersion = useVersion;
            SuppressDbUpdateConcurrencyException = suppressDbUpdateConcurrencyException;
            CommandTimeout = commandTimeout;
            //UseDbSet = useDbSet;
            WriteIndented = writeIndented;
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
        /// 更新件数が0件の場合は `DbUpdateConcurrencyException` をスローする
        /// </summary>
        public bool UseVersion { get; }

        /// <summary>
        /// VersionNoを更新条件に含める
        /// 更新件数0件でも `DbUpdateConcurrencyException` をスローしない
        /// </summary>
        public bool SuppressDbUpdateConcurrencyException { get; }

        public SqlKind SqlKind { get; }

        public SqlParserConfig Config { get; }

        public int CommandTimeout { get; }

        //public bool UseDbSet { get; }

        /// <summary>
        /// 改行およびインデントを行うかどうか
        /// </summary>
        public bool WriteIndented { get; }

        internal PropertyInfo VersionPropertyInfo { get; set; }

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

    }

    public enum SqlKind
    {
        Insert,
        Update,
        Delete,
        SelectSingleRow,
        Select

    }
}
