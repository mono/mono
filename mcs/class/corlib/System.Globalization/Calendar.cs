// System.Globalization.Calendar.cs
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

/// <remarks>
/// The class serves as a base class for calendar classes.
/// </remarks>
[Serializable]
[ComVisible (true)]
public abstract class Calendar : ICloneable
{
	/// <value>An protected integer property that gives the number of
	/// days in a week. It might be overridden.</value>
	internal virtual int M_DaysInWeek
	{
		get { return 7; }
	}

	/// <summary>
	/// The protected method creates the string used in the
	/// <see cref="T:System.ArgumentOutOfRangeException"/>
	/// </summary>
	/// <param name="a">An object that represents the smallest 
	/// allowable value.</param>
	/// <param name="b">An object that represents the greatest allowable
	/// value.</param>
	/// <returns>The string used in the
	/// <see cref="T:System.ArgumentOutOfRangeException"/>
	/// </returns>
	internal string M_ValidValues(object a, object b)
	{
		StringWriter sw = new StringWriter();
		sw.Write("Valid values are between {0} and {1}, inclusive.",
			a, b);
		return sw.ToString();
	}

	/// <summary>
	/// The protected method checks wether the parameter
	/// <paramref name="arg"/> is in the allowed range.
	/// </summary>
	/// <param name="param">A string that gives the name of the
	/// parameter to check.</param>
	/// <param name="arg">An integer that gives the value to check.
	/// </param>
	/// <param name="a">An integer that represents the smallest allowed
	/// value.</param>
	/// <param name="b">An integer that represents the greatest allowed
	/// value.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if the <paramref name="arg"/> is outside
	/// the allowed range.
	/// </exception>
	internal void M_ArgumentInRange(string param, int arg, int a, int b)
	{
		if (a <= arg && arg <= b)
			return;
		throw new ArgumentOutOfRangeException(param, M_ValidValues(a, b));
	}

	/// <summary>
	/// The protected method, that checks whether
	/// <paramref name="hour"/>, <paramref name="minute"/>,
	/// <paramref name="second"/>, and <parameref name="millisecond"/>
	/// are in their valid ranges
	/// </summary>
	/// <param name="hour">An integer that represents a hour, 
	/// should be between 0 and 23.</param>
	/// <param name="minute">An integer that represents a minute,
	/// should be between 0 and 59.</param>
	/// <param name="second">An integer that represents a second,
	/// should be between 0 and 59.</param>
	/// <param name="milliseconds">An integer that represents a number
	/// of milliseconds, should be between 0 and 999999.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The Exception is thrown, if one of the parameter is outside the
	/// allowed the range.
	/// </exception>
	internal void M_CheckHMSM(int hour, int minute, int second,
		int milliseconds)
	{
		M_ArgumentInRange("hour", hour, 0, 23);
		M_ArgumentInRange("minute", minute, 0, 59);
		M_ArgumentInRange("second", second, 0, 59);
		M_ArgumentInRange("milliseconds", milliseconds, 0, 999999);
	}

	/// <value>
	/// A represantation of the CurrentEra.
	/// </value>
	public const int CurrentEra = 0;

	/// <value>When overridden gives the eras supported by the
	/// calendar as an array of integers.
	/// </value>
	public abstract int[] Eras { get; }

	[NonSerialized]
	bool m_isReadOnly;

	[System.Runtime.InteropServices.ComVisible(false)]
	public virtual CalendarAlgorithmType AlgorithmType {
		get {
			return CalendarAlgorithmType.Unknown;
		}
	}

	[System.Runtime.InteropServices.ComVisible(false)]
	public virtual DateTime MaxSupportedDateTime {
		get {
			return DateTime.MaxValue;
		}
	}

	[System.Runtime.InteropServices.ComVisible(false)]
	public virtual DateTime MinSupportedDateTime {
		get {
			return DateTime.MinValue;
		}
	}

