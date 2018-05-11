using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public static class Program {
    public static int Main (string[] _args) {
        var args = new List<string> (_args);
        bool useStdout = false, showHelp = false, strictMode = false;

        for (int i = 0; i < args.Count; i++) {
            var arg = args[i];
            if (!arg.StartsWith ("-"))
                continue;

            switch (arg) {
                case "-?":
                case "--help":
                case "-h":
                    showHelp = true;
                    break;
                case "--trace":
                case "--trace1":
                    SourcesParser.TraceLevel = 1;
                    break;
                case "--trace2":
                    SourcesParser.TraceLevel = 2;
                    break;
                case "--trace3":
                    SourcesParser.TraceLevel = 3;
                    break;
                case "--trace4":
                    SourcesParser.TraceLevel = 4;
                    break;
                case "--stdout":
                    useStdout = true;
                    break;
                case "--strict":
                    strictMode = true;
                    break;
                default:
                    Console.Error.WriteLine ("Unrecognized switch " + arg);
                    break;
            }

            args.RemoveAt (i);
            i--;            
        }

        if (args.Count != 4)
            showHelp = true;

        if (showHelp) {
            Console.Error.WriteLine ("Usage: mcs/build/gensources.exe [options] (outputFileName|--stdout) libraryDirectoryAndName platformName profileName");
            Console.Error.WriteLine ("You can specify * for platformName and profileName to read all sources files");
            Console.Error.WriteLine ("Available options:");
            Console.Error.WriteLine ("--help -h -?");
            Console.Error.WriteLine ("  Show command line info");
            Console.Error.WriteLine ("--trace1 --trace2 --trace3 --trace4");
            Console.Error.WriteLine ("  Enable diagnostic output");
            Console.Error.WriteLine ("--stdout");
            Console.Error.WriteLine ("  Writes results to standard output (omit outputFileName if you use this)");
            Console.Error.WriteLine ("--strict");
            Console.Error.WriteLine ("  Produces an error exit code if files or directories are invalid/missing");
            return 1;
        }

        var myAssembly = Assembly.GetExecutingAssembly ();
        var codeBase = new Uri (myAssembly.CodeBase);
        var executablePath = Path.GetFullPath (codeBase.LocalPath);
        var executableDirectory = Path.GetDirectoryName (executablePath);

        var outFile = Path.GetFullPath (args[0]);
        var libraryFullName = Path.GetFullPath (args[1]);
        var platformName = args[2];
        var profileName = args[3];
        var platformsFolder = Path.Combine (executableDirectory, "platforms");
        var profilesFolder = Path.Combine (executableDirectory, "profiles");

        var libraryDirectory = Path.GetDirectoryName (libraryFullName);
        var libraryName = Path.GetFileName (libraryFullName);

        var parser = new SourcesParser (platformsFolder, profilesFolder);
        var result = parser.Parse (libraryDirectory, libraryName, platformName, profileName);

        if (SourcesParser.TraceLevel > 0)
            Console.Error.WriteLine ($"// Writing sources for platform {platformName} and profile {profileName}, relative to {libraryDirectory}, to {outFile}.");

        TextWriter output;
        if (useStdout)
            output = Console.Out;
        else
            output = new StreamWriter (outFile);

        using (output) {
            foreach (var fileName in result.GetFileNames ().OrderBy (s => s, StringComparer.Ordinal))
                output.WriteLine (fileName);
        }

        if (strictMode)
            return result.ErrorCount;
        else
            return 0;
    }
}

public struct ParseEntry {
    public string SourcesFileName;
    public string Directory;
    public string Pattern;
    public string HostPlatform;
    public string ProfileName;
}

public struct Source {
    public string FileName;
}

public class ParseResult {
    public readonly string LibraryDirectory, LibraryName;

    public readonly List<ParseEntry> Sources = new List<ParseEntry> ();
    public readonly List<ParseEntry> Exclusions = new List<ParseEntry> ();

    // FIXME: This is a bad spot for this value but enumerators don't have outparam support
    public int ErrorCount = 0;

    public ParseResult (string libraryDirectory, string libraryName) {
        LibraryDirectory = libraryDirectory;
        LibraryName = libraryName;
    }

