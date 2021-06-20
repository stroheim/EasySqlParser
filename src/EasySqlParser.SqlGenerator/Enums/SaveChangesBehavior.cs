using System;
using System.Collections.Generic;
using System.Text;

namespace EasySqlParser.SqlGenerator.Enums
{
    public enum SaveChangesBehavior
    {
        SqlContextOnly,
        DbContextFirst,
        SqlContextFirst
    }
}
