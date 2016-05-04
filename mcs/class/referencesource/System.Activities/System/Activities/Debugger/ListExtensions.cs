// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Debugger
{
    using System.Collections.Generic;

    internal static class ListExtensions
    {
        internal static BinarySearchResult MyBinarySearch<T>(this List<T> input, T item)
        {
            return new BinarySearchResult(input.BinarySearch(item), input.Count);
        }
    }
}
