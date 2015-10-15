// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// HashPartitionedStream.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;

namespace System.Linq.Parallel
{
    /// <summary>
    /// A repartitioning stream must take input data that has already been partitioned and
    /// redistribute its contents based on a new partitioning algorithm. This is accomplished
    /// by making each partition p responsible for redistributing its input data to the
    /// correct destination partition. Some input elements may remain in p, but many will now
    /// belong to a different partition and will need to move. This requires a great deal of
    /// synchronization, but allows threads to repartition data incrementally and in parallel.
    /// Each partition will "pull" data on-demand instead of partitions "pushing" data, which
    /// allows us to reduce some amount of synchronization overhead.
    ///
    /// We currently only offer one form of reparitioning via hashing.  This used to be an
    /// abstract base class, but we have eliminated that to get rid of some virtual calls on
    /// hot code paths.  Uses a key selection algorithm with mod'ding to determine destination.
    ///
    /// </summary>
    /// <typeparam name="TInputOutput"></typeparam>
    /// <typeparam name="THashKey"></typeparam>
    /// <typeparam name="TOrderKey"></typeparam>
    internal abstract class HashRepartitionStream<TInputOutput, THashKey, TOrderKey> : PartitionedStream<Pair<TInputOutput, THashKey>, TOrderKey>
    {
        private readonly IEqualityComparer<THashKey> m_keyComparer; // The optional key comparison routine.
        private readonly IEqualityComparer<TInputOutput> m_elementComparer; // The optional element comparison routine.
        private readonly int m_distributionMod; // The distribution value we'll use to scramble input.

        //---------------------------------------------------------------------------------------
        // Creates a new partition exchange operator.
        //

        internal HashRepartitionStream(
            int partitionsCount, IComparer<TOrderKey> orderKeyComparer, IEqualityComparer<THashKey> hashKeyComparer, 
            IEqualityComparer<TInputOutput> elementComparer)
            : base(partitionsCount, orderKeyComparer, OrdinalIndexState.Shuffled)
        {
            // elementComparer is used by operators that use elements themselves as the hash keys.
            // When elements are used as keys, THashKey should be NoKeyMemoizationRequired.
            m_keyComparer = hashKeyComparer;
            m_elementComparer = elementComparer;

            Contract.Assert(m_keyComparer == null || m_elementComparer == null);
            Contract.Assert(m_elementComparer == null || typeof(THashKey) == typeof(NoKeyMemoizationRequired));

            // We use the following constant when distributing hash-codes into partition streams.
            // It's an (arbitrary) prime number to account for poor hashing functions, e.g. those
            // that all the primitive types use (e.g. Int32 returns itself). The goal is to add some
            // degree of randomization to avoid predictable distributions for certain data sequences,
            // for the same reason prime numbers are frequently used in hashtable implementations.
            // For instance, if all hash-codes end up as even, we would starve half of the partitions
            // by just using the raw hash-code. This isn't perfect, of course, since a stream
            // of integers with the same value end up in the same partition, but helps.
            const int DEFAULT_HASH_MOD_DISTRIBUTION = 503;

            // We need to guarantee our distribution mod is greater than the number of partitions.
            m_distributionMod = DEFAULT_HASH_MOD_DISTRIBUTION;
            while (m_distributionMod < partitionsCount)
            {
                // We use checked arithmetic here.  We'll only overflow for huge numbers of partitions
                // (quite unlikely), so the remote possibility of an exception is fine.
                checked
                {
                    m_distributionMod *= 2;
                }
            }
        }

        //---------------------------------------------------------------------------------------
        // Manufactures a hash code for a given value or key.
        //

        // The hash-code used for null elements.
        private const int NULL_ELEMENT_HASH_CODE = 0;

        internal int GetHashCode(TInputOutput element)
        {
            return
                (0x7fffffff &
                    (m_elementComparer == null ? 
                        (element == null ? NULL_ELEMENT_HASH_CODE : element.GetHashCode()) :
                        m_elementComparer.GetHashCode(element)))
                        % m_distributionMod;
        }

        internal int GetHashCode(THashKey key)
        {
            return
                (0x7fffffff &
                    (m_keyComparer == null ?
                        (key == null ? NULL_ELEMENT_HASH_CODE : key.GetHashCode()) :
                        m_keyComparer.GetHashCode(key))) % m_distributionMod;
        }
    }

}
