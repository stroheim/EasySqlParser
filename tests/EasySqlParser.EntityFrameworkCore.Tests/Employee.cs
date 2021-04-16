using System;
using System.Collections.Generic;
using System.Text;
using EasySqlParser.SqlGenerator;
using EasySqlParser.SqlGenerator.Attributes;

namespace EasySqlParser.EntityFrameworkCore.Tests
{
    public class Employee
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Salary { get; set; }

        [Version]
        public long VersionNo { get; set; }
    }
}
