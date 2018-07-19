// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Debugger
{
    // Immutable, value >= 1
    internal class OneBasedCounter
    {
        private int value;

        internal OneBasedCounter(int value)
        {
            UnitTestUtility.Assert(value > 0, "value cannot less than one for OneBasedCounter");
            this.value = value;
        }

        internal int Value
        {
            get { return this.value; }
        }
    }
}
