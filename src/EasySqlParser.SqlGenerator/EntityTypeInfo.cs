using System.Collections.Generic;

namespace EasySqlParser.SqlGenerator
{
    internal class EntityTypeInfo
    {
        internal string SchemaName { get; set; }

        internal string TableName { get; set; }


        internal List<EntityColumnInfo> Columns { get; set; }

        internal List<EntityColumnInfo> KeyColumns { get; set; }

        internal EntityColumnInfo IdentityColumn { get; set; }

        internal List<EntityColumnInfo> SequenceColumns { get; set; }
    }
}
