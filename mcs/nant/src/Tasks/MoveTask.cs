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

    [TaskName("move")]
    public class MoveTask : CopyTask {

        /// <summary>
        /// Actually does the file (and possibly empty directory) copies.
        /// </summary>
        protected override void DoFileOperations() {
            if (FileCopyMap.Count > 0) {

                // loop thru our file list
                foreach (string sourcePath in FileCopyMap.Keys) {
                    string destinationPath = (string)FileCopyMap[sourcePath];
                    if (sourcePath == destinationPath) {
                        Log.WriteLine(LogPrefix + "Skipping self-move of {0}" + sourcePath);
                        continue;
                    }

                    try {
                        // check if directory exists
                        if (Directory.Exists(sourcePath)) {
                            Log.WriteLine(LogPrefix + "moving directory {0} to {1}", sourcePath, destinationPath);
                            Directory.Move(sourcePath, destinationPath);
                        }
                        else {

                            DirectoryInfo todir = new DirectoryInfo(destinationPath);
                            if ( !todir.Exists ) {
                                Directory.CreateDirectory( Path.GetDirectoryName(destinationPath) );
                            }

                            Log.WriteLine(LogPrefix + "Moving {0} to {1}", sourcePath, destinationPath);
                            // IM look into how Ant does this for directories
                            File.Move(sourcePath, destinationPath);
                        }

                    } catch (IOException ioe) {
                        string msg = String.Format("Failed to move {0} to {1}\n{2}", sourcePath, destinationPath, ioe.Message);
                        throw new BuildException(msg, Location);
                    }
                }
            }
        }
    }
}


