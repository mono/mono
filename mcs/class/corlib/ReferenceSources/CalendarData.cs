//
// CalendarData.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2015 Xamarin Inc (http://www.xamarin.com)
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

using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System.Globalization
{
	[StructLayout (LayoutKind.Sequential)]
	partial class CalendarData
	{
		public static int nativeGetTwoDigitYearMax (int calID)
		{
			// -1 mean OS does not override default BCL max year
			return -1;
		}

		static bool nativeGetCalendarData (CalendarData data, string localeName, int calendarId)
		{
			// TODO: Convert calendar-id to mono runtime calendar-id when it's used
			return data.fill_calendar_data (localeName.ToLowerInvariant (), calendarId);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern bool fill_calendar_data (string localeName, int datetimeIndex);
	}
}