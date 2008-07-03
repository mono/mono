//
// System.Globalization.PersianCalendar.cs
//
// Authors:
//   Roozbeh Pournader (roozbeh@farsiweb.info)
//   Ulrich Kunitz
//
// Copyright (C) 2002 Ulrich Kunitz
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2005 Sharif FarsiWeb, Inc. (http://www.farsiweb.info)
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

#if NET_2_0

namespace System.Globalization {

using System;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// This is the Persian calendar of Iran, also known as the Iranian
/// calendar or the Jalaali calendar. The Afghan calendar may or may
/// not be different, as different sources disagree about it.
/// </summary>
/// <remarks>
/// <para>The implemented algorithm is the simple 33-year arithmetic
/// calendar, which is not the same as the official Iranian calendar.
/// But this arithmetic calendar has been confirmed to produce the
/// same results as the official Iranian calendar at least
/// from 1925 C.E., when the calendar was officially introduced,
/// to 2088 C.E. This is the same algorithm that is used in .NET.
/// </para>
/// <para>The Iranian law explicitly mentions that the true solar year
/// should be used, which requires astronomical calculations of the
/// March equinox and the solar apparent noon. The exact locale for
/// observation of the apparent noon is not mentioned in the 1925 law,
/// but the current practice is using the 52.5 E meridian, which is
/// the meridian defining the official timezone of Iran.
/// </para>
/// <para>Also, please note that implementing the Persian calendar
/// using the 2820-year arithmetic algorithm, as suggested by
/// Ahmad Birashk and others, is less accurate than the 33-year
/// calendar: first, it fails earlier than the 33-year cycle in
/// matching the official astronomical calendar (first failure is
/// in 2025 C.E.), and second, the 2820-year suggested rule is based
/// on the mean tropical year, not the mean March equinoctial year.
/// </para>
/// </remarks>
#if NET_2_0
[Serializable]
#endif
public class PersianCalendar : Calendar {
	/// <summary>
	/// Constructor.
	/// </summary>
	public PersianCalendar() {
		M_AbbrEraNames = new string[] {"A.P."};
		M_EraNames = new string[] {"Anno Persico"};
		if (twoDigitYearMax == 99)
			// FIXME: the .NET documentation does not mention the default value,
			// This is the value mentioned in the .NET documentation example result.
			twoDigitYearMax = 1410;
	}

	/// <summary>
	/// The era number for the Anno Persico (A.P.) era, called
	/// plain PersianEra.
	/// </summary>
	public static readonly int PersianEra = 1;

	/// <summary>
	/// The
	/// <see cref="T:System.DateTime"/> ticks for first day of
	/// year 1 A.P.
	/// </summary>
	internal const long M_MinTicks = 196036416000000000L;

	/// <summary>
	/// The minimum year in the A.P. era supported.
	/// </summary>
	internal const int M_MinYear = 1;

	/// <value>Overridden. Gives the eras supported by the Persian
	/// calendar as an array of integers.
	/// </value>
	public override int[] Eras {
		get {
			return new int[] { PersianEra }; 
		}
	}
	
	public override int TwoDigitYearMax {
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
	/// A protected member checking a
	/// <see cref="T:System.DateTime"/> value.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/>
	/// to check.
	/// </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is before the
	/// year 1 A.P.
	/// </exception>
	internal void M_CheckDateTime(DateTime time)
	{
		if (time.Ticks < M_MinTicks)
			throw new ArgumentOutOfRangeException(
				"time",
				"Only positive Persian years are supported.");
	}

	/// <summary>
	/// A protected method checking the era number.
	/// </summary>
	/// <param name="era">The era number.</param>
	/// <exception name="T:System.ArgumentException">
	/// The exception is thrown if the era is not equal
	/// <see cref="F:PersianEra"/>.
	/// </exception>
	internal void M_CheckEra(ref int era)
	{
		if (era == CurrentEra)
			era = PersianEra;
		if (era != PersianEra)
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
	/// <see cref="F:PersianEra"/>.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the calendar year is outside of
	/// the allowed range.
	/// </exception>
	internal override void M_CheckYE(int year, ref int era)
	{
		M_CheckEra(ref era);
		if (year < M_MinYear || year > M_MaxYear)
			throw new ArgumentOutOfRangeException(
				"year",
				"Only Persian years between 1 and 9378," +
				" inclusive, are supported.");
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
	/// <see cref="F:PersianEra"/>.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the calendar year or month is
	/// outside of the allowed range.
	/// </exception>
	internal void M_CheckYME(int year, int month, ref int era)
	{
		M_CheckYE(year, ref era);
		if (month < 1 || month > 12)
			throw new ArgumentOutOfRangeException("month",
				"Month must be between one and twelve.");
		else if (year == M_MaxYear && month > 10)
			throw new ArgumentOutOfRangeException("month",
				"Months in year 9378 must be between one and ten.");
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
	/// <see cref="F:PersianEra"/>.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the calendar year, month, or day is
	/// outside of the allowed range.
	/// </exception>
	internal void M_CheckYMDE(int year, int month, int day,
	                          ref int era)
	{
		M_CheckYME(year, month, ref era);
		M_ArgumentInRange("day",
			day, 1, GetDaysInMonth(year, month, era));
		if (year == M_MaxYear && month == 10 && day > 10)
			throw new ArgumentOutOfRangeException("day",
				"Days in month 10 of year 9378 must" +
				" be between one and ten.");
	}

	internal const int epoch = 226895;

	// FIXME: this may need a "static". I don't know enough C#.
        internal int fixed_from_dmy(int day, int month, int year)
        {
		int k = epoch - 1;

		k += 365 * (year - 1);
		k += (8 * year + 21) / 33;
		if (month <= 7)
			k += 31 * (month - 1);
		else
			k += 30 * (month - 1) + 6;
		k += day;
		
		return k;
	}
	
	// FIXME: this may need a "static". I don't know enough C#.
	internal int year_from_fixed(int date)
	{
		return (33 * (date - epoch) + 3) / 12053 + 1;
	}

	// FIXME: this may need a "static". I don't know enough C#.
	internal void my_from_fixed(out int month, out int year,
		int date)
	{
		int day;
		
		year = year_from_fixed(date);
		day = date - fixed_from_dmy (1, 1, year);
		if (day < 216)
			month = day / 31 + 1;
		else
			month = (day - 6) / 30 + 1;
	}

	// FIXME: this may need a "static". I don't know enough C#.
	internal void dmy_from_fixed(out int day, out int month,
	                             out int year, int date)
	{
		year = year_from_fixed(date);
		day = date - fixed_from_dmy (1, 1, year);
		if (day < 216) {
			month = day / 31 + 1;
			day = day % 31 + 1;
		} else {
			month = (day-6) / 30 + 1;
			day = (day-6) % 30 + 1;
		}
	}

	// FIXME: this may need a "static". I don't know enough C#.
	internal bool is_leap_year(int year)
	{
		return (25 * year + 11) % 33 < 8;
	}

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
	/// <see cref="T:System.DateTime"/> return value is not in the years
	/// between 1 A.P. and 9999 C.E., inclusive.
	/// </exception>
	public override DateTime AddMonths(DateTime time, int months)
	{
		int rd = CCFixed.FromDateTime(time);
		int day, month, year;
		
		dmy_from_fixed(out day, out month, out year, rd);
		month += months;
		year += CCMath.div_mod(out month, month, 12);
		rd = fixed_from_dmy(day, month, year);
		DateTime t = CCFixed.ToDateTime(rd);
		t = t.Add(time.TimeOfDay);
		M_CheckDateTime(t);
		return t;
	}

	/// <summary>
	/// Overridden. Adds years to a given date.
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
	/// <see cref="T:System.DateTime"/> return value is not in the years
	/// between 1 A.P. and 9999 C.E., inclusive.
	/// </exception>
	public override DateTime AddYears(DateTime time, int years)
	{
		int rd = CCFixed.FromDateTime(time);
		int day, month, year;

		dmy_from_fixed(out day, out month, out year, rd);
		year += years;
		rd = fixed_from_dmy(day, month, year);
		DateTime t = CCFixed.ToDateTime(rd);
		t = t.Add(time.TimeOfDay);
		M_CheckDateTime(t);
		return t;
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
	/// <see cref="T:System.DateTime"/> parameter is not in the years
	/// between 1 A.P. and 9999 C.E., inclusive.
	/// </exception>
	public override int GetDayOfMonth(DateTime time)
	{
		int day, month, year;
		
		M_CheckDateTime(time);
		int rd = CCFixed.FromDateTime(time);
		dmy_from_fixed(out day, out month, out year, rd);
		return day;
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
	/// <see cref="T:System.DateTime"/> parameter is not in the years
	/// between 1 A.P. and 9999 C.E., inclusive.
	/// </exception>
	public override DayOfWeek GetDayOfWeek(DateTime time)
	{
		M_CheckDateTime(time);
		int rd = CCFixed.FromDateTime(time);
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
	/// <see cref="T:System.DateTime"/> parameter is not in the years
	/// between 1 A.P. and 9999 C.E., inclusive.
	/// </exception>
	public override int GetDayOfYear(DateTime time)
	{
		M_CheckDateTime(time);
		int rd = CCFixed.FromDateTime(time);
		int year = year_from_fixed(rd);
		int rd1_1 = fixed_from_dmy(1, 1, year);
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
	/// <param name="era">An integer that gives the era of the specified
	/// year.</param>
	/// <returns>An integer that gives the number of days of the
	/// specified month.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if <paramref name="month"/>,
	/// <paramref name="year"/> ,or <paramref name="era"/> is outside
	/// the allowed range.
	/// </exception>
	public override int GetDaysInMonth(int year, int month, int era)
	{
		M_CheckYME(year, month, ref era);
		if (month <= 6) {
			return 31;
		} else if (month == 12 && !is_leap_year(year)) {
			return 29;
		} else {
			return 30;
		}
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
	/// <paramref name="year"/> or <paramref name="era"/> are outside the
	/// allowed range.
	/// </exception>
	public override int GetDaysInYear(int year, int era)
	{
		M_CheckYE(year, ref era);
		return is_leap_year(year) ? 366 : 365;
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
	/// <see cref="T:System.DateTime"/> parameter is not in the years
	/// between 1 A.P. and 9999 C.E., inclusive.
	/// </exception>
	public override int GetEra(DateTime time)
	{
		M_CheckDateTime(time);
		return PersianEra;
	}

#if NET_2_0
	public override int GetLeapMonth (int year, int era)
	{
		return 0;
	}
#endif

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
	/// <see cref="T:System.DateTime"/> parameter is not in the years
	/// between 1 A.P. and 9999 C.E., inclusive.
	/// </exception>
	public override int GetMonth(DateTime time)
	{
		M_CheckDateTime(time);
		int rd = CCFixed.FromDateTime(time);
		int month, year;
		my_from_fixed(out month, out year, rd);
		return month;
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
	public override int GetMonthsInYear(int year, int era)
	{
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
	/// <see cref="T:System.DateTime"/> parameter is not in the years
	/// between 1 A.P. and 9999 C.E., inclusive.
	/// </exception>
	public override int GetYear(DateTime time)
	{
		M_CheckDateTime(time);
		int rd = CCFixed.FromDateTime(time);
		return year_from_fixed(rd);
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
	public override bool IsLeapDay(int year, int month, int day,
	                               int era)
	{
		M_CheckYMDE(year, month, day, ref era);
		return is_leap_year(year) && month == 12 && day == 30;
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
	public override bool IsLeapMonth(int year, int month, int era)
	{
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
	public override bool IsLeapYear(int year, int era)
	{
		M_CheckYE(year, ref era);
		return is_leap_year(year);
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
		                            int hour, int minute,
		                            int second, int millisecond,
	                                    int era)
	{
		M_CheckYMDE(year, month, day, ref era);
		M_CheckHMSM(hour, minute, second, millisecond);
		int rd = fixed_from_dmy(day, month, year);
		return CCFixed.ToDateTime(rd,
			hour, minute, second, millisecond);
	}

	// FIXME: Calendar.cs and HebrewCalendar.cs are different in
	// how they handle this. I have randomly chosen the
	// HebrewCalendar.cs implementation.
	public override int ToFourDigitYear (int year)
	{
		M_ArgumentInRange ("year", year, 0, 99);
		
		int baseExtra = this.twoDigitYearMax % 100;
		int baseCentury = this.twoDigitYearMax - baseExtra;
		
		if (year <= baseExtra)
			return baseCentury + year;
		else
			return baseCentury + year - 100;
	}

#if NET_2_0
	public override CalendarAlgorithmType AlgorithmType {
		get {
			return CalendarAlgorithmType.SolarCalendar;
		}
	}

	static DateTime PersianMin = new DateTime (622, 3, 21, 0, 0, 0);
	static DateTime PersianMax = new DateTime (9999, 12, 31, 11, 59, 59);
		
	public override DateTime MinSupportedDateTime {
		get {
			return PersianMin;
		}
	}

	public override DateTime MaxSupportedDateTime {
		get {
			return PersianMax;
		}
	}
#endif
} // class PersianCalendar
	
} // namespace System.Globalization

#endif
