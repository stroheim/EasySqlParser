using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySqlParser.SourceGeneratorSandbox
{
    public class SqlCondition
    {
        public List<string> MiddleNames { get; set; }
        public DateTime? BirthDateFrom { get; set; }
        public DateTime? BirthDateTo { get; set; }
        public string FirstName { get; set; }
    }
}
