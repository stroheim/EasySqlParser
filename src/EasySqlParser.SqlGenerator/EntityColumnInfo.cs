using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Text;

namespace EasySqlParser.SqlGenerator
{
    public class EntityColumnInfo
    {
        public string ColumnName { get; set; }

        public PropertyInfo PropertyInfo { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsVersion { get; set; }

        public bool IsIdentity { get; set; }

        public bool IsSequence { get; set; }

        public bool IsCurrentTimestamp { get; set; }

        public SequenceGeneratorAttribute SequenceGeneratorAttribute { get; set; }

        public CurrentTimestampAttribute CurrentTimestampAttribute { get; set; }

        public bool IsSoftDeleteKey { get; set; }

        public bool IsCurrentUser { get; set; }

        public CurrentUserAttribute CurrentUserAttribute { get;  set; }

        public string TypeName { get; set; }

        public DbType DbType { get; set; }

        public int? StringMaxLength { get; set; }

        public bool IsDateTime { get; set; }

        /// <summary>
        /// Ef Core ValueConverter#ConvertToProvider.
        /// Write to data store.
        /// </summary>
        public Func<object, object> ConvertToProvider { get; set; }

        /// <summary>
        /// Ef Core ValueConverter#ConvertFromProvider.
        /// Read from data store.
        /// </summary>
        public Func<object, object> ConvertFromProvider { get; set; }


        public EntityColumnInfo Clone()
        {
            return (EntityColumnInfo) MemberwiseClone();
        }
    }
}
