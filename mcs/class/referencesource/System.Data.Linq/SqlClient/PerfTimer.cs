using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace System.Data.Linq.SqlClient {
#if PERFORMANCE_BUILD
    class PerfTimer {
        long startTime;
        long stopTime;
        long frequency;

        public PerfTimer() {
            QueryPerformanceFrequency(out frequency);
        }

        public void Start() {
            Thread.Sleep(0);
            QueryPerformanceCounter(out startTime);
        }

        public void Stop() {
            QueryPerformanceCounter(out stopTime);
        }

        public long Duration {
            get { return (long)( 1000000.0 * (double)(stopTime - startTime) / (double) frequency ); }
        }

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long count);

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long frequency);
    }
#endif
}
