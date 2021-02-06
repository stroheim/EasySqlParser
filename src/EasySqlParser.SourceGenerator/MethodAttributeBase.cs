using System;
using System.Collections.Generic;
using System.Text;

namespace EasySqlParser.SourceGenerator
{
    // TODO: DOC
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class MethodAttributeBase : Attribute
    {
        /// <summary>
        /// 2-way-sql file path
        /// 
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// SQLコマンドのタイムアウト
        /// </summary>
        public int CommandTimeout { get; set; } = 30;

    }
}
