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
        string baseDir = null;

        for (int i = 0; i < args.Count; i++) {
            var arg = args[i];
            if (!arg.StartsWith ("-"))
                continue;

            string argValue = null;
            var offset = arg.IndexOf(':');
            if (offset >= 0) {
                argValue = arg.Substring (offset + 1);
                arg = arg.Substring (0, offset);
            }

            switch (arg) {
                case "-?":
                case "--help":
                case "-h":
                    showHelp = true;
                    break;
                case "--trace":
                    if (argValue != null)
                        SourcesParser.TraceLevel = int.Parse(argValue);
                    else
                        SourcesParser.TraceLevel = 1;
                    break;
                case "--stdout":
                    useStdout = true;
                    break;
                case "--strict":
                    strictMode = true;
                    break;
                case "--basedir":
                    baseDir = argValue;
                    break;
                default:
                    Console.Error.WriteLine ($"// Unrecognized switch {arg}. Aborting.");
                    return 1;
            }

            args.RemoveAt (i);
            i--;            
        }

        if ((args.Count < 3) || (args.Count > 4))
            showHelp = true;

        if (showHelp) {
            Console.Error.WriteLine ("Usage: mcs/build/gensources.exe [options] (outputFileName|--stdout) libraryDirectoryAndName platformName profileName");
            Console.Error.WriteLine ("or     mcs/build/gensources.exe [options] (outputFileName|--stdout) (--baseDir:<dir>) sourcesFile exclusionsFile");
            Console.Error.WriteLine ("You can specify * for platformName and profileName to read all sources files");
            Console.Error.WriteLine ("Available options:");
            Console.Error.WriteLine ("--help -h -?");
            Console.Error.WriteLine ("  Show command line info");
            Console.Error.WriteLine ("--trace:n");
            Console.Error.WriteLine ("  Enable diagnostic output, at tracing level n (1-4)");
            Console.Error.WriteLine ("--stdout");
            Console.Error.WriteLine ("  Writes results to standard output (omit outputFileName if you use this)");
            Console.Error.WriteLine ("--strict");
            Console.Error.WriteLine ("  Produces an error exit code if files or directories are invalid/missing or other warnings occur");
            Console.Error.WriteLine ("--basedir:<dir>");
            Console.Error.WriteLine ("  Sets the base directory when reading a single sources/exclusions pair (default is the directory containing the sources file)");
            return 1;
        }

        var myAssembly = Assembly.GetExecutingAssembly ();
        var codeBase = new Uri (myAssembly.CodeBase);
        var executablePath = Path.GetFullPath (codeBase.LocalPath);
        var executableDirectory = Path.GetDirectoryName (executablePath);

        var outFile = Path.GetFullPath (args[0]);

        var platformsFolder = Path.Combine (executableDirectory, "platforms");
        var profilesFolder = Path.Combine (executableDirectory, "profiles");
        if (!Directory.Exists (platformsFolder) || !Directory.Exists (profilesFolder)) {
            Console.Error.WriteLine ($"// Platforms and/or profiles folders are missing: '{platformsFolder}' '{profilesFolder}'. Aborting.");
            return 1;
        }

        var parser = new SourcesParser (platformsFolder, profilesFolder);

        ParseResult result;

        if (args.Count == 3) {
            var sourcesFile = Path.GetFullPath (args[1]);
            var excludesFile = Path.GetFullPath (args[2]);
            var directory = Path.GetDirectoryName (sourcesFile);
            if ((Path.GetDirectoryName (excludesFile) != directory) && (baseDir == null)) {
                Console.Error.WriteLine ("// Sources and exclusions files are in different directories. Aborting.");
                return 1;
            }

            result = parser.Parse (baseDir ?? directory, sourcesFile, excludesFile);

            if (SourcesParser.TraceLevel > 0)
                Console.Error.WriteLine ($"// Writing sources from {sourcesFile} minus {excludesFile}, to {outFile}.");
        } else if (args.Count == 4) {
            if (baseDir != null)
                Console.Error.WriteLine ($"// WARNING: baseDir has no effect in this mode");

            var libraryFullName = Path.GetFullPath (args[1]);
            var platformName = args[2].Trim ();
            var profileName = args[3].Trim ();
            var libraryDirectory = Path.GetDirectoryName (libraryFullName);
            var libraryName = Path.GetFileName (libraryFullName);
            result = parser.Parse (libraryDirectory, libraryName, platformName, profileName);

            if (SourcesParser.TraceLevel > 0)
                Console.Error.WriteLine ($"// Writing sources for platform {platformName} and profile {profileName}, relative to {libraryDirectory}, to {outFile}.");
        } else {
            throw new Exception ();
        }

        var fileNames = result.GetMatches ()
            .OrderBy (e => e.RelativePath, StringComparer.Ordinal)
            .Select (e => e.RelativePath)
            .Distinct ()
            .ToList ();

        var unexpectedEmptyResult = (fileNames.Count == 0);

        // HACK: We have sources files that are literally empty, so as long as *some* sources files were
        //  parsed during this invocation and they are all empty, producing no matching file names is not
        //  an error.
        if ((result.SourcesFiles.Count > 0) && result.SourcesFiles.Values.All(sf => sf.Sources.Count == 0))
            unexpectedEmptyResult = false;

        if ((result.ErrorCount > 0) || unexpectedEmptyResult) {
            Console.Error.WriteLine ($"// gensources produced {result.ErrorCount} error(s) and a set of {fileNames.Count} filename(s)");
            Console.Error.WriteLine ($"// Invoked with '{Environment.CommandLine}'");
            Console.Error.WriteLine ($"// Working directory was '{Environment.CurrentDirectory}'");

            if (strictMode) {
                // HACK: Make ignores non-zero exit codes so we need to delete the sources file ???
                if (!useStdout && File.Exists (outFile))
                    File.Delete (outFile);

                return result.ErrorCount + 1;
            }
        }

        TextWriter output;
        if (useStdout)
            output = Console.Out;
        else
            output = new StreamWriter (outFile);

        using (output) {
            foreach (var fileName in fileNames)
                output.WriteLine (fileName);
        }

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
    public bool IsFallback;

    public override string ToString () {
        var fallbackString = IsFallback ? " fallback" : "";
        return $"{Key}{fallbackString} -> [{Sources?.FileName}, {Exclusions?.FileName}]";
    }
}

