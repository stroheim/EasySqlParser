using System;
using System.Collections.Generic;
using System.Text;

namespace EasySqlParser.SqlGenerator
{
    public class EntityTypeInfo
    {
        public string SchemaName { get; set; }

        public string TableName { get; set; }

        // いる？
        internal string EntityName { get; set; }

        public List<EntityColumnInfo> Columns { get; set; }

        public List<EntityColumnInfo> KeyColumns { get; set; }
    }
}
