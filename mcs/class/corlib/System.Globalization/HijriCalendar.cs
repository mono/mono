// System.Globalization.HijriCalendar.cs
//
// (C) Ulrich Kunitz 2002
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Globalization {

using System;
using System.IO;
using System.Runtime.InteropServices;


/// <summary>
/// This is the Hijri calendar which might be called Islamic calendar. 
/// </summary>
/// <remarks>
/// <para>The calendar supports only dates in the HijriEra starting with the 
/// epoch.
/// </para>
/// <para>
/// The epoch of the Hijri Calendar might be adjusted by the 
/// <see cref="F:System.Globalization.HijriCalendar.AddHijriDate"/>
/// property. See the discussion of the
/// <see cref="F:CalendricalCalculations.HijriCalendar.epoch">
/// epoch
/// </see>
/// of the Hijri calendar.
/// </para>
/// <para>The implementation uses the
/// <see cref="N:CalendricalCalculations"/> namespace.
/// </para>
/// </remarks>
[Serializable]
[ComVisible (true)]
[MonoLimitation ("Serialization format not compatible with .NET")]
public class HijriCalendar : Calendar {
	/// <summary>
	/// Constructor.
	/// </summary>
	public HijriCalendar() {
		M_AbbrEraNames = new string[] {"A.H."};
		M_EraNames = new string[] {"Anno Hegirae"};
		if (twoDigitYearMax == 99)
			twoDigitYearMax = 1451;
	}

	/// <summary>
	/// The era number for the Anno Hegirae (A.H.) era.
	/// </summary>
	public static readonly int HijriEra = 1;

	/// <summary>
	/// The minimum fixed day number supported by the Hijri calendar.
	/// </summary>
	internal static readonly int M_MinFixed =
		CCHijriCalendar.fixed_from_dmy(1, 1, 1);
	/// <summary>
	/// The maximum fixed day number supported by the Hijri calendar.
	/// </summary>
	internal static readonly int M_MaxFixed =
		CCGregorianCalendar.fixed_from_dmy(31, 12, 9999);

	/// <value>Overridden. Gives the eras supported by the Gregorian
	/// calendar as an array of integers.
	/// </value>
	public override int[] Eras {
		get {
			return new int[] { HijriEra }; 
		}
	}

	// FIXME: [MonoTODO ("Add call into operating system")]
	public override int TwoDigitYearMax 
	{
		get {
			return twoDigitYearMax;
		}
		set {
			CheckReadOnly ();
			M_ArgumentInRange ("value", value, 100, M_MaxYear);

			twoDigitYearMax = value;
		}
	}

	/// <summary>
	/// Protected field storing the
	/// <see cref="F:AddHijriDate"/>.
	/// </summary>
	internal int M_AddHijriDate = 0;

	// TODO: I don't know currently, which sign to use with the parameter.
	/// <value>An integer property representing the adjustment to the epoch
	/// of the Hijri calendar. Not supported by .NET.
	/// </value>
	internal virtual int AddHijriDate {
		get {
			return M_AddHijriDate;
		}
		set {
			CheckReadOnly ();
			if (value < -3 && value > 3)
				throw new ArgumentOutOfRangeException(
					"AddHijriDate",
					"Value should be between -3 and 3.");
			M_AddHijriDate = value;
		}
	}
	
	/// <summary>
	/// A protected method checking an
	/// <see cref="F:AddHijriDate"/> adjusted fixed day number.
	/// </summary>
	/// <param name="param">A string giving the name of the parameter
	/// to check.</param>
	/// <param name="rdHijri">An integer giving the AddHijriDate adjusted
	/// fixed day number.
	/// </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// Exception is thrown, if the AddHijriDate adjusted fixed day
	/// number is outside the supported range.
	/// </exception>
	internal void M_CheckFixedHijri(string param, int rdHijri) {
		if (rdHijri < M_MinFixed || rdHijri > M_MaxFixed-AddHijriDate) {
			StringWriter sw = new StringWriter();
			int day, month, year;
			CCHijriCalendar.dmy_from_fixed(out day, out month,
				out year, M_MaxFixed-AddHijriDate);
			if (AddHijriDate != 0) {
				sw.Write("This HijriCalendar " +
					"(AddHijriDate {0})" +
					" allows dates from 1. 1. 1 to " +
					"{1}. {2}. {3}.",
					AddHijriDate,
					day, month, year);
			} else {
				sw.Write("HijriCalendar allows dates from " +
					"1.1.1 to {0}.{1}.{2}.",
					day, month, year);
			}
			throw new ArgumentOutOfRangeException(param,
				sw.ToString());
		}
	}

	/// <summary>
	/// A protected member checking a
	/// <see cref="T:System.DateTime"/> value.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/>
	/// to check.
	/// </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the supported 
	/// range of the Hijri calendar.
	/// </exception>
	internal void M_CheckDateTime(DateTime time) {
		int rd = CCFixed.FromDateTime(time) - AddHijriDate;
		M_CheckFixedHijri("time", rd);
	}

	/// <summary>
	/// Protected member which computes the
	/// <see cref="F:AddHijriDate"/>
	/// adjusted fixed day number from a
	/// <see cref="T:System.DateTime"/>.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/>
	/// to convert.
	/// </param>
	/// <returns>The
	/// <see cref="F:AddHijriDate"/> adjusted fixed day number.
	/// </returns>
	internal int M_FromDateTime(DateTime time) {
		return CCFixed.FromDateTime(time) - AddHijriDate;
	}

	/// <summary>
	/// Protected member which converts the
	/// <see cref="F:AddHijriDate"/>
	/// adjusted fixed day number the a
	/// <see cref="T:System.DateTime"/> value.
	/// </summary>
	/// <param name="rd">The
	/// <see cref="F:AddHijriDate"/> adjusted fixed day number.
	/// </param>
	/// <returns>The converted
	/// <see cref="T:System.DateTime"/> value.
	/// </returns>
	internal DateTime M_ToDateTime(int rd) {
		return CCFixed.ToDateTime(rd+AddHijriDate);
	}

	/// <summary>
	/// Protected member which converts the
	/// <see cref="F:AddHijriDate"/>
	/// adjusted fixed day number the a
	/// <see cref="T:System.DateTime"/> value using a number
	/// of time parameters.
	/// </summary>
	/// <param name="date">The
	/// <see cref="F:AddHijriDate"/> adjusted fixed day number.
	/// </param>
	/// <param name="hour">An integer that specifies the hour.
	/// </param>
	/// <param name="minute">An integer that specifies the minute.
	/// </param>
	/// <param name="second">An integer that gives the second.
	/// </param>
	/// <param name="milliseconds">An integer that gives the
	/// milliseconds.
	/// </param>
	/// <returns>The converted
	/// <see cref="T:System.DateTime"/> value.
	/// </returns>
	internal DateTime M_ToDateTime(int date,
		int hour, int minute, int second, int milliseconds)
	{
		return CCFixed.ToDateTime(date+AddHijriDate,
			hour, minute, second, milliseconds);
	}

	/// <summary>
	/// A protected method checking the era number.
	/// </summary>
	/// <param name="era">The era number.</param>
	/// <exception name="T:System.ArgumentException">
	/// The exception is thrown if the era is not equal
	/// <see cref="F:HijriEra"/>.
	/// </exception>
	internal void M_CheckEra(ref int era) {
		if (era == CurrentEra)
			era = HijriEra;
		if (era != HijriEra)
			throw new ArgumentException("Era value was not valid.");
	}

	/// <summary>
	/// A protected method checking calendar year and the era number.
	/// </summary>
	/// <param name="year">An integer representing the calendar year.
	/// </param>
	/// <param name="era">The era number.</param>
	/// <exception cref="T:System.ArgumentException">
	/// The exception is thrown if the era is not equal
	/// <see cref="F:HijriEra"/>.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the calendar year is outside of
	/// the allowed range.
	/// </exception>
	internal override void M_CheckYE(int year, ref int era) {
		M_CheckEra(ref era);
		M_ArgumentInRange("year", year, 1, 9666);
	}

	/// <summary>
	/// A protected method checking the calendar year, month, and
	/// era number.
	/// </summary>
	/// <param name="year">An integer representing the calendar year.
	/// </param>
	/// <param name="month">An integer giving the calendar month.
	/// </param>
	/// <param name="era">The era number.</param>
	/// <exception cref="T:System.ArgumentException">
	/// The exception is thrown if the era is not equal
	/// <see cref="F:HijriEra"/>.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the calendar year or month is
	/// outside of the allowed range.
	/// </exception>
	internal void M_CheckYME(int year, int month, ref int era) {
		M_CheckYE(year, ref era);
		if (month < 1 || month > 12)
			throw new ArgumentOutOfRangeException("month",
				"Month must be between one and twelve.");
		if (year == 9666) {
			int rd = CCHijriCalendar.fixed_from_dmy(1, month, year);
			M_CheckFixedHijri("month", rd);
		}
	}

	/// <summary>
	/// A protected method checking the calendar day, month, and year
	/// and the era number.
	/// </summary>
	/// <param name="year">An integer representing the calendar year.
	/// </param>
	/// <param name="month">An integer giving the calendar month.
	/// </param>
	/// <param name="day">An integer giving the calendar day.
	/// </param>
	/// <param name="era">The era number.</param>
	/// <exception cref="T:System.ArgumentException">
	/// The exception is thrown if the era is not equal
	/// <see cref="F:HijriEra"/>.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the calendar year, month, or day is
	/// outside of the allowed range.
	/// </exception>
	internal void M_CheckYMDE(int year, int month, int day, ref int era)
	{
		M_CheckYME(year, month, ref era);
		M_ArgumentInRange("day", day, 1,
			GetDaysInMonth(year, month, HijriEra));
		if (year == 9666) {
			int rd = CCHijriCalendar.fixed_from_dmy(day, month,
				year);
			M_CheckFixedHijri("day", rd);
		}
	}

#if false
	//
	// The following routines are commented out as they do not appear on the .NET Framework 1.1
	//

	/// <summary>
	/// Overridden. Adds days to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// days.
	/// </param>
	/// <param name="days">The number of days to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="days"/> to the specified
	/// DateTime.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> return value is not in the
	/// supported range of the Hijri calendar.
	/// </exception>
	public override DateTime AddDays(DateTime time, int days) {
		DateTime t = base.AddDays(time, days);
		M_CheckDateTime(t);
		return t;
	}

	/// <summary>
	/// Overridden. Adds hours to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// hours.
	/// </param>
	/// <param name="hours">The number of hours to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="hours"/> to the specified
	/// DateTime.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> return value is not in the
	/// supported range of the Hijri calendar.
	/// </exception>
	public override DateTime AddHours(DateTime time, int hours) {
		DateTime t = base.AddHours(time, hours);
		M_CheckDateTime(t);
		return t;
	}

	/// <summary>
	/// Overridden. Adds milliseconds to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// milliseconds.
	/// </param>
	/// <param name="milliseconds">The number of milliseconds given as
	/// double to add. Keep in mind the 100 nanosecond resolution of 
	/// <see cref="T:System.DateTime"/>.
	/// </param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="milliseconds"/> to the specified
	/// DateTime.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> return value is not in the
	/// supported range of the Hijri calendar.
	/// </exception>
	public override DateTime AddMilliseconds(DateTime time,
		double milliseconds)
	{
		DateTime t = base.AddMilliseconds(time, milliseconds);
		M_CheckDateTime(t);
		return t;
	}

	/// <summary>
	/// Overridden. Adds minutes to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// minutes.
	/// </param>
	/// <param name="minutes">The number of minutes to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="minutes"/> to the specified
	/// DateTime.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> return value is not in the
	/// supported range of the Hijri calendar.
	/// </exception>
	public override DateTime AddMinutes(DateTime time, int minutes) {
		DateTime t = base.AddMinutes(time, minutes);
		M_CheckDateTime(t);
		return t;
	}

	/// <summary>
	/// Overridden. Adds seconds to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// seconds.
	/// </param>
	/// <param name="seconds">The number of seconds to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="seconds"/> to the specified
	/// DateTime.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> return value is not in the
	/// supported range of the Hijri calendar.
	/// </exception>
	public override DateTime AddSeconds(DateTime time, int seconds) {
		DateTime t = base.AddSeconds(time, seconds);
		M_CheckDateTime(t);
		return t;
	}

	/// <summary>
	/// Overridden. Adds weeks to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// weeks.
	/// </param>
	/// <param name="weeks">The number of weeks to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="weeks"/> to the specified
	/// DateTime.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> return value is not in the
	/// supported range of the Hijri calendar.
	/// </exception>
	public override DateTime AddWeeks(DateTime time, int weeks) {
		DateTime t = base.AddWeeks(time, weeks);
		M_CheckDateTime(t);
		return t;
	}

	/// <summary>
	/// Overridden. Gives the hour of the specified time.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies the
	/// time.
	/// </param>
	/// <returns>An integer that gives the hour of the specified time,
	/// starting with 0.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the
	/// supported range of the Hijri calendar.
	/// </exception>
	public override int GetHour(DateTime time) {
		M_CheckDateTime(time);
		return base.GetHour(time);
	}

	/// <summary>
	/// Overridden. Gives the milliseconds in the current second
	/// of the specified time.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies the
	/// time.
	/// </param>
	/// <returns>An integer that gives the milliseconds in the seconds
	/// of the specified time, starting with 0.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the
	/// supported range of the Hijri calendar.
	/// </exception>
	public override double GetMilliseconds(DateTime time) {
		M_CheckDateTime(time);
		return base.GetMilliseconds(time);
	}

	/// <summary>
	/// Overridden. Gives the minute of the specified time.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies the
	/// time.
	/// </param>
	/// <returns>An integer that gives the minute of the specified time,
	/// starting with 0.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the
	/// supported range of the Hijri calendar.
	/// </exception>
	public override int GetMinute(DateTime time) {
		M_CheckDateTime(time);
		return base.GetMinute(time);
	}

	/// <summary>
	/// Overridden. Gives the second of the specified time.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies the
	/// time.
	/// </param>
	/// <returns>An integer that gives the second of the specified time,
	/// starting with 0.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the
	/// supported range of the Hijri calendar.
	/// </exception>
	public override int GetSecond(DateTime time) {
		M_CheckDateTime(time);
		return base.GetMinute(time);
	}
#endif
	
	/// <summary>
	/// Overrideden. Adds months to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// months.
	/// </param>
	/// <param name="months">The number of months to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="months"/> to the specified
	/// DateTime.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> return value is not in the
	/// supported range of the Hijri calendar.
	/// </exception>
	public override DateTime AddMonths(DateTime time, int months) {
		int rd = M_FromDateTime(time);
		int day, month, year;
		CCHijriCalendar.dmy_from_fixed(
			out day, out month, out year, rd);
		month += months;
		year += CCMath.div_mod(out month, month, 12);
		rd = CCHijriCalendar.fixed_from_dmy(day, month, year);
		M_CheckFixedHijri("time", rd);
		DateTime t = M_ToDateTime(rd);
		return t.Add(time.TimeOfDay);
	}

	/// <summary>
	/// Overrideden. Adds years to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// years.
	/// </param>
	/// <param name="years">The number of years to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="years"/> to the specified
	/// DateTime.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> return value is not in the
	/// supported range of the Hijri calendar.
	/// </exception>
	public override DateTime AddYears(DateTime time, int years) {
		int rd = M_FromDateTime(time);
		int day, month, year;
		CCHijriCalendar.dmy_from_fixed(
			out day, out month, out year, rd);
		year += years;
		rd = CCHijriCalendar.fixed_from_dmy(day, month, year);
		M_CheckFixedHijri("time", rd);
		DateTime t = M_ToDateTime(rd);
		return t.Add(time.TimeOfDay);
	}
		
	/// <summary>
	/// Overriden. Gets the day of the month from
	/// <paramref name="time"/>.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer giving the day of months, starting with 1.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the
	/// supported range of the Hijri calendar.
	/// </exception>
	public override int GetDayOfMonth(DateTime time) {
		int rd = M_FromDateTime(time);
		M_CheckFixedHijri("time", rd);
		return CCHijriCalendar.day_from_fixed(rd);
	}

	/// <summary>
	/// Overriden. Gets the day of the week from the specified date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer giving the day of months, starting with 1.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the
	/// supported range of the Hijri calendar.
	/// </exception>
	public override DayOfWeek GetDayOfWeek(DateTime time) {
		int rd = M_FromDateTime(time);
		M_CheckFixedHijri("time", rd);
		return (DayOfWeek)CCFixed.day_of_week(rd);
	}

	/// <summary>
	/// Overridden. Gives the number of the day in the year.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer representing the day of the year,
	/// starting with 1.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the
	/// supported range of the Hijri calendar.
	/// </exception>
	public override int GetDayOfYear(DateTime time) {
		int rd = M_FromDateTime(time);
		M_CheckFixedHijri("time", rd);
		int year = CCHijriCalendar.year_from_fixed(rd);
		int rd1_1 = CCHijriCalendar.fixed_from_dmy(1, 1, year);
		return rd - rd1_1 + 1;
	}

	/// <summary>
	/// Overridden. Gives the number of days in the specified month
	/// of the given year and era.
	/// </summary>
	/// <param name="year">An integer that gives the year.
	/// </param>
	/// <param name="month">An integer that gives the month, starting
	/// with 1.</param>
	/// <param name="era">An intger that gives the era of the specified
	/// year.</param>
	/// <returns>An integer that gives the number of days of the
	/// specified month.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if <paramref name="month"/>,
	/// <paramref name="year"/> ,or <paramref name="era"/> is outside
	/// the allowed range.
	/// </exception>
	public override int GetDaysInMonth(int year, int month, int era) {
		M_CheckYME(year, month, ref era);
		int rd1 = CCHijriCalendar.fixed_from_dmy(1, month, year);
		int rd2 = CCHijriCalendar.fixed_from_dmy(1, month+1, year);
		return rd2 - rd1;
	}

	/// <summary>
	/// Overridden. Gives the number of days of the specified
	/// year of the given era. 
	/// </summary>
	/// <param name="year">An integer that specifies the year. 
	/// </param>
	/// <param name="era">An ineger that specifies the era.
	/// </param>
	/// <returns>An integer that gives the number of days of the
	/// specified year.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeExceiption">
	/// The exception is thrown, if
	/// <paramref name="year"/> is outside the allowed range.
	/// </exception>
	public override int GetDaysInYear(int year, int era) {
		M_CheckYE(year, ref era);
		int rd1 = CCHijriCalendar.fixed_from_dmy(1, 1, year);
		int rd2 = CCHijriCalendar.fixed_from_dmy(1, 1, year+1);
		return rd2 - rd1;
	}
		

	/// <summary>
	/// Overridden. Gives the era of the specified date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer representing the era of the calendar.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the
	/// supported range of the Hijri calendar.
	/// </exception>
	public override int GetEra(DateTime time) {
		M_CheckDateTime(time);
		return HijriEra;
	}

	[ComVisible (false)]
	public override int GetLeapMonth (int year, int era)
	{
		return 0;
	}

	/// <summary>
	/// Overridden. Gives the number of the month of the specified
	/// date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer representing the month, 
	/// starting with 1.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the
	/// supported range of the Hijri calendar.
	/// </exception>
	public override int GetMonth(DateTime time) {
		int rd = M_FromDateTime(time);
		M_CheckFixedHijri("time", rd);
		return CCHijriCalendar.month_from_fixed(rd);
	}

	/// <summary>
	/// Overridden. Gives the number of months in the specified year 
	/// and era.
	/// </summary>
	/// <param name="year">An integer that specifies the year.
	/// </param>
	/// <param name="era">An integer that specifies the era.
	/// </param>
	/// <returns>An integer that gives the number of the months in the
	/// specified year.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if the year or the era are not valid.
	/// </exception>
	public override int GetMonthsInYear(int year, int era) {
		M_CheckYE(year, ref era);
		return 12;
	}

	/// <summary>
	/// Overridden. Gives the number of the year of the specified
	/// date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer representing the year, 
	/// starting with 1.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the
	/// supported range of the Hijri calendar.
	/// </exception>
	public override int GetYear(DateTime time) {
		int rd = M_FromDateTime(time);
		M_CheckFixedHijri("time", rd);
		return CCHijriCalendar.year_from_fixed(rd);
	}

	/// <summary>
	/// Overridden. Tells whether the given day 
	/// is a leap day.
	/// </summary>
	/// <param name="year">An integer that specifies the year in the
	/// given era.
	/// </param>
	/// <param name="month">An integer that specifies the month.
	/// </param>
	/// <param name="day">An integer that specifies the day.
	/// </param>
	/// <param name="era">An integer that specifies the era.
	/// </param>
	/// <returns>A boolean that tells whether the given day is a leap
	/// day.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if the year, month, day, or era is not
	/// valid.
	/// </exception>
	public override bool IsLeapDay(int year, int month, int day, int era)
	{
		M_CheckYMDE(year, month, day, ref era);
		return IsLeapYear(year) && month == 12 && day == 30;
	}

	/// <summary>
	/// Overridden. Tells whether the given month 
	/// is a leap month.
	/// </summary>
	/// <param name="year">An integer that specifies the year in the
	/// given era.
	/// </param>
	/// <param name="month">An integer that specifies the month.
	/// </param>
	/// <param name="era">An integer that specifies the era.
	/// </param>
	/// <returns>A boolean that tells whether the given month is a leap
	/// month.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if the year, month, or era is not
	/// valid.
	/// </exception>
	public override bool IsLeapMonth(int year, int month, int era) {
		M_CheckYME(year, month, ref era);
		return false;
	}

	/// <summary>
	/// Overridden. Tells whether the given year
	/// is a leap year.
	/// </summary>
	/// <param name="year">An integer that specifies the year in the
	/// given era.
	/// </param>
	/// <param name="era">An integer that specifies the era.
	/// </param>
	/// <returns>A boolean that tells whether the given year is a leap
	/// year.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if the year or era is not
	/// valid.
	/// </exception>
	public override bool IsLeapYear(int year, int era) {
		M_CheckYE(year, ref era);
		return CCHijriCalendar.is_leap_year(year);
	}

	/// <summary>
	/// Overridden. Creates the
	/// <see cref="T:System.DateTime"/> from the parameters.
	/// </summary>
	/// <param name="year">An integer that gives the year in the
	/// <paramref name="era"/>.
	/// </param>
	/// <param name="month">An integer that specifies the month.
	/// </param>
	/// <param name="day">An integer that specifies the day.
	/// </param>
	/// <param name="hour">An integer that specifies the hour.
	/// </param>
	/// <param name="minute">An integer that specifies the minute.
	/// </param>
	/// <param name="second">An integer that gives the second.
	/// </param>
	/// <param name="milliseconds">An integer that gives the
	/// milliseconds.
	/// </param>
	/// <param name="era">An integer that specifies the era.
	/// </param>
	/// <returns>A
	/// <see cref="T:system.DateTime"/> representig the date and time.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if at least one of the parameters
	/// is out of range.
	/// </exception>
	public override DateTime ToDateTime(int year, int month, int day,
		int hour, int minute, int second, int millisecond,
		int era)
	{
		M_CheckYMDE(year, month, day, ref era);
		M_CheckHMSM(hour, minute, second, millisecond);
		int rd = CCHijriCalendar.fixed_from_dmy(day, month, year);
		return M_ToDateTime(rd,
			hour, minute, second, millisecond);
	}

	public override int ToFourDigitYear(int year)
	{
		return base.ToFourDigitYear (year);
	}

#if !NET_2_1
	[ComVisible (false)]
	public override CalendarAlgorithmType AlgorithmType  {
		get {
			return CalendarAlgorithmType.LunarCalendar;
		}
	}
#endif

	static DateTime Min = new DateTime (622, 7, 18, 0, 0, 0);
	static DateTime Max = new DateTime (9999, 12, 31, 11, 59, 59);
		
	[ComVisible (false)]
	public override DateTime MinSupportedDateTime {
		get {
			return Min;
		}
	}

	[ComVisible (false)]
	public override DateTime MaxSupportedDateTime {
		get {
			return Max;
		}
	}
	
	[MonoTODO ()]
	public int HijriAdjustment {
		get { throw new NotImplementedException (); }
		set { throw new NotImplementedException (); }
	}
} // class HijriCalendar
	
} // namespace System.Globalization
