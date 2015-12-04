// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// Wrapper.cs
//
// <OWNER>[....]</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

namespace System.Linq.Parallel
{
    /// <summary>
    /// A struct to wrap any arbitrary object reference or struct.  Used for situations
    /// where we can't tolerate null values (like keys for hashtables).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal struct Wrapper<T>
    {
        internal T Value;

        internal Wrapper(T value)
        {
            this.Value = value;
        }
    }
}
