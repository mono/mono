// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// PartitionedDataSource.cs
//
// <OWNER>[....]</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Threading;
using System.Diagnostics.Contracts;

namespace System.Linq.Parallel
{
    /// <summary>
    /// Contiguous range chunk partitioning attempts to improve data locality by keeping
    /// data close together in the incoming data stream together in the outgoing partitions.
    /// There are really three types of partitions that are used internally:
    ///
    ///     1. If the data source is indexable--like an array or List_T--we can actually
    ///        just compute the range indexes and avoid doing any copying whatsoever. Each
    ///        "partition" is just an enumerator that will walk some subset of the data.
    ///     2. If the data source has an index (different than being indexable!), we can
    ///        turn this into a range scan of the index. We can roughly estimate distribution
    ///        and ensure an evenly balanced set of partitions.
    ///     3. If we can't use 1 or 2, we instead partition "on demand" by chunking the contents
    ///        of the source enumerator as they are requested. The unfortunate thing is that
    ///        this requires synchronization, since consumers may be running in parallel. We
    ///        amortize the cost of this by giving chunks of items when requested instead of
    ///        one element at a time. Note that this approach also works for infinite streams.
    ///
    /// In all cases, the caller can request that enumerators walk elements in striped
    /// contiguous chunks. If striping is requested, then each partition j will yield elements
    /// in the data source for which ((i / s)%p) == j, where i is the element's index, s is
    /// a chunk size calculated by the system with the intent of aligning on cache lines, and
    /// p is the number of partitions. If striping is not requested, we use the same algorith,
    /// only, instead of aligning on cache lines, we use a chunk size of l / p, where l
    /// is the length of the input and p is the number of partitions.
    ///
    /// Notes:
    ///     This is used as the default partitioning strategy by much of the PLINQ infrastructure.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class PartitionedDataSource<T> : PartitionedStream<T, int>
    {

        //---------------------------------------------------------------------------------------
        // Just constructs a new partition stream.
        //

        internal PartitionedDataSource(IEnumerable<T> source, int partitionCount, bool useStriping)
            : base(
                partitionCount,
                Util.GetDefaultComparer<int>(), 
                source is IList<T> ? OrdinalIndexState.Indexible : OrdinalIndexState.Correct)
        {
            InitializePartitions(source, partitionCount, useStriping);
        }

        //---------------------------------------------------------------------------------------
        // This method just creates the individual partitions given a data source.
        // 
        // Notes:
        //     We check whether the data source is an IList<T> and, if so, we can partition
        //     "in place" by calculating a set of indexes. Otherwise, we return an enumerator that
        //     performs partitioning lazily. Depending on which case it is, the enumerator may
        //     contain synchronization (i.e. the latter case), meaning callers may occ----ionally
        //     block when enumerating it.
        //

        private void InitializePartitions(IEnumerable<T> source, int partitionCount, bool useStriping)
        {
            Contract.Assert(source != null);
            Contract.Assert(partitionCount > 0);

            // If this is a wrapper, grab the internal wrapped data source so we can uncover its real type.
            ParallelEnumerableWrapper<T> wrapper = source as ParallelEnumerableWrapper<T>;
            if (wrapper != null)
            {
                source = wrapper.WrappedEnumerable;
                Contract.Assert(source != null);
            }

            // Check whether we have an indexable data source.
            IList<T> sourceAsList = source as IList<T>;
            if (sourceAsList != null)
            {
                QueryOperatorEnumerator<T, int>[] partitions = new QueryOperatorEnumerator<T, int>[partitionCount];
                int listCount = sourceAsList.Count;

                // We use this below to specialize enumerators when possible.
                T[] sourceAsArray = source as T[];

                // If range partitioning is used, chunk size will be unlimited, i.e. -1.
                int maxChunkSize = -1;

                if (useStriping)
                {
                    maxChunkSize = Scheduling.GetDefaultChunkSize<T>();

                    // The minimum chunk size is 1.
                    if (maxChunkSize < 1)
                    {
                        maxChunkSize = 1;
                    }
                }

                // Calculate indexes and construct enumerators that walk a subset of the input.
                for (int i = 0; i < partitionCount; i++)
                {
                    if (sourceAsArray != null)
                    {
                        // If the source is an array, we can use a fast path below to index using
                        // 'ldelem' instructions rather than making interface method calls.
                        if (useStriping)
                        {
                            partitions[i] = new ArrayIndexRangeEnumerator(sourceAsArray, partitionCount, i, maxChunkSize);
                        }
                        else
                        {
                            partitions[i] = new ArrayContiguousIndexRangeEnumerator(sourceAsArray, partitionCount, i);
                        }
                        TraceHelpers.TraceInfo("ContigousRangePartitionExchangeStream::MakePartitions - (array) #{0} {1}", i, maxChunkSize);
                    }
                    else
                    {
                        // Create a general purpose list enumerator object.
                        if (useStriping)
                        {
                            partitions[i] = new ListIndexRangeEnumerator(sourceAsList, partitionCount, i, maxChunkSize);
                        }
                        else
                        {
                            partitions[i] = new ListContiguousIndexRangeEnumerator(sourceAsList, partitionCount, i);
                        }
                        TraceHelpers.TraceInfo("ContigousRangePartitionExchangeStream::MakePartitions - (list) #{0} {1})", i, maxChunkSize);
                    }
                }

                Contract.Assert(partitions.Length == partitionCount);
                m_partitions = partitions;
            }
            else
            {
                // We couldn't use an in-place partition. Shucks. Defer to the other overload which
                // accepts an enumerator as input instead.
                m_partitions = MakePartitions(source.GetEnumerator(), partitionCount);
            }
        }

        //---------------------------------------------------------------------------------------
        // This method just creates the individual partitions given a data source. See the base
        // class for more details on this method's contracts. This version takes an enumerator,
        // and so it can't actually do an in-place partition. We'll instead create enumerators
        // that coordinate with one another to lazily (on demand) grab chunks from the enumerator.
        // This clearly is much less efficient than the fast path above since it requires
        // synchronization. We try to amortize that cost by retrieving many elements at once
        // instead of just one-at-a-time.
        //

        private static QueryOperatorEnumerator<T, int>[] MakePartitions(IEnumerator<T> source, int partitionCount)
        {
            Contract.Assert(source != null);
            Contract.Assert(partitionCount > 0);

            // At this point we were unable to efficiently partition the data source. Instead, we
            // will return enumerators that lazily partition the data source.
            QueryOperatorEnumerator<T, int>[] partitions = new QueryOperatorEnumerator<T, int>[partitionCount];

            // The following is used for synchronization between threads.
            object sharedSyncLock = new object();
            Shared<int> sharedCurrentIndex = new Shared<int>(0);
            Shared<int> sharedPartitionCount = new Shared<int>(partitionCount);
            Shared<bool> sharedExeceptionTracker = new Shared<bool>(false);

            // Create a new lazy chunking enumerator per partition, sharing the same lock.
            for (int i = 0; i < partitionCount; i++)
            {
                partitions[i] = new ContiguousChunkLazyEnumerator(
                    source, sharedExeceptionTracker, sharedSyncLock, sharedCurrentIndex, sharedPartitionCount);
            }

            return partitions;
        }

        //---------------------------------------------------------------------------------------
        // This enumerator walks a range within an indexable data type. It's abstract. We assume
        // callers have validated that the ranges are legal given the data. IndexRangeEnumerator
        // handles both striped and range partitioning.
        //
        // PLINQ creates one IndexRangeEnumerator per partition. Together, the enumerators will
        // cover the entire list or array.
        //
        // In this context, the term "range" represents the entire array or list. The range is
        // split up into one or more "sections". Each section is split up into as many "chunks" as
        // we have partitions. i-th chunk in each section is assigned to partition i.
        //
        // All sections but the last one contain partitionCount * maxChunkSize elements, except
        // for the last section which may contain fewer.
        //
        // For example, if the input is an array with 2,101 elements, maxChunkSize is 128
        // and partitionCount is 4, all sections except the last one will contain 128*4 = 512
        // elements. The last section will contain 2,101 - 4*512 = 53 elements.
        //
        // All sections but the last one will be evenly divided among partitions: the first 128
        // elements will go into partition 0, the next 128 elements into partition 1, etc.
        //
        // The last section is divided as evenly as possible. In the above example, the split would
        // be 14-13-13-13.
        //

        // A copy of the index enumerator specialized for array indexing. It uses 'ldelem'
        // instructions for element retrieval, rather than using a method call.
        internal sealed class ArrayIndexRangeEnumerator : QueryOperatorEnumerator<T, int>
        {
            private readonly T[] m_data; // The elements to iterate over.
            private readonly int m_elementCount; // The number of elements to iterate over.
            private readonly int m_partitionCount; // The number of partitions.
            private readonly int m_partitionIndex; // The index of the current partition.
            private readonly int m_maxChunkSize; // The maximum size of a chunk. -1 if unlimited.
            private readonly int m_sectionCount; // Precomputed in ctor: the number of sections the range is split into.
            private Mutables m_mutables; // Lazily allocated mutable variables.

            class Mutables
            {
                internal Mutables()
                {
                    // Place the enumerator just before the first element
                    m_currentSection = -1;
                }

                internal int m_currentSection; // 0-based index of the current section.
                internal int m_currentChunkSize; // The number of elements in the current chunk.
                internal int m_currentPositionInChunk; // 0-based position within the current chunk.
                internal int m_currentChunkOffset; // The offset of the current chunk from the beginning of the range.
            }

            internal ArrayIndexRangeEnumerator(T[] data, int partitionCount, int partitionIndex, int maxChunkSize)
            {
                Contract.Assert(data != null, "data musn't be null");
                Contract.Assert(partitionCount > 0, "partitionCount must be positive");
                Contract.Assert(partitionIndex >= 0, "partitionIndex can't be negative");
                Contract.Assert(partitionIndex < partitionCount, "partitionIndex must be less than partitionCount");
                Contract.Assert(maxChunkSize > 0, "maxChunkSize must be positive or -1");

                m_data = data;
                m_elementCount = data.Length;
                m_partitionCount = partitionCount;
                m_partitionIndex = partitionIndex;
                m_maxChunkSize = maxChunkSize;

                int sectionSize = maxChunkSize * partitionCount;
                Contract.Assert(sectionSize > 0);

                // Section count is ceiling(elementCount / sectionSize)
                m_sectionCount = m_elementCount / sectionSize +
                    ((m_elementCount % sectionSize) == 0 ? 0 : 1);
            }

            internal override bool MoveNext(ref T currentElement, ref int currentKey)
            {
                // Lazily allocate the mutable holder.
                Mutables mutables = m_mutables;
                if (mutables == null)
                {
                    mutables = m_mutables = new Mutables();
                }

                // If we are aren't within the chunk, we need to find another.
                if (++mutables.m_currentPositionInChunk < mutables.m_currentChunkSize || MoveNextSlowPath())
                {
                    currentKey = mutables.m_currentChunkOffset + mutables.m_currentPositionInChunk;
                    currentElement = m_data[currentKey];
                    return true;
                }

                return false;
            }

            private bool MoveNextSlowPath()
            {
                Mutables mutables = m_mutables;
                Contract.Assert(mutables != null);
                Contract.Assert(mutables.m_currentPositionInChunk >= mutables.m_currentChunkSize);

                // Move on to the next section.
                int currentSection = ++mutables.m_currentSection;
                int sectionsRemaining = m_sectionCount - currentSection;

                // If empty, return right away.
                if (sectionsRemaining <= 0)
                {
                    return false;
                }

                // Compute the offset of the current section from the beginning of the range
                int currentSectionOffset = currentSection * m_partitionCount * m_maxChunkSize;
                mutables.m_currentPositionInChunk = 0;

                // Now do something different based on how many chunks remain.
                if (sectionsRemaining > 1)
                {
                    // We are not on the last section. The size of this chunk is simply m_maxChunkSize.
                    mutables.m_currentChunkSize = m_maxChunkSize;
                    mutables.m_currentChunkOffset = currentSectionOffset + m_partitionIndex * m_maxChunkSize;
                }
                else
                {
                    // We are on the last section. Compute the size of the chunk to ensure even distribution
                    // of elements.
                    int lastSectionElementCount = m_elementCount - currentSectionOffset;
                    int smallerChunkSize = lastSectionElementCount / m_partitionCount;
                    int biggerChunkCount = lastSectionElementCount % m_partitionCount;

                    mutables.m_currentChunkSize = smallerChunkSize;
                    if (m_partitionIndex < biggerChunkCount)
                    {
                        mutables.m_currentChunkSize++;
                    }
                    if (mutables.m_currentChunkSize == 0)
                    {
                        return false;
                    }

                    mutables.m_currentChunkOffset =
                        currentSectionOffset                            // The beginning of this section
                        + m_partitionIndex * smallerChunkSize           // + the start of this chunk if all chunks were "smaller"
                        + (m_partitionIndex < biggerChunkCount ? m_partitionIndex : biggerChunkCount); // + the number of "bigger" chunks before this chunk
                }

                return true;
            }
        }

        // A contiguous index enumerator specialized for array indexing. It uses 'ldelem'
        // instructions for element retrieval, rather than using a method call.
        internal sealed class ArrayContiguousIndexRangeEnumerator : QueryOperatorEnumerator<T, int>
        {
            private readonly T[] m_data; // The elements to iterate over.
            private readonly int m_startIndex; // Where to begin iterating.
            private readonly int m_maximumIndex; // The maximum index to iterate over.
            private Shared<int> m_currentIndex; // The current index (lazily allocated).

            internal ArrayContiguousIndexRangeEnumerator(T[] data, int partitionCount, int partitionIndex)
            {
                Contract.Assert(data != null, "data musn't be null");
                Contract.Assert(partitionCount > 0, "partitionCount must be positive");
                Contract.Assert(partitionIndex >= 0, "partitionIndex can't be negative");
                Contract.Assert(partitionIndex < partitionCount, "partitionIndex must be less than partitionCount");

                m_data = data;

                // Compute the size of the chunk to ensure even distribution of elements.
                int smallerChunkSize = data.Length / partitionCount;
                int biggerChunkCount = data.Length % partitionCount;

                // Our start index is our index times the small chunk size, plus the number
                // of "bigger" chunks before this one.
                int startIndex = partitionIndex * smallerChunkSize +
                    (partitionIndex < biggerChunkCount ? partitionIndex : biggerChunkCount);

                m_startIndex = startIndex - 1; // Subtract one for the first call.
                m_maximumIndex = startIndex + smallerChunkSize + // And add one if we're responsible for part of the
                    (partitionIndex < biggerChunkCount ? 1 : 0); // leftover chunks.

                Contract.Assert(m_currentIndex == null, "Expected deferred allocation to ensure it happens on correct thread");
            }

            internal override bool MoveNext(ref T currentElement, ref int currentKey)
            {
                // Lazily allocate the current index if needed.
                if (m_currentIndex == null)
                {
                    m_currentIndex = new Shared<int>(m_startIndex);
                }

                // Now increment the current index, check bounds, and so on.
                int current = ++m_currentIndex.Value;
                if (current < m_maximumIndex)
                {
                    currentKey = current;
                    currentElement = m_data[current];
                    return true;
                }

                return false;
            }
        }

        // A copy of the index enumerator specialized for IList<T> indexing. It calls through
        // the IList<T> interface for element retrieval.
        internal sealed class ListIndexRangeEnumerator : QueryOperatorEnumerator<T, int>
        {
            private readonly IList<T> m_data; // The elements to iterate over.
            private readonly int m_elementCount; // The number of elements to iterate over.
            private readonly int m_partitionCount; // The number of partitions.
            private readonly int m_partitionIndex; // The index of the current partition.
            private readonly int m_maxChunkSize; // The maximum size of a chunk. -1 if unlimited.
            private readonly int m_sectionCount; // Precomputed in ctor: the number of sections the range is split into.
            private Mutables m_mutables; // Lazily allocated mutable variables.

            class Mutables
            {
                internal Mutables()
                {
                    // Place the enumerator just before the first element
                    m_currentSection = -1;
                }

                internal int m_currentSection; // 0-based index of the current section.
                internal int m_currentChunkSize; // The number of elements in the current chunk.
                internal int m_currentPositionInChunk; // 0-based position within the current chunk.
                internal int m_currentChunkOffset; // The offset of the current chunk from the beginning of the range.
            }

            internal ListIndexRangeEnumerator(IList<T> data, int partitionCount, int partitionIndex, int maxChunkSize)
            {
                Contract.Assert(data != null, "data musn't be null");
                Contract.Assert(partitionCount > 0, "partitionCount must be positive");
                Contract.Assert(partitionIndex >= 0, "partitionIndex can't be negative");
                Contract.Assert(partitionIndex < partitionCount, "partitionIndex must be less than partitionCount");
                Contract.Assert(maxChunkSize > 0, "maxChunkSize must be positive or -1");

                m_data = data;
                m_elementCount = data.Count;
                m_partitionCount = partitionCount;
                m_partitionIndex = partitionIndex;
                m_maxChunkSize = maxChunkSize;

                int sectionSize = maxChunkSize * partitionCount;
                Contract.Assert(sectionSize > 0);

                // Section count is ceiling(elementCount / sectionSize)
                m_sectionCount = m_elementCount / sectionSize +
                    ((m_elementCount % sectionSize) == 0 ? 0 : 1);
            }

            internal override bool MoveNext(ref T currentElement, ref int currentKey)
            {
                // Lazily allocate the mutable holder.
                Mutables mutables = m_mutables;
                if (mutables == null)
                {
                    mutables = m_mutables = new Mutables();
                }

                // If we are aren't within the chunk, we need to find another.
                if (++mutables.m_currentPositionInChunk < mutables.m_currentChunkSize || MoveNextSlowPath())
                {
                    currentKey = mutables.m_currentChunkOffset + mutables.m_currentPositionInChunk;
                    currentElement = m_data[currentKey];
                    return true;
                }

                return false;
            }

            private bool MoveNextSlowPath()
            {
                Mutables mutables = m_mutables;
                Contract.Assert(mutables != null);
                Contract.Assert(mutables.m_currentPositionInChunk >= mutables.m_currentChunkSize);

                // Move on to the next section.
                int currentSection = ++mutables.m_currentSection;
                int sectionsRemaining = m_sectionCount - currentSection;

                // If empty, return right away.
                if (sectionsRemaining <= 0)
                {
                    return false;
                }

                // Compute the offset of the current section from the beginning of the range
                int currentSectionOffset = currentSection * m_partitionCount * m_maxChunkSize;
                mutables.m_currentPositionInChunk = 0;

                // Now do something different based on how many chunks remain.
                if (sectionsRemaining > 1)
                {
                    // We are not on the last section. The size of this chunk is simply m_maxChunkSize.
                    mutables.m_currentChunkSize = m_maxChunkSize;
                    mutables.m_currentChunkOffset = currentSectionOffset + m_partitionIndex * m_maxChunkSize;
                }
                else
                {
                    // We are on the last section. Compute the size of the chunk to ensure even distribution
                    // of elements.
                    int lastSectionElementCount = m_elementCount - currentSectionOffset;
                    int smallerChunkSize = lastSectionElementCount / m_partitionCount;
                    int biggerChunkCount = lastSectionElementCount % m_partitionCount;

                    mutables.m_currentChunkSize = smallerChunkSize;
                    if (m_partitionIndex < biggerChunkCount)
                    {
                        mutables.m_currentChunkSize++;
                    }
                    if (mutables.m_currentChunkSize == 0)
                    {
                        return false;
                    }

                    mutables.m_currentChunkOffset =
                        currentSectionOffset                            // The beginning of this section
                        + m_partitionIndex * smallerChunkSize           // + the start of this chunk if all chunks were "smaller"
                        + (m_partitionIndex < biggerChunkCount ? m_partitionIndex : biggerChunkCount); // + the number of "bigger" chunks before this chunk
                }

                return true;
            }
        }

        // A contiguous index enumerator specialized for IList<T> indexing. It calls through
        // the IList<T> interface for element retrieval.
        internal sealed class ListContiguousIndexRangeEnumerator : QueryOperatorEnumerator<T, int>
        {
            private readonly IList<T> m_data; // The elements to iterate over.
            private readonly int m_startIndex; // Where to begin iterating.
            private readonly int m_maximumIndex; // The maximum index to iterate over.
            private Shared<int> m_currentIndex; // The current index (lazily allocated).

            internal ListContiguousIndexRangeEnumerator(IList<T> data, int partitionCount, int partitionIndex)
            {
                Contract.Assert(data != null, "data musn't be null");
                Contract.Assert(partitionCount > 0, "partitionCount must be positive");
                Contract.Assert(partitionIndex >= 0, "partitionIndex can't be negative");
                Contract.Assert(partitionIndex < partitionCount, "partitionIndex must be less than partitionCount");

                m_data = data;

                // Compute the size of the chunk to ensure even distribution of elements.
                int smallerChunkSize = data.Count / partitionCount;
                int biggerChunkCount = data.Count % partitionCount;

                // Our start index is our index times the small chunk size, plus the number
                // of "bigger" chunks before this one.
                int startIndex = partitionIndex * smallerChunkSize +
                    (partitionIndex < biggerChunkCount ? partitionIndex : biggerChunkCount);

                m_startIndex = startIndex - 1; // Subtract one for the first call.
                m_maximumIndex = startIndex + smallerChunkSize + // And add one if we're responsible for part of the
                    (partitionIndex < biggerChunkCount ? 1 : 0); // leftover chunks.

                Contract.Assert(m_currentIndex == null, "Expected deferred allocation to ensure it happens on correct thread");
            }

            internal override bool MoveNext(ref T currentElement, ref int currentKey)
            {
                // Lazily allocate the current index if needed.
                if (m_currentIndex == null)
                {
                    m_currentIndex = new Shared<int>(m_startIndex);
                }

                // Now increment the current index, check bounds, and so on.
                int current = ++m_currentIndex.Value;
                if (current < m_maximumIndex)
                {
                    currentKey = current;
                    currentElement = m_data[current];
                    return true;
                }

                return false;
            }
        }

        //---------------------------------------------------------------------------------------
        // This enumerator lazily grabs chunks of data from the underlying data source. It is
        // safe for this data source to be enumerated by multiple such enumerators, since it has
        // been written to perform proper synchronization.
        //

        private class ContiguousChunkLazyEnumerator : QueryOperatorEnumerator<T, int>
        {
            private const int chunksPerChunkSize = 7; // the rate at which to double the chunksize (double chunksize every 'r' chunks). MUST BE == (2^n)-1 for some n.
            private readonly IEnumerator<T> m_source; // Data source to enumerate.
            private readonly object m_sourceSyncLock; // Lock to use for all synchronization.
            private readonly Shared<int> m_currentIndex; // The index shared by all.
            private readonly Shared<int> m_activeEnumeratorsCount; // How many enumerators over the same source have not been disposed yet?
            private readonly Shared<bool> m_exceptionTracker;
            private Mutables m_mutables; // Any mutable fields on this enumerator. These mutables are local and persistent
            
            class Mutables
            {
                internal Mutables()
                {
                    m_nextChunkMaxSize = 1; // We start the chunk size at 1 and grow it later.
                    m_chunkBuffer = new T[Scheduling.GetDefaultChunkSize<T>()]; // Pre-allocate the array at the maximum size.
                    m_currentChunkSize = 0; // The chunk begins life begins empty.
                    m_currentChunkIndex = -1;
                    m_chunkBaseIndex = 0;
                    m_chunkCounter = 0;
                }

                internal readonly T[] m_chunkBuffer;      // Buffer array for the current chunk being enumerated.
                internal int m_nextChunkMaxSize;  // The max. chunk size to use for the next chunk.
                internal int m_currentChunkSize;  // The element count for our current chunk.
                internal int m_currentChunkIndex; // Our current index within the temporary chunk.
                internal int m_chunkBaseIndex;    // The start index from which the current chunk was taken.
                internal int  m_chunkCounter;  
            }

            //---------------------------------------------------------------------------------------
            // Constructs a new enumerator that lazily retrieves chunks from the provided source.
            //

            internal ContiguousChunkLazyEnumerator(
                IEnumerator<T> source, Shared<bool> exceptionTracker, object sourceSyncLock, Shared<int> currentIndex, Shared<int> degreeOfParallelism)
            {
                Contract.Assert(source != null);
                Contract.Assert(sourceSyncLock != null);
                Contract.Assert(currentIndex != null);

                m_source = source;
                m_sourceSyncLock = sourceSyncLock;
                m_currentIndex = currentIndex;
                m_activeEnumeratorsCount = degreeOfParallelism;
                m_exceptionTracker = exceptionTracker;    
            }

            //---------------------------------------------------------------------------------------
            // Just retrieves the current element from our current chunk.
            //

            internal override bool MoveNext(ref T currentElement, ref int currentKey)
            {
                Mutables mutables = m_mutables;
                if (mutables == null)
                {
                    mutables = m_mutables = new Mutables();
                }

                Contract.Assert(mutables.m_chunkBuffer != null);

                // Loop until we've exhausted our data source.
                while (true)
                {
                    // If we have elements remaining in the current chunk, return right away.
                    T[] chunkBuffer = mutables.m_chunkBuffer;
                    int currentChunkIndex = ++mutables.m_currentChunkIndex;
                    if (currentChunkIndex < mutables.m_currentChunkSize)
                    {
                        Contract.Assert(m_source != null);
                        Contract.Assert(chunkBuffer != null);
                        Contract.Assert(mutables.m_currentChunkSize > 0);
                        Contract.Assert(0 <= currentChunkIndex && currentChunkIndex < chunkBuffer.Length);
                        currentElement = chunkBuffer[currentChunkIndex];
                        currentKey = mutables.m_chunkBaseIndex + currentChunkIndex;

                        return true;
                    }

                    // Else, it could be the first time enumerating this object, or we may have
                    // just reached the end of the current chunk and need to grab another one? In either
                    // case, we will look for more data from the underlying enumerator.  Because we
                    // share the same enumerator object, we have to do this under a lock.

                    lock (m_sourceSyncLock)
                    {
                        Contract.Assert(0 <= mutables.m_nextChunkMaxSize && mutables.m_nextChunkMaxSize <= chunkBuffer.Length);

                        // Accumulate a chunk of elements from the input.
                        int i = 0;

                        if (m_exceptionTracker.Value)
                        {
                            return false;
                        }

                        try
                        {
                            for (; i < mutables.m_nextChunkMaxSize && m_source.MoveNext(); i++)
                            {
                                // Read the current entry into our buffer.
                                chunkBuffer[i] = m_source.Current;
                            }
                        }
                        catch 
                        {
                            m_exceptionTracker.Value = true;
                            throw;
                        }

                        // Store the number of elements we fetched from the data source.
                        mutables.m_currentChunkSize = i;

                        // If we've emptied the enumerator, return immediately.
                        if (i == 0)
                        {
                            return false;
                        }

                        // Increment the shared index for all to see. Throw an exception on overflow.
                        mutables.m_chunkBaseIndex = m_currentIndex.Value;
                        checked
                        {
                            m_currentIndex.Value += i;
                        }
                    }

                    // Each time we access the data source, we grow the chunk size for the next go-round.
                    // We grow the chunksize once per 'chunksPerChunkSize'. 
                    if (mutables.m_nextChunkMaxSize < chunkBuffer.Length)
                    {
                        if ((mutables.m_chunkCounter++ & chunksPerChunkSize) == chunksPerChunkSize)
                        {
                            mutables.m_nextChunkMaxSize = mutables.m_nextChunkMaxSize * 2;
                            if (mutables.m_nextChunkMaxSize > chunkBuffer.Length)
                            {
                                mutables.m_nextChunkMaxSize = chunkBuffer.Length;
                            }
                        }
                    }

                    // Finally, reset our index to the beginning; loop around and we'll return the right values.
                    mutables.m_currentChunkIndex = -1;
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (Interlocked.Decrement(ref m_activeEnumeratorsCount.Value) == 0)
                {
                    m_source.Dispose();
                }
            }
        }

    }
}
