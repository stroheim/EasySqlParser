using System;
using System.Data;
using System.Reflection;
using EasySqlParser.SqlGenerator.Attributes;

namespace EasySqlParser.SqlGenerator.Metadata
{
    /// <summary>
    ///     Entity column information.
    /// </summary>
    public class EntityColumnInfo
    {
        /// <summary>
        ///     Gets or sets column name.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        ///     Gets or sets <see cref="PropertyInfo"/>.
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        ///     Gets or sets whether primary key column.
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        ///     Gets or sets whether version column.
        /// </summary>
        public bool IsVersion { get; set; }

        /// <summary>
        ///     Gets or sets whether identity column.
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        ///     Gets or sets whether sequence column.
        /// </summary>
        public bool IsSequence { get; set; }

        /// <summary>
        ///     Gets or sets whether current timestamp column.
        /// </summary>
        public bool IsCurrentTimestamp { get; set; }

        /// <summary>
        ///     Gets or sets <see cref="SequenceGeneratorAttribute"/>.
        /// </summary>
        public SequenceGeneratorAttribute SequenceGeneratorAttribute { get; set; }

        /// <summary>
        ///     Gets or sets <see cref="CurrentTimestampAttribute"/>.
        /// </summary>
        public CurrentTimestampAttribute CurrentTimestampAttribute { get; set; }

        /// <summary>
        ///     Gets or sets whether soft delete key column.
        /// </summary>
        public bool IsSoftDeleteKey { get; set; }

        /// <summary>
        ///     Gets or sets column data type name.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        ///     Gets or sets column <see cref="DbType"/>.
        /// </summary>
        public DbType DbType { get; set; }

        /// <summary>
        ///     Gets or sets maxlength of string column.
        /// </summary>
        public int? StringMaxLength { get; set; }

        /// <summary>
        ///     Gets or sets whether datetime column.
        /// </summary>
        public bool IsDateTime { get; set; }

        /// <summary>
        ///     Ef Core ValueConverter#ConvertToProvider.
        ///     Write to data store.
        /// </summary>
        public Func<object, object> ConvertToProvider { get; set; }

        /// <summary>
        ///     Ef Core ValueConverter#ConvertFromProvider.
        ///     Read from data store.
        /// </summary>
        public Func<object, object> ConvertFromProvider { get; set; }

        /// <summary>
        ///     Gets or sets nullable underlying type.
        /// </summary>
        public Type NullableUnderlyingType { get; set; }

        /// <summary>
        ///     Copy current instance.
        /// </summary>
        /// <returns></returns>
        public EntityColumnInfo Clone()
        {
            return (EntityColumnInfo) MemberwiseClone();
        }
    }
}