	// LAMESPEC: huh, why not Calendar but Object?
	[ComVisible (false)]
	public virtual object Clone ()
	{
		Calendar c = (Calendar) MemberwiseClone ();
		c.m_isReadOnly = false;
		return c;
	}

	[ComVisible (false)]
	public virtual int GetLeapMonth (int year)
	{
		return GetLeapMonth (year, GetEra (ToDateTime (year, 1, 1, 0, 0, 0, 0)));
	}

	[ComVisible (false)]
	public virtual int GetLeapMonth (int year, int era)
	{
		int max = GetMonthsInYear (year, era);
		for (int i = 1; i <= max; i++)
			if (IsLeapMonth (year, i, era))
				return i;
		return 0;
	}

	[ComVisible (false)]
	public bool IsReadOnly {
		get { return m_isReadOnly; }
	}

	[ComVisible (false)]
	public static Calendar ReadOnly (Calendar calendar)
	{
		if (calendar.m_isReadOnly)
			return calendar;
		Calendar c = (Calendar) calendar.Clone ();
		c.m_isReadOnly = true;
		return c;
	}

	internal void CheckReadOnly ()
	{
		if (m_isReadOnly)
			throw new InvalidOperationException ("This Calendar is read-only.");
	}

	/// <summary>
	/// The protected member stores the value for the
	/// <see cref="P:TwoDigitYearMax"/>
	/// property.
	/// </summary>
	[NonSerialized]
	internal int twoDigitYearMax;
	

	/// <summary>
	/// Private field containing the maximum year for the calendar.
	/// </summary>
	[NonSerialized]
	private int M_MaxYearValue = 0;

