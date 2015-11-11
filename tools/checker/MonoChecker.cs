using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CppSharp.AST;
using CppSharp.AST.Extensions;
using CppSharp.Parser;
using Newtonsoft.Json;

namespace CppSharp
{
    /**
     * This tool reads the AST of a Mono source checkout.
     */
    static class MonoChecker
    {
        static string CompilationDatabasePath = @"";

        public static void Main(string[] args)
        {
            ParseCommandLineArgs(args);

            Console.WriteLine();
            Console.WriteLine("Parsing Mono's source code...");

            var options = new DriverOptions();

            var log = new TextDiagnosticPrinter();
            var driver = new Driver(options, log);

            Setup(driver);
            driver.Setup();

            BuildParseOptions(driver);
            if (!driver.ParseCode())
                return;

            Check(driver.ASTContext);
        }

        static void ParseCommandLineArgs(string[] args)
        {
            var needsArgs = string.IsNullOrWhiteSpace(CompilationDatabasePath);

            if (!needsArgs)
                return;

            if (args.Length >= 1)
                CompilationDatabasePath = Path.GetFullPath(args[0]);
            else
                CompilationDatabasePath = "compile_commands.json";

            if (!File.Exists(CompilationDatabasePath)) {
                Console.WriteLine("Could not find JSON compilation database '{0}'",
                    CompilationDatabasePath);
                Environment.Exit(0);
            }
        }

        static string GetXcodeToolchainPath()
        {
            var toolchains = Directory.EnumerateDirectories("/Applications", "Xcode*")
                .ToList();
            toolchains.Sort();

            var toolchainPath = toolchains.LastOrDefault();
            if (toolchainPath == null)
                throw new Exception("Could not find a valid Xcode SDK");

            return toolchainPath;
        }

        static string GetXcodeBuiltinIncludesFolder()
        {
            var toolchainPath = GetXcodeToolchainPath();

            var toolchains = Directory.EnumerateDirectories(Path.Combine(toolchainPath,
                "Contents/Developer/Toolchains")).ToList();
            toolchains.Sort();

            toolchainPath = toolchains.LastOrDefault();
            if (toolchainPath == null)
                throw new Exception("Could not find a valid Xcode toolchain");

            var includePaths = Directory.EnumerateDirectories(Path.Combine(toolchainPath,
                "usr/lib/clang")).ToList();
            var includePath = includePaths.LastOrDefault();

            if (includePath == null)
                throw new Exception("Could not find a valid Clang include folder");

            return Path.Combine(includePath, "include");
        }

        static void SetupXcode(Driver driver)
        {
            var options = driver.Options;

            var builtinsPath = GetXcodeBuiltinIncludesFolder();
            options.addSystemIncludeDirs(builtinsPath);

            var includePath = "/usr/include";
            options.addSystemIncludeDirs(includePath);

            options.NoBuiltinIncludes = true;
            options.NoStandardIncludes = true;
        }        

        static void Setup(Driver driver)
        {
            var options = driver.Options;
            options.DryRun = true;
            options.Verbose = false;
            options.LibraryName = "Mono";
            options.MicrosoftMode = false;
            options.addArguments("-xc");
            options.addArguments("-std=gnu99");

            SetupXcode(driver);
        }

        struct CompileUnit
        {
            public string directory;
            public string command;
            public string file;
        }

        static List<CompileUnit> CleanCompileUnits(List<CompileUnit> database)
        {
            // The compilation database we get from Bear has duplicated entries
            // for the same files, so clean it up before passing it down to
            // further processing.
            var units = new List<CompileUnit>();

            foreach (var unit in database) {
                // Ignore compile units compiled with PIC (Position-independent code)
                if (unit.command.EndsWith("-fPIC -DPIC"))
                    continue;

                // Ignore compile units that are compiled with gcc since in OSX
                //  it's a wrapper for the real compiler (clang) for which there'll
                //  be another entry.
                if (unit.command.Contains("gcc"))
                    continue;

                // Ignore the static runtime build.
                if (unit.command.Contains("_static_la"))
                    continue;

                // Ignore the Boehm runtime build.
                if (unit.command.Contains("libmonoruntime_la"))
                    continue;                    

                units.Add(unit);
            }

            return units;
        }

        static void BuildParseOptions(Driver driver)
        {
            var json = File.ReadAllText(CompilationDatabasePath);
            var compileUnits = JsonConvert.DeserializeObject<List<CompileUnit>>(json);

            compileUnits = CleanCompileUnits(compileUnits);
            compileUnits = compileUnits.OrderBy(unit => unit.file).ToList();

            foreach (var unit in compileUnits) {
                var source = driver.Project.AddFile(unit.file);
                source.Options = driver.BuildParseOptions(source);

                var args = unit.command.Split(new char[] {' '}).Skip(1);
                foreach (var arg in args) {
                    // Skip some arguments that Clang complains about...
                    var arguments = new List<string> {
                        "-no-cpp-precomp",
                        "-Qunused-arguments",
                        "-fno-strict-aliasing",
                        "-Qunused-arguments",
                        "-MD",
                        "-MF",
                        "-c"
                    };

                    if (arguments.Contains(arg))
                        continue;

                    source.Options.addArguments(arg);
                }
            }
        }

        static void Check(ASTContext context)
        {
            // TODO: Implement checking here
        }
    }
}
