// System.Globalization.GregorianCalendar.cs
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
/// This is the Gregorian calendar.
/// </summary>
/// <remarks>
/// <para>The Gregorian calendar supports only the Common Era from
/// the Gregorian year 1 to the Gregorian year 9999.
/// </para>
/// <para>The implementation uses the
/// <see cref="N:CalendricalCalculations"/> namespace.
/// </para>
/// </remarks>
[Serializable]
#if NET_2_0
[ComVisible (true)]
#endif
[MonoTODO ("Serialization format not compatible with .NET")]
public class GregorianCalendar : Calendar {

	/// <summary>
	/// The era number for the Common Era (C.E.) or Anno Domini (A.D.)
	/// respective.
	/// </summary>
	public const int ADEra = 1;

	/// <value>Overridden. Gives the eras supported by the Gregorian
	/// calendar as an array of integers.
	/// </value>
	public override int[] Eras {
		get {
			return new int[] { ADEra }; 
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
	/// A protected member storing the
	/// <see cref="T:System.Globalization.GregorianCalendarTypes"/>.
	/// </summary>
	[NonSerialized]
	internal GregorianCalendarTypes m_type;

	/// <value>
	/// The property stores the 
	/// <see cref="T:System.Globalization.GregorianCalendarTypes"/>.
	/// </value>
	public virtual GregorianCalendarTypes CalendarType {
		get { return m_type; }
		set { 
			CheckReadOnly ();
			// mscorlib 1:0:3300:0 doesn't check anything here
			m_type = value;
		}
	}

	/// <summary>
	/// A protected method checking the era number.
	/// </summary>
	/// <param name="era">The era number.</param>
	/// <exception name="T:System.ArgumentException">
	/// The exception is thrown if the era is not equal
	/// <see cref="M:ADEra"/>.
	/// </exception>
	internal void M_CheckEra(ref int era) {
		if (era == CurrentEra)
			era = ADEra;
		if (era != ADEra)
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
	/// <see cref="M:ADEra"/>.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the calendar year is outside of
	/// the allowed range.
	/// </exception>
	internal override void M_CheckYE(int year, ref int era) {
		M_CheckEra(ref era);
		M_ArgumentInRange("year", year, 1, 9999);
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
	/// <see cref="M:ADEra"/>.
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
	/// <see cref="M:ADEra"/>.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the calendar year, month, or day is
	/// outside of the allowed range.
	/// </exception>
	internal void M_CheckYMDE(int year, int month, int day, ref int era)
	{
		M_CheckYME(year, month, ref era);
		M_ArgumentInRange("day", day, 1,
			GetDaysInMonth(year, month, era));
	}

	/// <summary>
	/// Overridden. Adds months to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// months.
	/// </param>
	/// <param name="months">The number of months to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="months"/> to the specified
	/// DateTime.</returns>
	public override DateTime AddMonths(DateTime time, int months) {
		return CCGregorianCalendar.AddMonths(time, months);
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
	public override DateTime AddYears(DateTime time, int years) {
		return CCGregorianCalendar.AddYears(time, years);
	}
		
	/// <summary>
	/// Overridden. Gets the day of the month from
	/// <paramref name="time"/>.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer giving the day of months, starting with 1.
	/// </returns>
	public override int GetDayOfMonth(DateTime time) {
		return CCGregorianCalendar.GetDayOfMonth(time);
	}

	/// <summary>
	/// Overridden. Gets the day of the week from the specified date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer giving the day of months, starting with 1.
	/// </returns>
	public override DayOfWeek GetDayOfWeek(DateTime time) {
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
	public override int GetDayOfYear(DateTime time) {
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
		// mscorlib doesn't check year, probably a bug; we do
		M_CheckYME(year, month, ref era);
		return CCGregorianCalendar.GetDaysInMonth(year, month);
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
		return CCGregorianCalendar.GetDaysInYear(year);
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
	public override int GetEra(DateTime time) {
		return ADEra;
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
	public override int GetMonth(DateTime time) {
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
	public override int GetYear(DateTime time) {
		return CCGregorianCalendar.GetYear(time);
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
		return CCGregorianCalendar.IsLeapDay(year, month, day);
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
		return CCGregorianCalendar.is_leap_year(year);
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
	/// <returns>
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
		return CCGregorianCalendar.ToDateTime(
			year, month, day,
			hour, minute, second, millisecond);
	}

	public override int ToFourDigitYear(int year)
	{
		return base.ToFourDigitYear (year);
	}
					
	/// <summary>
	/// Constructor that sets the
	/// Gregorian calendar type (
	/// <see cref="T:System.Globalization.GregorianCalendarTypes"/>).
	/// </summary>
	/// <param name="type">The parameter specifies the Gregorian 
	/// calendar type.
	/// </param>
	public GregorianCalendar(GregorianCalendarTypes type) {
		CalendarType = type;
		M_AbbrEraNames = new string[] {"AD"};
		M_EraNames = new string[] {"A.D."};
		if (twoDigitYearMax == 99)
			twoDigitYearMax = 2029;
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

	static DateTime? Min, Max;
		
	[ComVisible (false)]
	public override DateTime MinSupportedDateTime {
		get {
			if (Min == null)
				Min = new DateTime (1, 1, 1, 0, 0, 0);
			return Min.Value;
		}
	}

	[ComVisible (false)]
	public override DateTime MaxSupportedDateTime {
		get {
			if (Max == null)
				Max = new DateTime (9999, 12, 31, 11, 59, 59);
			return Max.Value;
		}
	}
#endif
	
	/// <summary>
	/// Default constructor. Sets the Gregorian calendar type to 
	/// <see
	/// cref="F:System.Globalization.GregorianCalendarTypes.Localized"/>.
	/// </summary>
	public GregorianCalendar() : this(GregorianCalendarTypes.Localized) {}
} // class GregorianCalendar
	
} // namespace System.Globalization
