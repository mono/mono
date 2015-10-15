// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// OrderedHashRepartitionEnumerator.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Threading;
using System.Diagnostics.Contracts;

namespace System.Linq.Parallel
{
    /// <summary>
    /// This enumerator handles the actual coordination among partitions required to
    /// accomplish the repartitioning operation, as explained above.  In addition to that,
    /// it tracks order keys so that order preservation can flow through the enumerator.
    /// </summary>
    /// <typeparam name="TInputOutput">The kind of elements.</typeparam>
    /// <typeparam name="THashKey">The key used to distribute elements.</typeparam>
    /// <typeparam name="TOrderKey">The kind of keys found in the source.</typeparam>
    internal class OrderedHashRepartitionEnumerator<TInputOutput, THashKey, TOrderKey> : QueryOperatorEnumerator<Pair<TInputOutput, THashKey>, TOrderKey>
    {
        private const int ENUMERATION_NOT_STARTED = -1; // Sentinel to note we haven't begun enumerating yet.

        private readonly int m_partitionCount; // The number of partitions.
        private readonly int m_partitionIndex; // Our unique partition index.
        private readonly Func<TInputOutput, THashKey> m_keySelector; // A key-selector function.
        private readonly HashRepartitionStream<TInputOutput, THashKey, TOrderKey> m_repartitionStream; // A repartitioning stream.
        private readonly ListChunk<Pair<TInputOutput, THashKey>>[,] m_valueExchangeMatrix; // Matrix to do inter-task communication of values.
        private readonly ListChunk<TOrderKey>[,] m_keyExchangeMatrix; // Matrix to do inter-task communication of order keys.
        private readonly QueryOperatorEnumerator<TInputOutput, TOrderKey> m_source; // The immediate source of data.
        private CountdownEvent m_barrier; // Used to signal and wait for repartitions to complete.
        private readonly CancellationToken m_cancellationToken; // A token for canceling the process.
        private Mutables m_mutables; // Mutable fields for this enumerator.

        class Mutables
        {
            internal int m_currentBufferIndex; // Current buffer index.
            internal ListChunk<Pair<TInputOutput, THashKey>> m_currentBuffer; // The buffer we're currently enumerating.
            internal ListChunk<TOrderKey> m_currentKeyBuffer; // The buffer we're currently enumerating.
            internal int m_currentIndex; // Current index into the buffer.

            internal Mutables()
            {
                m_currentBufferIndex = ENUMERATION_NOT_STARTED;
            }
        }

        //---------------------------------------------------------------------------------------
        // Creates a new repartitioning enumerator.
        //
        // Arguments:
        //     source            - the data stream from which to pull elements
        //     useOrdinalOrderPreservation - whether order preservation is required
        //     partitionCount    - total number of partitions
        //     partitionIndex    - this operator's unique partition index
        //     repartitionStream - the stream object to use for partition selection
        //     barrier           - a latch used to signal task completion
        //     buffers           - a set of buffers for inter-task communication
        //

        internal OrderedHashRepartitionEnumerator(
            QueryOperatorEnumerator<TInputOutput, TOrderKey> source, int partitionCount, int partitionIndex,
            Func<TInputOutput, THashKey> keySelector, OrderedHashRepartitionStream<TInputOutput, THashKey, TOrderKey> repartitionStream, CountdownEvent barrier,
            ListChunk<Pair<TInputOutput, THashKey>>[,] valueExchangeMatrix, ListChunk<TOrderKey>[,] keyExchangeMatrix, CancellationToken cancellationToken)
        {
            Contract.Assert(source != null);
            Contract.Assert(keySelector != null || typeof(THashKey) == typeof(NoKeyMemoizationRequired));
            Contract.Assert(repartitionStream != null);
            Contract.Assert(barrier != null);
            Contract.Assert(valueExchangeMatrix != null);
            Contract.Assert(valueExchangeMatrix.GetLength(0) == partitionCount, "expected square matrix of buffers (NxN)");
            Contract.Assert(valueExchangeMatrix.GetLength(1) == partitionCount, "expected square matrix of buffers (NxN)");
            Contract.Assert(0 <= partitionIndex && partitionIndex < partitionCount);

            m_source = source;
            m_partitionCount = partitionCount;
            m_partitionIndex = partitionIndex;
            m_keySelector = keySelector;
            m_repartitionStream = repartitionStream;
            m_barrier = barrier;
            m_valueExchangeMatrix = valueExchangeMatrix;
            m_keyExchangeMatrix = keyExchangeMatrix;
            m_cancellationToken = cancellationToken;
        }

