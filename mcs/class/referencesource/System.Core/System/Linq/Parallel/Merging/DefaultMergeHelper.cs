// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// DefaultMergeHelper.cs
//
// <OWNER>[....]</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace System.Linq.Parallel
{
    /// <summary>
    /// The default merge helper uses a set of straightforward algorithms for output
    /// merging. Namely, for synchronous merges, the input data is yielded from the
    /// input data streams in "depth first" left-to-right order. For asynchronous merges,
    /// on the other hand, we use a biased choice algorithm to favor input channels in
    /// a "fair" way. No order preservation is carried out by this helper. 
    /// </summary>
    /// <typeparam name="TInputOutput"></typeparam>
    /// <typeparam name="TIgnoreKey"></typeparam>
    internal class DefaultMergeHelper<TInputOutput, TIgnoreKey> : IMergeHelper<TInputOutput>
    {
        private QueryTaskGroupState m_taskGroupState; // State shared among tasks.
        private PartitionedStream<TInputOutput, TIgnoreKey> m_partitions; // Source partitions.
        private AsynchronousChannel<TInputOutput>[] m_asyncChannels; // Destination channels (async).
        private SynchronousChannel<TInputOutput>[] m_syncChannels; // Destination channels ([....]).
        private IEnumerator<TInputOutput> m_channelEnumerator; // Output enumerator.
        private TaskScheduler m_taskScheduler; // The task manager to execute the query.
        private bool m_ignoreOutput; // Whether we're enumerating "for effect".

        //-----------------------------------------------------------------------------------
        // Instantiates a new merge helper.
        //
        // Arguments:
        //     partitions   - the source partitions from which to consume data.
        //     ignoreOutput - whether we're enumerating "for effect" or for output.
        //     pipeline     - whether to use a pipelined merge.
        //

        internal DefaultMergeHelper(PartitionedStream<TInputOutput, TIgnoreKey> partitions, bool ignoreOutput, ParallelMergeOptions options, 
            TaskScheduler taskScheduler, CancellationState cancellationState, int queryId)
        {
            Contract.Assert(partitions != null);

            m_taskGroupState = new QueryTaskGroupState(cancellationState, queryId);
            m_partitions = partitions;
            m_taskScheduler = taskScheduler;
            m_ignoreOutput = ignoreOutput;
            IntValueEvent consumerEvent = new IntValueEvent();

            TraceHelpers.TraceInfo("DefaultMergeHelper::.ctor(..): creating a default merge helper");

            // If output won't be ignored, we need to manufacture a set of channels for the consumer.
            // Otherwise, when the merge is executed, we'll just invoke the activities themselves.
            if (!ignoreOutput)
            {
                // Create the asynchronous or synchronous channels, based on whether we're pipelining.
                if (options != ParallelMergeOptions.FullyBuffered)
                {
                    if (partitions.PartitionCount > 1)
                    {
                        m_asyncChannels =
                            MergeExecutor<TInputOutput>.MakeAsynchronousChannels(partitions.PartitionCount, options, consumerEvent, cancellationState.MergedCancellationToken);
                        m_channelEnumerator = new AsynchronousChannelMergeEnumerator<TInputOutput>(m_taskGroupState, m_asyncChannels, consumerEvent);
                    }
                    else
                    {
                        // If there is only one partition, we don't need to create channels. The only producer enumerator
                        // will be used as the result enumerator.
                        m_channelEnumerator = ExceptionAggregator.WrapQueryEnumerator(partitions[0], m_taskGroupState.CancellationState).GetEnumerator();
                    }
                }
                else
                {
                    m_syncChannels =
                        MergeExecutor<TInputOutput>.MakeSynchronousChannels(partitions.PartitionCount);
                    m_channelEnumerator = new SynchronousChannelMergeEnumerator<TInputOutput>(m_taskGroupState, m_syncChannels);
                }

                Contract.Assert(m_asyncChannels == null || m_asyncChannels.Length == partitions.PartitionCount);
                Contract.Assert(m_syncChannels == null || m_syncChannels.Length == partitions.PartitionCount);
                Contract.Assert(m_channelEnumerator != null, "enumerator can't be null if we're not ignoring output");
            }
        }

        //-----------------------------------------------------------------------------------
        // Schedules execution of the merge itself.
        //
        // Arguments:
        //    ordinalIndexState - the state of the ordinal index of the merged partitions
        //

        void IMergeHelper<TInputOutput>.Execute()
        {
            if (m_asyncChannels != null)
            {
                SpoolingTask.SpoolPipeline<TInputOutput, TIgnoreKey>(m_taskGroupState, m_partitions, m_asyncChannels, m_taskScheduler);
            }
            else if (m_syncChannels != null)
            {
                SpoolingTask.SpoolStopAndGo<TInputOutput, TIgnoreKey>(m_taskGroupState, m_partitions, m_syncChannels, m_taskScheduler);
            }
            else if (m_ignoreOutput)
            {
                SpoolingTask.SpoolForAll<TInputOutput, TIgnoreKey>(m_taskGroupState, m_partitions, m_taskScheduler);
            }
            else
            {
                // The last case is a pipelining merge when DOP = 1. In this case, the consumer thread itself will compute the results,
                // so we don't need any tasks to compute the results asynchronously.
                Contract.Assert(m_partitions.PartitionCount == 1);
            }
        }

        //-----------------------------------------------------------------------------------
        // Gets the enumerator from which to enumerate output results.
        //

        IEnumerator<TInputOutput> IMergeHelper<TInputOutput>.GetEnumerator()
        {
            Contract.Assert(m_ignoreOutput || m_channelEnumerator != null);
            return m_channelEnumerator;
        }

        //-----------------------------------------------------------------------------------
        // Returns the results as an array.
        //
        // @





        public TInputOutput[] GetResultsAsArray()
        {
            if (m_syncChannels != null)
            {
                // Right size an array.
                int totalSize = 0;
                for (int i = 0; i < m_syncChannels.Length; i++)
                {
                    totalSize += m_syncChannels[i].Count;
                }
                TInputOutput[] array = new TInputOutput[totalSize];

                // And then blit the elements in.
                int current = 0;
                for (int i = 0; i < m_syncChannels.Length; i++)
                {
                    m_syncChannels[i].CopyTo(array, current);
                    current += m_syncChannels[i].Count;
                }
                return array;
            }
            else
            {
                List<TInputOutput> output = new List<TInputOutput>();
                using (IEnumerator<TInputOutput> enumerator = ((IMergeHelper<TInputOutput>)this).GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        output.Add(enumerator.Current);
                    }
                }

                return output.ToArray();
            }            
        }
    }
}
