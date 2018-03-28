//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.Runtime
{
    using System.Security;
    using System.Runtime.Interop;

    static class Ticks
    {
        public static long Now
        {
            [Fx.Tag.SecurityNote(Miscellaneous = "Why isn't the SuppressUnmanagedCodeSecurity attribute working in this case?")]
            [SecuritySafeCritical]
            get
            {
                long time;
#pragma warning disable 1634
#pragma warning suppress 56523 // function has no error return value
#pragma warning restore 1634
                UnsafeNativeMethods.GetSystemTimeAsFileTime(out time);
                return time;
            }
        }

        public static long FromMilliseconds(int milliseconds)
        {
            return checked((long)milliseconds * TimeSpan.TicksPerMillisecond);
        }

        public static int ToMilliseconds(long ticks)
        {
            return checked((int)(ticks / TimeSpan.TicksPerMillisecond));
        }

        public static long FromTimeSpan(TimeSpan duration)
        {
            return duration.Ticks;
        }

        public static TimeSpan ToTimeSpan(long ticks)
        {
            return new TimeSpan(ticks);
        }

        public static long Add(long firstTicks, long secondTicks)
        {
            if (firstTicks == long.MaxValue || firstTicks == long.MinValue)
            {
                return firstTicks;
            }
            if (secondTicks == long.MaxValue || secondTicks == long.MinValue)
            {
                return secondTicks;
            }
            if (firstTicks >= 0 && long.MaxValue - firstTicks <= secondTicks)
            {
                return long.MaxValue - 1;
            }
            if (firstTicks <= 0 && long.MinValue - firstTicks >= secondTicks)
            {
                return long.MinValue + 1;
            }
            return checked(firstTicks + secondTicks);
        }
    }
}
