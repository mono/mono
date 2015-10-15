// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// QueryAggregationOptions.cs
//
// <OWNER>igoro</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

namespace System.Linq.Parallel
{
    /// <summary>
    /// An enum to specify whether an aggregate operator is associative, commutative,
    /// neither, or both. This influences query analysis and execution: associative
    /// aggregations can run in parallel, whereas non-associative cannot; non-commutative
    /// aggregations must be run over data in input-order. 
    /// </summary>
    [Flags]
    internal enum QueryAggregationOptions
    {
        None = 0,
        Associative = 1,
        Commutative = 2,
        AssociativeCommutative = (Associative | Commutative) // For convenience.
    }
}