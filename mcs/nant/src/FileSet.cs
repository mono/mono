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

namespace SourceForge.NAnt {

    using System;
    using System.Collections.Specialized;
    using System.IO;

    public class FileSet {

        /// <summary>
        /// Used to determine if a file has a more recent last write time then the specified write time.
        /// </summary>
        /// <param name="fileNames">A collection of filenames to check last write times against.</param>
        /// <param name="targetLastWriteTime">The datetime to compare against.</param>
        /// <returns><c>True</c> if at least one file in <c>fileNames</c> has a last write time greater than <c>targetLastWriteTime</c>.</returns>
        public static bool MoreRecentLastWriteTime(StringCollection fileNames, DateTime targetLastWriteTime) {
            foreach (string fileName in fileNames) {
                FileInfo fileInfo = new FileInfo(fileName);
                if (!fileInfo.Exists) {
                    return true;
                }
                if (fileInfo.LastWriteTime > targetLastWriteTime) {
                    return true;
                }
            }
            return false;
        }

        // We can't just use the DirectoryScanner's includes/excludes collections
        // because when we do a Scan() we need to first expand any macros.

        StringCollection _includes = new StringCollection();
        StringCollection _excludes = new StringCollection();
        DirectoryScanner _scanner = null;
        string _baseDirectory;
        bool _includeAllByDefault;
        Task _task = null;

        public FileSet(bool includeAllByDefault) {
            IncludeAllByDefault = includeAllByDefault;
            Excludes.Add("**/CVS/*");
            Excludes.Add("**/.cvsignore");
        }

        /// <remarks>
        /// Will be automagically set in Task.AutoInitializeAttributes() if
        /// file set has TaskFileSetAttribute set on it.
        /// </remarks>
        // TODO: change this to IMacroExpander
        public Task Task {
            get { return _task; }
            set { _task = value; }
        }

        public string BaseDirectory {
            get { return _baseDirectory; }
            set { _baseDirectory = value; }
        }

        /// <summary>Determines if scan should produce everything or nothing
        /// if there are no Includes set.  Default false.</summary>
        public bool IncludeAllByDefault {
            get { return _includeAllByDefault; }
            set { _includeAllByDefault = value; }
        }

        public StringCollection Includes {
            get { return _includes; }
        }

        public StringCollection Excludes {
            get { return _excludes; }
        }

        public void Scan() {
            // get project (only for expanding macros)
            Project expander = Task.Project;

            _scanner = new DirectoryScanner();
            _scanner.BaseDirectory = expander.GetFullPath(BaseDirectory);;

            foreach (string path in Includes) {
                _scanner.Includes.Add(expander.ExpandText(path));
            }
            if (Includes.Count <= 0 && IncludeAllByDefault) {
                _scanner.Includes.Add("**");
            }

            foreach (string path in Excludes) {
                _scanner.Excludes.Add(expander.ExpandText(path));
            }

            _scanner.Scan();
        }

        public StringCollection DirectoryNames {
            get {
                if (_scanner == null) {
                    Scan();
                }
                return _scanner.DirectoryNames;
            }
        }

        public StringCollection FileNames {
            get {
                if (_scanner == null) {
                    Scan();
                }
                return _scanner.FileNames;
            }
        }
    }
}