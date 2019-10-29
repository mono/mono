// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System
{
    public readonly struct GCMemoryInfo
    {
        /// <summary>
        /// High memory load threshold when the last GC occured
        /// </summary>
        public long HighMemoryLoadThresholdBytes { get; }

        /// <summary>
        /// Memory load when the last GC ocurred
        /// </summary>
        public long MemoryLoadBytes { get; }

        /// <summary>
        /// Total available memory for the GC to use when the last GC ocurred.
        ///
        /// If the environment variable COMPlus_GCHeapHardLimit is set,
        /// or "Server.GC.HeapHardLimit" is in runtimeconfig.json, this will come from that.
        /// If the program is run in a container, this will be an implementation-defined fraction of the container's size.
        /// Else, this is the physical memory on the machine that was available for the GC to use when the last GC occurred.
        /// </summary>
        public long TotalAvailableMemoryBytes { get; }

        /// <summary>
        /// The total heap size when the last GC ocurred
        /// </summary>
        public long HeapSizeBytes { get; }

        /// <summary>
        /// The total fragmentation when the last GC ocurred
        /// We define this as the total ammount of memory allocated to the heap,
        /// minus the total amount contained in objects within the heap. This it 
        /// includes both internal and external fragmentation.
        /// </summary>
        public long FragmentedBytes { get; }

        internal GCMemoryInfo(long highMemoryLoadThresholdBytes,
                              long memoryLoadBytes,
                              long totalAvailableMemoryBytes,
                              long heapSizeBytes,
                              long fragmentedBytes)
        {
            HighMemoryLoadThresholdBytes = highMemoryLoadThresholdBytes;
            MemoryLoadBytes = memoryLoadBytes;
            TotalAvailableMemoryBytes = totalAvailableMemoryBytes;
            HeapSizeBytes = heapSizeBytes;
            FragmentedBytes = fragmentedBytes;
        }
    }
}