public class ParseResult {
    public readonly string LibraryDirectory;

    public readonly Dictionary<(string hostPlatform, string profile), TargetParseResult> TargetDictionary = new Dictionary<(string hostPlatform, string profile), TargetParseResult> ();

    public readonly Dictionary<string, SourcesFile> SourcesFiles = new Dictionary<string, SourcesFile> ();
    public readonly Dictionary<string, SourcesFile> ExclusionFiles = new Dictionary<string, SourcesFile> ();

    // FIXME: This is a bad spot for this value but enumerators don't have outparam support
    public int ErrorCount = 0;

    public ParseResult (string libraryDirectory) {
        LibraryDirectory = Path.GetFullPath (libraryDirectory);
    }

    public IEnumerable<TargetParseResult> Targets {
        get {
            return TargetDictionary.Values;
        }
    }

    private string GetRelativePath (string fullPath, string relativeToDirectory) {
        fullPath = fullPath.Replace ("\\", "/");
        relativeToDirectory = relativeToDirectory.Replace ("\\", "/");

        if (!relativeToDirectory.EndsWith ("/"))
            relativeToDirectory += "/";

        try {
            var dirUri = new Uri (relativeToDirectory);
            var pathUri = new Uri (fullPath);

            var relativePath = Uri.UnescapeDataString (
                dirUri.MakeRelativeUri (pathUri).OriginalString
            ).Replace ("/", SourcesParser.DirectorySeparator)
             .Replace (SourcesParser.DirectorySeparator + SourcesParser.DirectorySeparator, SourcesParser.DirectorySeparator);

            return relativePath;
        } catch (Exception) {
            Console.Error.WriteLine ($"// Parse error when treating '{fullPath}' as a URI relative to directory '{relativeToDirectory}'");
            ErrorCount += 1;
            return fullPath;
        }

        /*
        if (SourcesParser.TraceLevel >= 4)
            Console.Error.WriteLine ($"// {fullPath} -> {relativePath}");
        */
    }

