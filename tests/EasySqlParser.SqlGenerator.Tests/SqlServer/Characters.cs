﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EasySqlParser.SqlGenerator.Tests.SqlServer
{
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
        public decimal? Height { get; set; }

        [Column("CREATE_DATE")]
        public DateTime CreateDate { get; set; }

        [Version]
        [Column("VERSION")]
        public long VersionNo { get; set; }


    }
}