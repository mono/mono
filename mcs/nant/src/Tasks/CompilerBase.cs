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
// Mike Krueger (mike@icsharpcode.net)

namespace SourceForge.NAnt {

    using System;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.IO;

    public abstract class CompilerBase : ExternalProgramBase {

        string _responseFileName;

        // Microsoft common compiler options
        [TaskAttribute("output", Required=true)]
        string _output = null;

        [TaskAttribute("target", Required=true)]
        string _target = null;

        [TaskAttribute("debug")]
        [BooleanValidator()]
        string _debug = Boolean.FalseString;

        [TaskAttribute("define")]
        string _define = null;

        [TaskAttribute("win32icon")]
        string _win32icon = null;

        [TaskFileSet("references")]
        FileSet _references = new FileSet(false);

        [TaskFileSet("resources")]
        FileSet _resources = new FileSet(false);

        [TaskFileSet("modules")]
        FileSet _modules = new FileSet(false);

        [TaskFileSet("sources")]
        FileSet _sources = new FileSet(true); // include all by default

        public string Output        { get { return _output; } }
        public string OutputTarget  { get { return _target; } }
        public bool Debug           { get { return Convert.ToBoolean(_debug); } }
        public string Define        { get { return _define; } }
        public string Win32Icon     { get { return _win32icon; } }
        public FileSet References   { get { return _references; } }
        public FileSet Resources    { get { return _resources; } }
        public FileSet Modules      { get { return _modules; } }
        public FileSet Sources      { get { return _sources; } }

        public override string ProgramFileName  { get { return Name; } }
        public override string ProgramArguments { get { return "@" + _responseFileName; } }

        protected virtual void WriteOptions(TextWriter writer) {
        }

	protected virtual void WriteOption(TextWriter writer, string name) {
	    writer.WriteLine("/{0}", name);
	}

	protected virtual void WriteOption(TextWriter writer, string name, string arg) {
	    writer.WriteLine("/{0}:{1}", name, arg);
	}

        protected string GetOutputPath() {
            return Path.GetFullPath(Path.Combine(BaseDirectory, Project.ExpandText(Output)));
        }

        protected virtual bool NeedsCompiling() {
            // return true as soon as we know we need to compile

            FileInfo outputFileInfo = new FileInfo(GetOutputPath());
            if (!outputFileInfo.Exists) {
                return true;
            }

            if (FileSet.MoreRecentLastWriteTime(Sources.FileNames, outputFileInfo.LastWriteTime)) {
                return true;
            }
            if (FileSet.MoreRecentLastWriteTime(References.FileNames, outputFileInfo.LastWriteTime)) {
                return true;
            }
            if (FileSet.MoreRecentLastWriteTime(Modules.FileNames, outputFileInfo.LastWriteTime)) {
                return true;
            }

            // if we made it here then we don't have to recompile
            return false;
        }

        protected override void ExecuteTask() {
            if (NeedsCompiling()) {
                // create temp response file to hold compiler options
                _responseFileName = Path.GetTempFileName();
                StreamWriter writer = new StreamWriter(_responseFileName);

                try {
                    if (References.BaseDirectory == null) {
                        References.BaseDirectory = BaseDirectory;
                    }
                    if (Modules.BaseDirectory == null) {
                        Modules.BaseDirectory = BaseDirectory;
                    }
                    if (Sources.BaseDirectory == null) {
                        Sources.BaseDirectory = BaseDirectory;
                    }

                    Log.WriteLine(LogPrefix + "Compiling {0} files to {1}", Sources.FileNames.Count, GetOutputPath());

                    // specific compiler options
                    WriteOptions(writer);

                    // Microsoft common compiler options
		    WriteOption(writer, "nologo");
		    WriteOption(writer, "target", OutputTarget);
		    WriteOption(writer, "out", GetOutputPath());
                    if (Debug) {
			WriteOption(writer, "debug");
			WriteOption(writer, "define", "DEBUG");
			WriteOption(writer, "define", "TRACE");
                    }
                    if (Define != null) {
			WriteOption(writer, "define", Define);
                    }
                    if (Win32Icon != null) {
			WriteOption(writer, "win32icon", Win32Icon);
                    }
                    foreach (string fileName in References.FileNames) {
                        WriteOption(writer, "reference", fileName);
                    }
                    foreach (string fileName in Modules.FileNames) {
                        WriteOption(writer, "addmodule", fileName);
                    }
                    foreach (string fileName in Resources.FileNames) {
                        WriteOption(writer, "resource", fileName);
                    }
                    foreach (string fileName in Sources.FileNames) {
                        writer.WriteLine(fileName);
                    }
                    // Make sure to close the response file otherwise contents
                    // will not be written to disc and EXecuteTask() will fail.
                    writer.Close();

                    if (Verbose) {
                        // display response file contents
                        Log.WriteLine(LogPrefix + "Contents of " + _responseFileName);
                        /*
                        StreamReader reader = File.OpenText(_responseFileName);
                        string line = reader.ReadLine();
                        while (line != null) {
                            Log.WriteLine(LogPrefix + "  " + line);
                            line = reader.ReadLine();
                        }
                        reader.Close();
                        */

                        StreamReader reader = File.OpenText(_responseFileName);
                        Log.WriteLine(reader.ReadToEnd());
                        reader.Close();

                    }

                    // call base class to do the work
                    base.ExecuteTask();

                } finally {
                    // make sure we delete response file even if an exception is thrown
                    writer.Close(); // make sure stream is closed or file cannot be deleted
                    File.Delete(_responseFileName);
                    _responseFileName = null;
                }
            }
        }
    }
}
