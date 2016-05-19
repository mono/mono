// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// IPartitionedStreamRecipient.cs
//
// <OWNER>igoro</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

namespace System.Linq.Parallel
{
    /// <summary>
    /// IPartitionedStreamRecipient is essentially a generic action on a partitioned stream,
    /// whose generic type parameter is the type of the order keys in the partitioned stream.
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    interface IPartitionedStreamRecipient<TElement>
    {
        void Receive<TKey>(PartitionedStream<TElement, TKey> partitionedStream);
    }
}