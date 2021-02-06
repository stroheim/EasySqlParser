using System;
using System.Collections.Generic;
using System.Text;

namespace EasySqlParser.SourceGenerator
{
    // TODO: DOC
    /// <summary>
    /// Attribute for INSERT sql
    /// </summary>
    public class InsertAttribute : NonQueryAttribute
    {
        //public InsertAttribute(bool excludeNull = false)
        //{
        //    ExcludeNull = excludeNull;
        //}

        public bool ExcludeNull { get; set; } = false;
    }
}
