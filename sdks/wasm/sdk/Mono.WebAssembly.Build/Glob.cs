//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

// Reuse of code from https://github.com/Microsoft/workbooks/blob/master/Agents/Xamarin.Interactive/ProcessControl/Glob.cs 
namespace Xamarin.ProcessControl
{
    static class Glob
    {
        static string[] ParseParts(string path, out string rootComponent)
        {
            rootComponent = null;

            if (path == null)
                return null;

            rootComponent = Path.GetPathRoot(path);
            if (String.IsNullOrEmpty(rootComponent))
                rootComponent = null;

            return path
                .Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
                .Where(part => !String.IsNullOrEmpty(part) && part != ".")
                .ToArray();
        }

        public static Regex GetRegex(string pattern)
        {
            return GlobEnumerator.GetRegex(pattern);
        }

        /// <summary>
        /// Perform a glob expansion on <paramref name="pattern"/> with shell style
        /// behavior, using the current working directory as a base path. If the
        /// pattern cannot be expanded, the pattern itself will be yielded.
        /// </summary>
        public static IEnumerable<string> ShellExpand(string pattern)
        {
            var any = false;

            foreach (var expansion in Expand(pattern))
            {
                any = true;
                yield return expansion;
            }

            if (!any)
                yield return pattern;
        }

        /// <summary>
        /// Expand the specified <paramref name="pattern"/> against the current working
        /// directory. If the pattern cannot be expanded, nothing will be yielded.
        /// </summary>
        public static IEnumerable<string> Expand(string pattern)
        {
            return Expand(".", pattern);
        }

        /// <summary>
        /// Expand the specified <paramref name="pattern"/> against the
        /// <paramref name="basePath"/>. If the pattern cannot be expanded,
        /// nothing will be yielded.</summary>
        public static IEnumerable<string> Expand(string basePath, string pattern)
        {
            if (basePath == null)
                throw new ArgumentNullException(nameof(basePath));

            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            var parts = ParseParts(pattern, out string rootComponent);

            return PatternEnumerator.Create(parts).Enumerate(basePath);
        }

        abstract class PatternEnumerator
        {
            public static PatternEnumerator Create(string[] patternParts)
            {
                if (patternParts == null)
                    return new TerminalEnumerator();

                if (patternParts.Length == 0)
                    throw new ArgumentException("must have at least one element", nameof(patternParts));

                if (patternParts[0] == null)
                    throw new ArgumentException("must not have null elements", nameof(patternParts));

                var root = patternParts[0];
                string[] childParts = null;

                if (patternParts.Length > 1)
                {
                    childParts = new string[patternParts.Length - 1];
                    Array.Copy(patternParts, 1, childParts, 0, childParts.Length);
                }

                if (root == "**")
                    return new RecursiveDirectoryEnumerator(childParts);
                else if (root.Contains("**"))
                    throw new ArgumentException("invalid pattern: '**' can only be an " +
                        $"entire path component ({root}", nameof(patternParts));
                else if (GlobEnumerator.CanHandleWildcards(root))
                    return new GlobEnumerator(root, childParts);
                else
                    return new VerbatimEnumerator(root, childParts);
            }

            protected PatternEnumerator Successor { get; private set; }

            protected PatternEnumerator() { }

            protected PatternEnumerator(string[] childParts)
            {
                Successor = Create(childParts);
            }

            public abstract IEnumerable<string> Enumerate(string basePath);
        }

        class TerminalEnumerator : PatternEnumerator
        {
            internal TerminalEnumerator()
            {
            }

            public override IEnumerable<string> Enumerate(string basePath)
            {
                yield return basePath;
            }
        }

        class VerbatimEnumerator : PatternEnumerator
        {
            readonly string name;

            internal VerbatimEnumerator(string name, string[] childParts) : base(childParts)
            {
                this.name = name;
            }

            public override IEnumerable<string> Enumerate(string basePath)
            {
                if (Directory.Exists(basePath))
                {
                    var path = Path.Combine(basePath, name);
                    if (Directory.Exists(path) || File.Exists(path))
                    {
                        foreach (var expanded in Successor.Enumerate(path))
                            yield return expanded;
                    }
                }
            }
        }

        class RecursiveDirectoryEnumerator : PatternEnumerator
        {
            internal RecursiveDirectoryEnumerator(string[] childParts) : base(childParts)
            {
            }

            static IEnumerable<string> EnumerateDirectories(string basePath)
            {
                yield return basePath;
                foreach (var directory in Directory.EnumerateDirectories(basePath, "*", SearchOption.AllDirectories))
                    yield return directory;
            }

            public override IEnumerable<string> Enumerate(string basePath)
            {
                if (Directory.Exists(basePath))
                {
                    var yielded = new HashSet<string>();
                    foreach (var directory in EnumerateDirectories(basePath))
                    {
                        foreach (var path in Successor.Enumerate(directory))
                        {
                            if (!yielded.Contains(path))
                            {
                                yield return path;
                                yielded.Add(path);
                            }
                        }
                    }
                }
            }
        }

        class GlobEnumerator : PatternEnumerator
        {
            internal static bool CanHandleWildcards(string pattern)
            {
                return pattern.Any(c => c == '*' || c == '?' || c == '[' || c == '{');
            }

            public static Regex GetRegex(string pattern)
            {
                return new Regex(TranslatePattern(pattern), RegexOptions.Multiline);
            }

            static string TranslatePattern(string pattern)
            {
                var builder = new StringBuilder(pattern.Length * 2);

                var i = 0;
                var n = pattern.Length;
                var braceDepth = 0;

                while (i < n)
                {
                    var c = pattern[i];
                    i++;

                    if (c == '*')
                        builder.Append(".*");
                    else if (c == '?')
                        builder.Append('.');
                    else if (c == '[')
                    {
                        var j = i;

                        if (j < n && pattern[j] == '!')
                            j++;
                        if (j < n && pattern[j] == ']')
                            j++;
                        while (j < n && pattern[j] != ']')
                            j++;

                        if (j >= n)
                            builder.Append("\\[");
                        else
                        {
                            var subpattern = pattern.Substring(i, j - i).Replace("\\", "\\\\");
                            i = j + 1;
                            if (subpattern[0] == '!')
                                subpattern = '^' + subpattern.Substring(1);
                            else if (subpattern[0] == '^')
                                subpattern = "\\" + subpattern;
                            builder.Append('[');
                            builder.Append(subpattern);
                            builder.Append(']');
                        }
                    }
                    else if (c == '{')
                    {
                        braceDepth++;
                        builder.Append('(');
                    }
                    else if (c == '}')
                    {
                        braceDepth--;
                        builder.Append(')');
                    }
                    else if (c == ',' && braceDepth > 0)
                        builder.Append('|');
                    else
                        builder.Append(Regex.Escape(c.ToString()));
                }

                builder.Append('$');

                return builder.ToString();
            }

            readonly Regex pattern;

            internal GlobEnumerator(string pattern, string[] childParts) : base(childParts)
            {
                this.pattern = GetRegex(pattern);
            }

            public override IEnumerable<string> Enumerate(string basePath)
            {
                if (Directory.Exists(basePath))
                {
                    foreach (var path in Directory.EnumerateFileSystemEntries(basePath))
                    {
                        var name = Path.GetFileName(path);
                        if (pattern.IsMatch(name))
                        {
                            foreach (var expanded in Successor.Enumerate(path))
                                yield return expanded;
                        }
                    }
                }
            }
        }
    }
}