﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasySqlParser.SqlGenerator.Tests.SqlServer
{
    [Table("EMP", Schema = "dbo")]
    public class Employee
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NAME")]
        public string Name { get; set; }

        [Column("SALARY")]
        public decimal Salary { get; set; }

        [Version]
        [Column("VERSION")]
        public long VersionNo { get; set; }
    }

    [Table("EMP_WITH_DATE", Schema = "dbo")]
    public class EmployeeWithDate
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NAME")]
        public string Name { get; set; }

        [Column("SALARY")]
        public decimal Salary { get; set; }

        [Column("DELETE_FLAG")]
        [SoftDeleteKey]
        public bool DeleteFlag { get; set; }

        [Column("CREATE_DATETIME")]
        [CurrentTimestamp("GETDATE()", GenerationStrategy.Insert)]
        public DateTime CreateDateTime { get; set; }

        [Column("UPDATE_DATETIME")]
        [CurrentTimestamp("GETDATE()", GenerationStrategy.Update)]
        public DateTime? UpdateDateTime { get; set; }


        [Column("DELETE_DATETIME")]
        [CurrentTimestamp("GETDATE()", GenerationStrategy.SoftDelete)]
        public DateTime? DeleteDateTime { get; set; }


        [Version]
        [Column("VERSION")]
        public long VersionNo { get; set; }

    }

    [Table("EMP_WITH_DATE_USER", Schema = "dbo")]
    public class EmployeeWithDateAndUser
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NAME")]
        public string Name { get; set; }

        [Column("SALARY")]
        public decimal Salary { get; set; }

        [Column("DELETE_FLAG")]
        [SoftDeleteKey]
        public bool DeleteFlag { get; set; }

        [Column("CREATE_DATETIME")]
        [CurrentTimestamp("GETDATE()", GenerationStrategy.Insert)]
        public DateTime CreateDateTime { get; set; }

        [Column("CREATE_USER")]
        [CurrentUser(GenerationStrategy.Insert)]
        public string CreateUser { get; set; }

        [Column("UPDATE_DATETIME")]
        [CurrentTimestamp("GETDATE()", GenerationStrategy.Update)]
        public DateTime? UpdateDateTime { get; set; }

        [Column("UPDATE_USER")]
        [CurrentUser(GenerationStrategy.Update)]
        public string UpdateUser { get; set; }


        [Column("DELETE_DATETIME")]
        [CurrentTimestamp("GETDATE()", GenerationStrategy.SoftDelete)]
        public DateTime? DeleteDateTime { get; set; }

        [Column("DELETE_USER")]
        [CurrentUser(GenerationStrategy.SoftDelete)]
        public string DeleteUser { get; set; }



        [Version]
        [Column("VERSION")]
        public long VersionNo { get; set; }


    }

    [Table("EMP_IDENTITY", Schema = "dbo")]
    public class EmployeeIdentity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NAME")]
        public string Name { get; set; }

        [Version]
        [Column("VERSION")]
        public long VersionNo { get; set; }
    }

    [Table("EMP_SEQ", Schema = "dbo")]
    public class EmployeeSeq
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NAME")]
        public string Name { get; set; }

        [SequenceGenerator("BYTE_SEQ")]
        [Column("BYTE_COL")]
        public byte ByteCol { get; set; }

        [SequenceGenerator("SHORT_SEQ")]
        [Column("SHORT_COL")]
        public short ShortCol { get; set; }

        [SequenceGenerator("INT_SEQ")]
        [Column("INT_COL")]
        public int IntCol { get; set; }

        [SequenceGenerator("LONG_SEQ")]
        [Column("LONG_COL")]
        public long LongCol { get; set; }

        [SequenceGenerator("DECIMAL_SEQ")]
        [Column("DECIMAL_COL")]
        public decimal DecimalCol { get; set; }

        [SequenceGenerator("STRING_SEQ", PaddingLength = 6, Prefix = "T")]
        [Column("STRING_COL")]
        public string StringCol { get; set; }

        [Version]
        [Column("VERSION")]
        public long VersionNo { get; set; }

    }


}
