// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  Stopwatch
**
** Purpose: Implementation for Stopwatch class.
**
** Date:  Nov 27, 2002
**
===========================================================*/

namespace System.Diagnostics 
{
    using Microsoft.Win32;
    using System;
#if FEATURE_NETCORE
    using System.Security;
#endif

    // This class uses high-resolution performance counter if installed hardware 
    // does not support it. Otherwise, the class will fall back to DateTime class
    // and uses ticks as a measurement.

    public class Stopwatch {
        private const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;

        private long elapsed;
        private long startTimeStamp;
        private bool isRunning;

        // "Frequency" stores the frequency of the high-resolution performance counter, 
        // if one exists. Otherwise it will store TicksPerSecond. 
        // The frequency cannot change while the system is running,
        // so we only need to initialize it once. 
        public static readonly long Frequency;
        public static readonly bool IsHighResolution;

        // performance-counter frequency, in counts per ticks.
        // This can speed up conversion from high frequency performance-counter 
        // to ticks. 
        private static readonly double tickFrequency;  

#if FEATURE_NETCORE
        [SecuritySafeCritical]
#endif
        static Stopwatch() {                       
            bool succeeded = SafeNativeMethods.QueryPerformanceFrequency(out Frequency);            
            if(!succeeded) {
                IsHighResolution = false; 
                Frequency = TicksPerSecond;
                tickFrequency = 1;
            }
            else {
                IsHighResolution = true;
                tickFrequency = TicksPerSecond;
                tickFrequency /= Frequency;
            }   
        }

        public Stopwatch() {
            Reset();
        }

        public void Start() {
            // Calling start on a running Stopwatch is a no-op.
            if(!isRunning) {
                startTimeStamp = GetTimestamp();                 
                isRunning = true;
            }
        }

        public static Stopwatch StartNew() {
            Stopwatch s = new Stopwatch();
            s.Start();
            return s;
        }

        public void Stop() {
            // Calling stop on a stopped Stopwatch is a no-op.
            if( isRunning) {
                long endTimeStamp = GetTimestamp();                 
                long elapsedThisPeriod = endTimeStamp - startTimeStamp;
                elapsed += elapsedThisPeriod;
                isRunning = false;

                if (elapsed < 0) {
                    // When measuring small time periods the StopWatch.Elapsed* 
                    // properties can return negative values.  This is due to 
                    // bugs in the basic input/output system (BIOS) or the hardware
                    // abstraction layer (HAL) on machines with variable-speed CPUs
                    // (e.g. Intel SpeedStep).

                    elapsed = 0;
                }
            }
        }

        public void Reset() {
            elapsed = 0;
            isRunning = false;
            startTimeStamp = 0;
        }

        // Convenience method for replacing {sw.Reset(); sw.Start();} with a single sw.Restart()
        public void Restart() {
            elapsed = 0;
            startTimeStamp = GetTimestamp();
            isRunning = true;
        }

        public bool IsRunning { 
            get { return isRunning; }
        }

        public TimeSpan Elapsed {
            get { return new TimeSpan( GetElapsedDateTimeTicks()); }
        }
        
        public long ElapsedMilliseconds { 
            get { return GetElapsedDateTimeTicks()/TicksPerMillisecond; }    
        }  
        
        public long ElapsedTicks { 
            get { return GetRawElapsedTicks(); }
        }

#if FEATURE_NETCORE
        [SecuritySafeCritical]
#endif
        public static long GetTimestamp() {
            if(IsHighResolution) {
                long timestamp = 0;    
                SafeNativeMethods.QueryPerformanceCounter(out timestamp);
                return timestamp;
            }
            else {
                return DateTime.UtcNow.Ticks;
            }   
        }

        // Get the elapsed ticks.        
#if FEATURE_NETCORE
        public long GetRawElapsedTicks() {
#else
        private long GetRawElapsedTicks() {
#endif
            long timeElapsed = elapsed;

            if( isRunning) {
                // If the StopWatch is running, add elapsed time since
                // the Stopwatch is started last time. 
                long currentTimeStamp = GetTimestamp();                 
                long elapsedUntilNow = currentTimeStamp - startTimeStamp;
                timeElapsed += elapsedUntilNow;
            }
            return timeElapsed;
        }   

        // Get the elapsed ticks.        
#if FEATURE_NETCORE
        public long GetElapsedDateTimeTicks() {
#else
        private long GetElapsedDateTimeTicks() {
#endif
            long rawTicks = GetRawElapsedTicks();
            if( IsHighResolution) {
                // convert high resolution perf counter to DateTime ticks
                double dticks = rawTicks;
                dticks *= tickFrequency;
                return unchecked((long)dticks);                        
            }
            else {
                return rawTicks;
            }
        }   

    }
}
