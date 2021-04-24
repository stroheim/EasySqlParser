﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using EasySqlParser.SqlGenerator.Attributes;

namespace EasySqlParser.Dapper.Tests.Sqlite
{
    [Entity]
    [Table("EMP")]
    public class Employee
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NAME")]
        public string Name { get; set; }

        [Column("SALARY")]
        public double Salary { get; set; }

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
}