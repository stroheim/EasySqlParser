using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
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
            EntityTypeInfo = Cache<T>.EntityTypeInfo;
        }

        internal QueryBuilderParameter()
        {
            EntityTypeInfo = Cache<T>.EntityTypeInfo;
        }

        // generic type cache
        private static class Cache<TK>
        {
            static Cache()
            {
                var type = typeof(TK);
                var entityInfo = new EntityTypeInfo
                {
                    TableName = type.Name
                };
                var table = type.GetCustomAttribute<TableAttribute>();
                if (table != null)
                {
                    entityInfo.TableName = table.Name;
                    entityInfo.SchemaName = table.Schema;
                }
                var props = type.GetProperties();
                var columns = new List<EntityColumnInfo>();
                var keyColumns = new List<EntityColumnInfo>();
                var sequenceColumns = new List<EntityColumnInfo>();
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


                    var column = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                    columnInfo.ColumnName = propertyInfo.Name;
                    if (column != null)
                    {
                        columnInfo.ColumnName = column.Name;
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

                    columns.Add(columnInfo);
                }

                entityInfo.Columns = columns.AsReadOnly();
                entityInfo.KeyColumns = keyColumns.AsReadOnly();
                entityInfo.SequenceColumns = sequenceColumns.AsReadOnly();
                EntityTypeInfo = entityInfo;
            }

            // ReSharper disable once StaticMemberInGenericType
            internal static EntityTypeInfo EntityTypeInfo { get; }

        }

        public EntityTypeInfo EntityTypeInfo { get; }

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

        internal Dictionary<string, (EntityColumnInfo columnInfo, IDbDataParameter dataParameter)>
            ReturningColumns { get; set; } =
            new Dictionary<string, (EntityColumnInfo columnInfo, IDbDataParameter dataParameter)>();

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

        public void ApplyReturningColumns()
        {
            if (ReturningColumns.Count == 0) return;
            foreach (var kvp in ReturningColumns)
            {
                var returningValue = kvp.Value.dataParameter.Value;
                kvp.Value.columnInfo.PropertyInfo.SetValue(Entity, returningValue);
            }
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
                        return CommandExecutionType.ExecuteNonQuery;
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
