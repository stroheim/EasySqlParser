using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EasySqlParser.SqlGenerator.Tests.Sqlite
{
    [Entity]
    [Table("MetalGearCharacters")]
    public class Characters
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NAME")]
        public string Name { get; set; }

        [Column("HEIGHT")]
        public double? Height { get; set; }

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
            builder.AppendLine($"{nameof(Height)}\t{Height}");
            builder.AppendLine($"{nameof(CreateDate)}\t{CreateDate}");
            builder.AppendLine($"{nameof(VersionNo)}\t{VersionNo}");
            return builder.ToString();
        }
    }
}
