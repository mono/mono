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


// TODO: move this into the task documentation (once we figure out how tasks
// should be documented - xml??
/*

verbose: Show name of each deleted file ("true"/"false"). Default is "false"
when omitted.

quiet: If the file does not exist, do not display a diagnostic message or
modify the exit status to reflect an error (unless Ant has been invoked with
the -verbose or -debug switches). This means that if a file or directory cannot
be deleted, then no error is reported. This setting emulates the -f option to
the Unix "rm" command. ("true"/"false"). Default is "false" meaning things are
"noisy". Setting this to true, implies setting failonerror to false.

failonerror: This flag (which is only of relevance if 'quiet' is false),
controls whether an error -such as a failure to delete a file- stops the build
task, or is merely reported to the screen. The default is "true"

*/

namespace SourceForge.NAnt {

    using System;
    using System.IO;

    [TaskName("delete")]
    public class DeleteTask : Task {

        [TaskAttribute("file")]
        string _file = null;

        [TaskAttribute("dir")]
        string _dir = null;

        [TaskAttribute("verbose")]
        [BooleanValidator()]
        string _verbose = Boolean.FalseString;

        [TaskAttribute("failonerror")]
        [BooleanValidator()]
        string _failOnError = Boolean.TrueString;

        /// <summary>If true then delete empty directories when using filesets.</summary>
        [TaskAttribute("includeEmptyDirs")]
        [BooleanValidator()]
        string _includeEmptyDirs = Boolean.FalseString;

        [TaskFileSet("fileset")]
        FileSet _fileset = new FileSet(false);

        public string FileName       { get { return _file; } }
        public string DirectoryName  { get { return _dir; } }
        public bool FailOnError      { get { return Convert.ToBoolean(_failOnError); } }
        public bool IncludeEmptyDirectories { get { return Convert.ToBoolean(_includeEmptyDirs); } }
        public FileSet DeleteFileSet { get { return _fileset; } }

        public bool Verbose { 
            get {
                return (Project.Verbose || Convert.ToBoolean(_verbose));
            } 
        }

        protected override void ExecuteTask() {

            // limit task to deleting either a file or a directory or a file set
            if (FileName != null && DirectoryName != null) {
                throw new BuildException("Cannot specify 'file' and 'dir' in the same delete task", Location);
            }

            // try to delete specified file
            if (FileName != null) {
                string path = null;
                try {
                    path = Project.GetFullPath(FileName);
                } catch (Exception e) {
                    string msg = String.Format("Could not determine path from {0}", FileName);
                    throw new BuildException(msg, Location, e);
                }
                DeleteFile(path);

            // try to delete specified directory
            } else if (DirectoryName != null) {
                string path = null;
                try {
                    path = Project.GetFullPath(DirectoryName);
                } catch (Exception e) {
                    string msg = String.Format("Could not determine path from {0}", DirectoryName);
                    throw new BuildException(msg, Location, e);
                }
                DeleteDirectory(path);

            // delete files/directories in fileset
            } else {
                // only use the file set if file and dir attributes have NOT been set
                foreach (string path in DeleteFileSet.FileNames) {
                    DeleteFile(path);
                }

                if (IncludeEmptyDirectories) {
                    foreach (string path in DeleteFileSet.DirectoryNames) {
                        // only delete EMPTY directories (no files, no directories)
                        DirectoryInfo dirInfo = new DirectoryInfo(path);

                        if ((dirInfo.GetFiles().Length == 0) && (dirInfo.GetDirectories().Length == 0)) {
                            DeleteDirectory(path);
                        }
                    }
                }
            }
        }

        void DeleteDirectory(string path) {
            try {
                if (Directory.Exists(path)) {
                    if (Verbose) {
                        Log.WriteLine(LogPrefix + "Deleting directory {0}", path);
                    }
                    if (path.Length > 10) {
                        Directory.Delete(path, true);
                    } else {
                        // TODO: remove this once this task is fully tested and NAnt is at 1.0
                        Console.WriteLine(LogPrefix + "Path {0} is too close to root to delete this early in development", path);
                    }
                } else {
                    throw new DirectoryNotFoundException();
                }
            } catch (Exception e) {
                if (FailOnError) {
                    string msg = String.Format("Cannot delete directory {0}", path);
                    throw new BuildException(msg, Location, e);
                }
            }
        }

        void DeleteFile(string path) {
            try {
                if (File.Exists(path)) {
                    if (Verbose) {
                        Log.WriteLine(LogPrefix + "Deleting file {0}", path);
                    }
                    File.Delete(path);
                } else {
                    throw new FileNotFoundException();
                }
            } catch (Exception e) {
                if (FailOnError) {
                    string msg = String.Format("Cannot delete file {0}", path);
                    throw new BuildException(msg, Location, e);
                }
            }
        }
    }
}
