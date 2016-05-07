// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// PartitionerQueryOperator.cs
//
// <OWNER>[....]</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Linq.Parallel;
using System.Diagnostics.Contracts;
using System.Threading;
#if SILVERLIGHT
using System.Core; // for System.Core.SR
#endif

namespace System.Linq.Parallel
{
    /// <summary>
    /// A QueryOperator that represents the output of the query partitioner.AsParallel().
    /// </summary>
    internal class PartitionerQueryOperator<TElement> : QueryOperator<TElement>
    {
        private Partitioner<TElement> m_partitioner; // The partitioner to use as data source.

        internal PartitionerQueryOperator(Partitioner<TElement> partitioner)
            : base(false, QuerySettings.Empty)
        {
            m_partitioner = partitioner;
        }

        internal bool Orderable
        {
            get { return m_partitioner is OrderablePartitioner<TElement>; }
        }

        internal override QueryResults<TElement> Open(QuerySettings settings, bool preferStriping)
        {
            // Notice that the preferStriping argument is not used. Partitioner<T> does not support
            // striped partitioning.

            return new PartitionerQueryOperatorResults(m_partitioner, settings);
        }

        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TElement> AsSequentialQuery(CancellationToken token)
        {
            using (IEnumerator<TElement> enumerator = m_partitioner.GetPartitions(1)[0])
            {
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
        }

        //---------------------------------------------------------------------------------------
        // The state of the order index of the results returned by this operator.
        //

        internal override OrdinalIndexState OrdinalIndexState
        {
            get { return GetOrdinalIndexState(m_partitioner); }
        }

        /// <summary>
        /// Determines the OrdinalIndexState for a partitioner 
        /// </summary>
        internal static OrdinalIndexState GetOrdinalIndexState(Partitioner<TElement> partitioner)
        {
            OrderablePartitioner<TElement> orderablePartitioner = partitioner as OrderablePartitioner<TElement>;

            if (orderablePartitioner == null)
            {
                return OrdinalIndexState.Shuffled;
            }

            if (orderablePartitioner.KeysOrderedInEachPartition)
            {
                if (orderablePartitioner.KeysNormalized)
                {
                    return OrdinalIndexState.Correct;
                }
                else
                {
                    return OrdinalIndexState.Increasing;
                }
            }
            else
            {
                return OrdinalIndexState.Shuffled;
            }
        }


        //---------------------------------------------------------------------------------------
        // Whether this operator performs a premature merge that would not be performed in
        // a similar sequential operation (i.e., in LINQ to Objects).
        //

        internal override bool LimitsParallelism
        {
            get { return false; }
        }


        /// <summary>
        /// QueryResults for a PartitionerQueryOperator
        /// </summary>
        private class PartitionerQueryOperatorResults : QueryResults<TElement>
        {
            private Partitioner<TElement> m_partitioner; // The data source for the query

            private QuerySettings m_settings; // Settings collected from the query

            internal PartitionerQueryOperatorResults(Partitioner<TElement> partitioner, QuerySettings settings)
            {
                m_partitioner = partitioner;
                m_settings = settings;
            }

            internal override void GivePartitionedStream(IPartitionedStreamRecipient<TElement> recipient)
            {
                Contract.Assert(m_settings.DegreeOfParallelism.HasValue);
                int partitionCount = m_settings.DegreeOfParallelism.Value;

                OrderablePartitioner<TElement> orderablePartitioner = m_partitioner as OrderablePartitioner<TElement>;

                // If the partitioner is not orderable, it will yield zeros as order keys. The order index state
                // is irrelevant.
                OrdinalIndexState indexState = (orderablePartitioner != null)
                    ? GetOrdinalIndexState(orderablePartitioner)
                    : OrdinalIndexState.Shuffled;

                PartitionedStream<TElement, int> partitions = new PartitionedStream<TElement, int>(
                    partitionCount,
                    Util.GetDefaultComparer<int>(),
                    indexState);

                if (orderablePartitioner != null)
                {
                    IList<IEnumerator<KeyValuePair<long, TElement>>> partitionerPartitions =
                        orderablePartitioner.GetOrderablePartitions(partitionCount);

                    if (partitionerPartitions == null)
                    {
                        throw new InvalidOperationException(SR.GetString(SR.PartitionerQueryOperator_NullPartitionList));
                    }

                    if (partitionerPartitions.Count != partitionCount)
                    {
                        throw new InvalidOperationException(SR.GetString(SR.PartitionerQueryOperator_WrongNumberOfPartitions));
                    }

                    for (int i = 0; i < partitionCount; i++)
                    {
                        IEnumerator<KeyValuePair<long, TElement>> partition = partitionerPartitions[i];
                        if (partition == null)
                        {
                            throw new InvalidOperationException(SR.GetString(SR.PartitionerQueryOperator_NullPartition));
                        }

                        partitions[i] = new OrderablePartitionerEnumerator(partition);
                    }
                }
                else
                {
                    IList<IEnumerator<TElement>> partitionerPartitions =
                        m_partitioner.GetPartitions(partitionCount);

                    if (partitionerPartitions == null)
                    {
                        throw new InvalidOperationException(SR.GetString(SR.PartitionerQueryOperator_NullPartitionList));
                    }

                    if (partitionerPartitions.Count != partitionCount)
                    {
                        throw new InvalidOperationException(SR.GetString(SR.PartitionerQueryOperator_WrongNumberOfPartitions));
                    }

                    for (int i = 0; i < partitionCount; i++)
                    {
                        IEnumerator<TElement> partition = partitionerPartitions[i];
                        if (partition == null)
                        {
                            throw new InvalidOperationException(SR.GetString(SR.PartitionerQueryOperator_NullPartition));
                        }

                        partitions[i] = new PartitionerEnumerator(partition);
                    }
                }

                recipient.Receive<int>(partitions);
            }

        }

        /// <summary>
        /// Enumerator that converts an enumerator over key-value pairs exposed by a partitioner
        /// to a QueryOperatorEnumerator used by PLINQ internally.
        /// </summary>
        private class OrderablePartitionerEnumerator : QueryOperatorEnumerator<TElement, int>
        {
            private IEnumerator<KeyValuePair<long, TElement>> m_sourceEnumerator;

            internal OrderablePartitionerEnumerator(IEnumerator<KeyValuePair<long, TElement>> sourceEnumerator)
            {
                m_sourceEnumerator = sourceEnumerator;
            }

            internal override bool MoveNext(ref TElement currentElement, ref int currentKey)
            {
                if (!m_sourceEnumerator.MoveNext()) return false;

                KeyValuePair<long, TElement> current = m_sourceEnumerator.Current;
                currentElement = current.Value;

                checked
                {
                    currentKey = (int)current.Key;
                }

                return true;
            }

            protected override void Dispose(bool disposing)
            {
                Contract.Assert(m_sourceEnumerator != null);
                m_sourceEnumerator.Dispose();
            }
        }

        /// <summary>
        /// Enumerator that converts an enumerator over key-value pairs exposed by a partitioner
        /// to a QueryOperatorEnumerator used by PLINQ internally.
        /// </summary>
        private class PartitionerEnumerator : QueryOperatorEnumerator<TElement, int>
        {
            private IEnumerator<TElement> m_sourceEnumerator;

            internal PartitionerEnumerator(IEnumerator<TElement> sourceEnumerator)
            {
                m_sourceEnumerator = sourceEnumerator;
            }

            internal override bool MoveNext(ref TElement currentElement, ref int currentKey)
            {
                if (!m_sourceEnumerator.MoveNext()) return false;

                currentElement = m_sourceEnumerator.Current;
                currentKey = 0;

                return true;
            }

            protected override void Dispose(bool disposing)
            {
                Contract.Assert(m_sourceEnumerator != null);
                m_sourceEnumerator.Dispose();
            }
        }
    }

}
