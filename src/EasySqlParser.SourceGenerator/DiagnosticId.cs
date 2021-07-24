using System;
using System.Collections.Generic;
using System.Text;

namespace EasySqlParser.SourceGenerator
{
    /*
     * チェックすべきもの
     * methodに想定された属性が付加されているか
     * 戻り値の型
     * SQLファイルが存在するか
     */

    internal enum DiagnosticId
    {
        None=0,
    }

    internal static class DiagnosticIdExtension
    {
        internal static string Format(this DiagnosticId id)
        {
            return $"ESP{(int) id:D3}";
        }
    }
}
