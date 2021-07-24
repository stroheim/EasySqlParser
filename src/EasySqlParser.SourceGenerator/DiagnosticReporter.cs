using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace EasySqlParser.SourceGenerator
{
    // this idea from
    // https://github.com/canton7/RestEase/blob/master/src/Common/Implementation/Emission/DiagnosticReporter.Roslyn.cs
    internal class DiagnosticReporter
    {
        internal List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();

        private static DiagnosticDescriptor CreateDiagnosticDescriptor()
        {
            throw new NotImplementedException();
        }
    }
}
