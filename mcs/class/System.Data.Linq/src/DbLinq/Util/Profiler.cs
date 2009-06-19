using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace DbLinq.Util
{
#if !MONO_STRICT
    public
#endif
    static class Profiler
    {
        [ThreadStatic]
        private static Stopwatch timer = new Stopwatch();
        [ThreadStatic]
        private static long prevTicks;
        [ThreadStatic]
        private static bool profiling;
        [ThreadStatic]
        private static TextWriter log;

        private static TextWriter Log
        {
            get
            {
                if (log == null)
                    log = Console.Out;
                return log;
            }
            set 
            { 
                log = value; 
            }
        }

        [Conditional("DEBUG")]
        public static void Start()
        {
            profiling = true;
            prevTicks = 0;
            timer.Reset();
            timer.Start();
        }

        [Conditional("DEBUG")]
        public static void At(string format, params object[] args)
        {
            if (profiling)
            {
                timer.Stop();
                Log.Write("#AT(time={0:D12}, elapsed={1:D12}) ", timer.ElapsedTicks, timer.ElapsedTicks - prevTicks);
                prevTicks = timer.ElapsedTicks;
                Log.WriteLine(format, args);
                timer.Start();
            }
        }

        [Conditional("DEBUG")]
        public static void Stop()
        {
            profiling = false;
            timer.Stop();
        }
    }
}