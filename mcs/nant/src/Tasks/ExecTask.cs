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
    using System.IO;

    [TaskName("exec")]
    public class ExecTask : ExternalProgramBase {

        [TaskAttribute("program", Required=true)]
        string _program = null;

        [TaskAttribute("commandline")]
        string _commandline = null;

        [TaskAttribute("basedir")]
        string _baseDirectory = null;

        // Stop the buildprocess if the command exits with a returncode other than 0.
        [TaskAttribute("failonerror")]
        [BooleanValidator()]
        string _failonerror = Boolean.TrueString;

        // TODO: change this to Int32Parameter to ensure value is a valid Int32 type after text expansion
        [TaskAttribute("timeout")]
        [Int32Validator()]
        string _timeout = Int32.MaxValue.ToString();

        public override string ProgramFileName  { get { return Project.GetFullPath(_program); } }
        public override string ProgramArguments { get { return _commandline; } }
        public override string BaseDirectory    { get { return Project.GetFullPath(_baseDirectory); } }
        public override int    TimeOut          { get { return Convert.ToInt32(_timeout); } }
        public override bool   FailOnError      { get { return Convert.ToBoolean(_failonerror); } }

        protected override void ExecuteTask() {
            Log.WriteLine(LogPrefix + "{0} {1}", Path.GetFileName(ProgramFileName), GetCommandLine());
            base.ExecuteTask();
        }
    }
}