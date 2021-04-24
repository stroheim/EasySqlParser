﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using EasySqlParser.SqlGenerator.Attributes;
using EasySqlParser.SqlGenerator.Enums;

namespace EasySqlParser.EntityFrameworkCore.Tests.Oracle
{
    [Entity]
    [Table("EF_MetalGearCharacters")]
    public class Characters
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NAME")]
        [StringLength(30)]
        public string Name { get; set; }

        [Column("HEIGHT")]
        public decimal? Height { get; set; }

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