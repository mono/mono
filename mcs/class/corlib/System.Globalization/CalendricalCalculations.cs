// CalendricalCalculations.cs
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

using System.Collections;

/// <summary>A class that provides mathematical functions.</summary>
/// <remarks>
/// <para>
/// We are breaking the .Net
/// naming conventions to be compatible to the "Calendrical Calculations"
/// bool. 
/// </para>
/// </remarks>
static class CCMath {
	/// <summary>
	/// A static method which rounds a double value.
	/// </summary>
	/// <param name="x">The double value to round.</param>
	/// <returns>The rounded double.</returns>
	public static double round(double x) {
		return System.Math.Floor(x+0.5);
	}

	/// <summary>
	/// A static method that computes the remainder of the division
	/// of two doubles.
	/// </summary>
	/// <param name="x">The double value which is divided.</param>
	/// <param name="y">The divisor.</param>
	/// <returns>The remainder as double value.</returns>
	public static double mod(double x, double y) {
		return x - y * System.Math.Floor(x/y);
	}

	/// <summary>
	/// The static method divides two integers.
	/// </summary>
	/// <param name="x">The integer x value.</param>
	/// <param name="y">The integer y value.</param>
	/// <returns>The qotient of x and y defined by floor(x/y).
	/// </returns>
	/// <remarks>
	/// Please notify that the function is not compatible to the standard
	/// integer divide operation /.
	/// </remarks>
	public static int div(int x, int y) {
		return (int)System.Math.Floor((double)x/(double)y);
	}

	/// <summary>
	/// The static method computes the remainder of two integers.
	/// </summary>
	/// <param name="x">The integer value which will be divided.</param>
	/// <param name="y">The divisor integer value.</param>
	/// <returns> The remainder as integer value.</returns>
	/// <remarks>
	/// Please notify that the method is not compatible to the C#
	/// remainder operation %.
	/// </remarks>
	public static int mod(int x, int y) {
		return x - y * div(x, y);
	}

	/// <summary>
	/// A static method that combines integer division and remainder
	/// computation.
	/// </summary>
	/// <param name="remainder">Remainder integer output value.
	/// </param>
	/// <param name="x">Integer to be divided.</param>
	/// <param name="y">Divisor integer value.</param>
	/// <returns>The quotient as integer.</returns>
	/// <seealso cref="M:div"/>
	/// <seealso cref="M:mod"/>
	public static int div_mod(out int remainder, int x, int y) {
		int d = div(x, y);
		remainder = x - y * d;
		return d;
	}

	/// <summary>
	/// A static method returning the sign of the argument.
	/// </summary>
	/// <param name="x">The double argument.</param>
	/// <returns>An integer value: -1 for a negative argument;
	/// 0 for a zero argument, and 1 for a positive argument.
	/// </returns>
	public static int signum(double x) {
		if (x < 0.0)
			return -1;
		if (x == 0.0)
			return 0;
		return 1;
	}

	/// <summary>
	/// A static method returning the sign of the integer
	/// argument.
	/// </summary>
	/// <param name="x">The integer argument.</param>
	/// <returns>An integer value: -1 for a negative argument;
	/// 0 for a zero argument, and 1 for a positive argument.
	/// </returns>
	public static int signum(int x) {
		if (x < 0)
			return -1;
		if (x == 0)
			return 0;
		return 1;
	}

	/// <summary>
	/// An adjusted remainder function as defined in "Calendrical
	/// Calculations".
	/// </summary>
	/// <param name="x">The double x argument.</param>
	/// <param name="y">The double y argument, the divisor.</param>
	/// <returns>A double value representing remainder; but instead 0.0
	/// the divisor y is returned.
	/// </returns>
	public static double amod(double x, double y) {
		double d = mod(x, y);
		return (d == 0.0) ? y : d;
	}

	/// <summary>
	/// The adjusted remainder functions for integers as defined in
	/// "Calendrical Calculations".
	/// </summary>
	/// <param name="x">The integer argument to be divided.</param>
	/// <param name="y">The integer divisor argument.</param>
	/// <returns>The remainder as an integer; however instead 0 
	/// is the divisor y returned.
	/// </returns>
	public static int amod(int x, int y) {
		int i = mod(x, y);
		return (i == 0) ? y : i;
	}
}

/// <summary>The class implements methods to handle the fixed date value from
/// the "Calendrical Calculations" books.
/// </summary>
/// <remarks>
/// <para>
/// For implementing the Calendar classes I used the algorithms from the
/// book "Calendrical Calculations" by Nachum Dershowitz and Edward M.
/// Rheingold, second reprint 1998. Trying to prevent the introduction of new
/// bugs, I implemented their algorithms in the
/// <see cref="N:CalendricalCalculations"/>
/// namespace and wrapped it in the calendar classes.
/// </para>
/// <para>
/// The fixed day number is also known as R.D. - rata die.
/// Midnight at the onset of Monday,
/// January 1, year 1 (Gregorian) is R.D. 1.
/// </para>
/// <para>Here are all my references:</para>
/// <list type="table">
/// <item><description>
/// [1] Nachum Dershowitz and Edward M. Rheingold: "Calendrical Calculations";
/// Cambridge University Press; second reprint 1998.
/// </description></item>
/// <item><description>
/// [2] P. Kenneth Seidelmann (ed.): "Explanatory Supplement to the Astronomical
/// Almanac"; University Science Books, Sausalito; 1992 
/// </description></item>
/// <item><description>
/// [3] F. Richard Stephenson: "Historical Eclipses and Earth Rotation";
/// Cambridge University Press; 1997
/// </description></item>
/// </list>
/// </remarks>
static class CCFixed {
	/// <summary>The method computes the
	/// <see cref="T:System.DateTime"/>
	/// from a fixed day number.
	/// </summary>
	/// <param name="date">A integer representing the fixed day number.
	/// </param>
	/// <returns>The <see cref="T:System.DateTime"/> representing
	/// the date.
	/// </returns>
	public static System.DateTime ToDateTime(int date) {
		long ticks = (date - 1) * System.TimeSpan.TicksPerDay;
		return new System.DateTime(ticks);
	}

	/// <summary>The method computes the
	/// <see cref="T:System.DateTime"/>
	/// from a fixed day number and time arguments.
	/// </summary>
	/// <param name="date">An integer representing the fixed day number.
	/// </param>
	/// <param name="hour">An integer argument specifying the hour.
	/// </param>
	/// <param name="minute">An integer argument specifying the minute.
	/// </param>
	/// <param name="second">An integer argument giving the second.
	/// </param>
	/// <param name="milliseconds">An double argument specifying
	/// the milliseconds. Notice that
	/// <see cref="T:System.DateTime"/> has 100 nanosecond resolution.
	/// </param>
	/// <returns>The <see cref="T:System.DateTime"/> representing
	/// the date.
	/// </returns>
	public static System.DateTime ToDateTime(int date,
		int hour, int minute, int second, double milliseconds)
	{
		System.DateTime time = ToDateTime(date);
		time = time.AddHours(hour);
		time = time.AddMinutes(minute);
		time = time.AddSeconds(second);
		return time.AddMilliseconds(milliseconds);
	}

	/// <summary>
	/// A static method computing the fixed day number from a 
	/// <see cref="T:System.DateTime"/> value.
	/// </summary>
	/// <param name="time">A
	/// <see cref="T:System.DateTime"/> value representing the date.
	/// </param>
	/// <returns>The fixed day number as integer representing the date.
	/// </returns>
	public static int FromDateTime(System.DateTime time) {
		return 1 + (int)(time.Ticks / System.TimeSpan.TicksPerDay);
	}

	/// <summary>
	/// The static method computes the <see cref="T:DayOfWeek"/>.
	/// </summary>
	/// <param name="date">An integer representing the fixed day number.
	/// </param>
	/// <returns>The day of week.</returns>
	public static DayOfWeek day_of_week(int date) {
		return (DayOfWeek)CCMath.mod(date, 7);
	}

	/// <summary>
	/// The static method computes the date of a day of week on or before
	/// a particular date.
	/// </summary>
	/// <param name="date">An integer representing the date as
	/// fixed day number.
	/// </param>
	/// <param name="k">An integer representing the day of the week,
	/// starting with 0 for sunday.
	/// </param>
	/// <returns>The fixed day number of the day of week specified by k
	/// on or before the given date.
	/// </returns>
	public static int kday_on_or_before(int date, int k) {
		return date - (int)day_of_week(date-k);
	}

