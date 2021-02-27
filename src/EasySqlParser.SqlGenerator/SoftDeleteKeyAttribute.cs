using System;
using System.Collections.Generic;
using System.Text;

namespace EasySqlParser.SqlGenerator
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SoftDeleteKeyAttribute : Attribute
    {
    }
}
