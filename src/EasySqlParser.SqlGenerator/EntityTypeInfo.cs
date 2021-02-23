using System.Collections.Generic;

namespace EasySqlParser.SqlGenerator
{
    public class EntityTypeInfo
    {
        public string SchemaName { get; internal set; }

        public string TableName { get; internal set; }


        public IReadOnlyList<EntityColumnInfo> Columns { get; internal set; }

        public IReadOnlyList<EntityColumnInfo> KeyColumns { get; internal set; }

        public EntityColumnInfo IdentityColumn { get; internal set; }

        public IReadOnlyList<EntityColumnInfo> SequenceColumns { get; internal set; }
    }
}
