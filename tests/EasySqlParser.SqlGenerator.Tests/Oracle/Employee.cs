using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using EasySqlParser.SqlGenerator.Attributes;
using EasySqlParser.SqlGenerator.Enums;

namespace EasySqlParser.SqlGenerator.Tests.Oracle
{
    [Entity]
    [Table("EMP")]
    public class Employee
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NAME")]
        [StringLength(30)]
        public string Name { get; set; }

        [Column("SALARY")]
        public decimal Salary { get; set; }

        [Version]
        [Column("VERSION")]
        public long VersionNo { get; set; }

        public string GetDebugString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{nameof(Id)}\t{Id}");
            builder.AppendLine($"{nameof(Name)}\t{Name}");
            builder.AppendLine($"{nameof(Salary)}\t{Salary}");
            builder.AppendLine($"{nameof(VersionNo)}\t{VersionNo}");
            return builder.ToString();
        }
    }

    [Entity]
    [Table("EMP_WITH_DATE_USER")]
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
        [CurrentTimestamp("CURRENT_TIMESTAMP", GenerationStrategy.Insert)]
        public DateTime CreateDateTime { get; set; }

        [Column("CREATE_USER")]
        public string CreateUser { get; set; }

        [Column("UPDATE_DATETIME")]
        [CurrentTimestamp("CURRENT_TIMESTAMP", GenerationStrategy.Update)]
        public DateTime? UpdateDateTime { get; set; }

        [Column("UPDATE_USER")]
        public string UpdateUser { get; set; }


        [Column("DELETE_DATETIME")]
        [CurrentTimestamp("CURRENT_TIMESTAMP", GenerationStrategy.SoftDelete)]
        public DateTime? DeleteDateTime { get; set; }

        [Column("DELETE_USER")]
        public string DeleteUser { get; set; }



        [Version]
        [Column("VERSION")]
        public long VersionNo { get; set; }


    }

    [Entity]
    [Table("EMP_IDENTITY")]
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

    [Entity]
    [Table("EMP_SEQ")]
    public class EmployeeSeq
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NAME")]
        [StringLength(30)]
        public string Name { get; set; }

        [SequenceGenerator("LONG_SEQ")]
        [Column("LONG_COL")]
        public long LongCol { get; set; }

        [SequenceGenerator("STRING_SEQ", PaddingLength = 6, Prefix = "T")]
        [Column("STRING_COL")]
        [StringLength(10)]
        public string StringCol { get; set; }

        [Version]
        [Column("VERSION")]
        public long VersionNo { get; set; }

    }


}
