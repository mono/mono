// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// Shared.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-


namespace System.Linq.Parallel
{
    /// <summary>
    /// A very simple primitive that allows us to share a value across multiple threads.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Shared<T>
    {
        internal T Value;

        internal Shared(T value)
        {
            this.Value = value;
        }

    }
}
