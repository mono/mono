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
    using System.IO;

    [TaskName("csc")]
    public class CscTask : CompilerBase {

        // C# specific compiler options
        [TaskAttribute("doc")]
        string _doc = null;

        protected override void WriteOptions(TextWriter writer) {
            WriteOption(writer, "fullpaths");
            if (_doc != null) {
                WriteOption(writer, "doc", _doc);
            }
        }

        protected override bool NeedsCompiling() {
            // TODO: add checks for any referenced files OR return false to always compile
            return base.NeedsCompiling();
        }
    }
}
