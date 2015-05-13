//
// System.Management.AuthenticationLevel
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
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
using System.Globalization;

namespace System.Management
{
	public sealed class ManagementDateTimeConverter
	{
		private const int SIZEOFDMTFDATETIME = 25;

		private const int MAXSIZE_UTC_DMTF = 0x3e7;

		private const long MAXDATE_INTIMESPAN = 0x5f5e0ffL;

		private ManagementDateTimeConverter()
		{
		}

		public static DateTime ToDateTime(string dmtfDate)
		{
			DateTime minValue = DateTime.MinValue;
			int year = minValue.Year;
			DateTime dateTime = DateTime.MinValue;
			int month = dateTime.Month;
			DateTime minValue1 = DateTime.MinValue;
			int day = minValue1.Day;
			DateTime dateTime1 = DateTime.MinValue;
			int hour = dateTime1.Hour;
			DateTime minValue2 = DateTime.MinValue;
			int minute = minValue2.Minute;
			DateTime dateTime2 = DateTime.MinValue;
			int second = dateTime2.Second;
			int num = 0;
			string str = dmtfDate;
			if (str != null)
			{
				if (str.Length != 0)
				{
					if (str.Length == 25)
					{
						IFormatProvider format = (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int));
						long num1 = (long)0;
						try
						{
							string str1 = str.Substring(0, 4);
							if ("****" != str1)
							{
								year = int.Parse(str1, format);
							}
							str1 = str.Substring(4, 2);
							if ("**" != str1)
							{
								month = int.Parse(str1, format);
							}
							str1 = str.Substring(6, 2);
							if ("**" != str1)
							{
								day = int.Parse(str1, format);
							}
							str1 = str.Substring(8, 2);
							if ("**" != str1)
							{
								hour = int.Parse(str1, format);
							}
							str1 = str.Substring(10, 2);
							if ("**" != str1)
							{
								minute = int.Parse(str1, format);
							}
							str1 = str.Substring(12, 2);
							if ("**" != str1)
							{
								second = int.Parse(str1, format);
							}
							str1 = str.Substring(15, 6);
							if ("******" != str1)
							{
								num1 = long.Parse(str1, (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(long))) * (long)10;
							}
							if (year < 0 || month < 0 || day < 0 || hour < 0 || minute < 0 || second < 0 || num1 < (long)0)
							{
								throw new ArgumentOutOfRangeException("dmtfDate");
							}
						}
						catch
						{
							throw new ArgumentOutOfRangeException("dmtfDate");
						}
						DateTime dateTime3 = new DateTime(year, month, day, hour, minute, second, num);
						dateTime3 = dateTime3.AddTicks(num1);
						TimeZone currentTimeZone = TimeZone.CurrentTimeZone;
						TimeSpan utcOffset = currentTimeZone.GetUtcOffset(dateTime3);
						long ticks = utcOffset.Ticks / (long)0x23c34600;
						int num2 = 0;
						string str2 = str.Substring(22, 3);
						if ("***" != str2)
						{
							str2 = str.Substring(21, 4);
							try
							{
								num2 = int.Parse(str2, format);
							}
							catch
							{
								throw new ArgumentOutOfRangeException();
							}
							long num3 = (long)num2 - ticks;
							dateTime3 = dateTime3.AddMinutes((double)(num3 * (long)-1));
						}
						return dateTime3;
					}
					else
					{
						throw new ArgumentOutOfRangeException("dmtfDate");
					}
				}
				else
				{
					throw new ArgumentOutOfRangeException("dmtfDate");
				}
			}
			else
			{
				throw new ArgumentOutOfRangeException("dmtfDate");
			}
		}