    private static string GetRelativePath (string fullPath, string relativeToDirectory) {
        fullPath = fullPath.Replace (SourcesParser.DirectorySeparator, "/");
        relativeToDirectory = relativeToDirectory.Replace (SourcesParser.DirectorySeparator, "/");

        if (!relativeToDirectory.EndsWith (SourcesParser.DirectorySeparator))
            relativeToDirectory += SourcesParser.DirectorySeparator;
        var dirUri = new Uri (relativeToDirectory);
        var pathUri = new Uri (fullPath);

        var relativeUri = Uri.UnescapeDataString (
            dirUri.MakeRelativeUri (pathUri).OriginalString
        ).Replace ("/", SourcesParser.DirectorySeparator);

        if (SourcesParser.TraceLevel >= 4)
            Console.Error.WriteLine ($"// {fullPath} -> {relativeUri}");

        return relativeUri;
    }

    private IEnumerable<string> EnumerateMatches (
        IEnumerable<ParseEntry> entries,
        string hostPlatformName, string profileName
    ) {
        foreach (var entry in entries) {
            if (
                (hostPlatformName != null) &&
                (entry.HostPlatform ?? hostPlatformName) != hostPlatformName
            )
                continue;
            if (
                (profileName != null) &&
                (entry.ProfileName ?? profileName) != profileName
            )
                continue;

            var absolutePath = Path.Combine (entry.Directory, entry.Pattern);
            var absoluteDirectory = Path.GetDirectoryName (absolutePath);
            var absolutePattern = Path.GetFileName (absolutePath);

            if (SourcesParser.TraceLevel >= 3) {
                if ((absolutePattern != entry.Pattern) || (absoluteDirectory != entry.Directory))
                    Console.Error.WriteLine ($"// {entry.Directory} / {entry.Pattern} -> {absoluteDirectory} / {absolutePattern}");
            }            

            if (!Directory.Exists (absoluteDirectory)) {
                Console.Error.WriteLine ($"Directory does not exist: {Path.GetFullPath (absoluteDirectory)}");
                ErrorCount += 1;
                continue;
            }

            var matchingFiles = Directory.GetFiles (absoluteDirectory, absolutePattern);
            foreach (var fileName in matchingFiles) {
                var relativePath = GetRelativePath (fileName, LibraryDirectory);
                yield return relativePath;
            }
        }
    }

    // If you loaded sources files for multiple profiles, you can use the arguments here
    //  to filter the results
    public IEnumerable<string> GetFileNames (
        string hostPlatformName = null, string profileName = null
    ) {
        var encounteredFileNames = new HashSet<string> (StringComparer.Ordinal);

        var excludedFiles = new HashSet<string> (
            EnumerateMatches (Exclusions, hostPlatformName, profileName),
            StringComparer.Ordinal
        );

        foreach (var fileName in EnumerateMatches (Sources, hostPlatformName, profileName)) {
            if (excludedFiles.Contains (fileName)) {
                if (SourcesParser.TraceLevel >= 3)
                    Console.Error.WriteLine ($"// Excluding {fileName}");
                continue;
            }

            // Skip duplicates
            if (encounteredFileNames.Contains (fileName))
                continue;

            encounteredFileNames.Add (fileName);
            yield return fileName;
        }
    }
}

public class SourcesParser {
    public static readonly string DirectorySeparator = new String (Path.DirectorySeparatorChar, 1);    
    public static int TraceLevel = 0;

    private class State {
        public ParseResult Result;
        public string HostPlatform;
        public string ProfileName;

        public int SourcesFilesParsed, ExclusionsFilesParsed;

        public List<ParseEntry> ParsedSources {
            get {
                return Result.Sources;
            }
        }

        public List<ParseEntry> ParsedExclusions {
            get {
                return Result.Exclusions;
            }
        }
    }

    public readonly string[] AllHostPlatformNames;
    public readonly string[] AllProfileNames;

    private int ParseDepth = 0;

    public SourcesParser (
        string platformsFolder, string profilesFolder
    ) {
        AllHostPlatformNames = Directory.GetFiles (platformsFolder, "*.make")
            .Select (Path.GetFileNameWithoutExtension)
            .ToArray ();
        AllProfileNames = Directory.GetFiles (profilesFolder, "*.make")
            .Select (Path.GetFileNameWithoutExtension)
            .ToArray ();
    }

    public ParseResult Parse (string libraryDirectory, string libraryName, string hostPlatform, string profile) {
        var state = new State {
            Result = new ParseResult (libraryDirectory, libraryName),
            ProfileName = profile,
            HostPlatform = hostPlatform
        };

        var testPath = Path.Combine (libraryDirectory, $"{hostPlatform}_{profile}_{libraryName}");
        var ok = TryParseSingleFile (state, testPath + ".sources", false);
        TryParseSingleFile (state, testPath + ".exclude.sources", true);

        if (ok) {
            PrintSummary (state);
            return state.Result;
        }

        state.HostPlatform = null;

        testPath = Path.Combine (libraryDirectory, $"{profile}_{libraryName}");
        ok = TryParseSingleFile (state, testPath + ".sources", false);
        TryParseSingleFile (state, testPath + ".exclude.sources", true);

        if (ok) {
            PrintSummary (state);
            return state.Result;
        }

        state.ProfileName = null;

        testPath = Path.Combine (libraryDirectory, libraryName);
        TryParseSingleFile (state, testPath + ".sources", false);
        TryParseSingleFile (state, testPath + ".exclude.sources", true);

        PrintSummary (state);

        return state.Result;
    }

