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
        var platformName = args[2].Trim ();
        var profileName = args[3].Trim ();
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

public class SourcesFile {
    public readonly string FileName;
    public readonly bool   IsExclusion;

    public readonly List<ParseEntry>  Sources = new List<ParseEntry> ();
    public readonly List<ParseEntry>  Exclusions = new List<ParseEntry> ();
    public readonly List<SourcesFile> Includes = new List<SourcesFile> ();

    public SourcesFile (string fileName, bool isExclusion) {
        FileName = fileName;
        IsExclusion = isExclusion;
    }
}

public struct ParseEntry {
    public string Directory;
    public string Pattern;
}

public struct MatchEntry {
    public SourcesFile SourcesFile;
    public string RelativePath;
}

public class TargetParseResult {
    public (string hostPlatform, string profile) Key;
    public SourcesFile Sources, Exclusions;
}

public class ParseResult {
    public readonly string LibraryDirectory, LibraryName;

    public readonly Dictionary<(string hostPlatform, string profile), TargetParseResult> TargetDictionary = new Dictionary<(string hostPlatform, string profile), TargetParseResult> ();

    public readonly Dictionary<string, SourcesFile> SourcesFiles = new Dictionary<string, SourcesFile> ();
    public readonly Dictionary<string, SourcesFile> ExclusionFiles = new Dictionary<string, SourcesFile> ();

    // FIXME: This is a bad spot for this value but enumerators don't have outparam support
    public int ErrorCount = 0;

    public ParseResult (string libraryDirectory, string libraryName) {
        LibraryDirectory = libraryDirectory;
        LibraryName = libraryName;
    }

    public IEnumerable<TargetParseResult> Targets {
        get {
            return TargetDictionary.Values;
        }
    }

    private static string GetRelativePath (string fullPath, string relativeToDirectory) {
        fullPath = fullPath.Replace ("\\", "/");
        relativeToDirectory = relativeToDirectory.Replace ("\\", "/");

        if (!relativeToDirectory.EndsWith ("/"))
            relativeToDirectory += "/";
        var dirUri = new Uri (relativeToDirectory);
        var pathUri = new Uri (fullPath);

        var relativePath = Uri.UnescapeDataString (
            dirUri.MakeRelativeUri (pathUri).OriginalString
        ).Replace ("/", SourcesParser.DirectorySeparator)
         .Replace (SourcesParser.DirectorySeparator + SourcesParser.DirectorySeparator, SourcesParser.DirectorySeparator);

        /*
        if (SourcesParser.TraceLevel >= 4)
            Console.Error.WriteLine ($"// {fullPath} -> {relativePath}");
        */

        return relativePath;
    }

    private IEnumerable<MatchEntry> EnumerateMatches (
        SourcesFile sourcesFile,
        IEnumerable<ParseEntry> entries
    ) {
        var patternChars = new [] { '*', '?' };

        foreach (var entry in entries) {
            var absolutePath = Path.Combine (entry.Directory, entry.Pattern);
            var absoluteDirectory = Path.GetDirectoryName (absolutePath);
            var absolutePattern = Path.GetFileName (absolutePath);

            if (SourcesParser.TraceLevel >= 4) {
                if ((absolutePattern != entry.Pattern) || (absoluteDirectory != entry.Directory))
                    Console.Error.WriteLine ($"// {entry.Directory} / {entry.Pattern} -> {absoluteDirectory} / {absolutePattern}");
            }            

            if (!Directory.Exists (absoluteDirectory)) {
                Console.Error.WriteLine ($"Directory does not exist: '{Path.GetFullPath (absoluteDirectory)}'");
                ErrorCount += 1;
                continue;
            }

            if (absolutePattern.IndexOfAny (patternChars) >= 0) {
                var matchingFiles = Directory.GetFiles (absoluteDirectory, absolutePattern);
                if (matchingFiles.Length == 0) {
                    if (SourcesParser.TraceLevel > 0)
                        Console.Error.WriteLine ($"// No matches for pattern '{absolutePattern}'");
                }

                foreach (var fileName in matchingFiles) {
                    var relativePath = GetRelativePath (fileName, LibraryDirectory);
                    yield return new MatchEntry {
                        SourcesFile = sourcesFile,
                        RelativePath = relativePath,
                    };
                }
            } else {
                if (!File.Exists (absolutePath)) {
                    Console.Error.WriteLine ($"File does not exist: '{absolutePath}'");
                    ErrorCount += 1;
                } else {
                    var relativePath = GetRelativePath (absolutePath, LibraryDirectory);
                    yield return new MatchEntry {
                        SourcesFile = sourcesFile,
                        RelativePath = relativePath,
                    };
                }
            }
        }
    }

