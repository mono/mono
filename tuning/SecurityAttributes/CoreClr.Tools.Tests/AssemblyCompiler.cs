using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace CoreClr.Tools.Tests
{
    class AssemblyCompiler
    {
        public static AssemblyDefinition CompileTempAssembly(string code, params string[] references)
        {
            return AssemblyFactory.GetAssembly(CSharpCompiler.CompileTempAssembly(code, references));
        }
    }
}
