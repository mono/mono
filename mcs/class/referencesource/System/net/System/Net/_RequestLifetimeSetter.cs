using System;
using System.Diagnostics;

namespace System.Net
{
    internal class RequestLifetimeSetter
    {
        private long m_RequestStartTimestamp;

        internal RequestLifetimeSetter(long requestStartTimestamp)
        {
            m_RequestStartTimestamp = requestStartTimestamp;
        }

        internal static void Report(RequestLifetimeSetter tracker)
        {
            if (tracker != null)
            {
                NetworkingPerfCounters.Instance.IncrementAverage(NetworkingPerfCounterName.HttpWebRequestAvgLifeTime,
                    tracker.m_RequestStartTimestamp);
            }
        }
    }
}
