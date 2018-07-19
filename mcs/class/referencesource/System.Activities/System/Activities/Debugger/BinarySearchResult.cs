// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Debugger
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    [DebuggerDisplay("{this.ToString()}")]
    internal class BinarySearchResult
    {
        private int result;
        private int count;

        internal BinarySearchResult(int resultFromBinarySearch, int count)
        {
            this.result = resultFromBinarySearch;
            this.count = count;
        }

        internal bool IsFound
        {
            get { return this.result >= 0; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal int FoundIndex
        {
            get
            {
                UnitTestUtility.Assert(this.IsFound, "We should not call FoundIndex if we cannot find the element.");
                return this.result;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal int NextIndex
        {
            get
            {
                UnitTestUtility.Assert(!this.IsFound, "We should not call NextIndex if we found the element.");
                UnitTestUtility.Assert(this.IsNextIndexAvailable, "We should not call NextIndex if next index is not available.");
                return this.NextIndexValue;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal bool IsNextIndexAvailable
        {
            get
            {
                UnitTestUtility.Assert(!this.IsFound, "We should not call IsNextIndexAvailable if we found the element.");
                return this.NextIndexValue != this.count;
            }
        }

        private int NextIndexValue
        {
            get { return ~this.result; }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)", Justification = "Message used in debugger only.")]
        public override string ToString()
        {
            if (this.IsFound)
            {
                return string.Format("Data is found at index {0}.", this.FoundIndex);
            }
            else if (this.IsNextIndexAvailable)
            {
                return string.Format("Data is not found, the next index is {0}.", this.NextIndex);
            }
            else
            {
                return "Data is not found and there is no next index.";
            }
        }
    }
}
