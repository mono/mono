// System.Globalization.JapaneseCalendar.cs
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
using System.Runtime.InteropServices;

/// <summary>
/// This is the Japanese calendar. It differs from the Gregorian calendar
/// only in the years.
/// </summary>
/// <remarks>
/// <para>The Japanese calendar support four eras.</para>
/// <list type="table">
/// <listheader>
/// <term>era number</term>
/// <term>Gregorian start date</term>
/// <term>Gregorian end date</term>
/// </listheader>
/// <item>
/// <term>1</term>
/// <term>September 8, 1868</term>
/// <term>July 29, 1912</term>
/// </item>
/// <item>
/// <term>2</term>
/// <term>July 30, 1912</term>
/// <term>December 24, 1926</term>
/// </item>
/// <item>
/// <term>3</term>
/// <term>December 25, 1926</term>
/// <term>January 7, 1989</term>
/// </item>
/// <item>
/// <term>4</term>
/// <term>January 8, 1989</term>
/// <term>present</term>
/// </item>
/// </list>
/// <para>The implementation uses the
/// <see cref="N:CalendricalCalculations"/> namespace.
/// </para>
/// </remarks>
[Serializable]
#if NET_2_0
[ComVisible (true)]
#endif
[MonoTODO ("Serialization format not compatible with .NET")]
public class JapaneseCalendar : Calendar {
	/// <summary>
	/// Static protected field storing the
	/// <see cref="T:CalendricalCalculations.GregorianEraHandler"/>.
	/// </summary>
	internal static readonly CCGregorianEraHandler M_EraHandler;

	/// <summary>
	/// Static constructor, who creates and initializes
	/// <see cref="F:M_EraHandler"/>.
	/// </summary>
	static JapaneseCalendar() {
		M_EraHandler = new CCGregorianEraHandler();
		M_EraHandler.appendEra(1,
			CCGregorianCalendar.fixed_from_dmy(8, 9, 1868),
			CCGregorianCalendar.fixed_from_dmy(29, 7, 1912));
		M_EraHandler.appendEra(2,
			CCGregorianCalendar.fixed_from_dmy(30, 7, 1912),
			CCGregorianCalendar.fixed_from_dmy(24, 12, 1926));
		M_EraHandler.appendEra(3,
			CCGregorianCalendar.fixed_from_dmy(25, 12, 1926),
			CCGregorianCalendar.fixed_from_dmy(7, 1, 1989));
		M_EraHandler.appendEra(4,
			CCGregorianCalendar.fixed_from_dmy(8, 1, 1989));
	}

	/// <summary>
	/// Default constructor.
	/// </summary>
	public JapaneseCalendar() {
		M_AbbrEraNames = new string[] { "M", "T", "S", "H" };
		M_EraNames = new string[] { "Meiji", "Taisho", "Showa",
			"Heisei" };
	}
		

	/// <value>Overridden. Gives the eras supported by the
	/// calendar as an array of integers.
	/// </value>
	public override int[] Eras {
		get {
			return (int[])M_EraHandler.Eras.Clone();
		}
	}

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
	/// A protected member checking a
	/// <see cref="T:System.DateTime"/> value.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/>
	/// to check.
	/// </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is outside all
	/// supported eras.
	/// </exception>
	internal void M_CheckDateTime(DateTime time) {
		M_EraHandler.CheckDateTime(time);
	}

	/// <summary>
	/// A protected method checking the era number.
	/// </summary>
	/// <param name="era">The era number as reference. It is set
	/// to <see cref="F:CurrentEra"/>, if the input value is 0.</param>
	/// <exception name="T:System.ArgumentException">
	/// The exception is thrown if the era is not supported by the class.
	/// </exception>
	internal void M_CheckEra(ref int era) {
		if (era == CurrentEra)
			era = 4;
		if (!M_EraHandler.ValidEra(era))
			throw new ArgumentException("Era value was not valid.");
	}

	/// <summary>
	/// A protected method checking calendar year and the era number.
	/// </summary>
	/// <param name="year">An integer representing the calendar year.
	/// </param>
	/// <param name="era">The era number as reference.</param>
	/// <exception name="T:System.ArgumentException">
	/// The exception is thrown if the era is not supported by the class.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the calendar year is outside of
	/// the supported range.
	/// </exception>
	internal int M_CheckYEG(int year, ref int era) {
		M_CheckEra(ref era);
		return M_EraHandler.GregorianYear(year, era);
	}

	/// <summary>
	/// Checks whether the year is the era is valid, if era = CurrentEra
	/// the right value is set.
	/// </summary>
	/// <param name="year">The year to check.</param>
	/// <param name="era">The era to check.</Param>
	/// <exception cref="T:ArgumentOutOfRangeException">
	/// The exception will be thrown, if the year is not valid.
	/// </exception>
	internal override void M_CheckYE(int year, ref int era) {
		M_CheckYEG(year, ref era);
	}

