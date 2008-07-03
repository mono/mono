//
// System.Globalization.EastAsianLunisolarCalendar.cs
//
// Author
//	Ulrich Kunitz 2002
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C) Ulrich Kunitz 2002
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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

#if NET_2_0

namespace System.Globalization {

using System;

[Serializable]
[System.Runtime.InteropServices.ComVisible (true)]
public abstract class EastAsianLunisolarCalendar : Calendar {
	// FIXME: This is ok and it does not have to be something like 
	// CCEastAsianLunisolarEraHandler since it does not depend on
	// any lunisolar stuff.
	internal readonly CCEastAsianLunisolarEraHandler M_EraHandler;

	internal EastAsianLunisolarCalendar (CCEastAsianLunisolarEraHandler eraHandler)
	{
		M_EraHandler = eraHandler;
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
	
	internal void M_CheckDateTime(DateTime time) {
		M_EraHandler.CheckDateTime(time);
	}

	internal virtual int ActualCurrentEra {
		get { return 1; }
	}

	internal void M_CheckEra(ref int era) {
		if (era == CurrentEra)
			era = ActualCurrentEra;
		if (!M_EraHandler.ValidEra(era))
			throw new ArgumentException("Era value was not valid.");
	}

	internal int M_CheckYEG(int year, ref int era) {
		M_CheckEra(ref era);
		return M_EraHandler.GregorianYear(year, era);
	}

	internal override void M_CheckYE(int year, ref int era) {
		M_CheckYEG(year, ref era);
	}

	internal int M_CheckYMEG(int year, int month, ref int era) {
		int gregorianYear = M_CheckYEG(year, ref era);
		if (month < 1 || month > 12)
			throw new ArgumentOutOfRangeException("month",
				"Month must be between one and twelve.");
		return gregorianYear;
	}

	internal int M_CheckYMDEG(int year, int month, int day, ref int era)
	{
		int gregorianYear = M_CheckYMEG(year, month, ref era);
		M_ArgumentInRange("day", day, 1, GetDaysInMonth(year, month, era));
		return gregorianYear;
	}

	[MonoTODO]
	public override DateTime AddMonths(DateTime time, int months) {
		DateTime t = CCEastAsianLunisolarCalendar.AddMonths(time, months);
		M_CheckDateTime(t);
		return t;
	}

	[MonoTODO]
	public override DateTime AddYears(DateTime time, int years) {
		DateTime t = CCEastAsianLunisolarCalendar.AddYears(time, years);
		M_CheckDateTime(t);
		return t;
	}
		
	[MonoTODO]
	public override int GetDayOfMonth(DateTime time) {
		M_CheckDateTime(time);
		return CCEastAsianLunisolarCalendar.GetDayOfMonth(time);
	}

	[MonoTODO]
	public override DayOfWeek GetDayOfWeek(DateTime time) {
		M_CheckDateTime(time);
		int rd = CCFixed.FromDateTime(time);
		return (DayOfWeek)CCFixed.day_of_week(rd);
	}

	[MonoTODO]
	public override int GetDayOfYear(DateTime time) {
		M_CheckDateTime(time);
		return CCEastAsianLunisolarCalendar.GetDayOfYear(time);
	}

	[MonoTODO]
	public override int GetDaysInMonth(int year, int month, int era) {
		int gregorianYear = M_CheckYMEG(year, month, ref era);
		return CCEastAsianLunisolarCalendar.GetDaysInMonth(gregorianYear, month);
	}

	[MonoTODO]
	public override int GetDaysInYear(int year, int era) {
		int gregorianYear = M_CheckYEG(year, ref era);
		return CCEastAsianLunisolarCalendar.GetDaysInYear(gregorianYear);
	}
		

	[MonoTODO]
	public override int GetLeapMonth(int year, int era)
	{
		return base.GetLeapMonth(year, era);
	}

	[MonoTODO]
	public override int GetMonth(DateTime time) {
		M_CheckDateTime(time);
		return CCEastAsianLunisolarCalendar.GetMonth(time);
	}

	[MonoTODO]
	public override int GetMonthsInYear(int year, int era) {
		M_CheckYE(year, ref era);
		return IsLeapYear (year, era) ? 13: 12;
	}

	public override int GetYear(DateTime time) {
		// M_CheckDateTime not needed, because EraYeat does the
		// right thing.
		int rd = CCFixed.FromDateTime(time);
		int era;
		return M_EraHandler.EraYear(out era, rd);
	}

	public override bool IsLeapDay(int year, int month, int day, int era)
	{
		int gregorianYear = M_CheckYMDEG(year, month, day, ref era);
		// every day in LeapMonth is a LeapDay.
		return CCEastAsianLunisolarCalendar.IsLeapMonth (gregorianYear, month);
	}

	[MonoTODO]
	public override bool IsLeapMonth(int year, int month, int era) {
		int gregorianYear = M_CheckYMEG(year, month, ref era);
		return CCEastAsianLunisolarCalendar.IsLeapMonth(gregorianYear, month);
	}

	public override bool IsLeapYear(int year, int era) {
		int gregorianYear = M_CheckYEG(year, ref era);
		return CCEastAsianLunisolarCalendar.IsLeapYear (gregorianYear);
	}

	[MonoTODO]
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

	[MonoTODO]
	public override int ToFourDigitYear(int year) {
		if (year < 0)
			throw new ArgumentOutOfRangeException(
				"year", "Non-negative number required.");
		int era = CurrentEra;
		M_CheckYE(year, ref era);
		return year;
	}

	public override CalendarAlgorithmType AlgorithmType {
		get {
			return CalendarAlgorithmType.LunisolarCalendar;
		}
	}


	#region celestial/terrestial thingy
	public int GetCelestialStem (int sexagenaryYear)
	{
		if (sexagenaryYear < 1 || 60 < sexagenaryYear)
			throw new ArgumentOutOfRangeException ("sexagendaryYear is less than 0 or greater than 60");
		return (sexagenaryYear - 1) % 10 + 1;
	}

	public virtual int GetSexagenaryYear (DateTime time)
	{
		return (GetYear (time) - 1900) % 60;
	}

	public int GetTerrestrialBranch (int sexagenaryYear)
	{
		if (sexagenaryYear < 1 || 60 < sexagenaryYear)
			throw new ArgumentOutOfRangeException ("sexagendaryYear is less than 0 or greater than 60");
		return (sexagenaryYear - 1) % 12 + 1;
	}
	#endregion
}
}

#endif
