// TaiwanCalendar.cs
//
// (C) Ulrich Kunitz 2002
//

namespace System.Globalization {

using System;

/// <summary>
/// This is the Japanese calendar. It differs from the Gregorian calendar
/// only in the years.
/// </summary>
/// <remarks>
/// <para>The Japanese calendar support a single era starting at January 1,
/// 1912</para>
/// <para>The implementation uses the
/// <see cref="N:CalendricalCalculations"/> namespace.
/// </para>
/// </remarks>
[Serializable]
public class TaiwanCalendar : Calendar {
	/// <summary>
	/// Static protected field storing the
	/// <see cref="T:CalendricalCalculations.GregorianEraHandler"/>.
	/// </summary>
	internal static readonly CCGregorianEraHandler M_EraHandler;

	/// <summary>
	/// Static constructor, who creates and initializes
	/// <see cref="F:M_EraHandler"/>.
	/// </summary>
	static TaiwanCalendar() {
		M_EraHandler = new CCGregorianEraHandler();
		M_EraHandler.appendEra(1,
			CCGregorianCalendar.fixed_from_dmy(1, 1, 1912));
	}

	/// <summary>
	/// Default constructor.
	/// </summary>
	public TaiwanCalendar() {
		M_AbbrEraNames = new string[] {"T.C.E."};
		M_EraNames =  new string[] {"Taiwan current era"};
	}

	/// <value>Overridden. Gives the eras supported by the
	/// calendar as an array of integers.
	/// </value>
	public override int[] Eras {
		get {
			return (int[])M_EraHandler.Eras.Clone();
		}
	}

	int twoDigitYearMax = 99;
	
	public override int TwoDigitYearMax 
	{
		get {
			return twoDigitYearMax;
		}
		set {
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
			era = 1;
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
		M_ArgumentInRange("day", day, 1,
			GetDaysInMonth(year, month, era));
		return gregorianYear;
	}

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
	/// <see cref="T:System.DateTime"/> return value is outside all
	/// supported eras.
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
	/// <see cref="T:System.DateTime"/> return value is outside all
	/// supported eras.
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
	/// <see cref="T:System.DateTime"/> return value is outside all
	/// supported eras.
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
	/// <see cref="T:System.DateTime"/> return value is outside all
	/// supported eras.
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
	/// <see cref="T:System.DateTime"/> return value is outside all
	/// supported eras.
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
	/// <see cref="T:System.DateTime"/> return value is outside all
	/// supported eras.
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
	/// <see cref="T:System.DateTime"/> parameter is outside all
	/// supported eras.
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
	/// <see cref="T:System.DateTime"/> parameter is outside all
	/// supported eras.
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
	/// <see cref="T:System.DateTime"/> parameter is outside all
	/// supported eras.
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
	/// <see cref="T:System.DateTime"/> parameter is outside all
	/// supported eras.
	/// </exception>
	public override int GetSecond(DateTime time) {
		M_CheckDateTime(time);
		return base.GetMinute(time);
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
		M_CheckYEG(year, ref era);
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
		int hour, int minute, int second, int milliseconds,
		int era)
	{
		int gregorianYear = M_CheckYMDEG(year, month, day, ref era);
		M_CheckHMSM(hour, minute, second, milliseconds);
		return CCGregorianCalendar.ToDateTime(
			gregorianYear, month, day,
			hour, minute, second, milliseconds);
	}

	/// <summary>
	/// This functions returns simply the year for the Taiwan calendar.
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
} // class TaiwanCalendar
	
} // namespace System.Globalization
