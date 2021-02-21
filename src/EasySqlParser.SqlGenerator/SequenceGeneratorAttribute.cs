using System;
using System.Collections.Generic;
using System.Text;

namespace EasySqlParser.SqlGenerator
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SequenceGeneratorAttribute : Attribute
    {
        public SequenceGeneratorAttribute(string sequenceName)
        {
            SequenceName = sequenceName;
        }
        public string SequenceName { get; }
    }
}
