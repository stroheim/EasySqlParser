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
}
