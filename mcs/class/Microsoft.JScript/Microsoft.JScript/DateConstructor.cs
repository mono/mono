//
// DateConstructor.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

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

namespace Microsoft.JScript {

	public class DateConstructor : ScriptFunction {

		const double SECONDS_PER_MINUTE = 60.0;
		const double HOURS_PER_DAY = 24.0;
		const double MINUTES_PER_HOUR = 60.0;
		const double MINUTES_PER_DAY = HOURS_PER_DAY * MINUTES_PER_HOUR;
		const double SECONDS_PER_DAY = MINUTES_PER_DAY * SECONDS_PER_MINUTE;
		const double MS_PER_SECOND = 1000.0;
		const double MS_PER_MINUTE = SECONDS_PER_MINUTE * MS_PER_SECOND;
		const double MS_PER_DAY = SECONDS_PER_DAY * MS_PER_SECOND;

		internal static DateConstructor Ctr = new DateConstructor ();
		
		internal DateConstructor ()
		{
		}
		
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs)]
		public new DateObject CreateInstance (params Object[] args)
		{
			throw new NotImplementedException ();
		}

		public String Invoke ()
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute(0, JSBuiltin.Date_parse)]
		public static double parse (String str)
		{
			int year = -1;
			int mon = -1;
			int mday = -1;
			int hour = -1;
			int min = -1;
			int sec = -1;
			
			char c = '0';
			char si = '0';
			
			int i = 0;
			int n = -1;
			
			double tzoffset = -1;
			
			char prevc = '0';
			
			int limit = 0;
			
			bool seenplusminus = false;
			
			limit = str.Length;

			while (i < limit) {
				c = str [i];
				i++;
				if (c <= ' ' || c == ',' || c == '-') {
					if (i < limit) {
						si = str [i];
						if (c == '-' && '0' <= si && si <= '9')
							prevc = c;
					}
					continue;
				}
				
				if (c == '(') { /* comments) */
					int depth = 1;
					while (i < limit) {
						c = str [i];
						i++;
						if (c == '(')
							depth++;
						else if (c == ')')
							if (--depth <= 0)
								break;
					}
					continue;
				}

				if ('0' <= c && c <= '9') {
					n = c - '0';
					while (i < limit && '0' <= (c = str [i]) && c <= '9') {
						n = n * 10 + c - '0';
						i++;
					}
					
					/* allow TZA before the year, so
					 * 'Wed Nov 05 21:49:11 GMT-0800 1997'
					 * works */

					/* uses of seenplusminus allow : in TZA, so Java
					 * no-timezone style of GMT+4:30 works
					 */
					if ((prevc == '+' || prevc == '-') /* && year >= 0 */) {
						/* make ':' case below change tzoffset */
						seenplusminus = true;

						/* offset */
						if (n < 24)
							n = n * 60; /* EG. "GMT-3" */
						else
							n = n % 100 + n / 100 * 60; /* eg "GMT-0430" */
						if (prevc == '+') /* plus means east of GMT */
							n = -n;
						if (tzoffset != 0 && tzoffset != -1)
							return Double.NaN;
						tzoffset = n;
					} else if (n >= 70 || (prevc == '/' && mon >= 0 && mday >= 0 && year < 0)) {
						if (year >= 0)
							return Double.NaN;
						else if (c <= ' ' || c == ',' || c == '/' || i >= limit)
							year = n < 100 ? n + 1900 : n;
						else
							return Double.NaN;
					} else if (c == ':') {
						if (hour < 0)
							hour = /* byte */ n;
						else if (min < 0)
							min = /* byte */ n;
						else
							return Double.NaN;
					} else if (c == '/') {
						if (mon < 0)
							mon = /* byte */ n - 1;
						else if (mday < 0)
							mday = /* byte */ n;
						else
							return Double.NaN;
					} else if (i < limit &&  c != ',' && c > ' ' && c != '-')
						return Double.NaN;
					else if (seenplusminus && n < 60) { /* handle GMT-3:30 */
						if (tzoffset < 0)
							tzoffset -= n;
						else
							tzoffset += n;
					} else if (hour >= 0 && min < 0)
						min = /* byte */ n;
					else if (min >= 0 && sec < 0)
						sec = /* byte */ n;
					else if (mday < 0)
						mday = /* byte */ n;
					else 
						return Double.NaN;
					prevc = '0';
				} else if (c == '/' || c == ':' || c == '+' || c == '-')
					prevc = c;
				else {
					int st = i - 1;
					while (i < limit) {
						c = str [i];
						if (!(('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z')))
							break;
						i++;
					}

					int letterCount = i - st;
					if (letterCount < 2)
						return Double.NaN;

					/*
					 * Use ported code from jsdate.c rather than the locale-specific
					 * date-parsing code from Java, to keep js and rhino consistent.
					 * Is this the right strategy?
					 */
					string wtb = "am;pm;"
						+ "monday;tuesday;wednesday;thursday;friday;"
						+ "saturday;sunday;"
						+ "january;february;march;april;may;june;"
						+ "july;august;september;october;november;december;"
						+ "gmt;ut;utc;est;edt;cst;cdt;mst;mdt;pst;pdt;";

					int index = 0;

					for (int wtbOffset = 0;;) {
						int wtbNext = wtb.IndexOf (';', wtbOffset);

						if (wtbNext < 0)
							return Double.NaN;
						
						if (String.Compare (wtb, wtbOffset, str, st, letterCount, true) == 0)
							break;
						
						wtbOffset = wtbNext + 1;
						++index;
					}

					if (index < 2) {
						/*
						 * AM/PM. Count 12:30 AM as 00:30, 12:30 PM as
						 * 12:30, instead of blindly adding 12 if PM.
						 */
						if  (hour > 12 || hour < 0)
							return Double.NaN;
						else if (index == 0) {
							// AM
							if (hour == 12)
								hour = 0;
						} else {
							// PM
							if (hour != 12)
								hour += 12;
						}
					} else if ((index -= 2) < 7) {
						// ignore week days
					} else if ((index -= 7) < 12) {
						// month
						if (mon < 0)
							mon = index;
						else 
							return Double.NaN;
					} else {
						index -= 12;
						// timezones
						switch (index) {
						case 0 /* gmt */:
							tzoffset = 0; 
							break;

						case 1 /* ut */:
							tzoffset = 0;
							break;
							
						case 2 /* utc */:
							tzoffset = 0;
							break;

						case 3 /* est */:
							tzoffset = 5 * 60;
							break;

						case 4 /* edt */:
							tzoffset = 4 * 60;
							break;

						case 5 /* cst */:
							tzoffset = 6 * 60;
							break;

						case 6 /* cdt */:
							tzoffset = 5 * 60;
							break;

						case 7 /* mst */:
							tzoffset = 7 * 60;
							break;
							
						case 8 /* mdt */:
							tzoffset = 6 * 60;
							break;
							
						case 9 /* pst */:
							tzoffset = 8 * 60;
							break;
							
						case 10 /* pdt */:
							tzoffset = 7 * 60;
							break;
						}
					}
				}
			}

			if (year < 0 || mon < 0 || mday < 0)
				return Double.NaN;
			if (sec < 0)
				sec = 0;
			if (min < 0)
				min = 0;
			if (hour < 0)
				hour = 0;
			
			double msec = msec_from_date (year, mon, mday, hour, min, sec, 0);
			
			if (tzoffset == -1) { // no time zone specified, have to use local 
				Console.WriteLine ("FIXME: no time zone specified.");
				throw new NotImplementedException ();
			} else
				return msec + tzoffset * MS_PER_MINUTE;
		}

		//
		// find UTC time from given date... no 1900 correction!
		//
		static double msec_from_date (double year, double mon, double mday, double hour, double min, double sec, double msec)
		{
			double day, time, result;
			day = MakeDay (year, mon, mday);
			time = MakeTime (hour, min, sec, msec);
			result = MakeDate (day, time);
			return result;
		}

		static double MakeDay (double year, double month, double date)
		{
			year += Math.Floor (month / 12);
			month = month % 12;

			if (month < 0)
				month += 12;
			
			double year_day = Math.Floor (TimeFromYear (year) / MS_PER_DAY);
			double month_day = DayFromMonth ((int) month, (int) year);
			return year_day + month_day + date - 1;
		}

		static double MakeTime (double hour, double min, double sec, double ms)
		{
			return ((hour * MINUTES_PER_HOUR + min) * SECONDS_PER_MINUTE + sec) * MS_PER_SECOND + ms;
		}

		static double MakeDate (double day, double time)
		{
			return day * MS_PER_DAY + time;
		}

		static double TimeFromYear (double y)
		{
			return DayFromYear (y) * MS_PER_DAY;
		}

		static double DayFromYear (double y)
		{
			return ((365 * ((y) - 1970) + Math.Floor (((y) - 1969) / 4.0) - Math.Floor(((y) - 1901) / 100.0) + Math.Floor (((y) - 1601) / 400.0)));
		}

		static double DayFromMonth (int m , int year)
		{
			int day = m * 30;

			if (m >= 7)
				day += m / 2 - 1;
			else if (m >= 2)
				day += (m - 1) / 2 - 1;
			else 
				day += m;

			if (m >= 2 && IsLeapYear (year))
				++day;

			return day;
		}

		static bool IsLeapYear (int year)
		{
			return year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
		}
		
		[JSFunctionAttribute(0, JSBuiltin.Date_UTC)]
		public static double UTC (Object year, Object month, Object date, 
					  Object hours, Object minutes, Object seconds, Object ms)
		{
			throw new NotImplementedException ();
		}
	}
}
