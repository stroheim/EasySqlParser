using System;
using System.Collections.Generic;
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

        public SequenceGeneratorAttribute SequenceGeneratorAttribute { get; internal set; }

        public CurrentTimestampAttribute CurrentTimestampAttribute { get; internal set; }

        internal EntityColumnInfo Clone()
        {
            return (EntityColumnInfo) MemberwiseClone();
        }
    }
}