    public ParseResult Parse (string libraryDirectory, string libraryName) {
        var state = new State {
            Result = new ParseResult (libraryDirectory, libraryName)
        };

        string testPath = Path.Combine (libraryDirectory, libraryName);
        TryParseSingleFile (state, testPath + ".sources", false);
        TryParseSingleFile (state, testPath + ".exclude.sources", true);

        foreach (var profile in AllProfileNames) {
            state.ProfileName = profile;

            foreach (var hostPlatform in AllHostPlatformNames) {
                state.HostPlatform = hostPlatform;

                testPath = Path.Combine (libraryDirectory, $"{hostPlatform}_{profile}_{libraryName}");
                TryParseSingleFile (state, testPath + ".sources", false);
                TryParseSingleFile (state, testPath + ".exclude.sources", true);
            }

            state.HostPlatform = null;

            testPath = Path.Combine (libraryDirectory, $"{profile}_{libraryName}");
            TryParseSingleFile (state, testPath + ".sources", false);
            TryParseSingleFile (state, testPath + ".exclude.sources", true);
        }

        PrintSummary (state);

        return state.Result;
    }

    private void PrintSummary (State state) {
        if (TraceLevel > 0)
            Console.Error.WriteLine ($"// Parsed {state.SourcesFilesParsed} sources file(s) and {state.ExclusionsFilesParsed} exclusions file(s).");
    }

    private void HandleMetaDirective (State state, string directory, bool asExclusionsList, string directive) {
        var include = "#include ";
        if (directive.StartsWith (include))
            ParseSingleFile (state, Path.Combine (directory, directive.Substring (include.Length)), asExclusionsList);
    }

    private bool TryParseSingleFile (State state, string fileName, bool asExclusionsList) {
        if (!File.Exists (fileName))
            return false;

        ParseSingleFile (state, fileName, asExclusionsList);
        return true;
    }

    private void ParseSingleFile (State state, string fileName, bool asExclusionsList) {
        var nullStr = "<none>";
        if (TraceLevel >= 1)
            Console.Error.WriteLine ($"// {new String (' ', ParseDepth * 2)}{fileName}  [{state.HostPlatform ?? nullStr}] [{state.ProfileName ?? nullStr}]");
        ParseDepth += 1;

        var directory = Path.GetDirectoryName (fileName);

        using (var sr = new StreamReader (fileName)) {
            if (asExclusionsList)
                state.ExclusionsFilesParsed++;
            else
                state.SourcesFilesParsed++;

            string line;
            while ((line = sr.ReadLine ()) != null) {
                if (String.IsNullOrWhiteSpace (line))
                    continue;

                if (line.StartsWith ("#")) {
                    HandleMetaDirective (state, directory, asExclusionsList, line);
                    continue;
                }

                var parts = line.Split (':');

                if (parts.Length > 1) {
                    var explicitExclusions = parts[1].Split (',');

                    // gensources.sh implemented these explicit exclusions like so:
                    // ../foo/bar/*.cs:A.cs,B.cs
                    // This would generate exclusions for ../foo/bar/A.cs and ../foo/bar/B.cs,
                    //  not ./A.cs and ./B.cs as you might expect

                    var mainPatternDirectory = Path.GetDirectoryName (parts[0]);

                    foreach (var pattern in explicitExclusions) {
                        state.ParsedExclusions.Add (new ParseEntry {
                            SourcesFileName = fileName,
                            Directory = directory,
                            Pattern = Path.Combine (mainPatternDirectory, pattern),
                            HostPlatform = state.HostPlatform,
                            ProfileName = state.ProfileName
                        });
                    }
                }

                (asExclusionsList ? state.ParsedExclusions : state.ParsedSources)
                    .Add (new ParseEntry {
                        SourcesFileName = fileName,
                        Directory = directory,
                        Pattern = parts[0],
                        HostPlatform = state.HostPlatform,
                        ProfileName = state.ProfileName
                    });
            }
        }

        ParseDepth -= 1;
    }
}