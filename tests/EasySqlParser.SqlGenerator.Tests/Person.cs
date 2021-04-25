using System;
using System.Collections.Generic;
using System.Text;

namespace EasySqlParser.SqlGenerator.Tests
{
    public class Person
    {
        public int Id { get; set; }


        public string Name { get; set; }


        public int Age { get; set; }


        public bool HasChildren { get; set; }


        public DateTimeOffset CreatedAt { get; set; }


        public DateTimeOffset ModifiedAt { get; set; }

    }
}