    public IEnumerable<MatchEntry> EnumerateMatches (
        SourcesFile sourcesFile,
        IEnumerable<ParseEntry> entries,
        bool forExclusionsList
    ) {
        var patternChars = new [] { '*', '?' };

        var isFirstError = true;

        foreach (var entry in entries) {
            var absolutePath = Path.GetFullPath (Path.Combine (entry.Directory, entry.Pattern));
            var absoluteDirectory = Path.GetDirectoryName (absolutePath);
            var absolutePattern = Path.GetFileName (absolutePath);

            if (SourcesParser.TraceLevel >= 4) {
                if ((absolutePattern != entry.Pattern) || (absoluteDirectory != entry.Directory))
                    Console.Error.WriteLine ($"// {entry.Directory} / {entry.Pattern} -> {absoluteDirectory} / {absolutePattern}");
            }            

            if (!Directory.Exists (absoluteDirectory)) {
                if (isFirstError) {
                    isFirstError = false;
                    Console.Error.WriteLine ($"// Error(s) in file {sourcesFile.FileName}:");
                }

                if (forExclusionsList) {
                    Console.Error.WriteLine ($"(ignored) Directory does not exist: '{Path.GetFullPath (absoluteDirectory)}'");
                } else {
                    Console.Error.WriteLine ($"Directory does not exist: '{Path.GetFullPath (absoluteDirectory)}'");
                    ErrorCount += 1;
                }
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
                    if (isFirstError) {
                        isFirstError = false;
                        Console.Error.WriteLine ($"// Error(s) in file {sourcesFile.FileName}:");
                    }

                    if (forExclusionsList) {
                        Console.Error.WriteLine ($"(ignored) File does not exist: '{absolutePath}'");
                    } else {
                        Console.Error.WriteLine ($"File does not exist: '{absolutePath}'");
                        ErrorCount += 1;
                    }
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

    private IEnumerable<MatchEntry> GetMatchesFromFile (
        SourcesFile sourcesFile, HashSet<string> excludedFiles = null
    ) {
        if (sourcesFile == null)
            yield break;

        if (excludedFiles == null)
            excludedFiles = new HashSet<string> (StringComparer.Ordinal);

        foreach (var m in EnumerateMatches (sourcesFile, sourcesFile.Exclusions, true))
            excludedFiles.Add (m.RelativePath);

        foreach (var include in sourcesFile.Includes) {
            foreach (var m in GetMatchesFromFile (include, excludedFiles))
                yield return m;
        }

        // FIXME: This is order-sensitive
        foreach (var entry in EnumerateMatches (sourcesFile, sourcesFile.Sources, false)) {
            if (excludedFiles.Contains (entry.RelativePath)) {
                if (SourcesParser.TraceLevel >= 3)
                    Console.Error.WriteLine ($"// Excluding {entry.RelativePath}");
                continue;
            }

            yield return entry;
        }
    }

    public IEnumerable<MatchEntry> GetMatches (TargetParseResult target = null) {
        var excludedFiles = new HashSet<string> (StringComparer.Ordinal);

        if (target == null) {
            if (TargetDictionary.Count == 0)
                yield break;

            target = Targets.First ();
        }

        if (target == null)
            throw new ArgumentNullException ("target");

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

    public ParseResult Parse (string libraryDirectory, string sourcesFileName, string exclusionsFileName) {
        var state = new State {
            Result = new ParseResult (libraryDirectory)
        };

        var tpr = new TargetParseResult {
            Key = (hostPlatform: null, profile: null)
        };

        var parsedTarget = ParseIntoTarget (state, tpr, sourcesFileName, exclusionsFileName, null, overrideDirectory: libraryDirectory);

        PrintSummary (state, sourcesFileName);
        return state.Result;
    }

    public ParseResult Parse (string libraryDirectory, string libraryName, string hostPlatform, string profile) {
        var state = new State {
            Result = new ParseResult (libraryDirectory),
            ProfileName = profile,
            HostPlatform = hostPlatform
        };

        var testPath = Path.Combine (libraryDirectory, $"{hostPlatform}_{profile}_{libraryName}");
        var ok = TryParseTargetInto (state, testPath);

        if (ok) {
            PrintSummary (state, testPath);
            return state.Result;
        }

        state.HostPlatform = null;

        testPath = Path.Combine (libraryDirectory, $"{profile}_{libraryName}");
        ok = TryParseTargetInto (state, testPath);

        if (ok) {
            PrintSummary (state, testPath);
            return state.Result;
        }

        testPath = Path.Combine (libraryDirectory, $"{hostPlatform}_defaultprofile_{libraryName}");
        ok = TryParseTargetInto (state, testPath);

        if (ok) {
            PrintSummary (state, testPath);
            return state.Result;
        }

        state.ProfileName = null;

        testPath = Path.Combine (libraryDirectory, libraryName);
        ok = TryParseTargetInto (state, testPath);

        PrintSummary (state, testPath);

        return state.Result;
    }

    private void StripFallbackTargetsOrDefaultTarget (
        State state, TargetParseResult defaultTarget, List<TargetParseResult> fallbackTargets, int maximumCount
    ) {
        if (fallbackTargets.Count == maximumCount) {
            // If we didn't find any platform specific targets, remove them and just leave one single
            //  platform-specific target entry
            foreach (var target in fallbackTargets)
                state.Result.TargetDictionary.Remove (target.Key);
        } else if (defaultTarget != null) {
            // Otherwise, strip the non-platform-specific target
            state.Result.TargetDictionary.Remove (defaultTarget.Key);
        }
    }

    public ParseResult Parse (string libraryDirectory, string libraryName) {
        var state = new State {
            Result = new ParseResult (libraryDirectory)
        };

        string originalTestPath = Path.Combine (libraryDirectory, libraryName);
        var defaultTarget = ParseTarget (state, originalTestPath, null);
        var profileFallbackTargets = new List<TargetParseResult> ();

        foreach (var profile in AllProfileNames) {
            state.ProfileName = profile;
            state.HostPlatform = null;

            var testPath = Path.Combine (libraryDirectory, $"{profile}_{libraryName}");
            var profileTarget = ParseTarget (state, testPath, defaultTarget);
            if ((profileTarget != null) && profileTarget.IsFallback)
                profileFallbackTargets.Add (profileTarget);

            var fallbackTargets = new List<TargetParseResult> ();

            foreach (var hostPlatform in AllHostPlatformNames) {
                state.HostPlatform = hostPlatform;

                testPath = Path.Combine (libraryDirectory, $"{hostPlatform}_{profile}_{libraryName}");
                var target = ParseTarget (state, testPath, profileTarget ?? defaultTarget);
                if ((target != null) && target.IsFallback)
                    fallbackTargets.Add (target);
            }

            StripFallbackTargetsOrDefaultTarget (state, profileTarget, fallbackTargets, AllHostPlatformNames.Length);
        }

        StripFallbackTargetsOrDefaultTarget (state, defaultTarget, profileFallbackTargets, AllProfileNames.Length);

        var platformFallbackTargets = new List<TargetParseResult> ();

        var ambiguousSourcesNames = new List<string> ();

        foreach (var hostPlatform in AllHostPlatformNames) {
            state.ProfileName = null;
            state.HostPlatform = hostPlatform;

            var testPath = Path.Combine (libraryDirectory, $"{hostPlatform}_defaultprofile_{libraryName}");
            var target = ParseTarget (state, testPath, defaultTarget);
            if ((target != null) && target.IsFallback)
                platformFallbackTargets.Add (target);

            if ((target == null) || target.IsFallback) {
                var oldTestPath = Path.Combine (libraryDirectory, $"{hostPlatform}_{libraryName}.sources");
                if (File.Exists (oldTestPath))
                    ambiguousSourcesNames.Add (oldTestPath);
                    
                oldTestPath = Path.Combine (libraryDirectory, $"{hostPlatform}_{libraryName}.exclude.sources");
                if (File.Exists (oldTestPath))
                    ambiguousSourcesNames.Add (oldTestPath);
            }
        }

        StripFallbackTargetsOrDefaultTarget (state, defaultTarget, platformFallbackTargets, AllHostPlatformNames.Length);

        foreach (var path in ambiguousSourcesNames) {
            if (!state.Result.SourcesFiles.ContainsKey (path) && !state.Result.ExclusionFiles.ContainsKey (path)) {
                Console.Error.WriteLine ($"// The file '{path}' was found but not used by sources parsing. Did you mean hostPlatform_defaultprofile_{libraryName}?");
                state.Result.ErrorCount += 1;
            }
        }

        PrintSummary (state, originalTestPath);

        return state.Result;
    }

    private void PrintSummary (State state, string testPath) {
        if (TraceLevel > 0)
            Console.Error.WriteLine ($"// Parsed {state.SourcesFilesParsed} sources file(s) and {state.ExclusionsFilesParsed} exclusions file(s) from path '{testPath}'.");
    }

    private void HandleMetaDirective (
        State state, SourcesFile file, 
        string pathDirectory, string includeDirectory,
        bool asExclusionsList, string directive
    ) {
        var include = "#include ";
        if (directive.StartsWith (include)) {
            var includeName = directive.Substring (include.Length).Trim ();
            var fileName = Path.Combine (includeDirectory, includeName);
            if (!File.Exists (fileName)) {
                Console.Error.WriteLine ($"// Include does not exist: {fileName}");
                state.Result.ErrorCount++;
                return;
            }

            var newFile = ParseSingleFile (state, fileName, asExclusionsList, overrideDirectory: pathDirectory);
            if (newFile == null) {
                Console.Error.WriteLine ($"// Failed to parse included file: {fileName}");
                state.Result.ErrorCount++;
                return;
            }

            file.Includes.Add (newFile);
        }
    }

    private bool TryParseTargetInto (State state, string prefix) {
        var result = ParseTarget (state, prefix, null);
        return (result != null);
    }

    private TargetParseResult ParseTarget (State state, string prefix, TargetParseResult fallbackTarget) {
        // FIXME: We determine the prefix for the pair of sources and exclusions,
        //  which may not match intended behavior:
        // if linux_net_4_x_foo.sources is present, but linux_net_4_x_foo.exclude.sources is not present,
        //  should net_4_x_foo.exclude.sources be used as a fallback? Maybe it should.
        // This won't do that though.

        var tpr = new TargetParseResult {
            Key = (hostPlatform: state.HostPlatform, profile: state.ProfileName)
        };

        var sourcesFileName = prefix + ".sources";
        var exclusionsFileName = prefix + ".exclude.sources";

        return ParseIntoTarget (state, tpr, sourcesFileName, exclusionsFileName, fallbackTarget);
    }

    private TargetParseResult ParseIntoTarget (
        State state, TargetParseResult tpr, 
        string sourcesFileName, string exclusionsFileName,
        TargetParseResult fallbackTarget, string overrideDirectory = null
    ) {
        if (!File.Exists (sourcesFileName)) {
            if (fallbackTarget != null) {
                if (TraceLevel >= 2)
                    Console.Error.WriteLine($"// Not found: {sourcesFileName}, falling back to {fallbackTarget}");
                tpr.Sources = fallbackTarget.Sources;
                tpr.Exclusions = fallbackTarget.Exclusions;
                tpr.IsFallback = true;
                state.Result.TargetDictionary.Add (tpr.Key, tpr);
                return tpr;
            } else {
                if (TraceLevel >= 1)
                    Console.Error.WriteLine($"// Not found: {sourcesFileName}");
                return null;
            }
        }

        tpr.Sources = ParseSingleFile (state, sourcesFileName, false, overrideDirectory: overrideDirectory);

        if (File.Exists (exclusionsFileName))
            tpr.Exclusions = ParseSingleFile (state, exclusionsFileName, true, overrideDirectory: overrideDirectory);

        state.Result.TargetDictionary.Add (tpr.Key, tpr);
        return tpr;
    }

    private SourcesFile ParseSingleFile (State state, string fileName, bool asExclusionsList, string overrideDirectory = null) {
        var fileTable = asExclusionsList ? state.Result.ExclusionFiles : state.Result.SourcesFiles;

        var nullStr = "<none>";

        if (fileTable.ContainsKey (fileName)) {
            if (TraceLevel >= 2)
                Console.Error.WriteLine ($"// {new String (' ', ParseDepth * 2)}{fileName}  (already parsed)");

            return fileTable[fileName];
        } else {
            if (TraceLevel >= 2)
                Console.Error.WriteLine ($"// {new String (' ', ParseDepth * 2)}{fileName}  [{state.HostPlatform ?? nullStr}] [{state.ProfileName ?? nullStr}]");
        }

        ParseDepth += 1;

        var includeDirectory = Path.GetDirectoryName (fileName);
        var pathDirectory = overrideDirectory ?? includeDirectory;
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
                    HandleMetaDirective (state, result, pathDirectory, includeDirectory, asExclusionsList, line);
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
                            Directory = pathDirectory,
                            Pattern = Path.Combine (mainPatternDirectory, pattern)
                        });
                    }
                }

                (asExclusionsList ? result.Exclusions : result.Sources)
                    .Add (new ParseEntry {
                        Directory = pathDirectory,
                        Pattern = parts[0]
                    });
            }
        }

        ParseDepth -= 1;
        return result;
    }
}