	/// <summary>
	/// The static method computes the date of a day of week on or after
	/// a particular date.
	/// </summary>
	/// <param name="date">An integer representing the date as
	/// fixed day number.
	/// </param>
	/// <param name="k">An integer representing the day of the week,
	/// starting with 0 for sunday.
	/// </param>
	/// <returns>The fixed day number of the day of week specified by k
	/// on or after the given date.
	/// </returns>
	public static int kday_on_or_after(int date, int k) {
		return kday_on_or_before(date+6, k);
	}

	/// <summary>
	/// The static method computes the date of a day of week that is
	/// nearest to a particular date.
	/// </summary>
	/// <param name="date">An integer representing the date as
	/// fixed day number.
	/// </param>
	/// <param name="k">An integer representing the day of the week,
	/// starting with 0 for sunday.
	/// </param>
	/// <returns>The fixed day number of the day of week neares to the
	/// given date.
	/// </returns>
	public static int kd_nearest(int date, int k) {
		return kday_on_or_before(date+3, k);
	}

	/// <summary>
	/// The static method computes the date of a day of week after
	/// a particular date.
	/// </summary>
	/// <param name="date">An integer representing the date as
	/// fixed day number.
	/// </param>
	/// <param name="k">An integer representing the day of the week,
	/// starting with 0 for sunday.
	/// </param>
	/// <returns>The fixed day number of the day of week specified by k
	/// after the given date.
	/// </returns>
	public static int kday_after(int date, int k) {
		return kday_on_or_before(date+7, k);
	}

	/// <summary>
	/// The static method computes the date of a day of week before
	/// a particular date.
	/// </summary>
	/// <param name="date">An integer representing the date as
	/// fixed day number.
	/// </param>
	/// <param name="k">An integer representing the day of the week,
	/// starting with 0 for sunday.
	/// </param>
	/// <returns>The fixed day number of the day of week specified by k
	/// before the given date.
	/// </returns>
	public static int kday_before(int date, int k) {
		return kday_on_or_before(date-1, k);
	}
} // class CCFixed

/// <summary>
/// A class encapsulating the functions of the Gregorian calendar as static
/// methods.
/// </summary>
/// <remarks>
/// <para>
/// This class is not compatible to
/// <see cref="T:System.Globalization.GregorianCalendar"/>.
/// </para>
/// <para>
/// The fixed day number is also known as R.D. - rata die.
/// Midnight at the onset of Monday,
/// January 1, year 1 (Gregorian) is R.D. 1.
/// </para>
/// <seealso cref="T:CCFixed"/>
/// </remarks>
static class CCGregorianCalendar {
	/// <summary>An integer defining the epoch of the Gregorian calendar
	/// as fixed day number.</summary>
	/// <remarks>The epoch is January 3, 1 C.E. (Julian).</remarks>
	const int epoch = 1;

	/// <summary>The enumeration defines the months of the Gregorian
	/// calendar.
	/// </summary>
	public enum Month {
		/// <summary>
		/// January.
		/// </summary>
		january = 1,
		/// <summary>
		/// February.
		/// </summary>
		february,
		/// <summary>
		/// March.
		/// </summary>
		march,
		/// <summary>
		/// April.
		/// </summary>
		april,
		/// <summary>
		/// May.
		/// </summary>
		may,
		/// <summary>
		/// June.
		/// </summary>
		june,
		/// <summary>
		/// July.
		/// </summary>
		july,
		/// <summary>
		/// August.
		/// </summary>
		august,
		/// <summary>
		/// September.
		/// </summary>
		september,
		/// <summary>
		/// October.
		/// </summary>
		october,
		/// <summary>
		/// November.
		/// </summary>
		november,
		/// <summary>
		/// December.
		/// </summary>
		december
	};


	/// <summary>
	/// The method tells whether the year is a leap year.
	/// </summary>
	/// <param name="year">An integer representing the Gregorian year.
	/// </param>
	/// <returns>A boolean which is true if <paramref name="year"/> is
	/// a leap year.
	/// </returns>
	public static bool is_leap_year(int year) {
		if (CCMath.mod(year, 4) != 0)
			return false;
		switch (CCMath.mod(year, 400)) {
			case 100: 
				return false;
			case 200:
				return false;
			case 300:
				return false;
		}
		return true;
	}

	/// <summary>
	/// The method returns the fixed day number of the given Gregorian
	/// date.
	/// </summary>
	/// <param name="day">An integer representing the day of the month,
	/// counting from 1.
	/// </param>
	/// <param name="month">An integer representing the month in the
	/// Gregorian year.
	/// </param>
	/// <param name="year">An integer representing the Gregorian year.
	/// Non-positive values are allowed also.
	/// </param>
	/// <returns>An integer value representing the fixed day number.
	/// </returns>
	public static int fixed_from_dmy(int day, int month, int year) {
		int k = epoch - 1;
		k += 365 * (year-1);
		k += CCMath.div(year-1, 4);
		k -= CCMath.div(year-1, 100);
		k += CCMath.div(year-1, 400);
		k += CCMath.div(367*month-362, 12);
		if (month > 2) {
			k += is_leap_year(year) ? -1 : -2;
		}

		k += day;

		return k;
	}

	/// <summary>
	/// The method computes the Gregorian year from a fixed day number.
	/// </summary>
	/// <param name="date">The fixed day number.
	/// </param>
	/// <returns>An integer value giving the Gregorian year of the date.
	/// </returns>
	public static int year_from_fixed(int date) {
		int d		= date - epoch;
		int n_400	= CCMath.div_mod(out d, d, 146097);
		int n_100	= CCMath.div_mod(out d, d, 36524);
		int n_4		= CCMath.div_mod(out d, d, 1461);
		int n_1		= CCMath.div(d, 365);

		int year = 400*n_400 + 100*n_100 + 4*n_4 + n_1;
		return (n_100 == 4 || n_1 == 4) ? year : year + 1;
	}

	/// <summary>
	/// The method computes the Gregorian year and month from a fixed day
	/// number.
	/// </summary>
	/// <param name="month">The output value giving the Gregorian month.
	/// </param>
	/// <param name="year">The output value giving the Gregorian year.
	/// </param>
	/// <param name="date">An integer value specifying the fixed day
	/// number.</param>
	public static void my_from_fixed(out int month, out int year,
		int date)
	{
		year = year_from_fixed(date);

		int prior_days = date - fixed_from_dmy(1, (int)Month.january,
			year);
		
		int correction;
		if (date < fixed_from_dmy(1, (int)Month.march, year)) {
			correction = 0;
		} else if (is_leap_year(year)) {
			correction = 1;
		} else {
			correction = 2;
		}

		month = CCMath.div(12 * (prior_days + correction) + 373, 367);

	}

	/// <summary>
	/// The method computes the Gregorian year, month, and day from a
	/// fixed day number.
	/// </summary>
	/// <param name="day">The output value returning the day of the
	/// month.
	/// </param>
	/// <param name="month">The output value giving the Gregorian month.
	/// </param>
	/// <param name="year">The output value giving the Gregorian year.
	/// </param>
	/// <param name="date">An integer value specifying the fixed day
	/// number.</param>
	public static void dmy_from_fixed(out int day, out int month,
		out int year,
		int date)
	{
		my_from_fixed(out month, out year, date);
		day = date - fixed_from_dmy(1, month, year) + 1;
	}

	/// <summary>A method computing the Gregorian month from a fixed
	/// day number.
	/// </summary>
	/// <param name="date">An integer specifying the fixed day number.
	/// </param>
	/// <returns>An integer value representing the Gregorian month.
	/// </returns>
	public static int month_from_fixed(int date) {
		int month, year;

		my_from_fixed(out month, out year, date);
		return month;
	}

	/// <summary>
	/// A method computing the day of the month from a fixed day number.
	/// </summary>
	/// <param name="date">An integer specifying the fixed day number.
	/// </param>
	/// <returns>An integer value representing the day of the month.
	/// </returns>
	public static int day_from_fixed(int date) {
		int day, month, year;

		dmy_from_fixed(out day, out month, out year, date);
		return day;
	}

