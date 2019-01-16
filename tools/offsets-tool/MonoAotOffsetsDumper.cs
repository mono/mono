using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CppSharp.AST;
using CppSharp.AST.Extensions;
using CppSharp.Parser;

namespace CppSharp
{
    /**
     * This tool dumps the offsets of structures used in the Mono VM needed
     * by the AOT compiler for cross-compiling code to target platforms
     * different than the host the compiler is being invoked on.
     * 
     * It takes two arguments: the path to your clone of the Mono repo and
     * the path to the root of Android NDK.
     */
    static class MonoAotOffsetsDumper
    {
        static string MonoDir = @"";

        static List<string> Abis = new List<string> ();
        static string OutputDir;
        static string OutputFile;

        static string MonodroidDir = @"";
        static string AndroidNdkPath = @"";
        static string MaccoreDir = @"";
        static string TargetDir = @"";
        static bool GenIOS;
        static bool GenAndroid;

        public enum TargetPlatform
        {
            Android,
            iOS,
            WatchOS,
            OSX
        }

        public class Target
        {
            public Target()
            {
                Defines = new List<string>();
                Arguments = new List<string>();
            }

            public Target(Target target)
            {
                Platform = target.Platform;
                Triple = target.Triple;
                Build = target.Build;
                Defines = target.Defines;
                Arguments = target.Arguments;
            }

            public TargetPlatform Platform;
            public string Triple;
            public string Build;            
            public List<string> Defines;
            public List<string> Arguments;
        };

        public static List<Target> Targets = new List<Target>();

        public static IEnumerable<Target> AndroidTargets
        {
            get { return Targets.Where ((t) => t.Platform == TargetPlatform.Android); }
        }

        public static IEnumerable<Target> DarwinTargets
        {
            get
            {
                return Targets.Where ((t) => t.Platform == TargetPlatform.iOS ||
                    t.Platform == TargetPlatform.WatchOS ||
                                      t.Platform == TargetPlatform.OSX);
            }
        }

        public static IEnumerable<Target> iOSTargets
        {
            get
            {
                return Targets.Where ((t) => t.Platform == TargetPlatform.iOS);
            }
        }

        public static void SetupAndroidTargets()
        {
            Targets.Add (new Target {
                Platform = TargetPlatform.Android,
                Triple = "i686-none-linux-android",
                Defines = { "TARGET_X86" }
            });

            Targets.Add (new Target {
                Platform = TargetPlatform.Android,
                Triple = "x86_64-none-linux-android",
                Defines = { "TARGET_AMD64" }
            });            

            Targets.Add (new Target {
                Platform = TargetPlatform.Android,
                Triple = "armv5-none-linux-androideabi",
                Defines = { "TARGET_ARM", "ARM_FPU_VFP", "HAVE_ARMV5" }
            });

            Targets.Add (new Target {
                Platform = TargetPlatform.Android,
                Triple = "armv7-none-linux-androideabi",
                Defines = { "TARGET_ARM", "ARM_FPU_VFP", "HAVE_ARMV5", "HAVE_ARMV6",
                    "HAVE_ARMV7"
                }
            });

            Targets.Add (new Target {
                Platform = TargetPlatform.Android,
                Triple = "aarch64-v8a-linux-android",
                Defines = { "TARGET_ARM64" }
            });            

            /*Targets.Add(new Target {
                    Platform = TargetPlatform.Android,
                    Triple = "mipsel-none-linux-android",
                    Defines = { "TARGET_MIPS", "__mips__" }
                });*/

            foreach (var target in AndroidTargets)
                target.Defines.AddRange (new string[] { "HOST_ANDROID",
                    "TARGET_ANDROID", "MONO_CROSS_COMPILE", "USE_MONO_CTX"
                });
        }

