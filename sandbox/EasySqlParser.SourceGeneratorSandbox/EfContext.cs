using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EasySqlParser.SourceGeneratorSandbox
{
    public class EfContext : DbContext
    {
        private readonly string _connectionString;

        public EfContext()
        {

        }

        //public EfContext(string connectionString)
        //{
        //    _connectionString = connectionString;
        //}

        public virtual DbSet<Employee> Employees { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer(_connectionString);
        }

    }
}
