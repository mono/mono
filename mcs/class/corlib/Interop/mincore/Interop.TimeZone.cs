//
// Interop.TimeZone.cs
//
// Author:
//   Taken from CoreRT
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Runtime.InteropServices;

internal static partial class Interop
{
    internal static unsafe partial class mincore
    {
        internal struct SYSTEMTIME
        {
            internal ushort wYear;
            internal ushort wMonth;
            internal ushort wDayOfWeek;
            internal ushort wDay;
            internal ushort wHour;
            internal ushort wMinute;
            internal ushort wSecond;
            internal ushort wMilliseconds;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct TIME_DYNAMIC_ZONE_INFORMATION
        {
            internal int Bias;
            internal fixed char StandardName[32];
            internal SYSTEMTIME StandardDate;
            internal int StandardBias;
            internal fixed char DaylightName[32];
            internal SYSTEMTIME DaylightDate;
            internal int DaylightBias;
            internal fixed char TimeZoneKeyName[128];
            internal byte DynamicDaylightTimeDisabled;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct TIME_ZONE_INFORMATION
        {
            internal int Bias;
            internal fixed char StandardName[32];
            internal SYSTEMTIME StandardDate;
            internal int StandardBias;
            internal fixed char DaylightName[32];
            internal SYSTEMTIME DaylightDate;
            internal int DaylightBias;
        }

        [DllImport("api-ms-win-core-timezone-l1-1-0.dll")]
        internal extern static uint EnumDynamicTimeZoneInformation(uint dwIndex, TIME_DYNAMIC_ZONE_INFORMATION* lpTimeZoneInformation);

        [DllImport("api-ms-win-core-timezone-l1-1-0.dll")]
        internal extern static uint GetDynamicTimeZoneInformation(TIME_DYNAMIC_ZONE_INFORMATION* pTimeZoneInformation);

        [DllImport("api-ms-win-core-timezone-l1-1-0.dll")]
        internal extern static uint GetDynamicTimeZoneInformationEffectiveYears(TIME_DYNAMIC_ZONE_INFORMATION* lpTimeZoneInformation, out uint FirstYear, out uint LastYear);

        [DllImport("api-ms-win-core-timezone-l1-1-0.dll")]
        internal extern static bool GetTimeZoneInformationForYear(ushort wYear, TIME_DYNAMIC_ZONE_INFORMATION* pdtzi, TIME_ZONE_INFORMATION* ptzi);
    }
}
