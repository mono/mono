// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// ListQueryResults.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;

namespace System.Linq.Parallel
{
    /// <summary>
    /// Class to represent an IList{T} as QueryResults{T} 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ListQueryResults<T> : QueryResults<T>
    {
        private IList<T> m_source;
        private int m_partitionCount;
        private bool m_useStriping;

        internal ListQueryResults(IList<T> source, int partitionCount, bool useStriping)
        {
            m_source = source;
            m_partitionCount = partitionCount;
            m_useStriping = useStriping;
        }

        internal override void GivePartitionedStream(IPartitionedStreamRecipient<T> recipient)
        {
            PartitionedStream<T, int> partitionedStream = GetPartitionedStream();
            recipient.Receive<int>(partitionedStream);
        }

        internal override bool IsIndexible
        {
            get { return true; }
        }

        internal override int ElementsCount
        {
            get { return m_source.Count; }
        }

        internal override T GetElement(int index)
        {
            return m_source[index];
        }

        internal PartitionedStream<T, int> GetPartitionedStream()
        {
            return ExchangeUtilities.PartitionDataSource(m_source, m_partitionCount, m_useStriping);
        }
    }
}