	/// <summary>
	/// The method computes the difference between two Gregorian dates.
	/// </summary>
	/// <param name="dayA">The integer parameter gives the day of month
	/// of the first date.
	/// </param>
	/// <param name="monthA">The integer parameter gives the Gregorian
	/// month of the first date.
	/// </param>
	/// <param name="yearA">The integer parameter gives the Gregorian
	/// year of the first date.
	/// </param>
	/// <param name="dayB">The integer parameter gives the day of month
	/// of the second date.
	/// </param>
	/// <param name="monthB">The integer parameter gives the Gregorian
	/// month of the second date.
	/// </param>
	/// <param name="yearB">The integer parameter gives the Gregorian
	/// year of the second date.
	/// </param>
	/// <returns>An integer giving the difference of days from the first
	/// the second date.
	/// </returns>
	public static int date_difference(int dayA, int monthA, int yearA,
		int dayB, int monthB, int yearB)
	{
		return	fixed_from_dmy(dayB, monthB, yearB) -
			fixed_from_dmy(dayA, monthA, yearA);
	}

	/// <summary>
	/// The method computes the number of the day in the year from
	/// a Gregorian date.
	/// </summary>
	/// <param name="day">An integer representing the day of the month,
	/// counting from 1.
	/// </param>
	/// <param name="month">An integer representing the month in the
	/// Gregorian year.
	/// </param>
	/// <param name="year">An integer representing the Gregorian year.
	/// Non-positive values are allowed also.
	/// </param>
	/// <returns>An integer value giving the number of the day in the
	/// Gregorian year, counting from 1.
	/// </returns>
	public static int day_number(int day, int month, int year) {
		return date_difference(31, (int)Month.december, year-1,
			day, month, year);
	}

	/// <summary>
	/// The method computes the days remaining in the given Gregorian
	/// year from a Gregorian date.
	/// </summary>
	/// <param name="day">An integer representing the day of the month,
	/// counting from 1.
	/// </param>
	/// <param name="month">An integer representing the month in the
	/// Gregorian year.
	/// </param>
	/// <param name="year">An integer representing the Gregorian year.
	/// Non-positive values are allowed also.
	/// </param>
	/// <returns>An integer value giving the number of days remaining in
	/// the Gregorian year.
	/// </returns>
	public static int days_remaining(int day, int month, int year) {
		return date_difference(day, month, year,
			31, (int)Month.december, year);
	}

	// Helper functions for the Gregorian calendars.

	/// <summary>
	/// Adds months to the given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// months.
	/// </param>
	/// <param name="months">The number of months to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="months"/> to the specified
	/// DateTime.</returns>
	public static System.DateTime AddMonths(System.DateTime time,
		int months)
	{
		int rd = CCFixed.FromDateTime(time);
		int day, month, year;
		dmy_from_fixed(out day, out month, out year, rd);
		month += months;
		year += CCMath.div_mod(out month, month, 12);
		int maxday = GetDaysInMonth (year, month);
		if (day > maxday)
			day = maxday;
		rd = fixed_from_dmy(day, month, year);
		System.DateTime t = CCFixed.ToDateTime(rd);
		return t.Add(time.TimeOfDay);
	}

	/// <summary>
	/// Adds years to the given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// months.
	/// </param>
	/// <param name="years">The number of years to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="years"/> to the specified
	/// DateTime.</returns>
	public static System.DateTime AddYears(System.DateTime time,
		int years)
	{
		int rd = CCFixed.FromDateTime(time);
		int day, month, year;
		dmy_from_fixed(out day, out month, out year, rd);
		year += years;
		int maxday = GetDaysInMonth (year, month);
		if (day > maxday)
			day = maxday;
		rd = fixed_from_dmy(day, month, year);
		System.DateTime t = CCFixed.ToDateTime(rd);
		return t.Add(time.TimeOfDay);
	}

	/// <summary>
	/// Gets the of the month from <paramref name="time"/>.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer giving the day of months, starting with 1.
	/// </returns>
	public static int GetDayOfMonth(System.DateTime time) {
		return day_from_fixed(CCFixed.FromDateTime(time));
	}

	/// <summary>
	/// The method gives the number of the day in the year.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer representing the day of the year,
	/// starting with 1.</returns>
	public static int GetDayOfYear(System.DateTime time) {
		int rd = CCFixed.FromDateTime(time);
		int year = year_from_fixed(rd);
		int rd1_1 = fixed_from_dmy(1, 1, year);
		return rd - rd1_1 + 1;
	}

	/// <summary>
	/// A method that gives the number of days of the specified
	/// month of the <paramref name="year"/>.
	/// </summary>
	/// <param name="year">An integer that gives the year in the current
	/// era.</param>
	/// <param name="month">An integer that gives the month, starting
	/// with 1.</param>
	/// <returns>An integer that gives the number of days of the
	/// specified month.</returns>
	public static int GetDaysInMonth(int year, int month) {
		int rd1 = fixed_from_dmy(1, month, year);
		int rd2 = fixed_from_dmy(1, month+1, year);
		return rd2 - rd1;
	}

	/// <summary>
	/// The method gives the number of days in the specified year.
	/// </summary>
	/// <param name="year">An integer that gives the year.
	/// </param>
	/// <returns>An integer that gives the number of days of the
	/// specified year.</returns>
	public static int GetDaysInYear(int year) {
		int rd1 = fixed_from_dmy(1, 1, year);
		int rd2 = fixed_from_dmy(1, 1, year+1);
		return rd2 - rd1;
	}

	/// <summary>
	/// The method gives the number of the month of the specified
	/// date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer representing the month, 
	/// starting with 1.</returns>
	public static int GetMonth(System.DateTime time) {
		return month_from_fixed(CCFixed.FromDateTime(time));
	}

	/// <summary>
	/// The method gives the number of the year of the specified
	/// date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer representing the year. 
	/// </returns>
	public static int GetYear(System.DateTime time) {
		return year_from_fixed(CCFixed.FromDateTime(time));
	}

	/// <summary>
	/// A virtual method that tells whether the given day
	/// is a leap day.
	/// </summary>
	/// <param name="year">An integer that specifies the year.
	/// </param>
	/// <param name="month">An integer that specifies the month.
	/// </param>
	/// <param name="day">An integer that specifies the day.
	/// </param>
	/// <returns>A boolean that tells whether the given day is a leap
	/// day.
	/// </returns>
	public static bool IsLeapDay(int year, int month, int day) {
		return is_leap_year(year) && month == 2 && day == 29;
	}

	/// <summary>
	/// A method that creates the
	/// <see cref="T:System.DateTime"/> from the parameters.
	/// </summary>
	/// <param name="year">An integer that gives the year
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
	public static System.DateTime ToDateTime(int year, int month, int day,
		int hour, int minute, int second, int milliseconds)
	{
		return CCFixed.ToDateTime(fixed_from_dmy(day, month, year),
			hour, minute, second, milliseconds);
	}
} // class CCGregorianCalendar

/// <summary>
/// A class encapsulating the functions of the Julian calendar as static
/// methods.
/// </summary>
/// <remarks>
/// <para>The algorithms don't support a year 0. Years before Common Era
/// (B.C.E. or B.C.) are negative and years of Common Era (C.E. or A.D.)
/// are positive.
/// </para>
/// <para>
/// This class is not compatible to
/// <see cref="T:System.Globalization.JulianCalendar"/>.
/// </para>
/// <seealso cref="T:CCFixed"/>
/// </remarks>
static class CCJulianCalendar {
	/// <summary>An integer defining the epoch of the Julian calendar
	/// as fixed day number.</summary>
	/// <remarks>The epoch is December 30, 0 (Gregorian).</remarks>
	const int epoch = -1; // 30. 12. 0 Gregorian

	/// <summary>The enumeration defines the months of the Julian
	/// calendar.
	/// </summary>
	public enum Month {
		/// <summary>
		/// January.
		/// </summary>
		january = 1,
		/// <summary>
		/// February.
		/// </summary>
		february,
		/// <summary>
		/// March.
		/// </summary>
		march,
		/// <summary>
		/// April.
		/// </summary>
		april,
		/// <summary>
		/// May.
		/// </summary>
		may,
		/// <summary>
		/// June.
		/// </summary>
		june,
		/// <summary>
		/// July.
		/// </summary>
		july,
		/// <summary>
		/// August.
		/// </summary>
		august,
		/// <summary>
		/// September.
		/// </summary>
		september,
		/// <summary>
		/// October.
		/// </summary>
		october,
		/// <summary>
		/// November.
		/// </summary>
		november,
		/// <summary>
		/// December.
		/// </summary>
		december
	};

