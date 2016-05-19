// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;

    internal class LineColumnPair : Tuple<int, int>
    {
        internal LineColumnPair(int item1, int item2)
            : base(item1, item2)
        {
            SharedFx.Assert(item1 > 0 && item2 > 0, "item1 > 0&& item2 > 0");
        }

        internal int LineNumber
        {
            get { return this.Item1; }
        }

        internal int ColumnNumber
        {
            get { return this.Item2; }
        }
    }
}
