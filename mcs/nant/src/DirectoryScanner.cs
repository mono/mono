// NAnt - A .NET build tool
// Copyright (C) 2001 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Gerry Shaw (gerry_shaw@yahoo.com)

/*
Examples:
"**\*.class" matches all .class files/dirs in a directory tree.

"test\a??.java" matches all files/dirs which start with an 'a', then two
more characters and then ".java", in a directory called test.

"**" matches everything in a directory tree.

"**\test\**\XYZ*" matches all files/dirs that start with "XYZ" and where
there is a parent directory called test (e.g. "abc\test\def\ghi\XYZ123").

Example of usage:

DirectoryScanner scanner = DirectoryScanner();
scanner.Includes.Add("**\\*.class");
scanner.Exlucdes.Add("modules\\*\\**");
scanner.BaseDirectory = "test";
scanner.Scan();
foreach (string filename in GetIncludedFiles()) {
    Console.WriteLine(filename);
}
*/

namespace SourceForge.NAnt {

    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    public class DirectoryScanner {

        string _baseDirectory = Environment.CurrentDirectory;

        // holds the nant patterns
        StringCollection _includes = new StringCollection();
        StringCollection _excludes = new StringCollection();

        // holds the nant patterns converted to regular expression patterns
        StringCollection _includePatterns = null;
        StringCollection _excludePatterns = null;

        // holds the result from a scan
        StringCollection _fileNames = null;
        StringCollection _directoryNames = null;

        public StringCollection Includes {
            get { return _includes; }
        }

        public StringCollection Excludes {
            get { return _excludes; }
        }

        public string BaseDirectory {
            get { return _baseDirectory; }
            set { _baseDirectory = value; }
        }

        public StringCollection FileNames {
            get {
                if (_fileNames == null) {
                    Scan();
                }
                return _fileNames;
            }
        }

        public StringCollection DirectoryNames {
            get {
                if (_directoryNames == null) {
                    Scan();
                }
                return _directoryNames;
            }
        }

        public void Scan() {
            _includePatterns = new StringCollection();
            foreach (string pattern in Includes) {
                _includePatterns.Add(ToRegexPattern(pattern));
            }

            _excludePatterns = new StringCollection();
            foreach (string pattern in Excludes) {
                _excludePatterns.Add(ToRegexPattern(pattern));
            }

            _fileNames      = new StringCollection();
            _directoryNames = new StringCollection();

            ScanDirectory(Path.GetFullPath(BaseDirectory));
        }

        void ScanDirectory(string path) {
            // get info for the current directory
            DirectoryInfo currentDirectoryInfo = new DirectoryInfo(path);

            // scan subfolders
            foreach (DirectoryInfo directoryInfo in currentDirectoryInfo.GetDirectories()) {
                ScanDirectory(directoryInfo.FullName);
            }

            // scan files
            foreach (FileInfo fileInfo in currentDirectoryInfo.GetFiles()) {
                string filename = Path.Combine(path, fileInfo.Name);
                if (IsPathIncluded(filename)) {
                    _fileNames.Add(filename);
                }
            }

            // Check current path last so that delete task will correctly
            // delete empty directories.  This may *seem* like a special case
            // but it is more like formalizing something in a way that makes
            // writing the delete task easier :)
            if (IsPathIncluded(path)) {
                _directoryNames.Add(path);
            }
        }

        bool IsPathIncluded(string path) {
            bool included = false;

            // check path against includes
            foreach (string pattern in _includePatterns) {
                Match m = Regex.Match(path, pattern);
                if (m.Success) {
                    included = true;
                    break;
                }
            }

            // check path against excludes
            if (included) {
                foreach (string pattern in _excludePatterns) {
                    Match m = Regex.Match(path, pattern);
                    if (m.Success) {
                        included = false;
                        break;
                    }
                }
            }

            return included;
        }

        string ToRegexPattern(string nantPattern) {

            StringBuilder pattern = new StringBuilder(nantPattern);

            // NAnt patterns can use either / \ as a directory seperator.
            // We must replace both of these characters with Path.DirectorySeperatorChar
            pattern.Replace('/',  Path.DirectorySeparatorChar);
            pattern.Replace('\\', Path.DirectorySeparatorChar);

            // Patterns MUST be full paths.
            if (!Path.IsPathRooted(pattern.ToString())) {
                pattern = new StringBuilder(Path.Combine(BaseDirectory, pattern.ToString()));
            }

            // The '\' character is a special character in regular expressions
            // and must be escaped before doing anything else.
            pattern.Replace(@"\", @"\\");

            // Escape the rest of the regular expression special characters.
            // NOTE: Characters other than . $ ^ { [ ( | ) * + ? \ match themselves.
            // TODO: Decide if ] and } are missing from this list, the above
            // list of characters was taking from the .NET SDK docs.
            pattern.Replace(".", @"\.");
            pattern.Replace("$", @"\$");
            pattern.Replace("^", @"\^");
            pattern.Replace("{", @"\{");
            pattern.Replace("[", @"\[");
            pattern.Replace("(", @"\(");
            pattern.Replace(")", @"\)");
            pattern.Replace("+", @"\+");

            // Special case directory seperator string under Windows.
            string seperator = Path.DirectorySeparatorChar.ToString();
            if (seperator == @"\") {
                seperator = @"\\";
            }

            // Convert NAnt pattern characters to regular expression patterns.

            // SPECIAL CASE: to match subdirectory OR current directory.  If
            // we don't do this then we can write something like 'src/**/*.cs'
            // to match all the files ending in .cs in the src directory OR
            // subdirectories of src.
            pattern.Replace(seperator + "**", "(" + seperator + ".|)|");

            // | is a place holder for * to prevent it from being replaced in next line
            pattern.Replace("**", ".|");
            pattern.Replace("*", "[^" + seperator + "]*");
            pattern.Replace("?", "[^" + seperator + "]?");
            pattern.Replace('|', '*'); // replace place holder string

            // Help speed up the search
            pattern.Insert(0, '^'); // start of line
            pattern.Append('$'); // end of line

            return pattern.ToString();
        }
    }
}