	/// <summary>
	/// The method tells whether the year is a leap year.
	/// </summary>
	/// <param name="year">An integer representing the Julian year.
	/// </param>
	/// <returns>A boolean which is true if <paramref name="year"/> is
	/// a leap year.
	/// </returns>
	public static bool is_leap_year(int year) {
		return CCMath.mod(year, 4) == (year > 0 ? 0 : 3);
	}

	/// <summary>
	/// The method returns the fixed day number of the given Julian
	/// date.
	/// </summary>
	/// <param name="day">An integer representing the day of the month,
	/// counting from 1.
	/// </param>
	/// <param name="month">An integer representing the month in the
	/// Julian year.
	/// </param>
	/// <param name="year">An integer representing the Julian year.
	/// Positive and Negative values are allowed.
	/// </param>
	/// <returns>An integer value representing the fixed day number.
	/// </returns>
	public static int fixed_from_dmy(int day, int month, int year) {
		int y = year < 0 ? year+1 : year;
		int k = epoch - 1;
		k += 365 * (y-1);
		k += CCMath.div(y-1, 4);
		k += CCMath.div(367*month-362, 12);
		if (month > 2) {
			k += is_leap_year(year) ? -1 : -2;
		}
		k += day;

		return k;
	}

	/// <summary>
	/// The method computes the Julian year from a fixed day number.
	/// </summary>
	/// <param name="date">The fixed day number.
	/// </param>
	/// <returns>An integer value giving the Julian year of the date.
	/// </returns>
	public static int year_from_fixed(int date) {
		int approx = CCMath.div(4*(date-epoch)+1464, 1461);
		return approx <= 0 ? approx - 1 : approx;
	}

	/// <summary>
	/// The method computes the Julian year and month from a fixed day
	/// number.
	/// </summary>
	/// <param name="month">The output value giving the Julian month.
	/// </param>
	/// <param name="year">The output value giving the Julian year.
	/// </param>
	/// <param name="date">An integer value specifying the fixed day
	/// number.</param>
	public static void my_from_fixed(out int month, out int year, int date)
	{
		year = year_from_fixed(date);

		int prior_days = date - fixed_from_dmy(1, (int)Month.january,
			year);
		
		int correction;
		if (date < fixed_from_dmy(1, (int)Month.march, year)) {
			correction = 0;
		} else if (is_leap_year(year)) {
			correction = 1;
		} else {
			correction = 2;
		}

		month = CCMath.div(12 * (prior_days + correction) + 373, 367);
	}
	

	/// <summary>
	/// The method computes the Julian year, month, and day from a
	/// fixed day number.
	/// </summary>
	/// <param name="day">The output value returning the day of the
	/// month.
	/// </param>
	/// <param name="month">The output value giving the Julian month.
	/// </param>
	/// <param name="year">The output value giving the Julian year.
	/// </param>
	/// <param name="date">An integer value specifying the fixed day
	/// number.</param>
	public static void dmy_from_fixed(out int day, out int month,
		out int year, int date)
	{
		my_from_fixed(out month, out year, date);
		day = date - fixed_from_dmy(1, month, year) + 1;
	}

	/// <summary>A method computing the Julian month from a fixed
	/// day number.
	/// </summary>
	/// <param name="date">An integer specifying the fixed day number.
	/// </param>
	/// <returns>An integer value representing the Julian month.
	/// </returns>
	public static int month_from_fixed(int date) {
		int month, year;

		my_from_fixed(out month, out year, date);
		return month;
	}

	/// <summary>
	/// A method computing the day of the month from a fixed day number.
	/// </summary>
	/// <param name="date">An integer specifying the fixed day number.
	/// </param>
	/// <returns>An integer value representing the day of the month.
	/// </returns>
	public static int day_from_fixed(int date) {
		int day;
		int month;
		int year;

		dmy_from_fixed(out day, out month, out year, date);
		return day;
	}

	/// <summary>
	/// The method computes the difference between two Julian dates.
	/// </summary>
	/// <param name="dayA">The integer parameter gives the day of month
	/// of the first date.
	/// </param>
	/// <param name="monthA">The integer parameter gives the Julian
	/// month of the first date.
	/// </param>
	/// <param name="yearA">The integer parameter gives the Julian
	/// year of the first date.
	/// </param>
	/// <param name="dayB">The integer parameter gives the day of month
	/// of the second date.
	/// </param>
	/// <param name="monthB">The integer parameter gives the Julian
	/// month of the second date.
	/// </param>
	/// <param name="yearB">The integer parameter gives the Julian
	/// year of the second date.
	/// </param>
	/// <returns>An integer giving the difference of days from the first
	/// the second date.
	/// </returns>
	public static int date_difference(int dayA, int monthA, int yearA,
		int dayB, int monthB, int yearB)
	{
		return	fixed_from_dmy(dayB, monthB, yearB) -
			fixed_from_dmy(dayA, monthA, yearA);
	}

	/// <summary>
	/// The method computes the number of the day in the year from
	/// a Julian date.
	/// </summary>
	/// <param name="day">An integer representing the day of the month,
	/// counting from 1.
	/// </param>
	/// <param name="month">An integer representing the month in the
	/// Julian year.
	/// </param>
	/// <param name="year">An integer representing the Julian year.
	/// Negative values are allowed also.
	/// </param>
	/// <returns>An integer value giving the number of the day in the
	/// Julian year, counting from 1.
	/// </returns>
	public static int day_number(int day, int month, int year) {
		return date_difference(31, (int)Month.december, year-1,
			day, month, year);
	}

	/// <summary>
	/// The method computes the days remaining in the given Julian
	/// year from a Julian date.
	/// </summary>
	/// <param name="day">An integer representing the day of the month,
	/// counting from 1.
	/// </param>
	/// <param name="month">An integer representing the month in the
	/// Julian year.
	/// </param>
	/// <param name="year">An integer representing the Julian year.
	/// Negative values are allowed also.
	/// </param>
	/// <returns>An integer value giving the number of days remaining in
	/// the Julian year.
	/// </returns>
	public static int days_remaining(int day, int month, int year) {
		return date_difference(day, month, year,
			31, (int)Month.december, year);
	}
} // class CCJulianCalendar

/// <summary>
/// A class encapsulating the functions of the Hebrew calendar as static
/// methods.
/// </summary>
/// <remarks>
/// <para>
/// This class is not compatible to
/// <see cref="T:System.Globalization.HebrewCalendar"/>.
/// </para>
/// <seealso cref="T:CCFixed"/>
/// </remarks>
static class CCHebrewCalendar {
	/// <summary>An integer defining the epoch of the Hebrew calendar
	/// as fixed day number.</summary>
	/// <remarks>The epoch is October 10, 3761 B.C.E. (Julian).</remarks>
	const int epoch = -1373427;

	/// <summary>The enumeration defines the months of the Gregorian
	/// calendar.
	/// </summary>
	/// <remarks>
	/// The enumaration differs from .NET which defines Tishri as month 1.
	/// </remarks>
	public enum Month {
		/// <summary>
		/// Nisan.
		/// </summary>
		nisan	 = 1,
		/// <summary>
		/// Iyyar.
		/// </summary>
		iyyar,
		/// <summary>
		/// Sivan.
		/// </summary>
		sivan,
		/// <summary>
		/// Tammuz.
		/// </summary>
		tammuz,
		/// <summary>
		/// Av.
		/// </summary>
		av,
		/// <summary>
		/// Elul.
		/// </summary>
		elul,
		/// <summary>
		/// Tishri.
		/// </summary>
		tishri,
		/// <summary>
		/// Heshvan.
		/// </summary>
		heshvan,
		/// <summary>
		/// Kislev.
		/// </summary>
		kislev,
		/// <summary>
		/// Teveth.
		/// </summary>
		teveth,
		/// <summary>
		/// Shevat.
		/// </summary>
		shevat,
		/// <summary>
		/// Adar.
		/// </summary>
		adar,
		/// <summary>
		/// Adar I. Only in years with Adar II.
		/// </summary>
		adar_I = 12,
		/// <summary>
		/// Adar II. Only in years wirh Adar I.
		/// </summary>
		adar_II = 13,
	};

	/// <summary>
	/// The method tells whether the year is a leap year.
	/// </summary>
	/// <param name="year">An integer representing the Hebrew year.
	/// </param>
	/// <returns>A boolean which is true if <paramref name="year"/> is
	/// a leap year.
	/// </returns>
	public static bool is_leap_year(int year) {
		return CCMath.mod(7*year+1, 19) < 7; 
	}

