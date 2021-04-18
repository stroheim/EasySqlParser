using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EasySqlParser.EntityFrameworkCore.Tests.Oracle
{
    public class OracleContext : DbContext
    {
        public OracleContext(DbContextOptions<OracleContext> options) : base(options)
        {

        }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Characters> Characters { get; set; }

        public DbSet<MetalGearSeries> MetalGearSeries { get; set; }

        public DbSet<EmployeeSeq> EmployeeSeqs { get; set; }
    }
}
