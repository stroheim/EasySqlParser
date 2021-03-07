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
        public string ColumnName { get; internal set; }

        public PropertyInfo PropertyInfo { get; internal set; }

        public bool IsPrimaryKey { get; internal set; }

        public bool IsVersion { get; internal set; }

        public bool IsIdentity { get; internal set; }

        public bool IsSequence { get; internal set; }

        public bool IsCurrentTimestamp { get; internal set; }

        public SequenceGeneratorAttribute SequenceGeneratorAttribute { get; internal set; }

        public CurrentTimestampAttribute CurrentTimestampAttribute { get; internal set; }

        public bool IsSoftDeleteKey { get; internal set; }

        public bool IsCurrentUser { get; internal set; }

        public CurrentUserAttribute CurrentUserAttribute { get; internal set; }

        public string TypeName { get; internal set; }

        public DbType DbType { get; internal set; }

        public int? StringMaxLength { get; internal set; }

        public bool IsDateTime { get; internal set; }


        internal EntityColumnInfo Clone()
        {
            return (EntityColumnInfo) MemberwiseClone();
        }
    }
}
