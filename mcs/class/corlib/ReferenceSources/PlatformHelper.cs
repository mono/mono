using System.Diagnostics.CodeAnalysis;

namespace System.Threading
{
    /// <summary>
    /// A helper class to get the number of processors, it updates the numbers of processors every sampling interval.
    /// </summary>
    internal static class PlatformHelper
    {
        private const int PROCESSOR_COUNT_REFRESH_INTERVAL_MS = 30000; // How often to refresh the count, in milliseconds.
        private static volatile int s_processorCount; // The last count seen.
        private static volatile int s_lastProcessorCountRefreshTicks; // The last time we refreshed.

        /// <summary>
        /// Gets the number of available processors
        /// </summary>
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread safety")]
        internal static int ProcessorCount
        {
            get
            {
                int now = Environment.TickCount;
                int procCount = s_processorCount;
                if (procCount == 0 || (now - s_lastProcessorCountRefreshTicks) >= PROCESSOR_COUNT_REFRESH_INTERVAL_MS)
                {
                    s_processorCount = procCount = Environment.ProcessorCount;
                    s_lastProcessorCountRefreshTicks = now;
                }

                return procCount;
            }
        }

        /// <summary>
        /// Gets whether the current machine has only a single processor.
        /// </summary>
        internal static bool IsSingleProcessor
        {
            get { return ProcessorCount == 1; }
        }
    }
}