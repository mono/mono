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
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Xml;

    public abstract class ExternalProgramBase : Task {

        [TaskAttribute("verbose")]
        [BooleanValidator()]
        string _verbose = Boolean.FalseString;

        public abstract string ProgramFileName { get; }
        public abstract string ProgramArguments { get; }

        public virtual string BaseDirectory {
            get {
                if (Project != null) {
                    return Project.BaseDirectory;
                } else {
                    return null;
                }
            }
        }

        public virtual int TimeOut {
            get { return Int32.MaxValue; }
        }

        public virtual bool FailOnError {
            get { return true; }
        }

        public bool Verbose { 
            get {
                return (Project.Verbose || Convert.ToBoolean(_verbose));
            } 
        }

        StringCollection _args = new StringCollection();

        protected override void InitializeTask(XmlNode taskNode) {
            // initialize the _args collection
            foreach (XmlNode optionNode in taskNode.SelectNodes("arg")) {

                // TODO: decide if we should enforce arg elements not being able
                // to accept a file and value attribute on the same element.
                // Ideally this would be down via schema and since it doesn't
                // really hurt for now I'll leave it in.

                XmlNode valueNode = optionNode.SelectSingleNode("@value");
                if (valueNode != null) {
                    _args.Add(Project.ExpandText(valueNode.Value));
                }

                XmlNode fileNode  = optionNode.SelectSingleNode("@file");
                if (fileNode != null) {
                    _args.Add(Project.GetFullPath(Project.ExpandText(fileNode.Value)));
                }
            }
        }

        public string GetCommandLine() {
            // append any nested <arg> arguments to command line
            StringBuilder arguments = new StringBuilder(ProgramArguments);
            foreach (string arg in _args) {
                arguments = arguments.Append(' ');
                arguments = arguments.Append(arg);
            }
            return arguments.ToString();
        }

        protected override void ExecuteTask() {
            try {
                // create process to launch compiler (redirect standard output to temp buffer)
                Process process = new Process();
                process.StartInfo.FileName = ProgramFileName;
                process.StartInfo.Arguments = GetCommandLine();
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.WorkingDirectory = BaseDirectory;
                if (Verbose) {
                    Log.WriteLine(LogPrefix + "{0}>{1} {2}", process.StartInfo.WorkingDirectory, process.StartInfo.FileName, process.StartInfo.Arguments);
                }
                process.Start();
 
                // display standard output
                StreamReader reader = process.StandardOutput;
                string output = reader.ReadToEnd();
                if (output.Length > 0) {
                    int indentLevel = Log.IndentLevel;
                    Log.IndentLevel = 0;
                    Log.WriteLine(output);
                    Log.IndentLevel = indentLevel;
                }

                // wait for program to exit
                process.WaitForExit(TimeOut);

                if (FailOnError && process.ExitCode != 0) {
                    throw new BuildException("Program error, see build log for details.");
                }
            } catch (Exception e) {
                throw new BuildException(e.Message, Location, e);
            }
        }
    }
}