	/// <summary>
	/// The Method gives the number of the last month in a year, which
	/// is equal with the number of month in a Hebrew year.
	/// </summary>
	/// <param name="year">An integer representing the Hebrew year.
	/// </param>
	/// <returns>An integer giving the number of the last month of the 
	/// Hebrew year, which is the same as the numbers of month in the
	/// year.
	/// </returns>
	public static int last_month_of_year(int year) {
		return is_leap_year(year) ? 13 : 12;
	}

	
	/// <summary>The method is a helper function.</summary>
	/// <param name="year">An integer specifying the Hebrew year.
	/// </param>
	/// <returns>An integer representing the number of elapsed days
	/// until the Hebrew year.</returns>
	public static int elapsed_days(int year) {
		int months_elapsed = CCMath.div(235*year-234, 19);
		int r;
		int d = CCMath.div_mod(out r, months_elapsed, 1080);
		int parts_elapsed = 204 + 793 * r;
		int hours_elapsed = 11 + 12 * months_elapsed +
				    793 * d + CCMath.div(parts_elapsed, 1080);

		int day = 29*months_elapsed + CCMath.div(hours_elapsed, 24);

		if (CCMath.mod(3*(day+1), 7) < 3) {
			day += 1;
		}

		return day;
	}

	/// <summary>A method computing the delay of new year for the given
	/// Hebrew year.
	/// </summary>
	/// <param name="year">An integer that gives the Hebrew year.
	/// </param>
	/// <returns>The new year delay in days of the given Hebrew year.
	/// </returns>
	public static int new_year_delay(int year) {
		int ny1 = elapsed_days(year);
		int ny2 = elapsed_days(year+1);

		if (ny2 - ny1 == 356) {
			return 2;
		}
		int ny0 = elapsed_days(year-1);
		if (ny1 - ny0 == 382) {
			return 1;
		}
		return 0;
	}

	/// <summary>
	/// The method computes the last day of month (nummer of days in a
	/// month) of the given Hebrew year.
	/// </summary>
	/// <param name="month">The Hebrew month, allowed value between
	/// One and Thirteen.
	/// </param>
	/// <param name="year">An integer that gives the Hebrew year.
	/// </param>
	/// <returns>The number of the last day of the month of the given
	/// Hebrew year, which gives automatically the number of days in the
	/// month.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRange.Exception">
	/// The exception is thrown if month not between One and Thirteen.
	/// </exception>
	public static int last_day_of_month(int month, int year) {
		if (month < 1 || month > 13)
			throw new System.ArgumentOutOfRangeException("month",
				"Month should be between One and Thirteen.");
		switch (month) {
			case 2: return 29;
			case 4: return 29;
			case 6: return 29;
			case 8:
				if (!long_heshvan(year))
					return 29;
				break;
			case 9:
				if (short_kislev(year))
					return 29;
				break;
			case 10: return 29;
			case 12:
				if (!is_leap_year(year))
					return 29;
				break;
			case 13: return 29;
		}
		return 30;
	}
	
	/// <summary>
	/// The functions checks whether the month Heshvan is a long one
	/// in the given Hebrew year.
	/// </summary>
	/// <param name="year">An integer that gives the Hebrew year.
	/// </param>
	/// <returns>A boolean value: true if there is a long Heshvan
	/// in the given Hebrew year; false otherwise.
	/// </returns>
	public static bool long_heshvan(int year) {
		return CCMath.mod(days_in_year(year), 10) == 5;
	}

	/// <summary>
	/// The functions checks whether the month Kislev is a short one
	/// in the given Hebrew year.
	/// </summary>
	/// <param name="year">An integer that gives the Hebrew year.
	/// </param>
	/// <returns>A boolean value: true if there is a short Kislev
	/// in the given Hebrew year; false otherwise.
	/// </returns>
	public static bool short_kislev(int year) {
		return CCMath.mod(days_in_year(year), 10) == 3;
	}

	/// <summary>
	/// The functions gives the number of days in the specified Hebrew
	/// year.
	/// </summary>
	/// <param name="year">An integer that gives the Hebrew year.
	/// </param>
	/// <returns>The days of the Hebrew year as integer.
	/// </returns>
	public static int days_in_year(int year) {
		return	fixed_from_dmy(1, 7, year+1) -
			fixed_from_dmy(1, 7, year);
	}

	/// <summary>
	/// The method returns the fixed day number of the given Hebrew
	/// date.
	/// </summary>
	/// <param name="day">An integer representing the day of the month,
	/// counting from 1.
	/// </param>
	/// <param name="month">An integer representing the month in the
	/// Hebrew year.
	/// </param>
	/// <param name="year">An integer representing the Hebrew year.
	/// Non-positive values are allowed also.
	/// </param>
	/// <returns>An integer value representing the fixed day number.
	/// </returns>
	public static int fixed_from_dmy(int day, int month, int year) {
		int m;
		int k = epoch-1;
		k += elapsed_days(year);
		k += new_year_delay(year);

		if (month < 7) {
			int l = last_month_of_year(year);
			for (m = 7; m <= l; m++) {
				k += last_day_of_month(m, year);
			}
			for (m = 1; m < month; m++) {
				k += last_day_of_month(m, year);
			}
		}
		else {
			for (m = 7; m < month; m++) {
				k += last_day_of_month(m, year);
			}
		}
		
		k += day;

		return k;
	}

	/// <summary>
	/// The method computes the Hebrew year from a fixed day number.
	/// </summary>
	/// <param name="date">The fixed day number.
	/// </param>
	/// <returns>An integer value giving the Hebrew year of the date.
	/// </returns>
	public static int year_from_fixed(int date) {
		int approx = (int)System.Math.Floor(
			((double)(date - epoch))/(35975351.0/98496.0));
		int y;
		for (y = approx; date >= fixed_from_dmy(1, 7, y); y++) {} 
		return y-1;
	}

	/// <summary>
	/// The method computes the Hebrew year and month from a fixed day
	/// number.
	/// </summary>
	/// <param name="month">The output value giving the Hebrew month.
	/// </param>
	/// <param name="year">The output value giving the Hebrew year.
	/// </param>
	/// <param name="date">An integer value specifying the fixed day
	/// number.</param>
	public static void my_from_fixed(out int month, out int year,
		int date)
	{
		year = year_from_fixed(date);

		int start = date < fixed_from_dmy(1, 1, year) ? 7 : 1;

		for (month = start;
		     date > fixed_from_dmy(last_day_of_month(month, year),
		     				month, year);
		     month++)
		{}
	}

	/// <summary>
	/// The method computes the Hebrew year, month, and day from a
	/// fixed day number.
	/// </summary>
	/// <param name="day">The output value returning the day of the
	/// month.
	/// </param>
	/// <param name="month">The output value giving the Hebrew month.
	/// </param>
	/// <param name="year">The output value giving the Hebrew year.
	/// </param>
	/// <param name="date">An integer value specifying the fixed day
	/// number.</param>
	public static void dmy_from_fixed(out int day, out int month,
		out int year, int date)
	{
		my_from_fixed(out month, out year, date);
		day = date - fixed_from_dmy(1, month, year) + 1;
	}

	/// <summary>A method computing the Hebrew month from a fixed
	/// day number.
	/// </summary>
	/// <param name="date">An integer specifying the fixed day number.
	/// </param>
	/// <returns>An integer value representing the Hebrew month.
	/// </returns>
	public static int month_from_fixed(int date) {
		int month, year;

		my_from_fixed(out month, out year, date);
		return month;
	}

	/// <summary>
	/// A method computing the day of the month from a fixed day number.
	/// </summary>
	/// <param name="date">An integer specifying the fixed day number.
	/// </param>
	/// <returns>An integer value representing the day of the month.
	/// </returns>
	public static int day_from_fixed(int date) {
		int day, month, year;
		
		dmy_from_fixed(out day, out month, out year, date);
		return day;
	}