	/// <value>
	/// Get-only property returing the maximum allowed year for this
	/// class.
	/// </value>
	internal virtual int M_MaxYear {
		get {
			if (M_MaxYearValue == 0) {
				M_MaxYearValue = GetYear(DateTime.MaxValue);
			}
			return M_MaxYearValue;
		}
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
	internal virtual void M_CheckYE(int year, ref int era)
	{
		//
		// By default, we do nothing.
		//
		// This used to be an abstract method in Mono's implementation,
		// but that means that end-user code could not create their
		// own calendars.
		//
		// Binaries would also crash in this condition.
	}

	/// <value>
	/// <para>The property gives the maximum value for years with two
	/// digits. If the property has the value 2029, than the two-digit
	/// integer 29 results in the year 2029 and 30 in the 
	/// year 1930.</para>
	/// <para>It might be overridden.</para>
	/// </value>
	public virtual int TwoDigitYearMax {
		get { return twoDigitYearMax; }
		set {
			CheckReadOnly ();
			M_ArgumentInRange("year", value, 100, M_MaxYear);
			int era = CurrentEra;
			M_CheckYE(value, ref era);
			twoDigitYearMax = value;
		}
	}

	/// <summary>
	/// The virtual method adds days to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// days.
	/// </param>
	/// <param name="days">The number of days to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="days"/> to the specified
	/// DateTime.</returns>
	public virtual DateTime AddDays(DateTime time, int days) {
		return time.Add(TimeSpan.FromDays(days));
	}

	/// <summary>
	/// The virtual method adds hours to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// hours.
	/// </param>
	/// <param name="hours">The number of hours to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="hours"/> to the specified
	/// DateTime.</returns>
	public virtual DateTime AddHours(DateTime time, int hours) {
		return time.Add(TimeSpan.FromHours(hours));
	}

	/// <summary>
	/// The virtual method adds milliseconds to a given date.
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
	public virtual DateTime AddMilliseconds(DateTime time,
		double milliseconds)
	{
		return time.Add(TimeSpan.FromMilliseconds(milliseconds));
	}

	/// <summary>
	/// The virtual method adds minutes to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// minutes.
	/// </param>
	/// <param name="minutes">The number of minutes to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="minutes"/> to the specified
	/// DateTime.</returns>
	public virtual DateTime AddMinutes(DateTime time, int minutes) {
		return time.Add(TimeSpan.FromMinutes(minutes));
	}

	/// <summary>
	/// When overrideden adds months to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// months.
	/// </param>
	/// <param name="months">The number of months to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="months"/> to the specified
	/// DateTime.</returns>
	public abstract DateTime AddMonths(DateTime time, int months);

	/// <summary>
	/// The virtual method adds seconds to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// seconds.
	/// </param>
	/// <param name="seconds">The number of seconds to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="seconds"/> to the specified
	/// DateTime.</returns>
	public virtual DateTime AddSeconds(DateTime time, int seconds) {
		return time.Add(TimeSpan.FromSeconds(seconds));
	}

	/// <summary>
	/// A wirtual method that adds weeks to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// weeks.
	/// </param>
	/// <param name="weeks">The number of weeks to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="weeks"/> to the specified
	/// DateTime.</returns>
	public virtual DateTime AddWeeks(DateTime time, int weeks) {
		return time.AddDays(weeks * M_DaysInWeek);
	}

	/// <summary>
	/// When overrideden adds years to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// years.
	/// </param>
	/// <param name="years">The number of years to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="years"/> to the specified
	/// DateTime.</returns>
	public abstract DateTime AddYears(DateTime time, int years);

	/// <summary>
	/// When overriden gets the day of the month from
	/// <paramref name="time"/>.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer giving the day of months, starting with 1.
	/// </returns>
	public abstract int GetDayOfMonth(DateTime time);

	/// <summary>
	/// When overriden gets the day of the week from the specified date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer giving the day of months, starting with 1.
	/// </returns>
	public abstract DayOfWeek GetDayOfWeek(DateTime time);

	/// <summary>
	/// When overridden gives the number of the day in the year.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer representing the day of the year,
	/// starting with 1.</returns>
	public abstract int GetDayOfYear(DateTime time);

	/// <summary>
	/// A virtual method that gives the number of days of the specified
	/// month of the <paramref name="year"/> and the
	/// <see cref="P:CurrentEra"/>.
	/// </summary>
	/// <param name="year">An integer that gives the year in the current
	/// era.</param>
	/// <param name="month">An integer that gives the month, starting
	/// with 1.</param>
	/// <returns>An integer that gives the number of days of the
	/// specified month.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if <paramref name="month"/> or
	/// <paramref name="year"/> is outside the allowed range.
	/// </exception>
	public virtual int GetDaysInMonth(int year, int month) {
		return GetDaysInMonth(year, month, CurrentEra);
	}

	/// <summary>
	/// When overridden gives the number of days in the specified month
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
	public abstract int GetDaysInMonth(int year, int month, int era);

	/// <summary>
	/// A virtual method that gives the number of days of the specified
	/// year of the <see cref="P:CurrentEra"/>.
	/// </summary>
	/// <param name="year">An integer that gives the year in the current
	/// era.</param>
	/// <returns>An integer that gives the number of days of the
	/// specified year.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if
	/// <paramref name="year"/> is outside the allowed range.
	/// </exception>
	public virtual int GetDaysInYear(int year) {
		return GetDaysInYear(year, CurrentEra);
	}

	/// <summary>
	/// When overridden gives the number of days of the specified
	/// year of the given era.. 
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
	public abstract int GetDaysInYear(int year, int era);

	/// <summary>
	/// When overridden gives the era of the specified date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer representing the era of the calendar.
	/// </returns>
	public abstract int GetEra(DateTime time);

	/// <summary>
	/// Virtual method that gives the hour of the specified time.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies the
	/// time.
	/// </param>
	/// <returns>An integer that gives the hour of the specified time,
	/// starting with 0.</returns>
	public virtual int GetHour(DateTime time) {
		return time.TimeOfDay.Hours;
	}

	/// <summary>
	/// Virtual method that gives the milliseconds in the current second
	/// of the specified time.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies the
	/// time.
	/// </param>
	/// <returns>An integer that gives the milliseconds in the seconds
	/// of the specified time, starting with 0.</returns>
	public virtual double GetMilliseconds(DateTime time) {
		return time.TimeOfDay.Milliseconds;
	}

	/// <summary>
	/// Virtual method that gives the minute of the specified time.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies the
	/// time.
	/// </param>
	/// <returns>An integer that gives the minute of the specified time,
	/// starting with 0.</returns>
	public virtual int GetMinute(DateTime time) {
		return time.TimeOfDay.Minutes;
	}

	/// <summary>
	/// When overridden gives the number of the month of the specified
	/// date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer representing the month, 
	/// starting with 1.</returns>
	public abstract int GetMonth(DateTime time);

	/// <summary>
	/// Virtual method that gives the number of months of the specified
	/// year of the <see cref="M:CurrentEra"/>.
	/// </summary>
	/// <param name="year">An integer that specifies the year in the
	/// current era.
	/// </param>
	/// <returns>An integer that gives the number of the months in the
	/// specified year.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if the year is not allowed in the
	/// current era.
	/// </exception>
	public virtual int GetMonthsInYear(int year) {
		return GetMonthsInYear(year, CurrentEra);
	}

	/// <summary>
	/// When overridden gives the number of months in the specified year 
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
	public abstract int GetMonthsInYear(int year, int era);

	/// <summary>
	/// Virtual method that gives the second of the specified time.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies the
	/// time.
	/// </param>
	/// <returns>An integer that gives the second of the specified time,
	/// starting with 0.</returns>
	public virtual int GetSecond(DateTime time) {
		return time.TimeOfDay.Seconds;
	}

	/// <summary>
	/// A protected method to calculate the number of days between two
	/// dates.
	/// </summary>
	/// <param name="timeA">A <see cref="T:System.DateTime"/>
	/// representing the first date.
	/// </param>
	/// <param name="timeB">A <see cref="T:System.DateTime"/>
	/// representing the second date.
	/// </param>
	/// <returns>An integer that represents the difference of days
	/// between <paramref name="timeA"/> and <paramref name="timeB"/>.
	/// </returns>
	internal int M_DiffDays(DateTime timeA, DateTime timeB) {
		long diff = timeA.Ticks - timeB.Ticks;

		if (diff >= 0) {
			return (int)(diff/TimeSpan.TicksPerDay);
		}

		diff += 1;
		return -1 + (int)(diff/TimeSpan.TicksPerDay);
	}

	/// <summary>
	/// A protected method that gives the first day of the second week of
	/// the year.
	/// </summary>
	/// <param name="year">An integer that represents the year.</param>
	/// <param name="rule">The
	/// <see cref="T:System.Globalization.CalendarWeekRule"/>
	/// to be used for the calculation.
	/// </param>
	/// <param name="firstDayOfWeek">
	/// The <see cref="T:System.Globalization.DayOfWeek"/>
	/// specifying the first day in a week.
	/// </param>
	/// <returns>The <see cref="T:System.DateTime"/> representing 
	/// the first day of the second week of the year.
	/// </returns>
	internal DateTime M_GetFirstDayOfSecondWeekOfYear(
		int year, CalendarWeekRule rule, DayOfWeek firstDayOfWeek)
	{
		DateTime d1 = ToDateTime(year, 1, 1, 0, 0, 0, 0);
		int dow1 = (int)GetDayOfWeek(d1);
		int fdow = (int)firstDayOfWeek;
		int d = 0;

		switch (rule) {
		case CalendarWeekRule.FirstDay:
			if (fdow > dow1) {
				d += fdow - dow1;
			}
			else {
				d += fdow + M_DaysInWeek - dow1;
			}
			break;
		case CalendarWeekRule.FirstFullWeek:
			d = M_DaysInWeek;
			if (fdow >= dow1) {
				d += fdow - dow1;
			}
			else {
				d += fdow + M_DaysInWeek - dow1;
			}
			break;
		case CalendarWeekRule.FirstFourDayWeek:
			int dow4 = (dow1 + 3)%M_DaysInWeek;

			d = 3;
			if (fdow > dow4) {
				d += fdow - dow4;
			}
			else {
				d += fdow + M_DaysInWeek - dow4;
			}
			break;
		}

		return AddDays(d1, d);
	}

	/// <summary>
	/// A virtual method that gives the number of the week in the year.
	/// </summary>
	/// <param name="time">A 
	/// <see cref="T:System.DateTime"/> representing the date.
	/// </param>
	/// <param name="rule">The
	/// <see cref="T:System.Globalization.CalendarWeekRule"/>
	/// to be used for the calculation.
	/// </param>
	/// <param name="firstDayOfWeek">
	/// The <see cref="T:System.Globalization.DayOfWeek"/>
	/// specifying the first day in a week.
	/// </param>
	/// <returns>An integer representing the number of the week in the
	/// year, starting with 1.
	/// </returns>
	public virtual int GetWeekOfYear(DateTime time,
		CalendarWeekRule rule, 
		DayOfWeek firstDayOfWeek)
	{
		if (firstDayOfWeek < DayOfWeek.Sunday ||
		    DayOfWeek.Saturday < firstDayOfWeek)
		{
		    	throw new ArgumentOutOfRangeException("firstDayOfWeek",
				"Value is not a valid day of week.");
		}
		int year = GetYear(time);

		int days;

		while (true) {
			DateTime secondWeek = M_GetFirstDayOfSecondWeekOfYear(
				year, rule, firstDayOfWeek);
			days = M_DiffDays(time, secondWeek) + M_DaysInWeek;
			if (days >= 0)
				break;
			year -= 1;
		}

		return 1 + days/M_DaysInWeek;
	}

	/// <summary>
	/// When overridden gives the number of the year of the specified
	/// date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer representing the year, 
	/// starting with 1.</returns>
	public abstract int GetYear(DateTime time);

	/// <summary>
	/// A virtual method that tells whether the given day in the
	/// <see cref="M:CurrentEra"/> is a leap day.
	/// </summary>
	/// <param name="year">An integer that specifies the year in the
	/// current era.
	/// </param>
	/// <param name="month">An integer that specifies the month.
	/// </param>
	/// <param name="day">An integer that specifies the day.
	/// </param>
	/// <returns>A boolean that tells whether the given day is a leap
	/// day.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if the year, month or day is not valid
	/// the current era.
	/// </exception>
	public virtual bool IsLeapDay(int year, int month, int day) {
		return IsLeapDay(year, month, day, CurrentEra);
	}

	/// <summary>
	/// Tells when overridden whether the given day 
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
	public abstract bool IsLeapDay(int year, int month, int day, int era);

	/// <summary>
	/// A virtual method that tells whether the given month of the
	/// specified year in the
	/// <see cref="M:CurrentEra"/> is a leap month.
	/// </summary>
	/// <param name="year">An integer that specifies the year in the
	/// current era.
	/// </param>
	/// <param name="month">An integer that specifies the month.
	/// </param>
	/// <returns>A boolean that tells whether the given month is a leap
	/// month.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if the year or month is not valid
	/// the current era.
	/// </exception>
	public virtual bool IsLeapMonth(int year, int month) {
		return IsLeapMonth(year, month, CurrentEra);
	}

	/// <summary>
	/// Tells when overridden whether the given month 
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
	public abstract bool IsLeapMonth(int year, int month, int era);

	/// <summary>
	/// A virtual method that tells whether the given year
	/// in the
	/// <see cref="M:CurrentEra"/> is a leap year.
	/// </summary>
	/// <param name="year">An integer that specifies the year in the
	/// current era.
	/// </param>
	/// <returns>A boolean that tells whether the given year is a leap
	/// year.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if the year is not valid
	/// the current era.
	/// </exception>
	public virtual bool IsLeapYear(int year) {
		return IsLeapYear(year, CurrentEra);
	}

	/// <summary>
	/// Tells when overridden whether the given year
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
	public abstract bool IsLeapYear(int year, int era);

	/// <summary>
	/// A virtual method that creates the
	/// <see cref="T:System.DateTime"/> from the parameters.
	/// </summary>
	/// <param name="year">An integer that gives the year in the
	/// <see cref="M:CurrentEra"/>.
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
	/// <returns>A
	/// <see cref="T:system.DateTime"/> representig the date and time.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if at least one of the parameters
	/// is out of range.
	/// </exception>
	public virtual DateTime ToDateTime(int year, int month, int day,
		int hour, int minute, int second, int millisecond)
	{
		return ToDateTime (year, month, day, hour, minute, second, 
			millisecond, CurrentEra);
	}


	/// <summary>
	/// When overridden creates the
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
	public abstract DateTime ToDateTime(int year, int month, int day,
		int hour, int minute, int second, int millisecond,
		int era);

	/// <summary>
	/// A virtual method that converts a two-digit year to a four-digit
	/// year. It uses the <see cref="M:TwoDigitYearMax"/> property.
	/// </summary>
	/// <param name="year">An integer that gives the two-digit year.
	/// </param>
	/// <returns>An integer giving the four digit year.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the year is negative or the resulting 
	/// year is invalid.
	/// </exception>
	public virtual int ToFourDigitYear(int year) {
		if (year < 0)
			throw new ArgumentOutOfRangeException(
				"year", "Non-negative number required.");
		/* seems not to be the right thing to do, but .NET is
		 * doing it this way.
		 */
		if (year <= 99) {
			int year2 = TwoDigitYearMax%100;
			int d = year - year2;
			year = TwoDigitYearMax + d + (d <= 0 ? 0 : -100); 
		}
		int era = CurrentEra;
		M_CheckYE(year, ref era);
		return year;
	}

	// TwoDigitYearMax: Windows reads it from the Registry, we
	// should have an XML file with the defaults
	/// <summary>
	/// The default constructor, is sets the TwoDigitYearMax to 2029.
	/// </summary>
	/// <remarks>
	/// The .NET framework reads the value from the registry.
	/// We should implement it here. Currently I set the default values
	/// in the ctors of the derived classes, if it is 99.
	/// </remarks>
	protected Calendar() {
		twoDigitYearMax = 99;
	}

	/// <summary>Protected field storing the abbreviated era names.
	/// </summary>
	[NonSerialized]
	internal string[] M_AbbrEraNames;
	/// <summary>Protected field storing the era names.
	/// </summary>
	[NonSerialized]
	internal string[] M_EraNames;

	/// <value>
	/// The property stores the era names. It might be overwritten by
	/// CultureInfo.
	/// </value>
	internal string[] AbbreviatedEraNames {
		get {
			if (M_AbbrEraNames == null ||
			    M_AbbrEraNames.Length != Eras.Length)
				throw new Exception(
					"Internal: M_AbbrEraNames " +
					"wrong initialized!");
			return (string[])M_AbbrEraNames.Clone();
		}
		set {
			CheckReadOnly ();
			if (value.Length != Eras.Length) {
				StringWriter sw = new StringWriter();
				sw.Write("Array length must be equal Eras " +
					"length {0}.", Eras.Length);
				throw new ArgumentException(
					sw.ToString());
			} 
			M_AbbrEraNames = (string[])value.Clone();
		}
	}

	/// <value>
	/// The property stores the era names. It might be overwritten by
	/// CultureInfo.
	/// </value>
	internal string[] EraNames {
		get {
			if (M_EraNames == null || M_EraNames.Length != Eras.Length)
				throw new Exception ("Internal: M_EraNames not initialized!");

			return M_EraNames;
		}
		set {
			CheckReadOnly ();
			if (value.Length != Eras.Length) {
				StringWriter sw = new StringWriter();
				sw.Write("Array length must be equal Eras " +
					"length {0}.", Eras.Length);
				throw new ArgumentException(
					sw.ToString());
			} 
			M_EraNames = (string[])value.Clone();
		}
	}

#pragma warning disable 649
	internal int m_currentEraValue; // Unused, by MS serializes this
#pragma warning restore 649
}
	
}
