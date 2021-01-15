using System;
using System.Collections.Generic;
using System.Text;

namespace EasySqlParser.SourceGenerator
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class MethodAttributeBase : Attribute
    {
        public string FilePath { get; set; }
    }
}
