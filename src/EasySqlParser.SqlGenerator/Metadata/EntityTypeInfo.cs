using System.Collections.Generic;

namespace EasySqlParser.SqlGenerator.Metadata
{
    /// <summary>
    ///     Entity information.
    /// </summary>
    public class EntityTypeInfo
    {
        /// <summary>
        ///     Gets or sets a schema name.
        /// </summary>
        public string SchemaName { get; set; }

        /// <summary>
        ///     Gets or sets a table name.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        ///     Gets or sets entity columns.
        /// </summary>
        public IReadOnlyList<EntityColumnInfo> Columns { get; set; }

        /// <summary>
        ///     Gets or sets entity key columns.
        /// </summary>
        public IReadOnlyList<EntityColumnInfo> KeyColumns { get; set; }

        /// <summary>
        ///     Gets or sets a auto-generated column.
        /// </summary>
        public EntityColumnInfo IdentityColumn { get; set; }

        /// <summary>
        ///     Gets or sets a version column.
        /// </summary>
        public EntityColumnInfo VersionColumn { get; set; }

        /// <summary>
        ///     Gets or sets sequence columns.
        /// </summary>
        public IReadOnlyList<EntityColumnInfo> SequenceColumns { get; set; }

        /// <summary>
        ///     Gets or sets primary key name and column pairs
        /// </summary>
        public Dictionary<string, EntityColumnInfo> ColumnNameKeyDictionary { get; set; }

        /// <summary>
        ///     Gets or sets entity has soft delete flag.
        /// </summary>
        public bool HasSoftDeleteKey { get; set; }
    }
}
