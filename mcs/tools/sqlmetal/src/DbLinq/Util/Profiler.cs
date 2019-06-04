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

        private static Stopwatch Timer
        {
            get 
            {
                if (timer == null)
                    timer = new Stopwatch();
                return timer;
            }
        }

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
            Timer.Reset();
            Timer.Start();
        }

        [Conditional("DEBUG")]
        public static void At(string format, params object[] args)
        {
            if (profiling)
            {
                Timer.Stop();
                Log.Write("#AT(time={0:D12}, elapsed={1:D12}) ", Timer.ElapsedTicks, Timer.ElapsedTicks - prevTicks);
                prevTicks = Timer.ElapsedTicks;
                Log.WriteLine(format, args);
                Timer.Start();
            }
        }

        [Conditional("DEBUG")]
        public static void Stop()
        {
            profiling = false;
            Timer.Stop();
        }
    }
}