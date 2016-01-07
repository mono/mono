using System;
using System.IO;
using System.Reflection;

namespace NUnitLite.Tests
{
    /// <summary>
    /// Runner used to execute Silverlight tests under a CI
    /// server. The tests end up executing using the proper
    /// Silverlight assemblies except for mscorlib and
    /// System.dll, for which the desktop versions are used.
    /// 
    /// This approach works so long as the tests don't require 
    /// any classes or methods that are only present in the
    /// Silverlight versions of those assemblies.
    /// 
    /// Note that the sole argument to the executable is
    /// the assembly name of the assembly to be tested, not
    /// the file name, i.e. no extension. For example:
    ///   ci-test-runner nunitlite.tests
    /// </summary>
    public class SilverlightTestRunner
    {
        static void Main(string[] args)
        {
            if (args.Length != 1 || IsHelpOption(args[0]))
                DisplayUsageAndExit();

            RunTestAssembly(args[0]);
        }

        static void RunTestAssembly(string assemblyName)
        {
            Assembly assembly = null;
            try
            {
                assembly = Assembly.Load(assemblyName);
            }
            catch(Exception ex)
            {
                Console.WriteLine("ERROR: Unable to load assembly {0}", assemblyName);
                Console.WriteLine(ex.ToString());
                DisplayUsageAndExit();
            }

            MethodInfo entryPoint = FindEntryPoint(assembly);
            if (entryPoint == null)
            {
                Console.WriteLine("ERROR: Unable to find entry point in assembly {0}", assemblyName);
                DisplayUsageAndExit();
            }

            object[] argList = new object[] { new string[0] };
            entryPoint.Invoke(null, argList);
        }

        static MethodInfo FindEntryPoint(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    if (method.Name == "Main" && method.ReturnType == typeof(void))
                    {
                        ParameterInfo[] parameters = method.GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string[]))
                            return method;
                    }

            return null;
        }

        static bool IsHelpOption(string arg)
        {
            arg = arg.ToLower();

            if (arg == "--help" || arg == "-help" || arg == "-h")
                return true;

            if (arg[0] == Path.PathSeparator)
                return false;

            return arg == "/h" || arg == "/help";
        }

        static void DisplayUsageAndExit()
        {
            Console.WriteLine("Usage: sl-test-runner <assemblyname>");
            Console.WriteLine();
            Console.WriteLine("The single argument must be the name of the test assembly");
            Console.WriteLine("to be run, without a file extension. The assembly must contain");
            Console.WriteLine("an entry method 'Main' taking a string[] as its sole argument");
            Console.WriteLine("and returning void.");
            Console.WriteLine();

            Environment.Exit(0);
        }
    }
}
