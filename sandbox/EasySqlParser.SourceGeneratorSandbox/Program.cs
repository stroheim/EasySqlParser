using System;
using EasySqlParser.Configurations;
using EasySqlParser.SourceGeneratorSandbox.Interfaces;
using Microsoft.Data.SqlClient;

namespace EasySqlParser.SourceGeneratorSandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter(),
                "SqlResources");
            ConfigContainer.EnableCache = true;
            Console.Read();
        }
    }
}
