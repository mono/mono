// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Debugger
{
    using System.Diagnostics;

    // Immutable, lineNumber and linePosition always non-null.
    [DebuggerDisplay("({LineNumber.Value}:{LinePosition.Value})")]
    internal class DocumentLocation : IEquatable<DocumentLocation>, IComparable<DocumentLocation>
    {
        private OneBasedCounter lineNumber;
        private OneBasedCounter linePosition;

        internal DocumentLocation(OneBasedCounter lineNumber, OneBasedCounter linePosition)
        {
            UnitTestUtility.Assert(lineNumber != null, "lineNumber should not be null.");
            UnitTestUtility.Assert(linePosition != null, "linePosition should not be null.");
            this.lineNumber = lineNumber;
            this.linePosition = linePosition;
        }

        internal DocumentLocation(int lineNumber, int linePosition)
            : this(new OneBasedCounter(lineNumber), new OneBasedCounter(linePosition))
        {
        }

        internal OneBasedCounter LineNumber
        {
            get { return this.lineNumber; }
        }

        internal OneBasedCounter LinePosition
        {
            get { return this.linePosition; }
        }

        public bool Equals(DocumentLocation that)
        {
            if (that == null)
            {
                return false;
            }

            return (this.lineNumber.Value == that.lineNumber.Value) && (this.linePosition.Value == that.linePosition.Value);
        }

        public override int GetHashCode()
        {
            return this.lineNumber.Value.GetHashCode() ^ this.linePosition.Value.GetHashCode();
        }

        public int CompareTo(DocumentLocation that)
        {
            if (that == null)
            {
                // Following the convention we have in System.Int32 that anything is considered bigger than null.
                return 1;
            }

            if (this.lineNumber.Value == that.lineNumber.Value)
            {
                // The subtraction of two numbers >= 1 must not underflow integer.
                return this.linePosition.Value - that.linePosition.Value;
            }
            else
            {
                // The subtraction of two numbers >= 1 must not underflow integer.
                return this.lineNumber.Value - that.lineNumber.Value;
            }
        }
    }
}