		public static string ToDmtfDateTime(DateTime date)
		{
			string str;
			TimeZone currentTimeZone = TimeZone.CurrentTimeZone;
			TimeSpan utcOffset = currentTimeZone.GetUtcOffset(date);
			long ticks = utcOffset.Ticks / (long)0x23c34600;
			IFormatProvider format = (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int));
			if (Math.Abs(ticks) <= (long)0x3e7)
			{
				if (utcOffset.Ticks < (long)0)
				{
					string str1 = ticks.ToString(format);
					str = string.Concat("-", str1.Substring(1, str1.Length - 1).PadLeft(3, '0'));
				}
				else
				{
					long num = utcOffset.Ticks / (long)0x23c34600;
					str = string.Concat("+", num.ToString(format).PadLeft(3, '0'));
				}
			}
			else
			{
				date = date.ToUniversalTime();
				str = "+000";
			}
			int year = date.Year;
			string str2 = year.ToString(format).PadLeft(4, '0');
			int month = date.Month;
			str2 = string.Concat(str2, month.ToString(format).PadLeft(2, '0'));
			int day = date.Day;
			str2 = string.Concat(str2, day.ToString(format).PadLeft(2, '0'));
			int hour = date.Hour;
			str2 = string.Concat(str2, hour.ToString(format).PadLeft(2, '0'));
			int minute = date.Minute;
			str2 = string.Concat(str2, minute.ToString(format).PadLeft(2, '0'));
			int second = date.Second;
			str2 = string.Concat(str2, second.ToString(format).PadLeft(2, '0'));
			str2 = string.Concat(str2, ".");
			DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, 0);
			long ticks1 = (date.Ticks - dateTime.Ticks) * (long)0x3e8 / (long)0x2710;
			string str3 = ticks1.ToString((IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(long)));
			if (str3.Length > 6)
			{
				str3 = str3.Substring(0, 6);
			}
			str2 = string.Concat(str2, str3.PadLeft(6, '0'));
			str2 = string.Concat(str2, str);
			return str2;
		}

		public static string ToDmtfTimeInterval(TimeSpan timespan)
		{
			int days = timespan.Days;
			string str = days.ToString((IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int))).PadLeft(8, '0');
			IFormatProvider format = (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int));
			if ((long)timespan.Days > (long)0x5f5e0ff || timespan < TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException();
			}
			else
			{
				int hours = timespan.Hours;
				str = string.Concat(str, hours.ToString(format).PadLeft(2, '0'));
				int minutes = timespan.Minutes;
				str = string.Concat(str, minutes.ToString(format).PadLeft(2, '0'));
				int seconds = timespan.Seconds;
				str = string.Concat(str, seconds.ToString(format).PadLeft(2, '0'));
				str = string.Concat(str, ".");
				TimeSpan timeSpan = new TimeSpan(timespan.Days, timespan.Hours, timespan.Minutes, timespan.Seconds, 0);
				long ticks = (timespan.Ticks - timeSpan.Ticks) * (long)0x3e8 / (long)0x2710;
				string str1 = ticks.ToString((IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(long)));
				if (str1.Length > 6)
				{
					str1 = str1.Substring(0, 6);
				}
				str = string.Concat(str, str1.PadLeft(6, '0'));
				str = string.Concat(str, ":000");
				return str;
			}
		}

		public static TimeSpan ToTimeSpan(string dmtfTimespan)
		{
			int num = 0;
			int num1 = 0;
			int num2 = 0;
			int num3 = 0;
			IFormatProvider format = (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int));
			string str = dmtfTimespan;
			if (str != null)
			{
				if (str.Length != 0)
				{
					if (str.Length == 25)
					{
						if (str.Substring(21, 4) == ":000")
						{
							long num4 = (long)0;
							try
							{
								string str1 = str.Substring(0, 8);
								num = int.Parse(str1, format);
								str1 = str.Substring(8, 2);
								num1 = int.Parse(str1, format);
								str1 = str.Substring(10, 2);
								num2 = int.Parse(str1, format);
								str1 = str.Substring(12, 2);
								num3 = int.Parse(str1, format);
								str1 = str.Substring(15, 6);
								num4 = long.Parse(str1, (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(long))) * (long)10;
							}
							catch
							{
								throw new ArgumentOutOfRangeException("dmtfTimespan");
							}
							if (num < 0 || num1 < 0 || num2 < 0 || num3 < 0 || num4 < (long)0)
							{
								throw new ArgumentOutOfRangeException("dmtfTimespan");
							}
							else
							{
								TimeSpan timeSpan = new TimeSpan(num, num1, num2, num3, 0);
								TimeSpan timeSpan1 = TimeSpan.FromTicks(num4);
								timeSpan = timeSpan + timeSpan1;
								return timeSpan;
							}
						}
						else
						{
							throw new ArgumentOutOfRangeException("dmtfTimespan");
						}
					}
					else
					{
						throw new ArgumentOutOfRangeException("dmtfTimespan");
					}
				}
				else
				{
					throw new ArgumentOutOfRangeException("dmtfTimespan");
				}
			}
			else
			{
				throw new ArgumentOutOfRangeException("dmtfTimespan");
			}
		}
	}
}