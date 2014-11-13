//------------------------------------------------------------------------------
// <copyright file="Wildcard.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Wildcard
 * 
 * wildcard wrappers for Regex
 *
 * (1) Wildcard does straight string wildcarding with no path separator awareness
 * (2) WildcardUrl recognizes that forward / slashes are special and can't match * or ?
 * (3) WildcardDos recognizes that backward \ and : are special and can't match * or ?
 * 
 * Copyright (c) 1999, Microsoft Corporation
 */
namespace System.Web.Util {
    using System.Runtime.Serialization.Formatters;
    using System.Text.RegularExpressions;

    /*
     * Wildcard
     *
     * Wildcard patterns have three metacharacters:
     *
     * A ? is equivalent to .
     * A * is equivalent to .*
     * A , is equivalent to |
     *
     * Note that by each alternative is surrounded by \A...\z to anchor
     * at the edges of the string.
     */
    internal class Wildcard {
#if NOT_USED
        internal /*public*/ Wildcard(String pattern) : this (pattern, false) {
        }
#endif

        internal /*public*/ Wildcard(String pattern, bool caseInsensitive) {
            _pattern = pattern;
            _caseInsensitive = caseInsensitive;
        }

        internal String _pattern;
        internal bool _caseInsensitive;
        internal Regex _regex;

        protected static Regex metaRegex = new Regex("[\\+\\{\\\\\\[\\|\\(\\)\\.\\^\\$]");
        protected static Regex questRegex = new Regex("\\?");
        protected static Regex starRegex = new Regex("\\*");
        protected static Regex commaRegex = new Regex(",");
        protected static Regex slashRegex = new Regex("(?=/)");
        protected static Regex backslashRegex = new Regex("(?=[\\\\:])");

        /*
         * IsMatch returns true if the input is an exact match for the
         * wildcard pattern.
         */
        internal /*public*/ bool IsMatch(String input) {
            EnsureRegex();

            bool result =  _regex.IsMatch(input);

            return result;
        }
#if DONT_COMPILE
        internal /*public*/ String Pattern {
            get {
                return _pattern;
            }
        }
#endif
        /*
         * Builds the matching regex when needed
         */
        protected void EnsureRegex() {
            // threadsafe without protection because of gc

            if (_regex != null)
                return;

            _regex = RegexFromWildcard(_pattern, _caseInsensitive);
        }

        /*
         * Basic wildcard -> Regex conversion, no slashes
         */
        protected virtual Regex RegexFromWildcard(String pattern, bool caseInsensitive) {
            RegexOptions options = RegexOptions.None;

            // match right-to-left (for speed) if the pattern starts with a *

            if (pattern.Length > 0 && pattern[0] == '*')
                options = RegexOptions.RightToLeft | RegexOptions.Singleline;
            else
                options = RegexOptions.Singleline;

            // case insensitivity

            if (caseInsensitive)
                options |= RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

            // Remove regex metacharacters

            pattern = metaRegex.Replace(pattern, "\\$0");

            // Replace wildcard metacharacters with regex codes

            pattern = questRegex.Replace(pattern, ".");
            pattern = starRegex.Replace(pattern, ".*");
            pattern = commaRegex.Replace(pattern, "\\z|\\A");

            // anchor the pattern at beginning and end, and return the regex

            return new Regex("\\A" + pattern + "\\z", options);
        }
    }

    abstract internal class WildcardPath : Wildcard {
#if NOT_USED        
        internal /*public*/ WildcardPath(String pattern) : base(pattern) {
        }

        private Regex[][] _dirs;
#endif

        internal /*public*/ WildcardPath(String pattern, bool caseInsensitive) : base(pattern, caseInsensitive) {
        }

        private Regex _suffix;

        /*
         * IsSuffix returns true if a suffix of the input is an exact
         * match for the wildcard pattern.
         */
        internal /*public*/ bool IsSuffix(String input) {
            EnsureSuffix();
            return _suffix.IsMatch(input);
        }

#if NOT_USED        
        /*
         * AllowPrefix returns true if the input is an exact match for
         * a prefix-directory of the wildcard pattern (i.e., if it
         * is possible to match the wildcard pattern by adding
         * more subdirectories or a filename at the end of the path).
         */
        internal /*public*/ bool AllowPrefix(String prefix) {
            String[] dirs = SplitDirs(prefix);

            EnsureDirs();

            for (int i = 0; i < _dirs.Length; i++) {
                // pattern is shorter than prefix: reject
                if (_dirs[i].Length < dirs.Length)
                    goto NextAlt;

                for (int j = 0; j < dirs.Length; j++) {
                    // the jth directory doesn't match; path is not a prefix
                    if (!_dirs[i][j].IsMatch(dirs[j]))
                        goto NextAlt;
                }

                // one alternative passed: we pass.

                return true;

                NextAlt: 
                ;
            }

            return false;
        }

