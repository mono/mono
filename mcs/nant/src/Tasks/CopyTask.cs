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
// Ian MacLean (ian_maclean@another.com)

namespace SourceForge.NAnt {

    using System;
    using System.IO;
    using System.Xml;
    using System.Text;
    using System.Collections;
    using System.Collections.Specialized;

    [TaskName("copy")]
    public class CopyTask : Task {

        [TaskAttribute("file")]
        string _sourceFile = null;

        [TaskAttribute("tofile")]
        string _toFile = null;

        [TaskAttribute("todir")]
        string _toDirectory = null;

        [TaskAttribute("filtering")]
        [BooleanValidator()]
        string _filtering = Boolean.FalseString;

        [TaskAttribute("flatten")]
        [BooleanValidator()]
        string _flatten = Boolean.FalseString;

        [TaskAttribute("includeEmptyDirs")]
        [BooleanValidator()]
        string _includeEmptyDirs = Boolean.FalseString;

        [TaskFileSet("fileset")]
        FileSet _fileset = new FileSet(true); // include all by default

        [TaskAttribute("overwrite")]
        [BooleanValidator()]
        string _overwrite = Boolean.FalseString;

        [TaskAttribute("verbose")]
        [BooleanValidator()]
        string _verbose = Boolean.FalseString;

        [TaskAttribute("preserveLastModified")]
        [BooleanValidator()]
        string _preserveLastModified = Boolean.FalseString;

        Hashtable _fileCopyMap = new Hashtable();

        public string SourceFile        { get { return _sourceFile; } }
        public string ToFile            { get { return _toFile; } }
        public string ToDirectory       { get { return _toDirectory; } }
        public bool Filtering           { get { return Convert.ToBoolean(_filtering); } }
        public bool Flatten             { get { return Convert.ToBoolean(_flatten); } }
        public bool IncludeEmptyDirs    { get { return Convert.ToBoolean(_includeEmptyDirs); } }
        public bool Overwrite           { get { return Convert.ToBoolean(_overwrite); } }
        public bool PreserveLastModified{ get { return Convert.ToBoolean(_preserveLastModified); } }
        public FileSet CopyFileSet      { get { return _fileset; } }

        public bool Verbose { 
            get {
                return (Project.Verbose || Convert.ToBoolean(_verbose));
            } 
        }

        protected Hashtable FileCopyMap {
            get { return _fileCopyMap; }
        }

        /// <summary>
        /// Actually does the file (and possibly empty directory) copies.
        /// </summary>
        protected virtual void DoFileOperations() {
            int fileCount = FileCopyMap.Keys.Count;
            if (fileCount > 0) {
                if (ToDirectory != null) {
                    Log.WriteLine(LogPrefix + "Copying {0} files to {1}", fileCount, Project.GetFullPath(ToDirectory));
                } else {
                    Log.WriteLine(LogPrefix + "Copying {0} files", fileCount);
                }

                // loop thru our file list
                foreach (string sourcePath in FileCopyMap.Keys) {
                    string dstPath = (string)FileCopyMap[sourcePath];
                    if (sourcePath == dstPath) {
                        if (Verbose) {
                            Log.WriteLine(LogPrefix + "Skipping self-copy of {0}" + sourcePath);
                        }
                        continue;
                    }

                    try {
                        if (Verbose) {
                            Log.WriteLine(LogPrefix + "Copying {0} to {1}", sourcePath, dstPath);
                        }

                        // create directory if not present
                        string dstDirectory = Path.GetDirectoryName(dstPath);
                        if (!Directory.Exists(dstDirectory)) {
                            Directory.CreateDirectory(dstDirectory);
                            if (Verbose) {
                                Log.WriteLine(LogPrefix + "Created directory {0}", dstDirectory);
                            }
                        }

                        File.Copy(sourcePath, dstPath, true);
                    } catch (IOException ioe) {
                        string msg = String.Format("Cannot copy {0} to {1}", sourcePath, dstPath);
                        throw new BuildException(msg, Location, ioe);
                    }
                }
            }

            // TODO: handle empty directories in the fileset, refer to includeEmptyDirs attribute at
            // http://jakarta.apache.org/ant/manual/CoreTasks/copy.html
        }

        protected override void ExecuteTask() {

            string dstDirectoryPath = Project.GetFullPath(ToDirectory);
            string srcFilePath = Project.GetFullPath(SourceFile);
            FileInfo srcInfo = new FileInfo(srcFilePath);

            string dstFilePath;
            if (ToFile == null) {
                dstFilePath = dstDirectoryPath + Path.DirectorySeparatorChar + srcInfo.Name;
            } else {
                dstFilePath = Project.GetFullPath(ToFile);
            }

            FileInfo dstInfo = new FileInfo(dstFilePath);
            if (SourceFile != null) {
                if (srcInfo.Exists) {
                    // do the outdated check
                    bool outdated = (!dstInfo.Exists) || (srcInfo.LastWriteTime > dstInfo.LastWriteTime);

                    if (Overwrite || outdated) {
                        // add to a copy map of absolute verified paths
                        FileCopyMap.Add(srcFilePath, dstFilePath);
                    }
                } else {
                    Log.WriteLine(LogPrefix + "Could not find file {0} to copy.", srcFilePath);
                }
            } else {
                // get the complete path of the base directory of the fileset, ie, c:\work\nant\src
                string srcBasePath = Project.GetFullPath(CopyFileSet.BaseDirectory);
                string dstBasePath = Project.GetFullPath(ToDirectory);

                // if source file not specified use fileset
                foreach (string pathname in CopyFileSet.FileNames) {
                    // replace the fileset path with the destination path
                    // NOTE: big problems could occur if the file set base dir is rooted on a different drive
                    string dstPath = pathname.Replace(srcBasePath, dstBasePath);

                    srcInfo = new FileInfo(pathname);
                    dstInfo = new FileInfo(dstPath);

                    if (srcInfo.Exists) {
                        // do the outdated check
                        bool outdated = (!dstInfo.Exists) || (srcInfo.LastWriteTime > dstInfo.LastWriteTime);

                        if (Overwrite || outdated) {
                            FileCopyMap.Add(pathname, dstPath);
                        }
                    } else {
                        Log.WriteLine(LogPrefix + "Could not find file {0} to copy.", srcFilePath);
                    }
                }
            }

            // do all the actual copy operations now...
            DoFileOperations();
        }
    }
}
