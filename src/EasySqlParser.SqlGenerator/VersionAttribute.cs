using System;
using System.Collections.Generic;
using System.Text;

namespace EasySqlParser.SqlGenerator
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class VersionAttribute : Attribute
    {
    }
}
