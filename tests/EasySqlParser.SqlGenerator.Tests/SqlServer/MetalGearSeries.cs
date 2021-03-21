using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EasySqlParser.SqlGenerator.Tests.SqlServer
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

        [CurrentTimestamp("GETDATE()", GenerationStrategy.Insert)]
        [Column("CREATE_DATE")]
        public DateTime CreateDate { get; set; }

        [Version]
        [Column("VERSION")]
        public long VersionNo { get; set; }
    }
}
