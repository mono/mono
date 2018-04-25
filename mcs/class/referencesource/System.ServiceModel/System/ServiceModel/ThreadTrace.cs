//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Security;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    // Enable this code to track which thread operations occur on
#if false
    static class ThreadTrace
    {
        static LocalDataStoreSlot slot;
        static List<ThreadLog> logs;
        static string logFileName;
        static bool isEnabled;
        static long frequency;

        static ThreadTrace()
        {
            logFileName = Environment.GetEnvironmentVariable("ThreadTrace");
            if (logFileName == null)
                logFileName = "";
            isEnabled = logFileName.Length > 0;
            if (isEnabled)
            {
                slot = Thread.AllocateDataSlot();
                logs = new List<ThreadLog>();
                NativeMethods.QueryPerformanceFrequency(out frequency);
                Console.WriteLine("ThreadTrace: enabled");
                new Thread(ThreadProc).Start();
            }
        }

        static bool stopTracing;

        public static void StopTracing()
        {
            stopTracing = true;
        }

        static void ThreadProc()
        {
            while (!stopTracing)
            {
                Thread.Sleep(20000);
                WriteLogFile();
                Console.WriteLine("ThreadTrace: " + logFileName + " saved.");
            }
        }

        static object ThisLock
        {
            get { return logs; }
        }

        public static void Trace(string operation)
        {
            if (isEnabled)
            {
                TraceInternal(operation);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void TraceInternal(string operation)
        {
            long time;
            NativeMethods.QueryPerformanceCounter(out time);
            ThreadLog log = (ThreadLog)Thread.GetData(slot);
            if (log == null)
            {
                Thread currentThread = Thread.CurrentThread;
                log = new ThreadLog(currentThread);
                lock (ThisLock)
                {
                    logs.Add(log);
                }
                Thread.SetData(slot, log);
            }
            log.Append(time, operation);
        }

        static void WriteLogFile()
        {
            Trace("ThreadTrace.Save");
            TextWriter writer = File.CreateText(logFileName);
            using (writer)
            {
                ThreadLogSnapshot[] logSnapshots = new ThreadLogSnapshot[logs.Count];
                writer.Write("Time");
                for (int i = 0; i < logs.Count; i++)
                {
                    logSnapshots[i] = logs[i].GetSnapshot();
                    writer.Write(", Thread ");
                    writer.Write(i.ToString());
                }
                writer.WriteLine();
                writer.Write("(Ms)");
                foreach (ThreadLog log in logs)
                {
                    if (log.IsThreadPoolThread)
                        writer.Write(", (ThreadPool)");
                    else if (log.IsBackgroundThread)
                        writer.Write(", (Background)");
                    else
                        writer.Write(", (Main)");
                }
                writer.WriteLine();
                int[] indices = new int[logs.Count];
                int count = 0;
                for (int i = 0; i < logs.Count; i++)
                    count += logSnapshots[i].Count;
                for (int j = 0; j < count; j++)
                {
                    int earliestIndex = -1;
                    long earliestTime = long.MaxValue;
                    for (int i = 0; i < logs.Count; i++)
                    {
                        ThreadLogSnapshot logSnapshot = logSnapshots[i];
                        int index = indices[i];
                        if (index >= logSnapshot.Count)
                            continue;
                        long time = logSnapshot[index].time;
                        if (time < earliestTime)
                        {
                            earliestIndex = i;
                            earliestTime = time;
                        }
                    }
                    ThreadLogEntry entry = logSnapshots[earliestIndex][indices[earliestIndex]];
                    double timeInMilliseconds = (entry.time * 1000) / (double)frequency;
                    writer.Write(timeInMilliseconds);
                    for (int i = 0; i < logs.Count; i++)
                    {
                        writer.Write(", ");
                        if (i == earliestIndex)
                        {
                            writer.Write('\"');
                            writer.Write(entry.operation);
                            writer.Write('\"');
                        }
                    }
                    writer.WriteLine();
                    indices[earliestIndex]++;
                }
            }
        }

        struct ThreadLogEntry
        {
            public long time;
            public string operation;

            public ThreadLogEntry(long time, string operation)
            {
                this.time = time;
                this.operation = operation;
            }
        }

        class ThreadLogSnapshot
        {
            ThreadLogEntry[] entries;

            public ThreadLogSnapshot(ThreadLogEntry[] entries)
            {
                this.entries = entries;
            }

            public int Count
            {
                get
                {
                    return this.entries.Length;
                }
            }

            public ThreadLogEntry this[int index]
            {
                get
                {
                    return this.entries[index];
                }
            }
        }

        class ThreadLog
        {
            int count;
            ThreadLogEntry[] buffer;
            const int bufferSize = 5000;
            const int maxBuffers = 4096;
            bool isThreadPoolThread;
            bool isBackgroundThread;
            ThreadLogEntry[][] buffers;
            int bufferCount;

            public ThreadLog(Thread thread)
            {
                this.isThreadPoolThread = thread.IsThreadPoolThread;
                this.isBackgroundThread = thread.IsBackground;
                this.buffer = new ThreadLogEntry[bufferSize];
                this.buffers = new ThreadLogEntry[maxBuffers][];
            }

            object ThisLock
            {
                get { return this; }
            }

            public bool IsThreadPoolThread
            {
                get { return this.isThreadPoolThread; }
            }

            public bool IsBackgroundThread
            {
                get { return this.isBackgroundThread; }
            }

            public void Append(long time, string operation)
            {
                if (this.count == bufferSize)
                {
                    lock (ThisLock)
                    {
                        this.buffers[bufferCount++] = this.buffer;
                        this.buffer = new ThreadLogEntry[bufferSize];
                        this.count = 0;
                    }
                }

                this.buffer[this.count++] = new ThreadLogEntry(time, operation);
            }

            public ThreadLogSnapshot GetSnapshot()
            {
                int currentBufferCount;
                int currentCount;
                ThreadLogEntry[] currentBuffer;

                lock (ThisLock)
                {
                    currentBufferCount = this.bufferCount;
                    currentCount = this.count;
                    currentBuffer = this.buffer;
                }

                ThreadLogEntry[] entries = new ThreadLogEntry[currentBufferCount * bufferSize + currentCount];
                int index = 0;
                for (int i = 0; i < currentBufferCount; i++)
                {
                    Array.Copy(buffers[i], 0, entries, index, bufferSize);
                    index += bufferSize;
                }
                Array.Copy(currentBuffer, 0, entries, index, currentCount);
                return new ThreadLogSnapshot(entries);
            }
        }

        [SuppressUnmanagedCodeSecurity]
        static class NativeMethods
        {
            [DllImport("kernel32.dll")]
	    [ResourceExposure(ResourceScope.None)]
            public static extern int QueryPerformanceCounter(out long time);

            [DllImport("kernel32.dll")]
            [ResourceExposure(ResourceScope.None)]
            public static extern int QueryPerformanceFrequency(out long frequency);
        }
    }
#else
    static class ThreadTrace
    {
        public static void Trace(string operation)
        {
        }

        public static void StopTracing()
        {
        }

        public static void Save()
        {
        }
    }
#endif
}
