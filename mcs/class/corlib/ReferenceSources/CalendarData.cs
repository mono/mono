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

namespace System.Globalization
{
	class CalendarData
	{
		internal int iCurrentEra;

		private CalendarData (int calendarId)
		{
			// Japanese calendar is the only calendar with > 1 era. Its current era value
			// is total eras count in erras array
			if (calendarId == Calendar.CAL_JAPAN) {
				iCurrentEra = 4;
			} else {
				iCurrentEra = 1;
			}
		}

		public static int nativeGetTwoDigitYearMax (int calID)
		{
			// -1 mean OS does not override default BCL max year
			return -1;
		}

		internal static CalendarData GetCalendarData (int calendarId)
		{
			// calendarID is any of CAL_ constants from calendar.cs
			return new CalendarData (calendarId);
		}
	}
}