        public static void SetupiOSTargets()
        {
            Targets.Add(new Target {
                Platform = TargetPlatform.iOS,
                Triple = "arm-apple-darwin10",
                Build = "target7",
                Defines = { "TARGET_ARM", "ARM_FPU_VFP", "HAVE_ARMV5" }
            });

            Targets.Add(new Target {
                Platform = TargetPlatform.iOS,
                Triple = "aarch64-apple-darwin10",
                Build = "target64",                    
                Defines = { "TARGET_ARM64" }
            });

            foreach (var target in iOSTargets) {
                target.Defines.AddRange (new string[] { "HOST_DARWIN",
                    "TARGET_IOS", "TARGET_MACH", "MONO_CROSS_COMPILE", "USE_MONO_CTX",
                    "_XOPEN_SOURCE"
                });
            }

            Targets.Add(new Target {
                Platform = TargetPlatform.WatchOS,
                Triple = "armv7k-apple-darwin",
                Build = "targetwatch",
                Defines = { "TARGET_ARM", "ARM_FPU_VFP", "HAVE_ARMV5" }
            });

            foreach (var target in DarwinTargets) {
                target.Defines.AddRange (new string[] { "HOST_DARWIN",
                    "TARGET_IOS", "TARGET_MACH", "MONO_CROSS_COMPILE", "USE_MONO_CTX",
                    "_XOPEN_SOURCE"
                });
            }
        }

        public static void SetupOtherTargets()
        {
            if (Abis.Count != 1) {
                Console.WriteLine ("Exactly --abi= argument is required.");
                Environment.Exit (1);
            }
            string abi = Abis [0];
            if (abi == "i386-apple-darwin13.0.0") {
                Targets.Add(new Target {
                        Platform = TargetPlatform.OSX,
                        Triple = "i386-apple-darwin13.0.0",
                        Build = "",
                        Defines = { "TARGET_X86" },
                });
            } else {
                Console.WriteLine ($"Unsupported abi: {abi}.");
                Environment.Exit (1);
            }
        }

        static bool GetParentSubDirectoryPath(string parent, out string subdir)
        {
            var directory = Directory.GetParent(Directory.GetCurrentDirectory());

            while (directory != null) {
                var path = Path.Combine(directory.FullName, parent);

                if (Directory.Exists (path)) {
                    subdir = path;
                    return true;
                }

                directory = directory.Parent;
            }

            subdir = null;
            return false;
        }

        public static int Main(string[] args)
        {
            ParseCommandLineArgs(args);

            string monodroidDir;
            if (!Directory.Exists (MonodroidDir) &&
                GetParentSubDirectoryPath ("monodroid", out monodroidDir)) {
                MonodroidDir = Path.Combine (monodroidDir);
            }

            if (Directory.Exists (MonodroidDir) || GenAndroid)
                SetupAndroidTargets();

            string maccoreDir;
            if (!Directory.Exists (MaccoreDir) &&
                GetParentSubDirectoryPath ("maccore", out maccoreDir)) {
                MaccoreDir = Path.Combine (maccoreDir);
            }

            if (Directory.Exists(MaccoreDir) || GenIOS)
                SetupiOSTargets();

            if (Targets.Count == 0)
                SetupOtherTargets ();

            foreach (var target in Targets)
             {
                if (Abis.Any() && !Abis.Any (target.Triple.Contains))
                    continue;
                
                Console.WriteLine();
                Console.WriteLine("Processing triple: {0}", target.Triple);

                var options = new DriverOptions();

                var driver = new Driver(options);

                Setup(driver, target);
                driver.Setup();

                BuildParseOptions(driver, target);
                if (!driver.ParseCode())
                    return 1;

                Dump(driver.Context.ASTContext, driver.Context.TargetInfo, target);
            }
            return 0;
        }

        static void BuildParseOptions(Driver driver, Target target)
        {
            foreach (var header in driver.Options.Headers)
            {
                var source = driver.Project.AddFile(header);
                source.Options = driver.BuildParserOptions(source);

                if (header.Contains ("mini"))
                    continue;

                source.Options.AddDefines ("HAVE_SGEN_GC");
                source.Options.AddDefines ("HAVE_MOVING_COLLECTOR");
                source.Options.AddDefines("MONO_GENERATING_OFFSETS");
            }
        }

        static string GetAndroidNdkPath()
        {
            if (!String.IsNullOrEmpty (AndroidNdkPath))
                return AndroidNdkPath;

            // Find the Android NDK's path from Monodroid's config.
            var configFile = Path.Combine(MonodroidDir, "env.config");
            if (!File.Exists(configFile))
                throw new Exception("Expected a valid Monodroid environment config file at " + configFile);

            var config = File.ReadAllText(configFile);
            var match = Regex.Match(config, @"ANDROID_NDK_PATH\s*:=\s(.*)");
            return match.Groups[1].Value.Trim();
        }