        //---------------------------------------------------------------------------------------
        // Retrieves the next element from this partition.  All repartitioning operators across
        // all partitions cooperate in a barrier-style algorithm.  The first time an element is
        // requested, the repartitioning operator will enter the 1st phase: during this phase, it
        // scans its entire input and compute the destination partition for each element.  During
        // the 2nd phase, each partition scans the elements found by all other partitions for
        // it, and yield this to callers.  The only synchronization required is the barrier itself
        // -- all other parts of this algorithm are synchronization-free.
        //
        // Notes: One rather large penalty that this algorithm incurs is higher memory usage and a
        // larger time-to-first-element latency, at least compared with our old implementation; this
        // happens because all input elements must be fetched before we can produce a single output
        // element.  In many cases this isn't too terrible: e.g. a GroupBy requires this to occur
        // anyway, so having the repartitioning operator do so isn't complicating matters much at all.
        //

        internal override bool MoveNext(ref Pair<TInputOutput, THashKey> currentElement, ref TOrderKey currentKey)
        {
            if (m_partitionCount == 1)
            {
                TInputOutput current = default(TInputOutput);

                // If there's only one partition, no need to do any sort of exchanges.
                if (m_source.MoveNext(ref current, ref currentKey))
                {
                    currentElement = new Pair<TInputOutput, THashKey>(
                        current, m_keySelector == null ? default(THashKey) : m_keySelector(current));
                    return true;
                }

                return false;
            }

            Mutables mutables = m_mutables;
            if (mutables == null)
                mutables = m_mutables = new Mutables();

            // If we haven't enumerated the source yet, do that now.  This is the first phase
            // of a two-phase barrier style operation.
            if (mutables.m_currentBufferIndex == ENUMERATION_NOT_STARTED)
            {
                EnumerateAndRedistributeElements();
                Contract.Assert(mutables.m_currentBufferIndex != ENUMERATION_NOT_STARTED);
            }

            // Once we've enumerated our contents, we can then go back and walk the buffers that belong
            // to the current partition.  This is phase two.  Note that we slyly move on to the first step
            // of phase two before actually waiting for other partitions.  That's because we can enumerate
            // the buffer we wrote to above, as already noted.
            while (mutables.m_currentBufferIndex < m_partitionCount)
            {
                // If the queue is non-null and still has elements, yield them.
                if (mutables.m_currentBuffer != null)
                {
                    Contract.Assert(mutables.m_currentKeyBuffer != null);

                    if (++mutables.m_currentIndex < mutables.m_currentBuffer.Count)
                    {
                        // Return the current element.
                        currentElement = mutables.m_currentBuffer.m_chunk[mutables.m_currentIndex];
                        Contract.Assert(mutables.m_currentKeyBuffer != null, "expected same # of buffers/key-buffers");
                        currentKey = mutables.m_currentKeyBuffer.m_chunk[mutables.m_currentIndex];
                        return true;
                    }
                    else
                    {
                        // If the chunk is empty, advance to the next one (if any).
                        mutables.m_currentIndex = ENUMERATION_NOT_STARTED;
                        mutables.m_currentBuffer = mutables.m_currentBuffer.Next;
                        mutables.m_currentKeyBuffer = mutables.m_currentKeyBuffer.Next;
                        Contract.Assert(mutables.m_currentBuffer == null || mutables.m_currentBuffer.Count > 0);
                        Contract.Assert((mutables.m_currentBuffer == null) == (mutables.m_currentKeyBuffer == null));
                        Contract.Assert(mutables.m_currentBuffer == null || mutables.m_currentBuffer.Count == mutables.m_currentKeyBuffer.Count);
                        continue; // Go back around and invoke this same logic.
                    }
                }

                // We're done with the current partition.  Slightly different logic depending on whether
                // we're on our own buffer or one that somebody else found for us.
                if (mutables.m_currentBufferIndex == m_partitionIndex)
                {
                    // We now need to wait at the barrier, in case some other threads aren't done.
                    // Once we wake up, we reset our index and will increment it immediately after.
                    m_barrier.Wait(m_cancellationToken);
                    mutables.m_currentBufferIndex = ENUMERATION_NOT_STARTED;
                }

                // Advance to the next buffer.
                mutables.m_currentBufferIndex++;
                mutables.m_currentIndex = ENUMERATION_NOT_STARTED;

                if (mutables.m_currentBufferIndex == m_partitionIndex)
                {
                    // Skip our current buffer (since we already enumerated it).
                    mutables.m_currentBufferIndex++;
                }

                // Assuming we're within bounds, retrieve the next buffer object.
                if (mutables.m_currentBufferIndex < m_partitionCount)
                {
                    mutables.m_currentBuffer = m_valueExchangeMatrix[mutables.m_currentBufferIndex, m_partitionIndex];
                    mutables.m_currentKeyBuffer = m_keyExchangeMatrix[mutables.m_currentBufferIndex, m_partitionIndex];
                }
            }

            // We're done. No more buffers to enumerate.
            return false;
        }

