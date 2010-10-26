using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

namespace CoreClr.Tools.Tests
{
    public static class CecilUtilsForTests
    {
        public static AssemblyDefinition GetExecutingAssemblyDefinition()
        {
            return AssemblyFactory.GetAssembly(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);
        }
    }
}