        static void ParseCommandLineArgs(string[] args)
        {
            var showHelp = false;

            var options = new Mono.Options.OptionSet () {
                { "abi=", "ABI triple to generate", v => Abis.Add(v) },
                { "o|out=", "output directory", v => OutputDir = v },
                { "outfile=", "output directory", v => OutputFile = v },
                { "maccore=", "include directory", v => MaccoreDir = v },
                { "monodroid=", "top monodroid directory", v => MonodroidDir = v },
                { "android-ndk=", "Path to Android NDK", v => AndroidNdkPath = v },
                { "targetdir=", "Path to the directory containing the mono build", v =>TargetDir = v },
                { "mono=", "include directory", v => MonoDir = v },
                { "gen-ios", "generate iOS offsets", v => GenIOS = v != null },
                { "gen-android", "generate Android offsets", v => GenAndroid = v != null },
                { "h|help",  "show this message and exit",  v => showHelp = v != null },
            };

            try {
                options.Parse (args);
            }
            catch (Mono.Options.OptionException e) {
                Console.WriteLine (e.Message);
                Environment.Exit(0);
            }

            if (showHelp)
            {
                // Print usage and exit.
                Console.WriteLine("{0} <options>",
                    AppDomain.CurrentDomain.FriendlyName);
                options.WriteOptionDescriptions (Console.Out);
                Environment.Exit(0);
            }
        }

        static void Setup(Driver driver, Target target)
        {
            var options = driver.Options;
            options.DryRun = true;
            options.LibraryName = "Mono";

            var parserOptions = driver.ParserOptions;
            parserOptions.Verbose = false;
            parserOptions.MicrosoftMode = false;
            parserOptions.AddArguments("-xc");
            parserOptions.AddArguments("-std=gnu99");
            parserOptions.AddDefines("CPPSHARP");
            parserOptions.AddDefines("MONO_GENERATING_OFFSETS");

            foreach (var define in target.Defines)
                parserOptions.AddDefines(define);

            SetupToolchainPaths(driver, target);

            SetupMono(driver, target);
        }

        static void SetupMono(Driver driver, Target target)
        {
            string targetBuild;
            switch (target.Platform) {
            case TargetPlatform.Android:
                if (TargetDir == "") {
                    Console.Error.WriteLine ("The --targetdir= option is required when targeting android.");
                    Environment.Exit (1);
                }
                if (MonoDir == "") {
                    Console.Error.WriteLine ("The --mono= option is required when targeting android.");
                    Environment.Exit (1);
                }
                if (Abis.Count != 1) {
                    Console.Error.WriteLine ("Exactly one --abi= argument is required when targeting android.");
                    Environment.Exit (1);
                }
                targetBuild = TargetDir;
                break;
            case TargetPlatform.WatchOS:
            case TargetPlatform.iOS: {
                if (!string.IsNullOrEmpty (TargetDir)) {
                    targetBuild = TargetDir;
                } else {
                    string targetPath = Path.Combine (MaccoreDir, "builds");
                    if (!Directory.Exists (MonoDir))
                        MonoDir = Path.GetFullPath (Path.Combine (targetPath, "../../mono"));
                    targetBuild = Path.Combine(targetPath, target.Build);
                }
                break;
            }
            case TargetPlatform.OSX:
                if (MonoDir == "") {
                    Console.Error.WriteLine ("The --mono= option is required when targeting osx.");
                    Environment.Exit (1);
                }
                targetBuild = ".";
                break;
            default:
                throw new ArgumentOutOfRangeException ();
            }

            if (!Directory.Exists(targetBuild))
                throw new Exception(string.Format("Could not find the target build directory: {0}", targetBuild));

            var includeDirs = new[]
            {
                targetBuild,
                Path.Combine(targetBuild, "mono", "eglib"),
                MonoDir,
                Path.Combine(MonoDir, "mono"),
                Path.Combine(MonoDir, "mono", "mini"),
                Path.Combine(MonoDir, "mono", "eglib")
            };

            foreach (var inc in includeDirs)
                driver.ParserOptions.AddIncludeDirs(inc);

            var filesToParse = new[]
            {
                Path.Combine(MonoDir, "mono", "metadata", "metadata-cross-helpers.c"),
                Path.Combine(MonoDir, "mono", "mini", "mini-cross-helpers.c"),
            };

            foreach (var file in filesToParse)
                driver.Options.Headers.Add(file);
        }