        //---------------------------------------------------------------------------------------
        // Called when this enumerator is first enumerated; it must walk through the source
        // and redistribute elements to their slot in the exchange matrix.
        //

        private void EnumerateAndRedistributeElements()
        {
            Mutables mutables = m_mutables;
            Contract.Assert(mutables != null);

            ListChunk<Pair<TInputOutput, THashKey>>[] privateBuffers = new ListChunk<Pair<TInputOutput, THashKey>>[m_partitionCount];
            ListChunk<TOrderKey>[] privateKeyBuffers = new ListChunk<TOrderKey>[m_partitionCount];

            TInputOutput element = default(TInputOutput);
            TOrderKey key = default(TOrderKey);
            int loopCount = 0;
            while (m_source.MoveNext(ref element, ref key))
            {
                if ((loopCount++ & CancellationState.POLL_INTERVAL) == 0)
                    CancellationState.ThrowIfCanceled(m_cancellationToken);

                // Calculate the element's destination partition index, placing it into the
                // appropriate buffer from which partitions will later enumerate.
                int destinationIndex;
                THashKey elementHashKey = default(THashKey);
                if (m_keySelector != null)
                {
                    elementHashKey = m_keySelector(element);
                    destinationIndex = m_repartitionStream.GetHashCode(elementHashKey) % m_partitionCount;
                }
                else
                {
                    Contract.Assert(typeof(THashKey) == typeof(NoKeyMemoizationRequired));
                    destinationIndex = m_repartitionStream.GetHashCode(element) % m_partitionCount;
                }

                Contract.Assert(0 <= destinationIndex && destinationIndex < m_partitionCount,
                                "destination partition outside of the legal range of partitions");

                // Get the buffer for the destnation partition, lazily allocating if needed.  We maintain
                // this list in our own private cache so that we avoid accessing shared memory locations
                // too much.  In the original implementation, we'd access the buffer in the matrix ([N,M],
                // where N is the current partition and M is the destination), but some rudimentary
                // performance profiling indicates copying at the end performs better.
                ListChunk<Pair<TInputOutput, THashKey>> buffer = privateBuffers[destinationIndex];
                ListChunk<TOrderKey> keyBuffer = privateKeyBuffers[destinationIndex];
                if (buffer == null)
                {
                    const int INITIAL_PRIVATE_BUFFER_SIZE = 128;
                    Contract.Assert(keyBuffer == null);
                    privateBuffers[destinationIndex] = buffer = new ListChunk<Pair<TInputOutput, THashKey>>(INITIAL_PRIVATE_BUFFER_SIZE);
                    privateKeyBuffers[destinationIndex] = keyBuffer = new ListChunk<TOrderKey>(INITIAL_PRIVATE_BUFFER_SIZE);
                }

                buffer.Add(new Pair<TInputOutput, THashKey>(element, elementHashKey));
                keyBuffer.Add(key);

            }

            // Copy the local buffers to the shared space and then signal to other threads that
            // we are done.  We can then immediately move on to enumerating the elements we found
            // for the current partition before waiting at the barrier.  If we found a lot, we will
            // hopefully never have to physically wait.
            for (int i = 0; i < m_partitionCount; i++)
            {
                m_valueExchangeMatrix[m_partitionIndex, i] = privateBuffers[i];
                m_keyExchangeMatrix[m_partitionIndex, i] = privateKeyBuffers[i];
            }

            m_barrier.Signal();

            // Begin at our own buffer.
            mutables.m_currentBufferIndex = m_partitionIndex;
            mutables.m_currentBuffer = privateBuffers[m_partitionIndex];
            mutables.m_currentKeyBuffer = privateKeyBuffers[m_partitionIndex];
            mutables.m_currentIndex = ENUMERATION_NOT_STARTED;
        }

        protected override void Dispose(bool disposing)
        {
            if (m_barrier != null)
            {
                // Since this enumerator is being disposed, we will decrement the barrier,
                // in case other enumerators will wait on the barrier.
                if (m_mutables == null || (m_mutables.m_currentBufferIndex == ENUMERATION_NOT_STARTED))
                {
                    m_barrier.Signal();
                    m_barrier = null;
                }

                m_source.Dispose();
            }
        }
    }
}
