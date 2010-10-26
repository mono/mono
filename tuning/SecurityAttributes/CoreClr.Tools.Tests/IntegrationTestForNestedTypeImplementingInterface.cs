using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NUnit.Framework;

namespace CoreClr.Tools.Tests
{
    class IntegrationTestForNestedTypeImplementingInterface : IntegrationTestBase
    {
        // Step 1 - Step1: Detect methods that will fail at runtime if they're not [SC] or [SSC]
        [Test]
        public void Test()
        {
            var assembly1 = AssemblyCompiler.CompileTempAssembly(@"
            using System;
            using System.IO;
            using System.Runtime.CompilerServices;

            public interface IMyEnumerator
            {
                Object Current { get; }
            }

            public class SystemArray
            {
                private class SimpleEnumerator : IMyEnumerator
                {
                    SystemArray mycopy = null;
                    public Object Current
                    {
                        get { mycopy.DoSomethingBad(); return null; }
                    }
                }
                [MethodImpl(MethodImplOptions.InternalCall)]
                void DoSomethingBad()
                {
                }
            }
");

            var requirePrivilegesThemselves = DetectMethodsRequiringPrivileges(new TypeDefinition[]{} , assembly1);
            Console.WriteLine("Requires themselves: "+requirePrivilegesThemselves.Aggregate("", (c,i)=> c+i));
            //var requirePrivilegesThemselves = new List<string>() {"System.Void SystemArray/SimpleEnumerator::DoSomethingBad()"};
            var propagationReport = PropagateRequiredPrivileges(new TypeDefinition[]{}, new string[]{}, requirePrivilegesThemselves.ToArray(), assembly1);
            Console.WriteLine(propagationReport.InjectionInstructions);
            Assert.AreEqual(
@"SC-M: System.Void SystemArray::DoSomethingBad()
SC-M: System.Object SystemArray/SimpleEnumerator::get_Current()
SC-M: System.Object IMyEnumerator::get_Current()
",
 propagationReport.InjectionInstructions);
        }
    }
}
