// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Debugger
{
    using System.Diagnostics;

    // Immutable, start and end always non-null.
    [DebuggerDisplay("({Start.LineNumber.Value}:{Start.LinePosition.Value}) - ({End.LineNumber.Value}:{End.LinePosition.Value})")]
    internal class DocumentRange : IEquatable<DocumentRange>
    {
        private DocumentLocation start;
        private DocumentLocation end;

        internal DocumentRange(DocumentLocation start, DocumentLocation end)
        {
            UnitTestUtility.Assert(start != null, "DocumentRange.Start cannot be null");
            UnitTestUtility.Assert(end != null, "DocumentRange.End cannot be null");
            UnitTestUtility.Assert((start.LineNumber.Value < end.LineNumber.Value) || ((start.LineNumber.Value == end.LineNumber.Value) && (start.LinePosition.Value <= end.LinePosition.Value)), "Start cannot before go after End.");
            this.start = start;
            this.end = end;
        }

        internal DocumentRange(int startLineNumber, int startLinePosition, int endLineNumber, int endLinePosition)
            : this(new DocumentLocation(startLineNumber, startLinePosition), new DocumentLocation(endLineNumber, endLinePosition))
        {
        }

        internal DocumentLocation Start
        {
            get { return this.start; }
        }

        internal DocumentLocation End
        {
            get { return this.end; }
        }

        public bool Equals(DocumentRange other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Start.Equals(other.Start) && this.End.Equals(other.End);
        }

        public override int GetHashCode()
        {
            return this.Start.GetHashCode() ^ this.End.GetHashCode();
        }
    }
}