        static void SetupMSVC(Driver driver, string triple)
        {
            var parserOptions = driver.ParserOptions;

            parserOptions.Abi = Parser.AST.CppAbi.Microsoft;
            parserOptions.MicrosoftMode = true;

            var systemIncludeDirs = new[]
            {
                @"C:\Program Files (x86)\Windows Kits\8.1\Include\um",
                @"C:\Program Files (x86)\Windows Kits\8.1\Include\shared"
            };

            foreach (var inc in systemIncludeDirs)
                parserOptions.AddSystemIncludeDirs(inc);

            parserOptions.AddDefines("HOST_WIN32");
        }

        static void SetupToolchainPaths(Driver driver, Target target)
        {
            switch (target.Platform) {
            case TargetPlatform.Android:
                SetupAndroidNDK(driver, target);
                break;
            case TargetPlatform.iOS:
            case TargetPlatform.WatchOS:
            case TargetPlatform.OSX:
                SetupXcode(driver, target);
                break;
            default:
                throw new ArgumentOutOfRangeException ();
            }
        }        

        static string GetArchFromTriple(string triple)
        {
            if (triple.Contains("mips"))
                return "mips";

            if (triple.Contains("arm64") || triple.Contains("aarch64"))
                return "arm64";

            if (triple.Contains("arm"))
                return "arm";

            if (triple.Contains("i686"))
                return "x86";

            if (triple.Contains("x86_64"))
                return "x86_64";

            throw  new Exception("Unknown architecture from triple: " + triple);
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

        static string GetXcodeiOSIncludesFolder()
        {
            var toolchainPath = GetXcodeToolchainPath();

            var sdkPaths = Directory.EnumerateDirectories(Path.Combine(toolchainPath,
                "Contents/Developer/Platforms/iPhoneOS.platform/Developer/SDKs")).ToList();
            var sdkPath = sdkPaths.LastOrDefault();

            if (sdkPath == null)
                throw new Exception("Could not find a valid iPhone SDK");

            return Path.Combine(sdkPath, "usr/include");
        }

        static string GetXcodeOSXIncludesFolder()
        {
            var toolchainPath = GetXcodeToolchainPath();

            var sdkPaths = Directory.EnumerateDirectories(Path.Combine(toolchainPath,
                "Contents/Developer/Platforms/MacOSX.platform/Developer/SDKs")).ToList();
            var sdkPath = sdkPaths.LastOrDefault();

            if (sdkPath == null)
                throw new Exception("Could not find a valid OSX SDK");

            return Path.Combine(sdkPath, "usr/include");
        }

        static string GetXcodeWatchOSIncludesFolder()
        {
            var toolchainPath = GetXcodeToolchainPath();

            var sdkPaths = Directory.EnumerateDirectories(Path.Combine(toolchainPath,
                "Contents/Developer/Platforms/WatchOS.platform/Developer/SDKs")).ToList();
            var sdkPath = sdkPaths.LastOrDefault();

            if (sdkPath == null)
                throw new Exception("Could not find a valid WatchOS SDK");

            return Path.Combine(sdkPath, "usr/include");
        }

        static void SetupXcode(Driver driver, Target target)
        {
            var parserOptions = driver.ParserOptions;

            var builtinsPath = GetXcodeBuiltinIncludesFolder();
            string includePath;

            switch (target.Platform) {
            case TargetPlatform.iOS:
                includePath = GetXcodeiOSIncludesFolder();
                break;
            case TargetPlatform.WatchOS:
                includePath = GetXcodeWatchOSIncludesFolder();
                break;
            case TargetPlatform.OSX:
                includePath = GetXcodeOSXIncludesFolder();
                break;
            default:
                throw new ArgumentOutOfRangeException ();
            }

            parserOptions.AddSystemIncludeDirs(builtinsPath);
            parserOptions.AddSystemIncludeDirs(includePath);

            parserOptions.NoBuiltinIncludes = true;
            parserOptions.NoStandardIncludes = true;
            parserOptions.TargetTriple = target.Triple;
        }

        static string GetAndroidHostToolchainPath()
        {
            var androidNdkPath = GetAndroidNdkPath ();
            var toolchains = Directory.EnumerateDirectories(
                Path.Combine(androidNdkPath, "toolchains"), "llvm*").ToList();
            toolchains.Sort();

            var toolchainPath = toolchains.LastOrDefault();
            if (toolchainPath == null)
                throw new Exception("Could not find a valid NDK host toolchain");

            toolchains = Directory.EnumerateDirectories(Path.Combine(toolchainPath,
                "prebuilt")).ToList();
            toolchains.Sort();

            toolchainPath = toolchains.LastOrDefault();
            if (toolchainPath == null)
                throw new Exception("Could not find a valid NDK host toolchain");

            return toolchainPath;
        }

        static string GetAndroidBuiltinIncludesFolder()
        {
            var toolchainPath = GetAndroidHostToolchainPath();

            string clangToolchainPath = Path.Combine(toolchainPath, "lib64", "clang");
            if (!Directory.Exists (clangToolchainPath))
                clangToolchainPath = Path.Combine(toolchainPath, "lib", "clang");

            string includePath = null;
            if (Directory.Exists (clangToolchainPath)) {
                var includePaths = Directory.EnumerateDirectories(clangToolchainPath).ToList();
                includePath = includePaths.LastOrDefault();
            }
            if (includePath == null)
                throw new Exception("Could not find a valid Clang include folder");

            return Path.Combine(includePath, "include");
        }

        static void SetupAndroidNDK(Driver driver, Target target)
        {
            var options = driver.Options;
            var parserOptions = driver.ParserOptions;

            var builtinsPath = GetAndroidBuiltinIncludesFolder();
            parserOptions.AddSystemIncludeDirs(builtinsPath);

            var androidNdkRoot = GetAndroidNdkPath ();
            const int androidNdkApiLevel = 21;

            string arch = GetArchFromTriple(target.Triple);
            var toolchainPath = Path.Combine(androidNdkRoot, "platforms",
                "android-" + androidNdkApiLevel, "arch-" + arch,
                "usr", "include");

            if (!Directory.Exists (toolchainPath)) {
                // Android NDK r17 and newer no longer have per-platform include directories, they instead use a
                // unified set of headers
                toolchainPath = Path.Combine (AndroidNdkPath, "sysroot", "usr", "include");

                // The unified headers require that the target API level is defined as a macro - that's how they
                // differentiate between native APIs available for the given API level
                parserOptions.AddDefines ($"__ANDROID_API__={androidNdkApiLevel}");

                // And they also need to point to the per-arch `asm` directory
                string asmTriple;
                switch (arch) {
                        case "arm64":
                                asmTriple = "aarch64-linux-android";
                                break;

                        case "arm":
                                asmTriple = "arm-linux-androideabi";
                                break;

                        case "x86":
                                asmTriple = "i686-linux-android";
                                break;

                        case "x86_64":
                                asmTriple = "x86_64-linux-android";
                                break;

                        default:
                                throw new Exception ($"Unsupported architecture {arch}");
                }

                parserOptions.AddSystemIncludeDirs (Path.Combine (toolchainPath, asmTriple));
            }

            parserOptions.AddSystemIncludeDirs(toolchainPath);

            parserOptions.NoBuiltinIncludes = true;
            parserOptions.NoStandardIncludes = true;
            parserOptions.TargetTriple = target.Triple;
        }

        static uint GetTypeAlign(ParserTargetInfo target, ParserIntType type)
        {
            switch (type)
            {
                case ParserIntType.SignedChar:
                case ParserIntType.UnsignedChar:
                    return target.CharAlign;
                case ParserIntType.SignedShort:
                case ParserIntType.UnsignedShort:
                    return target.ShortAlign;
                case ParserIntType.SignedInt:
                case ParserIntType.UnsignedInt:
                    return target.IntAlign;
                case ParserIntType.SignedLong:
                case ParserIntType.UnsignedLong:
                    return target.LongAlign;
                case ParserIntType.SignedLongLong:
                case ParserIntType.UnsignedLongLong:
                    return target.LongLongAlign;
                default:
                    throw new Exception("Type has no alignment");
            }
        }

        static uint GetTypeSize(ParserTargetInfo target, ParserIntType type)
        {
            switch (type)
            {
                case ParserIntType.SignedChar:
                case ParserIntType.UnsignedChar:
                    return target.CharWidth;
                case ParserIntType.SignedShort:
                case ParserIntType.UnsignedShort:
                    return target.ShortWidth;
                case ParserIntType.SignedInt:
                case ParserIntType.UnsignedInt:
                    return target.IntWidth;
                case ParserIntType.SignedLong:
                case ParserIntType.UnsignedLong:
                    return target.LongWidth;
                case ParserIntType.SignedLongLong:
                case ParserIntType.UnsignedLongLong:
                    return target.LongLongWidth;
                default:
                    throw new Exception("Type has no size");
            }
        }

        static string GetTargetPlatformDefine(TargetPlatform target)
        {
            switch (target) {
            case TargetPlatform.Android:
                return "TARGET_ANDROID";
            case TargetPlatform.iOS:
                return "TARGET_IOS";
            case TargetPlatform.WatchOS:
                return "TARGET_WATCHOS";
            case TargetPlatform.OSX:
                return "TARGET_OSX";
            default:
                throw new ArgumentOutOfRangeException ();
            }
        }

        static void Dump(ASTContext ctx, ParserTargetInfo targetInfo, Target target)
        {
            string targetFile;

            if (!string.IsNullOrEmpty (OutputFile)) {
                targetFile = OutputFile;
            } else {
                targetFile = target.Triple;

                if (!string.IsNullOrEmpty (OutputDir))
                    targetFile = Path.Combine (OutputDir, targetFile);

                targetFile += ".h";
            }

            using (var writer = new StreamWriter(targetFile))
            //using (var writer = Console.Out)
            {
                writer.WriteLine("#ifndef USED_CROSS_COMPILER_OFFSETS");
                writer.WriteLine("#ifdef {0}", target.Defines[0]);
                writer.WriteLine ("#ifdef {0}", GetTargetPlatformDefine (target.Platform));
                writer.WriteLine("#ifndef HAVE_BOEHM_GC");
                writer.WriteLine("#define HAS_CROSS_COMPILER_OFFSETS");
                writer.WriteLine("#if defined (USE_CROSS_COMPILE_OFFSETS) || defined (MONO_CROSS_COMPILE)");
                writer.WriteLine("#if !defined (DISABLE_METADATA_OFFSETS)");
                writer.WriteLine("#define USED_CROSS_COMPILER_OFFSETS");

                DumpAligns(writer, targetInfo);
                DumpSizes(writer, targetInfo);
                DumpMetadataOffsets(writer, ctx, target);

                writer.WriteLine("#endif //disable metadata check");

                DumpJITOffsets(writer, ctx);

                writer.WriteLine("#endif //cross compiler checks");
                writer.WriteLine("#endif //gc check");
                writer.WriteLine("#endif //os check");
                writer.WriteLine("#endif //arch check");
                writer.WriteLine("#endif //USED_CROSS_COMPILER_OFFSETS check");
            }

            Console.WriteLine("Generated offsets file: {0}", targetFile);
        }

        static void DumpAligns(TextWriter writer, ParserTargetInfo target)
        {
            var aligns = new[]
            {
                new { Name = "gint8", Align = target.CharAlign},
                new { Name = "gint16", Align = target.ShortAlign},
                new { Name = "gint32", Align = target.IntAlign},
                new { Name = "gint64", Align = GetTypeAlign(target, target.Int64Type)},
                new { Name = "float", Align = target.FloatAlign},
                new { Name = "double", Align = target.DoubleAlign},
                new { Name = "gpointer", Align = GetTypeAlign(target, target.IntPtrType)},
            };

            // Write the alignment info for the basic types.
            foreach (var align in aligns)
                writer.WriteLine("DECL_ALIGN2({0},{1})", align.Name, align.Align / 8);
        }

        static void DumpSizes(TextWriter writer, ParserTargetInfo target)
        {
            var sizes = new[]
            {
                new { Name = "gint8", Size = target.CharWidth},
                new { Name = "gint16", Size = target.ShortWidth},
                new { Name = "gint32", Size = target.IntWidth},
                new { Name = "gint64", Size = GetTypeSize(target, target.Int64Type)},
                new { Name = "float", Size = target.FloatWidth},
                new { Name = "double", Size = target.DoubleWidth},
                new { Name = "gpointer", Size = GetTypeSize(target, target.IntPtrType)},
            };

            // Write the size info for the basic types.
            foreach (var size in sizes)
                writer.WriteLine("DECL_SIZE2({0},{1})", size.Name, size.Size / 8);
        }

        static Class GetClassFromTypedef(ITypedDecl typedef)
        {
            var type = typedef.Type.Desugar() as TagType;
            if (type == null)
                return null;

            var @class = type.Declaration as Class;

            return @class.IsIncomplete ?
                (@class.CompleteDeclaration as Class) : @class; 
        }

        static void DumpClasses(TextWriter writer, ASTContext ctx, IEnumerable<string> types,
            bool optional = false)
        {
            foreach (var @struct in types)
            {
                var @class = ctx.FindCompleteClass(@struct);
                if (@class == null)
                    @class = ctx.FindCompleteClass("_" + @struct);

                if (@class == null)
                {
                    var typedef = ctx.FindTypedef(@struct).FirstOrDefault(
                        decl => !decl.IsIncomplete);

                    if (typedef != null)
                        @class = GetClassFromTypedef(typedef);
                }

                if (@class == null && optional)
                    continue;

                if (@class == null)
                    throw new Exception("Expected to find struct definition for " + @struct);

                DumpStruct(writer, @class);
            }
        }

        static void DumpMetadataOffsets(TextWriter writer, ASTContext ctx, Target target)
        {
            var types = new List<string>
            {
                "MonoObject",
                "MonoObjectHandlePayload",
                "MonoClass",
                "MonoVTable",
                "MonoDelegate",
                "MonoInternalThread",
                "MonoMulticastDelegate",
                "MonoTransparentProxy",
                "MonoRealProxy",
                "MonoRemoteClass",
                "MonoArray",
                "MonoArrayBounds",
                "MonoSafeHandle",
                "MonoHandleRef",
                "MonoComInteropProxy",
                "MonoString",
                "MonoException",
                "MonoTypedRef",
                "MonoThreadsSync",
                "SgenThreadInfo",
                "SgenClientThreadInfo",
                "MonoProfilerCallContext"
            };

            DumpClasses(writer, ctx, types);
        }

        static void DumpJITOffsets(TextWriter writer, ASTContext ctx)
        {
            writer.WriteLine("#ifndef DISABLE_JIT_OFFSETS");
            writer.WriteLine("#define USED_CROSS_COMPILER_OFFSETS");

            var types = new[]
            {
                "MonoLMF",
                "MonoMethodRuntimeGenericContext",
                "MonoJitTlsData",
                "MonoGSharedVtMethodRuntimeInfo",
                "MonoContinuation",
                "MonoContext",
                "MonoDelegateTrampInfo",
            };

            DumpClasses(writer, ctx, types);

            var optionalTypes = new[]
            {
                "GSharedVtCallInfo",
                "SeqPointInfo",
                "DynCallArgs", 
                "MonoLMFTramp",
                "CallContext",
                "MonoFtnDesc"
            };

            DumpClasses(writer, ctx, optionalTypes, optional: true);

            writer.WriteLine("#endif //disable jit check");
        }

        static void DumpStruct(TextWriter writer, Class @class)
        {
            var name = @class.Name;
            if (name.StartsWith ("_", StringComparison.Ordinal))
                name = name.Substring (1);

            writer.WriteLine ("DECL_SIZE2({0},{1})", name, @class.Layout.Size);

            foreach (var field in @class.Fields)
            {
                if (field.IsBitField) continue;

                if (name == "SgenThreadInfo" && field.Name == "regs")
                    continue;

                var layout = @class.Layout.Fields.First(f => f.FieldPtr == field.OriginalPtr);

                writer.WriteLine("DECL_OFFSET2({0},{1},{2})", name, field.Name,
                    layout.Offset);
            }
        }
    }
}