    public IEnumerable<MatchEntry> GetMatchesFromFile (SourcesFile sourcesFile, HashSet<string> excludedFiles = null) {
        if (excludedFiles == null)
            excludedFiles = new HashSet<string> (StringComparer.Ordinal);

        foreach (var m in EnumerateMatches (sourcesFile, sourcesFile.Exclusions))
            excludedFiles.Add (m.RelativePath);

        foreach (var include in sourcesFile.Includes) {
            foreach (var m in GetMatchesFromFile (include, excludedFiles))
                yield return m;
        }

        // FIXME: This is order-sensitive
        foreach (var entry in EnumerateMatches (sourcesFile, sourcesFile.Sources)) {
            if (excludedFiles.Contains (entry.RelativePath)) {
                if (SourcesParser.TraceLevel >= 3)
                    Console.Error.WriteLine ($"// Excluding {entry.RelativePath}");
                continue;
            }

            yield return entry;
        }
    }

    public IEnumerable<MatchEntry> GetMatches () {
        var excludedFiles = new HashSet<string> (StringComparer.Ordinal);

        var target = Targets.First ();

        if (SourcesParser.TraceLevel >= 3)
            Console.Error.WriteLine ($"// Scanning sources file tree for {target.Key}");

        int count = 0;
        foreach (var m in GetMatchesFromFile (target.Exclusions, excludedFiles))
            excludedFiles.Add (m.RelativePath);

        foreach (var m in GetMatchesFromFile (target.Sources, excludedFiles)) {
            count++;
            yield return m;
        }

        if (SourcesParser.TraceLevel >= 3)
            Console.Error.WriteLine ($"// Scan complete. Generated {count} successful matches.");
    }