	/// <summary>
	/// The method computes the difference between two Hebrew dates.
	/// </summary>
	/// <param name="dayA">The integer parameter gives the day of month
	/// of the first date.
	/// </param>
	/// <param name="monthA">The integer parameter gives the Hebrew
	/// month of the first date.
	/// </param>
	/// <param name="yearA">The integer parameter gives the Hebrew
	/// year of the first date.
	/// </param>
	/// <param name="dayB">The integer parameter gives the day of month
	/// of the second date.
	/// </param>
	/// <param name="monthB">The integer parameter gives the Hebrew
	/// month of the second date.
	/// </param>
	/// <param name="yearB">The integer parameter gives the Hebrew
	/// year of the second date.
	/// </param>
	/// <returns>An integer giving the difference of days from the first
	/// the second date.
	/// </returns>
	public static int date_difference(int dayA, int monthA, int yearA,
		int dayB, int monthB, int yearB)
	{
		return	fixed_from_dmy(dayB, monthB, yearB) -
			fixed_from_dmy(dayA, monthA, yearA);
	}

	/// <summary>
	/// The method computes the number of the day in the year from
	/// a Hebrew date.
	/// </summary>
	/// <param name="day">An integer representing the day of the month,
	/// counting from 1.
	/// </param>
	/// <param name="month">An integer representing the month in the
	/// Hebrew year.
	/// </param>
	/// <param name="year">An integer representing the Hebrew year.
	/// </param>
	/// <returns>An integer value giving the number of the day in the
	/// Hebrew year, counting from 1.
	/// </returns>
	public static int day_number(int day, int month, int year) {
		return date_difference(1, 7, year,
			day, month, year) + 1;
	}

	/// <summary>
	/// The method computes the days remaining in the given Hebrew
	/// year from a Hebrew date.
	/// </summary>
	/// <param name="day">An integer representing the day of the month,
	/// counting from 1.
	/// </param>
	/// <param name="month">An integer representing the month in the
	/// Hebrew year.
	/// </param>
	/// <param name="year">An integer representing the Hebrew year.
	/// </param>
	/// <returns>An integer value giving the number of days remaining in
	/// the Hebrew year.
	/// </returns>
	public static int days_remaining(int day, int month, int year) {
		return date_difference(day, month, year,
			1, 7, year+1)-1;
	}
} // class HebrewCalendar


/// <summary>
/// A class encapsulating the functions of the Islamic calendar as static
/// methods.
/// </summary>
/// <remarks>
/// <para>There is no difference here in using Hijri or Islamic calendar.
/// </para>
/// <para>The epoch of the Islamic calendar isn't fixed, because we cannot
/// surely say today, when the crescent of the new moon has been observed 
/// around the July 16, 622 C.E. Julian. Even today the start and end of 
/// the month Ramadan is defined by religous authorities. So the calendar
/// can be offset by two days.
/// </para> 
/// <para>
/// We don't support the offset here, however we changed the epoch from
/// "Calendrical Calculations" to value, that .Net seems to be using.
/// </para>
/// <para>
/// This class is not compatible to
/// <see cref="T:System.Globalization.HijriCalendar"/>.
/// </para>
/// <seealso cref="T:CCFixed"/>
/// </remarks>
static class CCHijriCalendar {
	/// <summary>An integer defining the epoch of the Gregorian calendar
	/// as fixed day number.</summary>
	/// <remarks>
	/// <para>
	/// The epoch is given as 16 July 622 C.E. Julian (R.D. 227015)
	/// in Calendrical Calculations, the approximate date of
	/// the emigration of
	/// Muhammed to Medina. However there is no way to determine today
	/// the observation of the crescent of the new moon in July 622 C.E.
	/// (Julian). So there is some variability in the epoch.
	/// Religous authorities determine the epoch by observing the
	/// crescent of the new moon for the month Ramadan, so there might
	/// be an offsets by two days of the epoch.
	/// </para>
	/// <para>Windows
	/// supports an AddHijriDate parameter in the registry to adapt
	/// for it. It seems that the .NET implementation of
	/// HijriCalendar uses an epoch of 227014, so we use it here. The
	/// ArgumentOutOfRangeException gives July, 18 622 as epoch,
	/// which is 227014 supporting our theory.
	/// </para>
	/// </remarks>
	const int epoch = 227014;

	/// <summary>The enumeration defines the months of the Islamic
	/// calendar.
	/// </summary>
	public enum Month {
		/// <summary>
		/// Muharram.
		/// </summary>
		muharram = 1,
		/// <summary>
		/// Safar.
		/// </summary>
		safar,
		/// <summary>
		/// Rabi I.
		/// </summary>
		rabi_I,
		/// <summary>
		/// Rabi II.
		/// </summary>
		rabi_II,
		/// <summary>
		/// Jumada I.
		/// </summary>
		jumada_I,
		/// <summary>
		/// Jumada II.
		/// </summary>
		jumada_II,
		/// <summary>
		/// Rajab.
		/// </summary>
		rajab,
		/// <summary>
		/// Shaban.
		/// </summary>
		shaban,
		/// <summary>
		/// Ramadan.
		/// </summary>
		ramadan,
		/// <summary>
		/// Shawwal.
		/// </summary>
		shawwal,
		/// <summary>
		/// Dhu Al-Quada.
		/// </summary>
		dhu_al_quada,
		/// <summary>
		/// Dhu Al-Hijja.
		/// </summary>
		dhu_al_hijja,
	};

	/// <summary>
	/// The method tells whether the year is a leap year.
	/// </summary>
	/// <param name="year">An integer representing the Islamic year.
	/// </param>
	/// <returns>A boolean which is true if <paramref name="year"/> is
	/// a leap year.
	/// </returns>
	public static bool is_leap_year(int year) {
		return CCMath.mod(14+11*year, 30) < 11;
	}

	/// <summary>
	/// The method returns the fixed day number of the given Islamic
	/// date.
	/// </summary>
	/// <param name="day">An integer representing the day of the month,
	/// counting from 1.
	/// </param>
	/// <param name="month">An integer representing the month in the
	/// Islamic year.
	/// </param>
	/// <param name="year">An integer representing the Islamic year.
	/// Non-positive values are allowed also.
	/// </param>
	/// <returns>An integer value representing the fixed day number.
	/// </returns>
	public static int fixed_from_dmy(int day, int month, int year) {
		int k = epoch - 1;
		k += 354 * (year-1);
		k += CCMath.div(3+11*year, 30);
		k += (int)System.Math.Ceiling(29.5 * (double)(month-1));
		k += day;

		return k;
	}

	/// <summary>
	/// The method computes the Islamic year from a fixed day number.
	/// </summary>
	/// <param name="date">The fixed day number.
	/// </param>
	/// <returns>An integer value giving the Islamic year of the date.
	/// </returns>
	public static int year_from_fixed(int date) {
		return CCMath.div(30*(date-epoch)+10646, 10631);
	}

	/// <summary>
	/// The method computes the Islamic year and month from a fixed day
	/// number.
	/// </summary>
	/// <param name="month">The output value giving the Islamic month.
	/// </param>
	/// <param name="year">The output value giving the Islamic year.
	/// </param>
	/// <param name="date">An integer value specifying the fixed day
	/// number.</param>
	public static void my_from_fixed(out int month, out int year, int date)
	{
		year = year_from_fixed(date);

		int m = 1+(int)System.Math.Ceiling(
			((double)(date-29-fixed_from_dmy(1,1,year)))/29.5);

		month = m < 12 ? m : 12;
	}
	
	/// <summary>
	/// The method computes the Islamic year, month, and day from a
	/// fixed day number.
	/// </summary>
	/// <param name="day">The output value returning the day of the
	/// month.
	/// </param>
	/// <param name="month">The output value giving the Islamic month.
	/// </param>
	/// <param name="year">The output value giving the Islamic year.
	/// </param>
	/// <param name="date">An integer value specifying the fixed day
	/// number.</param>
	public static void dmy_from_fixed(out int day, out int month,
		out int year, int date)
	{
		my_from_fixed(out month, out year, date);
		day = date - fixed_from_dmy(1, month, year) + 1;
	}

	/// <summary>A method computing the Islamic month from a fixed
	/// day number.
	/// </summary>
	/// <param name="date">An integer specifying the fixed day number.
	/// </param>
	/// <returns>An integer value representing the Islamic month.
	/// </returns>
	public static int month_from_fixed(int date) {
		int month, year;

		my_from_fixed(out month, out year, date);
		return month;
	}

	/// <summary>
	/// A method computing the day of the month from a fixed day number.
	/// </summary>
	/// <param name="date">An integer specifying the fixed day number.
	/// </param>
	/// <returns>An integer value representing the day of the month.
	/// </returns>
	public static int day_from_fixed(int date) {
		int day;
		int month;
		int year;

		dmy_from_fixed(out day, out month, out year, date);
		return day;
	}

