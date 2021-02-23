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

        public string SchemaName { get; set; }

        public string Prefix { get; set; }

        public int PaddingLength { get; set; }
    }
}
