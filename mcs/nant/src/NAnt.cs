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
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;

    public class NAnt {
        public static int Main(string[] args) {
            int returnCode = 0;

            Log.IndentSize = 12;

            Project project = new Project();

            const string buildfileOption = "-buildfile:";
            const string basedirOption = "-basedir:";
            const string setOption = "-set:";
            const string helpOption = "-h"; // allow -h and -help
            const string verboseOption = "-verbose";

            bool showHelp = false;

            foreach (string arg in args) {
                if (arg.StartsWith(buildfileOption)) {
                    project.BuildFileName = arg.Substring(buildfileOption.Length);
                } else if (arg.StartsWith(basedirOption)) {
                    project.BaseDirectory = arg.Substring(basedirOption.Length);
                } else if (arg.StartsWith(basedirOption)) {
                    project.BaseDirectory = arg.Substring(basedirOption.Length);
                } else if (arg.StartsWith(setOption)) {
                    // TODO: implement user defined properties
                    // user defined properties from command line or file should be
                    // marked so that they cannot be overwritten by the build file
                    // ie, once set they are set for the rest of the build.
                    Match match = Regex.Match(arg, @"-set:(\w+)=(\w*)");
                    if (match.Success) {
                        string name = match.Groups[1].Value;
                        string value = match.Groups[2].Value;
                        project.Properties.AddReadOnly(name, value);
                    }
                } else if (arg.StartsWith(helpOption)) {
                    showHelp = true;
                } else if (arg.StartsWith(verboseOption)) {
                    project.Verbose = true;
                } else if (arg.Length > 0) {
                    // must be a target (or mistake ;)
                    project.BuildTargets.Add(arg);
                }
            }

            // Get version information directly from assembly.  This takes more
            // work but prevents the version numbers from getting out of sync.
            ProcessModule module = Process.GetCurrentProcess().MainModule;
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(module.FileName);
            string programName = Path.GetFileNameWithoutExtension(info.FileName); // in case the user has renamed the program

            if (showHelp) {
                const int optionPadding = 23;

                Console.WriteLine("NAnt Version {0} Copyright (C) 2001 Gerry Shaw", info.FileMajorPart + "." + info.FileMinorPart + "." + info.FileBuildPart);
                Console.WriteLine("http://nant.sourceforge.net/");
                Console.WriteLine();
                Console.WriteLine("NAnt comes with ABSOLUTELY NO WARRANTY.");
                Console.WriteLine("This is free software, and you are welcome to redistribute it under certain");
                Console.WriteLine("conditions set out by the GNU General Public License.  A copy of the license");
                Console.WriteLine("is available in the distribution package and from the NAnt web site.");
                Console.WriteLine();
                Console.WriteLine("usage: {0} [options] [target]", programName);
                Console.WriteLine();
                Console.WriteLine("options:");
                Console.WriteLine("  {0} use given buildfile", (buildfileOption + "<file>").PadRight(optionPadding));
                Console.WriteLine("  {0} set project base directory", (basedirOption + "<dir>").PadRight(optionPadding));
                Console.WriteLine("  {0} use value for given property", (setOption + "<property>=<value>").PadRight(optionPadding));
                Console.WriteLine("  {0} print this message", helpOption.PadRight(optionPadding));
                Console.WriteLine();
                Console.WriteLine("If no buildfile is specified the first file ending in .build will be used.");
            } else {
                if (!project.Run()) {
                    Console.WriteLine("Try `{0} -help' for more information.", programName);
                    returnCode = 1; // set return code to indicate an error occurred
                }
            }
            Log.Close();
            return returnCode;
        }
    }
}