        /*
         * Builds the matching regex array when needed
         */
        protected void EnsureDirs() {
            // threadsafe without protection because of gc

            if (_dirs != null)
                return;

            _dirs = DirsFromWildcard(_pattern);
        }
#endif

        /*
         * Builds the matching regex when needed
         */
        protected void EnsureSuffix() {
            // threadsafe without protection because of gc

            if (_suffix != null)
                return;

            _suffix = SuffixFromWildcard(_pattern, _caseInsensitive);
        }


        /*
         * Specialize for forward-slash and backward-slash cases
         */
        protected abstract Regex SuffixFromWildcard(String pattern, bool caseInsensitive);
        protected abstract Regex[][] DirsFromWildcard(String pattern);
        protected abstract String[] SplitDirs(String input);
    }

    /*
     * WildcardUrl
     *
     * The twist is that * and ? cannot match forward slashes,
     * and we can do an exact suffix match that starts after
     * any /, and we can also do a prefix prune.
     */
    internal class WildcardUrl : WildcardPath {
#if NOT_USED
        internal /*public*/ WildcardUrl(String pattern) : base(pattern) {
        }
#endif
        internal /*public*/ WildcardUrl(String pattern, bool caseInsensitive) : base(pattern, caseInsensitive) {
        }

        protected override String[] SplitDirs(String input) {
            return slashRegex.Split(input);
        }

        protected override Regex RegexFromWildcard(String pattern, bool caseInsensitive) {
            RegexOptions options;

            // match right-to-left (for speed) if the pattern starts with a *

            if (pattern.Length > 0 && pattern[0] == '*')
                options = RegexOptions.RightToLeft;
            else
                options = RegexOptions.None;

            // case insensitivity

            if (caseInsensitive)
                options |= RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

            // Remove regex metacharacters

            pattern = metaRegex.Replace(pattern, "\\$0");

            // Replace wildcard metacharacters with regex codes

            pattern = questRegex.Replace(pattern, "[^/]");
            pattern = starRegex.Replace(pattern, "[^/]*");
            pattern = commaRegex.Replace(pattern, "\\z|\\A");

            // anchor the pattern at beginning and end, and return the regex

            return new Regex("\\A" + pattern + "\\z", options);
        }

        protected override Regex SuffixFromWildcard(String pattern, bool caseInsensitive) {
            RegexOptions options;

            // match right-to-left (for speed)

            options = RegexOptions.RightToLeft;

            // case insensitivity

            if (caseInsensitive)
                options |= RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

            // Remove regex metacharacters

            pattern = metaRegex.Replace(pattern, "\\$0");

            // Replace wildcard metacharacters with regex codes

            pattern = questRegex.Replace(pattern, "[^/]");
            pattern = starRegex.Replace(pattern, "[^/]*");
            pattern = commaRegex.Replace(pattern, "\\z|(?:\\A|(?<=/))");

            // anchor the pattern at beginning and end, and return the regex

            return new Regex("(?:\\A|(?<=/))" + pattern + "\\z", options);
        }

        protected override Regex[][] DirsFromWildcard(String pattern) {
            String[] alts = commaRegex.Split(pattern);
            Regex[][] dirs = new Regex[alts.Length][];

            for (int i = 0; i < alts.Length; i++) {
                String[] dirpats = slashRegex.Split(alts[i]);

                Regex[] dirregex = new Regex[dirpats.Length];

                if (alts.Length == 1 && dirpats.Length == 1) {
                    // common case: no commas, no slashes: dir regex is same as top regex.

                    EnsureRegex();
                    dirregex[0] = _regex;
                }
                else {
                    for (int j = 0; j < dirpats.Length; j++) {
                        dirregex[j] = RegexFromWildcard(dirpats[j], _caseInsensitive);
                    }
                }

                dirs[i] = dirregex;
            }

            return dirs;
        }
    }
}
