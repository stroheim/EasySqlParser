using System;

namespace EasySqlParser.SqlGenerator.Attributes
{
    /// <summary>
    ///     Attribute for generating the sequence as an SQL statement.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SequenceGeneratorAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SequenceGeneratorAttribute"/> class.
        /// </summary>
        /// <param name="sequenceName"></param>
        public SequenceGeneratorAttribute(string sequenceName)
        {
            SequenceName = sequenceName;
        }

        /// <summary>
        ///     Gets the sequence name.
        /// </summary>
        public string SequenceName { get; }

        /// <summary>
        ///     Gets or sets the schema name.
        /// </summary>
        public string SchemaName { get; set; }

        /// <summary>
        ///     Gets or sets prefix to the generated sequence.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        ///     Gets or sets zero padding length to the generated sequence.
        /// </summary>
        public int PaddingLength { get; set; }

    }
}