	/// <summary>
	/// The method computes the difference between two Islamic dates.
	/// </summary>
	/// <param name="dayA">The integer parameter gives the day of month
	/// of the first date.
	/// </param>
	/// <param name="monthA">The integer parameter gives the Islamic
	/// month of the first date.
	/// </param>
	/// <param name="yearA">The integer parameter gives the Islamic
	/// year of the first date.
	/// </param>
	/// <param name="dayB">The integer parameter gives the day of month
	/// of the second date.
	/// </param>
	/// <param name="monthB">The integer parameter gives the Islamic
	/// month of the second date.
	/// </param>
	/// <param name="yearB">The integer parameter gives the Islamic
	/// year of the second date.
	/// </param>
	/// <returns>An integer giving the difference of days from the first
	/// the second date.
	/// </returns>
	public static int date_difference(int dayA, int monthA, int yearA,
		int dayB, int monthB, int yearB)
	{
		return	fixed_from_dmy(dayB, monthB, yearB) -
			fixed_from_dmy(dayA, monthA, yearA);
	}

	/// <summary>
	/// The method computes the number of the day in the year from
	/// a Islamic date.
	/// </summary>
	/// <param name="day">An integer representing the day of the month,
	/// counting from 1.
	/// </param>
	/// <param name="month">An integer representing the month in the
	/// Islamic year.
	/// </param>
	/// <param name="year">An integer representing the Islamic year.
	/// </param>
	/// <returns>An integer value giving the number of the day in the
	/// Islamic year, counting from 1.
	/// </returns>
	public static int day_number(int day, int month, int year) {
		return date_difference(31, 12, year-1, day, month, year);
	}

	/// <summary>
	/// The method computes the days remaining in the given Islamic
	/// year from a Islamic date.
	/// </summary>
	/// <param name="day">An integer representing the day of the month,
	/// counting from 1.
	/// </param>
	/// <param name="month">An integer representing the month in the
	/// Islamic year.
	/// </param>
	/// <param name="year">An integer representing the Islamic year.
	/// Non-positive values are allowed also.
	/// </param>
	/// <returns>An integer value giving the number of days remaining in
	/// the Islamic year.
	/// </returns>
	public static int days_remaining(int day, int month, int year) {
		return date_difference(day, month, year,31, 12, year);
	}
} // class CCHijriCalendar

internal class CCEastAsianLunisolarCalendar
{
	public static int fixed_from_dmy (int day, int month, int year)
	{
		/*
		int k = epoch - 1;
		k += 354 * (year - 1);
		k += CCMath.div (3+11*year, 30);
		k += (int) Math.Ceiling(29.53 * (double)(month-1));
		k += day;

		return k;
		*/
		throw new Exception ("fixed_from_dmy");
	}

	public static int year_from_fixed (int date)
	{
		throw new Exception ("year_from_fixed");
	}

	public static void my_from_fixed(out int month, out int year, int date)
	{
		/*
		year = year_from_fixed (date);

		int m = 1+(int)System.Math.Ceiling(
			((double)(date-29-fixed_from_dmy(1,1,year)))/29.5);

		month = m < 12 ? m : 12;
		*/
		throw new Exception ("my_from_fixed");
	}

	public static void dmy_from_fixed(out int day, out int month,
		out int year, int date)
	{
		/*
		my_from_fixed (out month, out year, date);
		day = date - fixed_from_dmy (1, month, year) + 1;
		*/
		throw new Exception ("dmy_from_fixed");
	}

	public static DateTime AddMonths (DateTime date, int months)
	{
		
		throw new Exception ("AddMonths");
	}

	public static DateTime AddYears (DateTime date, int years)
	{
		throw new Exception ("AddYears");
	}

	public static int GetDayOfMonth (DateTime date)
	{
		throw new Exception ("GetDayOfMonth");
	}

	public static int GetDayOfYear (DateTime date)
	{
		throw new Exception ("GetDayOfYear");
	}

	public static int GetDaysInMonth (int gyear, int month)
	{
		throw new Exception ("GetDaysInMonth");
	}

	public static int GetDaysInYear (int year)
	{
		throw new Exception ("GetDaysInYear");
	}

	public static int GetMonth (DateTime date)
	{
		throw new Exception ("GetMonth");
	}

	public static bool IsLeapMonth (int gyear, int month)
	{
		int goldenNumber = gyear % 19;

		bool chu = false;
		bool leap = false;
		double s = 0;
		for (int y = 0; y < goldenNumber; y++) {
			for (int l = 0, m = 1; m <= month; m++) {
				if (leap) {
					l += 30;
					leap = false;
					if (y == goldenNumber && m == month)
						return true;
				} else {
					l += chu ? 30 : 29;
					chu = !chu;
					s += 30.44;
					if (s - l > 29)
						leap = true;
				}
			}
		}
		return false;
	}

	public static bool IsLeapYear (int gyear)
	{

		// FIXME: it is still wrong.
		int d = gyear % 19;
		switch (d) {
		case 0: case 3: case 6: case 9: case 11: case 14: case 17:
			return true;
		default:
			return false;
		}
		/*
		int goldenNumber = (gyear - 1900) % 19;
		int epact = 29;
		bool leap = false;
		while (goldenNumber-- >= 0) {
			epact += 11;
			leap = epact > 30;
			if (epact > 30)
				epact -= 30;
		}
		return leap;
		*/
	}

	public static DateTime ToDateTime (int year, int month, int day, int hour, int minute, int second, int millisecond)
	{
		throw new Exception ("ToDateTime");
	}
}

/// <summary>
/// A class that supports the Gregorian based calendars with other eras
/// (e.g. <see cref="T:System.Gloablization.JapaneseCalendar"/>).
/// </summary>
[System.Serializable]
internal class CCGregorianEraHandler {
	/// <summary>
	/// A struct that represents a single era.
	/// </summary>
	[System.Serializable]
	struct Era {
		/// <summary>
		/// The integer number identifying the era.
		/// </summary>
		private int _nr;

		/// <value>
		/// A get-only property that gives the era integer number.
		/// </value>
		public int Nr { get { return _nr; } }

		/// <summary>This integer gives the first day of the era as
		/// fixed day number.
		/// </summary>
		private int _start; // inclusive
		/// <summary>
		/// This integer gives the gregorian year of the
		/// <see cref="M:_start"/> value.
		/// </summary>
		private int _gregorianYearStart;
		/// <summary>
		/// This integer gives the last day of the era as fixed day
		/// number.
		/// </summary>
		private int _end;   // inclusive	
		/// <summary>
		/// This integer gives the largest year number of this era.
		/// </summary>
		private int _maxYear;

		/// <summary>
		/// This constructor creates the era structure.
		/// </summary>
		/// <param name="nr">The integer number of the era.
		/// </param>
		/// <param name="start">The fixed day number defining the
		/// first day of the era.
		/// </param>
		/// <param name="end">The fixed day number that defines the
		/// last day of the era.
		/// </param>
		public Era(int nr, int start, int end) {
			if (nr == 0)
				throw new System.ArgumentException(
					"Era number shouldn't be zero.");
			_nr = nr;
			if (start > end) {
				throw new System.ArgumentException(
					"Era should start before end.");
			}
			_start = start;
			_end = end;

			_gregorianYearStart =
				CCGregorianCalendar.year_from_fixed(_start);
			int gregorianYearEnd =
				CCGregorianCalendar.year_from_fixed(_end);
			_maxYear = gregorianYearEnd - _gregorianYearStart + 1;
		}

		/// <summary>
		/// This method computes the Gregorian year from the year
		/// of this era.
		/// </summary>
		/// <param name="year">An integer giving the year in the
		/// era.
		/// </param>
		/// <returns>
		/// The Gregorian year as integer.
		/// </returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// The exception is thrown if the year isn't valid in this
		/// era.
		/// </exception>
		public int GregorianYear(int year) {
			if (year < 1 || year > _maxYear) {
				System.IO.StringWriter sw = 
					new System.IO.StringWriter();
				sw.Write(
					"Valid Values are between " +
					"{0} and {1}, inclusive.",
					1, _maxYear);
				throw new System.ArgumentOutOfRangeException(
					"year", sw.ToString());
			}
			return year + _gregorianYearStart - 1;
		}

		/// <summary>
		/// This function checks wether the given fixed day number is
		/// ion the time span of the era.
		/// </summary>
		/// <param name="date">An integer giving the fixed day
		/// number.
		/// </param>
		/// <returns>A boolean: true if the argument is in the time
		/// span of the era.
		/// </returns>
		public bool Covers(int date) {
			return _start <= date && date <= _end;
		}

