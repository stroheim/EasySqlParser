using System;
using System.Collections.Generic;
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

        public EntityColumnInfo Clone()
        {
            return (EntityColumnInfo) MemberwiseClone();
        }
    }
}
