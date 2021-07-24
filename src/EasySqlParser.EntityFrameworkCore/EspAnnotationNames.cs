using System;
using System.Collections.Generic;
using System.Text;

namespace EasySqlParser.EntityFrameworkCore
{
    internal class EspAnnotationNames
    {
        internal const string Prefix = "Esp:";

        internal const string CurrentTimestamp = Prefix + "CurrentTimestamp";

        internal const string SequenceGenerator = Prefix + "SequenceGenerator";

        internal const string SoftDeleteKey = Prefix + "SoftDeleteKey";

        internal const string Version = Prefix + "Version";
    }
}