	/// <summary>
	/// A protected method checking the calendar year, month, and
	/// era number.
	/// </summary>
	/// <param name="year">An integer representing the calendar year.
	/// </param>
	/// <param name="month">An integer giving the calendar month.
	/// </param>
	/// <param name="era">The era number as reference.</param>
	/// <exception name="T:System.ArgumentException">
	/// The exception is thrown if the era is not supported by the class.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the calendar year or month is
	/// outside of the supported range.
	/// </exception>
	internal int M_CheckYMEG(int year, int month, ref int era) {
		int gregorianYear = M_CheckYEG(year, ref era);
		if (month < 1 || month > 12)
			throw new ArgumentOutOfRangeException("month",
				"Month must be between one and twelve.");
		return gregorianYear;
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
	/// <param name="era">The era number as reference.</param>
	/// <exception name="T:System.ArgumentException">
	/// The exception is thrown if the era is not supported by the class.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the calendar year, month, or day is
	/// outside of the supported range.
	/// </exception>
	internal int M_CheckYMDEG(int year, int month, int day, ref int era)
	{
		int gregorianYear = M_CheckYMEG(year, month, ref era);
		M_ArgumentInRange("day", day, 1, GetDaysInMonth(year, month, era));
		return gregorianYear;
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
	/// The exception is thrown if
	/// <see cref="T:System.DateTime"/> return value is outside all
	/// supported eras.
	/// </exception>
	public override DateTime AddMonths(DateTime time, int months) {
		DateTime t = CCGregorianCalendar.AddMonths(time, months);
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
	/// The exception is thrown if
	/// <see cref="T:System.DateTime"/> return value is outside all
	/// supported eras.
	/// </exception>
	public override DateTime AddYears(DateTime time, int years) {
		DateTime t = CCGregorianCalendar.AddYears(time, years);
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
	/// <see cref="T:System.DateTime"/> parameter is outside all
	/// supported eras.
	/// </exception>
	public override int GetDayOfMonth(DateTime time) {
		M_CheckDateTime(time);
		return CCGregorianCalendar.GetDayOfMonth(time);
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
	/// <see cref="T:System.DateTime"/> parameter is outside all
	/// supported eras.
	/// </exception>
	public override DayOfWeek GetDayOfWeek(DateTime time) {
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
	/// <see cref="T:System.DateTime"/> parameter is outside all
	/// supported eras.
	/// </exception>
	public override int GetDayOfYear(DateTime time) {
		M_CheckDateTime(time);
		return CCGregorianCalendar.GetDayOfYear(time);
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
	public override int GetDaysInMonth(int year, int month, int era) {
		int gregorianYear = M_CheckYMEG(year, month, ref era);
		return CCGregorianCalendar.GetDaysInMonth(gregorianYear, month);
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
	public override int GetDaysInYear(int year, int era) {
		int gregorianYear = M_CheckYEG(year, ref era);
		return CCGregorianCalendar.GetDaysInYear(gregorianYear);
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
	/// <see cref="T:System.DateTime"/> parameter is outside all
	/// supported eras.
	/// </exception>
	public override int GetEra(DateTime time) {
		// M_CheckDateTime not needed, because EraYear does the
		// right thing.
		int rd = CCFixed.FromDateTime(time);
		int era;
		M_EraHandler.EraYear(out era, rd);
		return era;
	}

#if NET_2_0
	[ComVisible (false)]
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
	/// <see cref="T:System.DateTime"/> parameter is outside all
	/// supported eras.
	/// </exception>
	public override int GetMonth(DateTime time) {
		M_CheckDateTime(time);
		return CCGregorianCalendar.GetMonth(time);
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

#if NET_2_0
	[ComVisible (false)]
	public override int GetWeekOfYear (DateTime time, CalendarWeekRule rule, DayOfWeek firstDayOfWeek)
	{
		return base.GetWeekOfYear (time, rule, firstDayOfWeek);
	}
#endif

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
	/// <see cref="T:System.DateTime"/> parameter is outside all
	/// supported eras.
	/// </exception>
	public override int GetYear(DateTime time) {
		// M_CheckDateTime not needed, because EraYeat does the
		// right thing.
		int rd = CCFixed.FromDateTime(time);
		int era;
		return M_EraHandler.EraYear(out era, rd);
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
		int gregorianYear = M_CheckYMDEG(year, month, day, ref era);
		return CCGregorianCalendar.IsLeapDay(gregorianYear, month, day);
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
		M_CheckYMEG(year, month, ref era);
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
		int gregorianYear = M_CheckYEG(year, ref era);
		return CCGregorianCalendar.is_leap_year(gregorianYear);
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
		int gregorianYear = M_CheckYMDEG(year, month, day, ref era);
		M_CheckHMSM(hour, minute, second, millisecond);
		return CCGregorianCalendar.ToDateTime(
			gregorianYear, month, day,
			hour, minute, second, millisecond);
	}


	/// <summary>
	/// This functions returns simply the year for the Japanese calendar.
	/// </summary>
	/// <param name="year">An integer that gives the year.
	/// </param>
	/// <returns>The same argument as the year.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the year is negative or the resulting 
	/// year is invalid.
	/// </exception>
	public override int ToFourDigitYear(int year) {
		if (year < 0)
			throw new ArgumentOutOfRangeException(
				"year", "Non-negative number required.");
		int era = CurrentEra;
		M_CheckYE(year, ref era);
		return year;
	}

#if NET_2_0

#if !NET_2_1
	[ComVisible (false)]
	public override CalendarAlgorithmType AlgorithmType {
		get {
			return CalendarAlgorithmType.SolarCalendar;
		}
	}
#endif

	static DateTime JapanMin = new DateTime (1868, 9, 8, 0, 0, 0);
	static DateTime JapanMax = new DateTime (9999, 12, 31, 11, 59, 59);
		
	[ComVisible (false)]
	public override DateTime MinSupportedDateTime {
		get {
			return JapanMin;
		}
	}

	[ComVisible (false)]
	public override DateTime MaxSupportedDateTime {
		get {
			return JapanMax;
		}
	}
#endif

} // class JapaneseCalendar
	
} // namespace System.Globalization
