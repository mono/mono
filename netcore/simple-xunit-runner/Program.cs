using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xunit.Runners;

namespace Xunit
{
    public class SimpleXunitRunner
    {
        const bool runInParallel = false; // works, but output is a bit messy
        
        static string[] igoredTests =
            {
                // works as "TestDisplayName contains X"
                "IndexOfSequenceMultipleMatch_Char",
                "Test_ToLower_Culture",
                "Exception_TargetSite_Aot",
                "PointerTests"
            };
        
        public static void Main(string[] args)
        {
            if (args?.Length != 1)
            {
                Console.WriteLine("\nInvalid arguments.\n\tUsage: assembly-path\n");
                return;
            }

            string asmPath = args[0];
            Environment.CurrentDirectory = Path.GetDirectoryName(asmPath);
            asmPath = Path.GetFileName(asmPath);

            if (File.Exists("Microsoft.DotNet.XUnitExtensions.dll")) 
                Assembly.LoadFrom("Microsoft.DotNet.XUnitExtensions.dll");
            
            if (File.Exists("CoreFx.Private.TestUtilities.dll")) 
                Assembly.LoadFrom("CoreFx.Private.TestUtilities.dll");

            Assembly.LoadFrom(asmPath);
            
            var mre = new ManualResetEvent(false);
            int testsCount = 0;
            using (var runner = AssemblyRunner.WithoutAppDomain(asmPath))
            {
                runner.OnTestFailed += i => Console.WriteLine($"FAILED: {i.MethodName}. ExceptionMessage: {i.ExceptionMessage} {i.ExceptionStackTrace}");
                //runner.OnTestOutput += i => Console.WriteLine(i.TestDisplayName + " output: " + i.Output);
                runner.OnErrorMessage += i => Console.WriteLine("OnErrorMessage: " + i.ExceptionMessage);
                runner.OnTestSkipped += i => Console.WriteLine($"SKIPPED: {i.TestDisplayName}");
                runner.OnDiscoveryComplete += i => Console.WriteLine($"TestCasesDiscovered={i.TestCasesDiscovered}, TestCasesToRun={i.TestCasesToRun}");
                runner.OnTestPassed += i => Console.WriteLine($"PASSED: {i.TestCollectionDisplayName}");
                runner.TestCaseFilter += t => !igoredTests.Any(i => t.DisplayName.Contains(i));
                runner.OnDiagnosticMessage += i => Console.WriteLine("OnDiagnosticMessage: " + i.Message);
                runner.OnTestFinished += i => Console.WriteLine($"{i.TestDisplayName} finished");
                runner.OnTestStarting += i =>  Console.WriteLine($"{i.TestDisplayName} started... (#{testsCount++})");

                runner.OnExecutionComplete += i =>
                    {
                        Console.WriteLine($"Done:\n\n\tTotalTests={i.TotalTests}, TestsFailed={i.TestsFailed}, TestsSkipped={i.TestsSkipped}, ExecutionTime={i.ExecutionTime}\n");
                        mre.Set();
                    };

                runner.Start(parallel: runInParallel, diagnosticMessages: false, internalDiagnosticMessages: false);
                mre.WaitOne();
            }
            Console.ReadKey();
        }
    }
}
