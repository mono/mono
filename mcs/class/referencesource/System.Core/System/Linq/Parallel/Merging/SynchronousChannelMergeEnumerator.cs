// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// SynchronousChannelMergeEnumerator.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Diagnostics.Contracts;
#if SILVERLIGHT
using System.Core; // for System.Core.SR
#endif

namespace System.Linq.Parallel
{
    /// <summary>
    /// This enumerator merges multiple input channels into a single output stream. The merging process just
    /// goes from left-to-right, enumerating each channel in succession in its entirety.
    /// Assumptions:
    ///     Before enumerating this object, all producers for all channels must have finished enqueueing new
    ///     elements.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class SynchronousChannelMergeEnumerator<T> : MergeEnumerator<T>
    {
        private SynchronousChannel<T>[] m_channels; // The channel array we will enumerate, from left-to-right.
        private int m_channelIndex; // The current channel index. This moves through the array as we enumerate.
        private T m_currentElement; // The last element remembered during enumeration.

        //-----------------------------------------------------------------------------------
        // Instantiates a new enumerator for a set of channels.
        //

        internal SynchronousChannelMergeEnumerator(
            QueryTaskGroupState taskGroupState, SynchronousChannel<T>[] channels) : base(taskGroupState)
        {
            Contract.Assert(channels != null);
#if DEBUG
            foreach (SynchronousChannel<T> c in channels) Contract.Assert(c != null);
#endif

            m_channels = channels;
            m_channelIndex = -1;
        }

        //-----------------------------------------------------------------------------------
        // Retrieves the current element.
        //
        // Notes:
        //     This throws if we haven't begun enumerating or have gone past the end of the
        //     data source.
        //

        public override T Current
        {
            get
            {
                // If we're at the beginning or the end of the array, it's invalid to be
                // retrieving the current element. We throw.
                if (m_channelIndex == -1 || m_channelIndex == m_channels.Length)
                {
                    throw new InvalidOperationException(SR.GetString(SR.PLINQ_CommonEnumerator_Current_NotStarted));
                }

                return m_currentElement;
            }
        }

        //-----------------------------------------------------------------------------------
        // Positions the enumerator over the next element. This includes merging as we
        // enumerate, by just incrementing indexes, etc.
        //
        // Return Value:
        //     True if there's a current element, false if we've reached the end.
        //

        public override bool MoveNext()
        {
            Contract.Assert(m_channels != null);

            // If we're at the start, initialize the index.
            if (m_channelIndex == -1)
            {
                m_channelIndex = 0;
            }

            // If the index has reached the end, we bail.
            while (m_channelIndex != m_channels.Length)
            {
                SynchronousChannel<T> current = m_channels[m_channelIndex];
                Contract.Assert(current != null);

                if (current.Count == 0)
                {
                    // We're done with this channel, move on to the next one. We don't
                    // have to check that it's "done" since this is a synchronous consumer.
                    m_channelIndex++;
                }
                else
                {
                    // Remember the "current" element and return.
                    m_currentElement = current.Dequeue();
                    return true;
                }
            }

            TraceHelpers.TraceInfo("[timing]: {0}: Completed the merge", DateTime.Now.Ticks);

            // If we got this far, it means we've exhausted our channels.
            Contract.Assert(m_channelIndex == m_channels.Length);

            return false;
        }
    }
}
