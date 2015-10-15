//------------------------------------------------------------------------------
// <copyright file="SourceLineInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System.Diagnostics;

namespace System.Xml.Xsl {

    [DebuggerDisplay("({Line},{Pos})")]
    internal struct Location {
        private ulong value;

        public int Line { get { return (int)(this.value >> 32); } }
        public int Pos  { get { return (int)(this.value      ); } }

        public Location(int line, int pos) {
            this.value = (((ulong)line) << 32) | (uint)pos;
        }

        public Location(Location that) {
            this.value = that.value;
        }

        public bool LessOrEqual(Location that) {
            return this.value <= that.value;
        }
    }

    [DebuggerDisplay("{Uri} [{StartLine},{StartPos} -- {EndLine},{EndPos}]")]
    internal class SourceLineInfo : ISourceLineInfo {
        protected string    uriString;
        protected Location  start;
        protected Location  end;

        public SourceLineInfo(string uriString, int startLine, int startPos, int endLine, int endPos)
            : this(uriString, new Location(startLine, startPos), new Location(endLine, endPos))
        {}

        public SourceLineInfo(string uriString, Location start, Location end) {
            this.uriString = uriString;
            this.start     = start;
            this.end       = end;
            Validate(this);
        }

        public string   Uri       { get { return this.uriString ; } }
        public int      StartLine { get { return this.start.Line; } }
        public int      StartPos  { get { return this.start.Pos ; } }
        public int      EndLine   { get { return this.end.Line  ; } }
        public int      EndPos    { get { return this.end.Pos   ; } }
        public Location End       { get { return this.end       ; } }
        public Location Start     { get { return this.start     ; } }

        /// <summary>
        /// Magic number 0xfeefee is used in PDB to denote a section of IL that does not map to any user code.
        /// When VS debugger steps into IL marked with 0xfeefee, it will continue the step until it reaches
        /// some user code.
        /// </summary>
        protected const int NoSourceMagicNumber = 0xfeefee;

        public static SourceLineInfo NoSource = new SourceLineInfo(string.Empty, NoSourceMagicNumber, 0, NoSourceMagicNumber, 0);

        public bool IsNoSource {
            get { return this.StartLine == NoSourceMagicNumber; }
        }

        [Conditional("DEBUG")]
        public static void Validate(ISourceLineInfo lineInfo) {
            if (lineInfo.Start.Line == 0 || lineInfo.Start.Line == NoSourceMagicNumber) {
                Debug.Assert(lineInfo.Start.Line == lineInfo.End.Line);
                Debug.Assert(lineInfo.Start.Pos == 0 && lineInfo.End.Pos == 0);
            } else {
                Debug.Assert(0 < lineInfo.Start.Line && 0 < lineInfo.Start.Pos, "0 < start");
                Debug.Assert(0 < lineInfo.End.Line   && 0 < lineInfo.End.Pos  , "0 < end");
                Debug.Assert(lineInfo.Start.LessOrEqual(lineInfo.End), "start <= end");
            }
        }

        // Returns file path for local and network URIs. Used for PDB generating and error reporting.
        public static string GetFileName(string uriString) {
            Debug.Assert(uriString != null);
            Uri uri;

            if (uriString.Length != 0 &&
                System.Uri.TryCreate(uriString, UriKind.Absolute, out uri) &&
                uri.IsFile
            ) {
                return uri.LocalPath;
            }
            return uriString;
        }
    }
}
