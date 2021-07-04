using System;

namespace EasySqlParser.SqlGenerator.Attributes
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

        //internal string GetSequenceGeneratorSql(SqlParserConfig config)
        //{
        //    return PaddingLength == 0
        //        ? config.Dialect.GetNextSequenceSql(SequenceName, SchemaName)
        //        : config.Dialect.GetNextSequenceSqlZeroPadding(SequenceName, SchemaName,
        //            PaddingLength, Prefix);
        //}
    }
}
