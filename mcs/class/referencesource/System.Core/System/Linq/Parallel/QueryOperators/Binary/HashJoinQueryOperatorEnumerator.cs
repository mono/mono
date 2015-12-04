// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// HashJoinQueryOperatorEnumerator.cs
//
// <OWNER>[....]</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;

namespace System.Linq.Parallel
{
    /// <summary>
    /// This enumerator implements the hash-join algorithm as noted earlier.
    ///
    /// Assumptions:
    ///     This enumerator type won't work properly at all if the analysis engine didn't
    ///     ensure a proper hash-partition. We expect inner and outer elements with equal
    ///     keys are ALWAYS in the same partition. If they aren't (e.g. if the analysis is
    ///     busted) we'll silently drop items on the floor. :( 
    ///     
    ///     
    ///  This is the enumerator class for two operators:
    ///   - Join
    ///   - GroupJoin
    /// </summary>
    /// <typeparam name="TLeftInput"></typeparam>
    /// <typeparam name="TLeftKey"></typeparam>
    /// <typeparam name="TRightInput"></typeparam>
    /// <typeparam name="THashKey"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    internal class HashJoinQueryOperatorEnumerator<TLeftInput, TLeftKey, TRightInput, THashKey, TOutput> 
        : QueryOperatorEnumerator<TOutput,TLeftKey>
    {
        private readonly QueryOperatorEnumerator<Pair<TLeftInput, THashKey>, TLeftKey> m_leftSource; // Left (outer) data source. For probing.
        private readonly QueryOperatorEnumerator<Pair<TRightInput, THashKey>, int> m_rightSource; // Right (inner) data source. For building.
        private readonly Func<TLeftInput, TRightInput, TOutput> m_singleResultSelector; // Single result selector.
        private readonly Func<TLeftInput, IEnumerable<TRightInput>, TOutput> m_groupResultSelector; // Group result selector.
        private readonly IEqualityComparer<THashKey> m_keyComparer; // An optional key comparison object.
        private readonly CancellationToken m_cancellationToken;
        private Mutables m_mutables;

        private class Mutables
        {
            internal TLeftInput m_currentLeft; // The current matching left element.
            internal TLeftKey m_currentLeftKey; // The current index of the matching left element.
            internal HashLookup<THashKey, Pair<TRightInput, ListChunk<TRightInput>>> m_rightHashLookup; // The hash lookup.
            internal ListChunk<TRightInput> m_currentRightMatches; // Current right matches (if any).
            internal int m_currentRightMatchesIndex; // Current index in the set of right matches.
            internal int m_outputLoopCount;
        }

        //---------------------------------------------------------------------------------------
        // Instantiates a new hash-join enumerator.
        //

        internal HashJoinQueryOperatorEnumerator(
            QueryOperatorEnumerator<Pair<TLeftInput, THashKey>, TLeftKey> leftSource,
            QueryOperatorEnumerator<Pair<TRightInput, THashKey>, int> rightSource,
            Func<TLeftInput, TRightInput, TOutput> singleResultSelector,
            Func<TLeftInput, IEnumerable<TRightInput>, TOutput> groupResultSelector,
            IEqualityComparer<THashKey> keyComparer,
            CancellationToken cancellationToken)
        {
            Contract.Assert(leftSource != null);
            Contract.Assert(rightSource != null);
            Contract.Assert(singleResultSelector != null || groupResultSelector != null);

            m_leftSource = leftSource;
            m_rightSource = rightSource;
            m_singleResultSelector = singleResultSelector;
            m_groupResultSelector = groupResultSelector;
            m_keyComparer = keyComparer;
            m_cancellationToken = cancellationToken;
        }

        //---------------------------------------------------------------------------------------
        // MoveNext implements all the hash-join logic noted earlier. When it is called first, it
        // will execute the entire inner query tree, and build a hash-table lookup. This is the
        // Building phase. Then for the first call and all subsequent calls to MoveNext, we will
        // incrementally perform the Probing phase. We'll keep getting elements from the outer
        // data source, looking into the hash-table we built, and enumerating the full results.
        //
        // This routine supports both inner and outer (group) joins. An outer join will yield a
        // (possibly empty) list of matching elements from the inner instead of one-at-a-time,
        // as we do for inner joins.
        //

        internal override bool MoveNext(ref TOutput currentElement, ref TLeftKey currentKey)
        {
            Contract.Assert(m_singleResultSelector != null || m_groupResultSelector != null, "expected a compiled result selector");
            Contract.Assert(m_leftSource != null);
            Contract.Assert(m_rightSource != null);

            // BUILD phase: If we haven't built the hash-table yet, create that first.
            Mutables mutables = m_mutables;
            if (mutables == null)
            {
                mutables = m_mutables = new Mutables();
#if DEBUG
                int hashLookupCount = 0;
                int hashKeyCollisions = 0;
#endif
                mutables.m_rightHashLookup = new HashLookup<THashKey, Pair<TRightInput, ListChunk<TRightInput>>>(m_keyComparer);

                Pair<TRightInput, THashKey> rightPair = default(Pair<TRightInput, THashKey>);
                int rightKeyUnused = default(int);
                int i = 0;
                while (m_rightSource.MoveNext(ref rightPair, ref rightKeyUnused))
                {
                    if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                        CancellationState.ThrowIfCanceled(m_cancellationToken);

                    TRightInput rightElement = rightPair.First;
                    THashKey rightHashKey = rightPair.Second;

                    // We ignore null keys.
                    if (rightHashKey != null)
                    {
#if DEBUG
                        hashLookupCount++;
#endif

                        // See if we've already stored an element under the current key. If not, we
                        // lazily allocate a pair to hold the elements mapping to the same key.
                        const int INITIAL_CHUNK_SIZE = 2;
                        Pair<TRightInput, ListChunk<TRightInput>> currentValue = default(Pair<TRightInput, ListChunk<TRightInput>>);
                        if (!mutables.m_rightHashLookup.TryGetValue(rightHashKey, ref currentValue))
                        {
                            currentValue = new Pair<TRightInput, ListChunk<TRightInput>>(rightElement, null);

                            if (m_groupResultSelector != null)
                            {
                                // For group joins, we also add the element to the list. This makes
                                // it easier later to yield the list as-is.
                                currentValue.Second = new ListChunk<TRightInput>(INITIAL_CHUNK_SIZE);
                                currentValue.Second.Add(rightElement);
                            }

                            mutables.m_rightHashLookup.Add(rightHashKey, currentValue);
                        }
                        else
                        {
                            if (currentValue.Second == null)
                            {
                                // Lazily allocate a list to hold all but the 1st value. We need to
                                // re-store this element because the pair is a value type.
                                currentValue.Second = new ListChunk<TRightInput>(INITIAL_CHUNK_SIZE);
                                mutables.m_rightHashLookup[rightHashKey] = currentValue;
                            }

                            currentValue.Second.Add(rightElement);
#if DEBUG
                            hashKeyCollisions++;
#endif
                        }
                    }
                }

#if DEBUG
                TraceHelpers.TraceInfo("ParallelJoinQueryOperator::MoveNext - built hash table [count = {0}, collisions = {1}]",
                    hashLookupCount, hashKeyCollisions);
#endif
            }

            // PROBE phase: So long as the source has a next element, return the match.
            ListChunk<TRightInput> currentRightChunk = mutables.m_currentRightMatches;
            if (currentRightChunk != null && mutables.m_currentRightMatchesIndex == currentRightChunk.Count)
            {
                currentRightChunk = mutables.m_currentRightMatches = currentRightChunk.Next;
                mutables.m_currentRightMatchesIndex = 0;
            }

            if (mutables.m_currentRightMatches == null)
            {
                // We have to look up the next list of matches in the hash-table.
                Pair<TLeftInput, THashKey> leftPair = default(Pair<TLeftInput, THashKey>);
                TLeftKey leftKey = default(TLeftKey);
                while (m_leftSource.MoveNext(ref leftPair, ref leftKey))
                {
                    if ((mutables.m_outputLoopCount++ & CancellationState.POLL_INTERVAL) == 0)
                        CancellationState.ThrowIfCanceled(m_cancellationToken);

                    // Find the match in the hash table.
                    Pair<TRightInput, ListChunk<TRightInput>> matchValue = default(Pair<TRightInput, ListChunk<TRightInput>>);
                    TLeftInput leftElement = leftPair.First;
                    THashKey leftHashKey = leftPair.Second;

                    // Ignore null keys.
                    if (leftHashKey != null)
                    {
                        if (mutables.m_rightHashLookup.TryGetValue(leftHashKey, ref matchValue))
                        {
                            // We found a new match. For inner joins, we remember the list in case
                            // there are multiple value under this same key -- the next iteration will pick
                            // them up. For outer joins, we will use the list momentarily.
                            if (m_singleResultSelector != null)
                            {
                                mutables.m_currentRightMatches = matchValue.Second;
                                Contract.Assert(mutables.m_currentRightMatches == null || mutables.m_currentRightMatches.Count > 0,
                                                "we were expecting that the list would be either null or empty");
                                mutables.m_currentRightMatchesIndex = 0;

                                // Yield the value.
                                currentElement = m_singleResultSelector(leftElement, matchValue.First);
                                currentKey = leftKey;

                                // If there is a list of matches, remember the left values for next time.
                                if (matchValue.Second != null)
                                {
                                    mutables.m_currentLeft = leftElement;
                                    mutables.m_currentLeftKey = leftKey;
                                }

                                return true;
                            }
                        }
                    }

                    // For outer joins, we always yield a result.
                    if (m_groupResultSelector != null)
                    {
                        // Grab the matches, or create an empty list if there are none.
                        IEnumerable<TRightInput> matches = matchValue.Second;
                        if (matches == null)
                        {
                            matches = ParallelEnumerable.Empty<TRightInput>();
                        }

                        // Generate the current value.
                        currentElement = m_groupResultSelector(leftElement, matches);
                        currentKey = leftKey;
                        return true;
                    }
                }

                // If we've reached the end of the data source, we're done.
                return false;
            }

            // Produce the next element and increment our index within the matches.
            Contract.Assert(m_singleResultSelector != null);
            Contract.Assert(mutables.m_currentRightMatches != null);
            Contract.Assert(0 <= mutables.m_currentRightMatchesIndex && mutables.m_currentRightMatchesIndex < mutables.m_currentRightMatches.Count);

            currentElement = m_singleResultSelector(
                mutables.m_currentLeft, mutables.m_currentRightMatches.m_chunk[mutables.m_currentRightMatchesIndex]);
            currentKey = mutables.m_currentLeftKey;

            mutables.m_currentRightMatchesIndex++;

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            Contract.Assert(m_leftSource != null && m_rightSource != null);
            m_leftSource.Dispose();
            m_rightSource.Dispose();
        }
    }

}