		/// <summary>
		/// This function returns the year of the era and sets
		/// the era in an output parameter.
		/// </summary>
		/// <param name="era">An output parameter returning the
		/// era number.
		/// </param>
		/// <param name="date">An integer giving the fixed day
		/// number.
		/// </param>
		/// <returns>An integer giving the year of the era.
		/// </returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// The exception is thrown if date is outside of the time
		/// span of the era.
		/// </exception>
		public int EraYear(out int era, int date) {
			if (!Covers(date))
				throw new System.ArgumentOutOfRangeException(
					"date", 
					"Time was out of Era range.");
			int gregorianYear =
				CCGregorianCalendar.year_from_fixed(date);
			era = _nr;
			return gregorianYear - _gregorianYearStart + 1;
		}
	} // struct Era

	/// <summary>
	/// A private member storing the eras in a
	/// <see cref="T:System.Collections.SortedList"/>.
	/// </summary>
	private SortedList _Eras;

	/// <value>
	/// The property returns the era numbers as an array of integers.
	/// </value>
	public int[] Eras {
		get {
			int[] a = new int[_Eras.Count];

			for (int i = 0; i < _Eras.Count; i++) {
				Era e = (Era)_Eras.GetByIndex(i);
				a[i] = e.Nr;
			}

			return a;
		}
	}

	/// <summary>
	/// Constructor.
	/// </summary>
	public CCGregorianEraHandler() {
		_Eras = new SortedList();
	}

	/// <summary>
	/// Method adds an era to the GregorianEraHandler instance.
	/// </summary>
	/// <param name="nr">The integer number of the era.
	/// </param>
	/// <param name="rd_start">The fixed day number defining the
	/// first day of the era.
	/// </param>
	/// <param name="rd_end">The fixed day number that defines the
	/// last day of the era.
	/// </param>
	public void appendEra(int nr, int rd_start, int rd_end) {
		Era era = new Era(nr, rd_start, rd_end);
		_Eras[(System.Object)nr] = era;
	}
	/// <summary>
	/// Method adds a yet not-ended era to the GregorianEraHandler
	/// instance.
	/// </summary>
	/// <param name="nr">The integer number of the era.
	/// </param>
	/// <param name="rd_start">The fixed day number defining the
	/// first day of the era.
	/// </param>
	public void appendEra(int nr, int rd_start) {
		appendEra(nr, rd_start,
			CCFixed.FromDateTime(DateTime.MaxValue));
	}

	/// <summary>
	/// This method computes the Gregorian year from the year
	/// of the given era.
	/// </summary>
	/// <param name="year">An integer giving the year in the
	/// era.
	/// </param>
	/// <param name="era">An integer giving the era number.
	/// </param>
	/// <returns>
	/// The Gregorian year as integer.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the year isn't valid in this
	/// era.
	/// </exception>
	public int GregorianYear(int year, int era) {
		Era e = (Era)_Eras[(System.Object)era];
		return e.GregorianYear(year);
	}

	/// <summary>
	/// This function returns the year of the era and sets
	/// the era in an output parameter.
	/// </summary>
	/// <param name="era">An output parameter returning the
	/// era number.
	/// </param>
	/// <param name="date">An integer giving the fixed day
	/// number.
	/// </param>
	/// <returns>An integer giving the year of the era.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the fixed day number is outside of the
	/// time spans of all eras.
	/// </exception>
	public int EraYear(out int era, int date)
	{
		IList list = _Eras.GetValueList();

		foreach (Era e in list) {
			if (e.Covers(date))
				return e.EraYear(out era, date);
		}

		throw new System.ArgumentOutOfRangeException("date",
			"Time value was out of era range.");
	}

	/// <summary>
	/// The method checks whether a given
	/// <see cref="T:System.DateTime"/> is covered by any era.
	/// </summary>
	/// <param name="time">A 
	/// <see cref="T:System.DateTime"/> giving the date and time.
	/// </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the argument isn't inside the time
	/// span of any era.
	/// </exception>
	public void CheckDateTime(System.DateTime time) {
		int date = CCFixed.FromDateTime(time);

		if (!ValidDate(date))
			throw new System.ArgumentOutOfRangeException("time",
				"Time value was out of era range.");
	}
		
	/// <summary>
	/// The method tests whether a given
	/// fixed day number is covered by any era.
	/// </summary>
	/// <param name="date">An integer representing the fixed day number.
	/// </param>
	/// <returns> A boolean is returned: true if the argument is inside
	/// the time span of one era; false otherwise.
	/// </returns>
	public bool ValidDate(int date) {
		IList list = _Eras.GetValueList();

		foreach (Era e in list) {
			if (e.Covers(date))
				return true;
		}

		return false;
	}

	/// <summary>
	/// The method tests, whether the era number does exist.
	/// </summary>
	/// <param name="era">An integer giving the era number.
	/// </param>
	/// <returns>A boole value: True if the era number does exist;
	/// false otherwise.
	/// </returns>
	public bool ValidEra(int era) {
		return _Eras.Contains((System.Object)era);
	}
} // class CCGregorianEraHandler


// FIXME: remove this class. It should be identical to CCGregorianEraHandler
[System.Serializable]
internal class CCEastAsianLunisolarEraHandler
{
	[Serializable]
	struct Era 
	{
		private int _nr; // era index

		public int Nr {
			get { return _nr; }
		}

		private int _start; // inclusive
		private int _gregorianYearStart;
		private int _end;   // inclusive
		private int _maxYear;

		public Era (int nr, int start, int end)
		{
			if (nr == 0)
				throw new ArgumentException ("Era number shouldn't be zero.");
			_nr = nr;
			if (start > end)
				throw new ArgumentException ("Era should start before end.");
			_start = start;
			_end = end;

			_gregorianYearStart = CCGregorianCalendar.year_from_fixed (_start);
			int gregorianYearEnd = CCGregorianCalendar.year_from_fixed (_end);
			_maxYear = gregorianYearEnd - _gregorianYearStart + 1;
		}

		public int GregorianYear (int year) 
		{
			if (year < 1 || year > _maxYear)
				throw new ArgumentOutOfRangeException ("year", String.Format ("Valid Values are between {0} and {1}, inclusive.", 1, _maxYear));
			return year + _gregorianYearStart - 1;
		}

		public bool Covers (int date) {
			return _start <= date && date <= _end;
		}

		public int EraYear (out int era, int date) {
			if (!Covers (date))
				throw new ArgumentOutOfRangeException ("date", "Time was out of Era range.");
			int gregorianYear = CCGregorianCalendar.year_from_fixed (date);
			era = _nr;
			return gregorianYear - _gregorianYearStart + 1;
		}
	}

	private SortedList _Eras;

	public int [] Eras 
	{
		get {
			int[] a = new int [_Eras.Count];
			for (int i = 0; i < _Eras.Count; i++) {
				Era e = (Era) _Eras.GetByIndex (i);
				a[i] = e.Nr;
			}
			return a;
		}
	}

	public CCEastAsianLunisolarEraHandler ()
	{
		_Eras = new SortedList ();
	}

	public void appendEra (int nr, int rd_start, int rd_end)
	{
		Era era = new Era (nr, rd_start, rd_end);
		_Eras [nr] = era;
	}

	public void appendEra (int nr, int rd_start)
	{
		appendEra (nr, rd_start, CCFixed.FromDateTime (DateTime.MaxValue));
	}

	public int GregorianYear (int year, int era)
	{
		Era e = (Era) _Eras [era];
		return e.GregorianYear (year);
	}

	public int EraYear (out int era, int date)
	{
		foreach (Era e in _Eras.Values)
			if (e.Covers (date))
				return e.EraYear (out era, date);

		throw new ArgumentOutOfRangeException ("date", "Time value was out of era range.");
	}

	public void CheckDateTime (DateTime time)
	{
		int date = CCFixed.FromDateTime (time);

		if (!ValidDate (date))
			throw new ArgumentOutOfRangeException ("time", "Time value was out of era range.");
	}
		
	public bool ValidDate (int date)
	{
		foreach (Era e in _Eras.Values) {
			if (e.Covers (date))
				return true;
		}
		return false;
	}

	public bool ValidEra (int era)
	{
		return _Eras.Contains (era);
	}
}

} // namespace System.Globalization