    public IEnumerable<string> GetFileNames () {
        var encounteredFileNames = new HashSet<string> (StringComparer.Ordinal);

        foreach (var entry in GetMatches ()) {
            // Skip duplicates. We can't do this in GetMatches because we might want to have
            //  duplicate entries with different platform/profile info
            if (encounteredFileNames.Contains (entry.RelativePath))
                continue;

            encounteredFileNames.Add (entry.RelativePath);
            yield return entry.RelativePath;
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
        var ok = TryParseTarget (state, testPath);

        if (ok) {
            PrintSummary (state, testPath);
            return state.Result;
        }

        state.HostPlatform = null;

        testPath = Path.Combine (libraryDirectory, $"{profile}_{libraryName}");
        ok = TryParseTarget (state, testPath);

        if (ok) {
            PrintSummary (state, testPath);
            return state.Result;
        }

        testPath = Path.Combine (libraryDirectory, $"{hostPlatform}_{libraryName}");
        ok = TryParseTarget (state, testPath);

        if (ok) {
            PrintSummary (state, testPath);
            return state.Result;
        }

        state.ProfileName = null;

        testPath = Path.Combine (libraryDirectory, libraryName);
        ok = TryParseTarget (state, testPath);

        PrintSummary (state, testPath);

        return state.Result;
    }

    public ParseResult Parse (string libraryDirectory, string libraryName) {
        var state = new State {
            Result = new ParseResult (libraryDirectory, libraryName)
        };

        string testPath = Path.Combine (libraryDirectory, libraryName);
        TryParseTarget (state, testPath);

        foreach (var profile in AllProfileNames) {
            state.ProfileName = profile;

            foreach (var hostPlatform in AllHostPlatformNames) {
                state.HostPlatform = hostPlatform;

                testPath = Path.Combine (libraryDirectory, $"{hostPlatform}_{profile}_{libraryName}");
                TryParseTarget (state, testPath);
            }

            state.HostPlatform = null;

            testPath = Path.Combine (libraryDirectory, $"{profile}_{libraryName}");
            TryParseTarget (state, testPath);
        }

        foreach (var hostPlatform in AllHostPlatformNames) {
            state.ProfileName = null;
            state.HostPlatform = hostPlatform;

            testPath = Path.Combine (libraryDirectory, $"{hostPlatform}_{libraryName}");
            TryParseTarget (state, testPath);
        }

        PrintSummary (state, testPath);

        return state.Result;
    }

    private void PrintSummary (State state, string testPath) {
        if (TraceLevel > 0)
            Console.Error.WriteLine ($"// Parsed {state.SourcesFilesParsed} sources file(s) and {state.ExclusionsFilesParsed} exclusions file(s) from path '{testPath}'.");
    }

    private void HandleMetaDirective (State state, SourcesFile file, string directory, bool asExclusionsList, string directive) {
        var include = "#include ";
        if (directive.StartsWith (include)) {
            var fileName = Path.Combine (directory, directive.Substring (include.Length));
            var newFile = ParseSingleFile (state, fileName, asExclusionsList);
            if (newFile == null) {
                Console.Error.WriteLine($"// Include not found: {fileName}");
                state.Result.ErrorCount++;
            } else {
                file.Includes.Add (newFile);
            }
        }
    }

    private bool TryParseTarget (State state, string prefix) {
        var tpr = new TargetParseResult {
            Key = (hostPlatform: state.HostPlatform, profile: state.ProfileName)
        };

        var sourcesFileName = prefix + ".sources";
        var exclusionsFileName = prefix + ".exclude.sources";

        if (!File.Exists (sourcesFileName)) {
            if (TraceLevel >= 3)
                Console.Error.WriteLine($"// Not found: {sourcesFileName}");
            return false;
        }

        tpr.Sources = ParseSingleFile (state, sourcesFileName, false);

        if (File.Exists (exclusionsFileName))
            tpr.Exclusions = ParseSingleFile (state, exclusionsFileName, true);

        state.Result.TargetDictionary.Add (tpr.Key, tpr);

        return true;
    }

    private SourcesFile ParseSingleFile (State state, string fileName, bool asExclusionsList) {
        var fileTable = asExclusionsList ? state.Result.ExclusionFiles : state.Result.SourcesFiles;

        if (fileTable.ContainsKey (fileName))
            return fileTable[fileName];

        var nullStr = "<none>";
        if (TraceLevel >= 1)
            Console.Error.WriteLine ($"// {new String (' ', ParseDepth * 2)}{fileName}  [{state.HostPlatform ?? nullStr}] [{state.ProfileName ?? nullStr}]");
        ParseDepth += 1;

        var directory = Path.GetDirectoryName (fileName);
        var result = new SourcesFile (fileName, asExclusionsList);
        fileTable.Add (fileName, result);

        using (var sr = new StreamReader (fileName)) {
            if (asExclusionsList)
                state.ExclusionsFilesParsed++;
            else
                state.SourcesFilesParsed++;

            string line;
            while ((line = sr.ReadLine ()) != null) {
                if (line.StartsWith ("#")) {
                    HandleMetaDirective (state, result, directory, asExclusionsList, line);
                    continue;
                }

                line = line.Trim ();

                if (String.IsNullOrWhiteSpace (line))
                    continue;

                var parts = line.Split (':');

                if (parts.Length > 1) {
                    var explicitExclusions = parts[1].Split (',');

                    // gensources.sh implemented these explicit exclusions like so:
                    // ../foo/bar/*.cs:A.cs,B.cs
                    // This would generate exclusions for ../foo/bar/A.cs and ../foo/bar/B.cs,
                    //  not ./A.cs and ./B.cs as you might expect

                    var mainPatternDirectory = Path.GetDirectoryName (parts[0]);

                    foreach (var pattern in explicitExclusions) {
                        result.Exclusions.Add (new ParseEntry {
                            Directory = directory,
                            Pattern = Path.Combine (mainPatternDirectory, pattern)
                        });
                    }
                }

                (asExclusionsList ? result.Exclusions : result.Sources)
                    .Add (new ParseEntry {
                        Directory = directory,
                        Pattern = parts[0]
                    });
            }
        }

        ParseDepth -= 1;
        return result;
    }
}