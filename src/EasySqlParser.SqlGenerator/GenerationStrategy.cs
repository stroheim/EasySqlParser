using System;
using System.Collections.Generic;
using System.Text;

namespace EasySqlParser.SqlGenerator
{
    public enum GenerationStrategy
    {
        Always,
        Insert,
        Update,
        InsertOrDelete,
        SoftDelete
    }
}
