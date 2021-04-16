using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using EasySqlParser.SqlGenerator.Attributes;
using EasySqlParser.SqlGenerator.Enums;

namespace EasySqlParser.SqlGenerator.Tests.Postgres
{
    [Entity]
    [Table("MetalGearSeries")]
    public class MetalGearSeries
    {
        [Key]
        [SequenceGenerator("METAL_GEAR_SERIES_SEQ")]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NAME")]
        public string Name { get; set; }

        [Column("RELEASE_DATE")]
        public DateTime ReleaseDate { get; set; }

        [Column("PLATFORM")]
        public string Platform { get; set; }

        [CurrentTimestamp("CURRENT_TIMESTAMP", GenerationStrategy.Insert)]
        [Column("CREATE_DATE")]
        public DateTime CreateDate { get; set; }

        [Version]
        [Column("VERSION")]
        public long VersionNo { get; set; }

        public string GetDebugString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{nameof(Id)}\t{Id}");
            builder.AppendLine($"{nameof(Name)}\t{Name}");
            builder.AppendLine($"{nameof(ReleaseDate)}\t{ReleaseDate}");
            builder.AppendLine($"{nameof(Platform)}\t{Platform}");
            builder.AppendLine($"{nameof(CreateDate)}\t{CreateDate}");
            builder.AppendLine($"{nameof(VersionNo)}\t{VersionNo}");
            return builder.ToString();
        }
    }
}
