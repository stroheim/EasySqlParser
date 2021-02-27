using System;
using System.Collections.Generic;
using System.Text;
using EasySqlParser.Configurations;

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

        internal string GetSequenceGeneratorSql(SqlParserConfig config)
        {
            return PaddingLength == 0
                ? config.Dialect.GetNextSequenceSql(SequenceName, SchemaName)
                : config.Dialect.GetNextSequenceSqlZeroPadding(SequenceName, SchemaName,
                    PaddingLength, Prefix);
        }
    }
}
