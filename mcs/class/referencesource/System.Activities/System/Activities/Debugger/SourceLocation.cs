//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Debugger
{
    using System;
    using System.Activities.Debugger.Symbol;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime;

    // Identifies a specific location in the target source code.
    //
    // This source information is used in creating PDBs, which will be passed to the debugger,
    // which will resolve the source file based off its own source paths.
    // Source ranges can:
    // * refer to just an entire single line.
    // * can be a subset within a single line (when StartLine == EndLine)
    // * can also span multiple lines.
    // When column info is provided, the debugger will highlight the characters starting at the start line and start column,
    // and going up to but not including the character specified by the end line and end column.
    [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Our partial trust mechanisms require that this class remain Immutable. Do not add code that allows an instance of this class to change after creation without strict review.")]
    [DebuggerNonUserCode]
    [Serializable]
    [Fx.Tag.XamlVisible(false)]
    public class SourceLocation
    {
        string fileName;
        int startLine;
        int endLine;
        int startColumn;
        int endColumn;
        byte[] checksum;

        // Define a source location from a filename and line-number (1-based).
        // This is a convenience constructor to specify the entire line.
        // This does not load the source file to determine column ranges.
        public SourceLocation(string fileName, int line)
            : this(fileName, line, 1, line, int.MaxValue)
        {
        }

        public SourceLocation(
           string fileName,
           int startLine,
           int startColumn,
           int endLine,
           int endColumn)
            : this(fileName, null, startLine, startColumn, endLine, endColumn)
        {
        }

        // Define a source location in a file.
        // Line/Column are 1-based.
        internal SourceLocation(
            string fileName,
            byte[] checksum,
            int startLine,
            int startColumn,
            int endLine,
            int endColumn)
        {
            if (startLine <= 0)
            {
                throw FxTrace.Exception.Argument("startLine", SR.InvalidSourceLocationLineNumber("startLine", startLine));
            }

            if (startColumn <= 0)
            {
                throw FxTrace.Exception.Argument("startColumn", SR.InvalidSourceLocationColumn("startColumn", startColumn));
            }

            if (endLine <= 0)
            {
                throw FxTrace.Exception.Argument("endLine", SR.InvalidSourceLocationLineNumber("endLine", endLine));
            }

            if (endColumn <= 0)
            {
                throw FxTrace.Exception.Argument("endColumn", SR.InvalidSourceLocationColumn("endColumn", endColumn));
            }

            if (startLine > endLine)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("endLine", endLine, SR.OutOfRangeSourceLocationEndLine(startLine));
            }

            if ((startLine == endLine) && (startColumn > endColumn))
            {
                throw FxTrace.Exception.ArgumentOutOfRange("endColumn", endColumn, SR.OutOfRangeSourceLocationEndColumn(startColumn));
            }

            this.fileName = (fileName != null) ? fileName.ToUpperInvariant() : null;
            this.startLine = startLine;
            this.endLine = endLine;
            this.startColumn = startColumn;
            this.endColumn = endColumn;
            this.checksum = checksum;
        }

        public string FileName
        {
            get { return this.fileName; }
        }

        // Get the 1-based start line.
        public int StartLine
        {
            get { return this.startLine; }
        }

        // Get the 1-based starting column.
        public int StartColumn
        {
            get { return this.startColumn; }
        }

        // Get the 1-based end line. This should be greater or equal to StartLine.
        public int EndLine
        {
            get { return this.endLine; }
        }

        // Get the 1-based ending column.
        public int EndColumn
        {
            get { return this.endColumn; }
        }

        // get the checksum of the source file
        internal byte[] Checksum
        {
            get { return this.checksum; }
        }

        public bool IsSingleWholeLine
        {
            get
            {
                return this.endColumn == int.MaxValue && this.startLine == this.endLine && this.startColumn == 1;
            }
        }

        // Equality comparison function. This checks for strict equality and
        // not for superset or subset relationships.
        public override bool Equals(object obj)
        {
            SourceLocation rsl = obj as SourceLocation;
            if (rsl == null)
            {
                return false;
            }

            if (this.FileName != rsl.FileName)
            {
                return false;
            }            

            if (this.StartLine != rsl.StartLine ||
                this.StartColumn != rsl.StartColumn ||
                this.EndLine != rsl.EndLine ||
                this.EndColumn != rsl.EndColumn)
            {
                return false;
            }

            if (this.Checksum == null ^ rsl.Checksum == null)
            {
                return false;
            }
            else if ((this.Checksum != null && rsl.Checksum != null) && !this.Checksum.SequenceEqual(rsl.Checksum))
            {
                return false;
            }

            // everything matches
            return true;
        }

        // Get a hash code.
        public override int GetHashCode()
        {
            return (string.IsNullOrEmpty(this.FileName) ? 0 : this.FileName.GetHashCode()) ^
                    this.StartLine.GetHashCode() ^
                    this.StartColumn.GetHashCode() ^
                    ((this.Checksum == null) ? 0 : SymbolHelper.GetHexStringFromChecksum(this.Checksum).GetHashCode());
        }

        internal static bool IsValidRange(int startLine, int startColumn, int endLine, int endColumn)
        {
            return
                (startLine > 0) && (startColumn > 0) && (endLine > 0) && (endColumn > 0) &&
                ((startLine < endLine) || (startLine == endLine) && (startColumn < endColumn));

        }
    }
}
