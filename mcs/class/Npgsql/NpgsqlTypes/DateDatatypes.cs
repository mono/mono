// NpgsqlTypes\DateDatatypes.cs
//
// Author:
//	Jon Hanna. (jon@hackcraft.net)
//
//	Copyright (C) 2007-2008 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Npgsql;

//TODO: Lots of convenience methods! There should be nothing you can do with datetime and timestamp that you can't
//do just as well with these - and hence no reason not to choose these if they are appropriate.
//Similarly, lots of documentation is a must.

namespace NpgsqlTypes
{
	/// <summary>
	/// Represents the PostgreSQL interval datatype.
	/// <remarks>PostgreSQL differs from .NET in how it's interval type doesn't assume 24 hours in a day
	/// (to deal with 23- and 25-hour days caused by daylight savings adjustments) and has a concept
	/// of months that doesn't exist in .NET's <see cref="TimeSpan"/> class. (Neither datatype
	/// has any concessions for leap-seconds).
	/// <para>For most uses just casting to and from TimeSpan will work correctly &#x2014; in particular,
	/// the results of subtracting one <see cref="DateTime"/> or the PostgreSQL date, time and
	/// timestamp types from another should be the same whether you do so in .NET or PostgreSQL &#x2014;
	/// but if the handling of days and months in PostgreSQL is important to your application then you
	/// should use this class instead of <see cref="TimeSpan"/>.</para>
	/// <para>If you don't know whether these differences are important to your application, they
	/// probably arent! Just use <see cref="TimeSpan"/> and do not use this class directly &#x263a;</para>
	/// <para>To avoid forcing unnecessary provider-specific concerns on users who need not be concerned
	/// with them a call to <see cref="IDataRecord.GetValue(int)"/> on a field containing an
	/// <see cref="NpgsqlInterval"/> value will return a <see cref="TimeSpan"/> rather than an
	/// <see cref="NpgsqlInterval"/>. If you need the extra functionality of <see cref="NpgsqlInterval"/>
	/// then use <see cref="Npgsql.NpgsqlDataReader.GetInterval(Int32)"/>.</para>
	/// </remarks>
	/// <seealso cref="Ticks"/>
	/// <seealso cref="JustifyDays"/>
	/// <seealso cref="JustifyMonths"/>
	/// <seealso cref="Canonicalize()"/>
	/// </summary>
	[Serializable]
	public struct NpgsqlInterval : IComparable, IComparer, IEquatable<NpgsqlInterval>, IComparable<NpgsqlInterval>,
	                               IComparer<NpgsqlInterval>
	{
		#region Constants

		/// <summary>
		/// Represents the number of ticks (100ns periods) in one microsecond. This field is constant.
		/// </summary>
		public const long TicksPerMicrosecond = TimeSpan.TicksPerMillisecond/1000;

		/// <summary>
		/// Represents the number of ticks (100ns periods) in one millisecond. This field is constant.
		/// </summary>
		public const long TicksPerMillsecond = TimeSpan.TicksPerMillisecond;

		/// <summary>
		/// Represents the number of ticks (100ns periods) in one second. This field is constant.
		/// </summary>
		public const long TicksPerSecond = TimeSpan.TicksPerSecond;

		/// <summary>
		/// Represents the number of ticks (100ns periods) in one minute. This field is constant.
		/// </summary>
		public const long TicksPerMinute = TimeSpan.TicksPerMinute;

		/// <summary>
		/// Represents the number of ticks (100ns periods) in one hour. This field is constant.
		/// </summary>
		public const long TicksPerHour = TimeSpan.TicksPerHour;

		/// <summary>
		/// Represents the number of ticks (100ns periods) in one day. This field is constant.
		/// </summary>
		public const long TicksPerDay = TimeSpan.TicksPerDay;

		/// <summary>
		/// Represents the number of hours in one day (assuming no daylight savings adjustments). This field is constant.
		/// </summary>
		public const int HoursPerDay = 24;

		/// <summary>
		/// Represents the number of days assumed in one month if month justification or unjustifcation is performed.
		/// This is set to 30 for consistency with PostgreSQL. Note that this is means that month adjustments cause
		/// a year to be taken as 30 &#xd7; 12 = 360 rather than 356/366 days.
		/// </summary>
		public const int DaysPerMonth = 30;

		/// <summary>
		/// Represents the number of ticks (100ns periods) in one day, assuming 30 days per month. <seealso cref="DaysPerMonth"/>
		/// </summary>
		public const long TicksPerMonth = TicksPerDay*DaysPerMonth;

		/// <summary>
		/// Represents the number of months in a year. This field is constant.
		/// </summary>
		public const int MonthsPerYear = 12;

		/// <summary>
		/// Represents the maximum <see cref="NpgsqlInterval"/>. This field is read-only.
		/// </summary>
		public static readonly NpgsqlInterval MaxValue = new NpgsqlInterval(long.MaxValue);

		/// <summary>
		/// Represents the minimum <see cref="NpgsqlInterval"/>. This field is read-only.
		/// </summary>
		public static readonly NpgsqlInterval MinValue = new NpgsqlInterval(long.MinValue);

		/// <summary>
		/// Represents the zero <see cref="NpgsqlInterval"/>. This field is read-only.
		/// </summary>
		public static readonly NpgsqlInterval Zero = new NpgsqlInterval(0);

		#endregion

		private readonly int _months;
		private readonly int _days;
		private readonly long _ticks;

		#region Constructors

		/// <summary>
		/// Initializes a new <see cref="NpgsqlInterval"/> to the specified number of ticks.
		/// </summary>
		/// <param name="ticks">A time period expressed in 100ns units.</param>
		public NpgsqlInterval(long ticks)
		{
			_months = 0;
			_days = 0;
			_ticks = ticks;
		}

		/// <summary>
		/// Initializes a new <see cref="NpgsqlInterval"/> to hold the same time as a <see cref="TimeSpan"/>
		/// </summary>
		/// <param name="timespan">A time period expressed in a <see cref="TimeSpan"/></param>
		public NpgsqlInterval(TimeSpan timespan)
			: this(timespan.Ticks)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="NpgsqlInterval"/> to the specified number of months, days
		/// &amp; ticks.
		/// </summary>
		/// <param name="months">Number of months.</param>
		/// <param name="days">Number of days.</param>
		/// <param name="ticks">Number of 100ns units.</param>
		public NpgsqlInterval(int months, int days, long ticks)
		{
			_months = months;
			_days = days;
			_ticks = ticks;
		}

		/// <summary>
		/// Initializes a new <see cref="NpgsqlInterval"/> to the specified number of
		/// days, hours, minutes &amp; seconds.
		/// </summary>
		/// <param name="days">Number of days.</param>
		/// <param name="hours">Number of hours.</param>
		/// <param name="minutes">Number of minutes.</param>
		/// <param name="seconds">Number of seconds.</param>
		public NpgsqlInterval(int days, int hours, int minutes, int seconds)
			: this(0, days, new TimeSpan(hours, minutes, seconds).Ticks)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="NpgsqlInterval"/> to the specified number of
		/// days, hours, minutes, seconds &amp; milliseconds.
		/// </summary>
		/// <param name="days">Number of days.</param>
		/// <param name="hours">Number of hours.</param>
		/// <param name="minutes">Number of minutes.</param>
		/// <param name="seconds">Number of seconds.</param>
		/// <param name="milliseconds">Number of milliseconds.</param>
		public NpgsqlInterval(int days, int hours, int minutes, int seconds, int milliseconds)
			: this(0, days, new TimeSpan(hours, minutes, seconds, milliseconds).Ticks)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="NpgsqlInterval"/> to the specified number of
		/// months, days, hours, minutes, seconds &amp; milliseconds.
		/// </summary>
		/// <param name="months">Number of months.</param>
		/// <param name="days">Number of days.</param>
		/// <param name="hours">Number of hours.</param>
		/// <param name="minutes">Number of minutes.</param>
		/// <param name="seconds">Number of seconds.</param>
		/// <param name="milliseconds">Number of milliseconds.</param>
		public NpgsqlInterval(int months, int days, int hours, int minutes, int seconds, int milliseconds)
			: this(months, days, new TimeSpan(hours, minutes, seconds, milliseconds).Ticks)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="NpgsqlInterval"/> to the specified number of
		/// years, months, days, hours, minutes, seconds &amp; milliseconds.
		/// <para>Years are calculated exactly equivalent to 12 months.</para>
		/// </summary>
		/// <param name="years">Number of years.</param>
		/// <param name="months">Number of months.</param>
		/// <param name="days">Number of days.</param>
		/// <param name="hours">Number of hours.</param>
		/// <param name="minutes">Number of minutes.</param>
		/// <param name="seconds">Number of seconds.</param>
		/// <param name="milliseconds">Number of milliseconds.</param>
		public NpgsqlInterval(int years, int months, int days, int hours, int minutes, int seconds, int milliseconds)
			: this(years*12 + months, days, new TimeSpan(hours, minutes, seconds, milliseconds).Ticks)
		{
		}

		#endregion

		#region Whole Parts

		/// <summary>
		/// The total number of ticks(100ns units) contained. This is the resolution of the
		/// <see cref="NpgsqlInterval"/>  type. This ignores the number of days and
		/// months held. If you want them included use <see cref="UnjustifyInterval()"/> first.
		/// <remarks>The resolution of the PostgreSQL
		/// interval type is by default 1&#xb5;s = 1,000 ns. It may be smaller as follows:
		/// <list type="number">
		/// <item>
		/// <term>interval(0)</term>
		/// <description>resolution of 1s (1 second)</description>
		/// </item>
		/// <item>
		/// <term>interval(1)</term>
		/// <description>resolution of 100ms = 0.1s (100 milliseconds)</description>
		/// </item>
		/// <item>
		/// <term>interval(2)</term>
		/// <description>resolution of 10ms = 0.01s (10 milliseconds)</description>
		/// </item>
		/// <item>
		/// <term>interval(3)</term>
		/// <description>resolution of 1ms = 0.001s (1 millisecond)</description>
		/// </item>
		/// <item>
		/// <term>interval(4)</term>
		/// <description>resolution of 100&#xb5;s = 0.0001s (100 microseconds)</description>
		/// </item>
		/// <item>
		/// <term>interval(5)</term>
		/// <description>resolution of 10&#xb5;s = 0.00001s (10 microseconds)</description>
		/// </item>
		/// <item>
		/// <term>interval(6) or interval</term>
		/// <description>resolution of 1&#xb5;s = 0.000001s (1 microsecond)</description>
		/// </item>
		/// </list>
		/// <para>As such, if the 100-nanosecond resolution is significant to an application, a PostgreSQL interval will
		/// not suffice for those purposes.</para>
		/// <para>In more frequent cases though, the resolution of the interval suffices.
		/// <see cref="NpgsqlInterval"/> will always suffice to handle the resolution of any interval value, and upon
		/// writing to the database, will be rounded to the resolution used.</para>
		/// </remarks>
		/// <returns>The number of ticks in the instance.</returns>
		/// </summary>
		public long Ticks
		{
			get { return _ticks; }
		}

		/// <summary>
		/// Gets the number of whole microseconds held in the instance.
		/// <returns>An  in the range [-999999, 999999].</returns>
		/// </summary>
		public int Microseconds
		{
			get { return (int) (_ticks/10)%1000000; }
		}

		/// <summary>
		/// Gets the number of whole milliseconds held in the instance.
		/// <returns>An  in the range [-999, 999].</returns>
		/// </summary>
		public int Milliseconds
		{
			get { return (int) ((_ticks/TicksPerMillsecond)%1000); }
		}

		/// <summary>
		/// Gets the number of whole seconds held in the instance.
		/// <returns>An  in the range [-59, 59].</returns>
		/// </summary>
		public int Seconds
		{
			get { return (int) ((_ticks/TicksPerSecond)%60); }
		}

		/// <summary>
		/// Gets the number of whole minutes held in the instance.
		/// <returns>An  in the range [-59, 59].</returns>
		/// </summary>
		public int Minutes
		{
			get { return (int) ((_ticks/TicksPerMinute)%60); }
		}

		/// <summary>
		/// Gets the number of whole hours held in the instance.
		/// <remarks>Note that this can be less than -23 or greater than 23 unless <see cref="JustifyDays()"/>
		/// has been used to produce this instance.</remarks>
		/// </summary>
		public int Hours
		{
			get { return (int) (_ticks/TicksPerHour); }
		}

		/// <summary>
		/// Gets the number of days held in the instance.
		/// <remarks>Note that this does not pay attention to a time component with -24 or less hours or
		/// 24 or more hours, unless <see cref="JustifyDays()"/> has been called to produce this instance.</remarks>
		/// </summary>
		public int Days
		{
			get { return _days; }
		}

		/// <summary>
		/// Gets the number of months held in the instance.
		/// <remarks>Note that this does not pay attention to a day component with -30 or less days or
		/// 30 or more days, unless <see cref="JustifyMonths()"/> has been called to produce this instance.</remarks>
		/// </summary>
		public int Months
		{
			get { return _months; }
		}

		/// <summary>
		/// Returns a <see cref="TimeSpan"/> representing the time component of the instance.
		/// <remarks>Note that this may have a value beyond the range &#xb1;23:59:59.9999999 unless
		/// <see cref="JustifyDays()"/> has been called to produce this instance.</remarks>
		/// </summary>
		public TimeSpan Time
		{
			get { return new TimeSpan(_ticks); }
		}

		#endregion

		#region Total Parts

		/// <summary>
		/// The total number of ticks (100ns units) in the instance, assuming 24 hours in each day and
		/// 30 days in a month.
		/// </summary>
		public long TotalTicks
		{
			get { return Ticks + Days*TicksPerDay + Months*TicksPerMonth; }
		}

		/// <summary>
		/// The total number of microseconds in the instance, assuming 24 hours in each day and
		/// 30 days in a month.
		/// </summary>
		public double TotalMicroseconds
		{
			get { return TotalTicks/10d; }
		}

		/// <summary>
		/// The total number of milliseconds in the instance, assuming 24 hours in each day and
		/// 30 days in a month.
		/// </summary>
		public double TotalMilliseconds
		{
			get { return TotalTicks/(double) TicksPerMillsecond; }
		}

		/// <summary>
		/// The total number of seconds in the instance, assuming 24 hours in each day and
		/// 30 days in a month.
		/// </summary>
		public double TotalSeconds
		{
			get { return TotalTicks/(double) TicksPerSecond; }
		}

		/// <summary>
		/// The total number of minutes in the instance, assuming 24 hours in each day and
		/// 30 days in a month.
		/// </summary>
		public double TotalMinutes
		{
			get { return TotalTicks/(double) TicksPerMinute; }
		}

		/// <summary>
		/// The total number of hours in the instance, assuming 24 hours in each day and
		/// 30 days in a month.
		/// </summary>
		public double TotalHours
		{
			get { return TotalTicks/(double) TicksPerHour; }
		}

		/// <summary>
		/// The total number of days in the instance, assuming 24 hours in each day and
		/// 30 days in a month.
		/// </summary>
		public double TotalDays
		{
			get { return TotalTicks/(double) TicksPerDay; }
		}

		/// <summary>
		/// The total number of months in the instance, assuming 24 hours in each day and
		/// 30 days in a month.
		/// </summary>
		public double TotalMonths
		{
			get { return TotalTicks/(double) TicksPerMonth; }
		}

		#endregion

		#region Create From Part

		/// <summary>
		/// Creates an <see cref="NpgsqlInterval"/> from a number of ticks.
		/// </summary>
		/// <param name="ticks">The number of ticks (100ns units) in the interval.</param>
		/// <returns>A <see cref="Canonicalize()"/>d <see cref="NpgsqlInterval"/> with the given number of ticks.</returns>
		public static NpgsqlInterval FromTicks(long ticks)
		{
			return new NpgsqlInterval(ticks).Canonicalize();
		}

		/// <summary>
		/// Creates an <see cref="NpgsqlInterval"/> from a number of microseconds.
		/// </summary>
		/// <param name="ticks">The number of microseconds in the interval.</param>
		/// <returns>A <see cref="Canonicalize()"/>d <see cref="NpgsqlInterval"/> with the given number of microseconds.</returns>
		public static NpgsqlInterval FromMicroseconds(double micro)
		{
			return FromTicks((long) (micro*TicksPerMicrosecond));
		}

		/// <summary>
		/// Creates an <see cref="NpgsqlInterval"/> from a number of milliseconds.
		/// </summary>
		/// <param name="ticks">The number of milliseconds in the interval.</param>
		/// <returns>A <see cref="Canonicalize()"/>d <see cref="NpgsqlInterval"/> with the given number of milliseconds.</returns>
		public static NpgsqlInterval FromMilliseconds(double milli)
		{
			return FromTicks((long) (milli*TicksPerMillsecond));
		}

		/// <summary>
		/// Creates an <see cref="NpgsqlInterval"/> from a number of seconds.
		/// </summary>
		/// <param name="ticks">The number of seconds in the interval.</param>
		/// <returns>A <see cref="Canonicalize()"/>d <see cref="NpgsqlInterval"/> with the given number of seconds.</returns>
		public static NpgsqlInterval FromSeconds(double seconds)
		{
			return FromTicks((long) (seconds*TicksPerSecond));
		}

		/// <summary>
		/// Creates an <see cref="NpgsqlInterval"/> from a number of minutes.
		/// </summary>
		/// <param name="ticks">The number of minutes in the interval.</param>
		/// <returns>A <see cref="Canonicalize()"/>d <see cref="NpgsqlInterval"/> with the given number of minutes.</returns>
		public static NpgsqlInterval FromMinutes(double minutes)
		{
			return FromTicks((long) (minutes*TicksPerMinute));
		}

		/// <summary>
		/// Creates an <see cref="NpgsqlInterval"/> from a number of hours.
		/// </summary>
		/// <param name="ticks">The number of hours in the interval.</param>
		/// <returns>A <see cref="Canonicalize()"/>d <see cref="NpgsqlInterval"/> with the given number of hours.</returns>
		public static NpgsqlInterval FromHours(double hours)
		{
			return FromTicks((long) (hours*TicksPerHour));
		}

		/// <summary>
		/// Creates an <see cref="NpgsqlInterval"/> from a number of days.
		/// </summary>
		/// <param name="ticks">The number of days in the interval.</param>
		/// <returns>A <see cref="Canonicalize()"/>d <see cref="NpgsqlInterval"/> with the given number of days.</returns>
		public static NpgsqlInterval FromDays(double days)
		{
			return FromTicks((long) (days*TicksPerDay));
		}

		/// <summary>
		/// Creates an <see cref="NpgsqlInterval"/> from a number of months.
		/// </summary>
		/// <param name="ticks">The number of months in the interval.</param>
		/// <returns>A <see cref="Canonicalize()"/>d <see cref="NpgsqlInterval"/> with the given number of months.</returns>
		public static NpgsqlInterval FromMonths(double months)
		{
			return FromTicks((long) (months*TicksPerMonth));
		}

		#endregion

		#region Arithmetic

		/// <summary>
		/// Adds another interval to this instance and returns the result.
		/// </summary>
		/// <param name="interval">An <see cref="NpgsqlInterval"/> to add to this instance.</param>
		/// <returns>An <see cref="NpgsqlInterval"></see> whose values are the sums of the two instances.</returns>
		public NpgsqlInterval Add(NpgsqlInterval interval)
		{
			return new NpgsqlInterval(Months + interval.Months, Days + interval.Days, Ticks + interval.Ticks);
		}

		/// <summary>
		/// Subtracts another interval from this instance and returns the result.
		/// </summary>
		/// <param name="interval">An <see cref="NpgsqlInterval"/> to subtract from this instance.</param>
		/// <returns>An <see cref="NpgsqlInterval"></see> whose values are the differences of the two instances.</returns>
		public NpgsqlInterval Subtract(NpgsqlInterval interval)
		{
			return new NpgsqlInterval(Months - interval.Months, Days - interval.Days, Ticks - interval.Ticks);
		}

		/// <summary>
		/// Returns an <see cref="NpgsqlInterval"/> whose value is the negated value of this instance.
		/// </summary>
		/// <returns>An <see cref="NpgsqlInterval"/> whose value is the negated value of this instance.</returns>
		public NpgsqlInterval Negate()
		{
			return new NpgsqlInterval(-Months, -Days, -Ticks);
		}

		/// <summary>
		/// This absolute value of this instance. In the case of some, but not all, components being negative,
		/// the rules used for justification are used to determine if the instance is positive or negative.
		/// </summary>
		/// <returns>An <see cref="NpgsqlInterval"/> whose value is the absolute value of this instance.</returns>
		public NpgsqlInterval Duration()
		{
			return UnjustifyInterval().Ticks < 0 ? Negate() : this;
		}

		#endregion

		#region Justification

		/// <summary>
		/// Equivalent to PostgreSQL's justify_days function.
		/// </summary>
		/// <returns>An <see cref="NpgsqlInterval"/> based on this one, but with any hours outside of the range [-23, 23]
		/// converted into days.</returns>
		public NpgsqlInterval JustifyDays()
		{
			return new NpgsqlInterval(Months, Days + (int) (Ticks/TicksPerDay), Ticks%TicksPerDay);
		}

		/// <summary>
		/// Opposite to PostgreSQL's justify_days function.
		/// </summary>
		/// <returns>An <see cref="NpgsqlInterval"/> based on this one, but with any days converted to multiples of &#xB1;24hours.</returns>
		public NpgsqlInterval UnjustifyDays()
		{
			return new NpgsqlInterval(Months, 0, Ticks + Days*TicksPerDay);
		}

		/// <summary>
		/// Equivalent to PostgreSQL's justify_months function.
		/// </summary>
		/// <returns>An <see cref="NpgsqlInterval"/> based on this one, but with any days outside of the range [-30, 30]
		/// converted into months.</returns>
		public NpgsqlInterval JustifyMonths()
		{
			return new NpgsqlInterval(Months + Days/DaysPerMonth, Days%DaysPerMonth, Ticks);
		}

		/// <summary>
		/// Opposite to PostgreSQL's justify_months function.
		/// </summary>
		/// <returns>An <see cref="NpgsqlInterval"/> based on this one, but with any months converted to multiples of &#xB1;30days.</returns>
		public NpgsqlInterval UnjustifyMonths()
		{
			return new NpgsqlInterval(0, Days + Months*DaysPerMonth, Ticks);
		}

		/// <summary>
		/// Equivalent to PostgreSQL's justify_interval function.
		/// </summary>
		/// <returns>An <see cref="NpgsqlInterval"/> based on this one,
		/// but with any months converted to multiples of &#xB1;30days
		/// and then with any days converted to multiples of &#xB1;24hours</returns>
		public NpgsqlInterval JustifyInterval()
		{
			return JustifyMonths().JustifyDays();
		}

		/// <summary>
		/// Opposite to PostgreSQL's justify_interval function.
		/// </summary>
		/// <returns>An <see cref="NpgsqlInterval"/> based on this one, but with any months converted to multiples of &#xB1;30days and then any days converted to multiples of &#xB1;24hours;</returns>
		public NpgsqlInterval UnjustifyInterval()
		{
			return new NpgsqlInterval(Ticks + Days*TicksPerDay + Months*DaysPerMonth*TicksPerDay);
		}

		/// <summary>
		/// Produces a canonical NpgslInterval with 0 months and hours in the range of [-23, 23].
		/// <remarks>
		/// <para>
		/// While the fact that for many purposes, two different <see cref="NpgsqlInterval"/> instances could be considered
		/// equivalent (e.g. one with 2days, 3hours and one with 1day 27hours) there are different possible canonical forms.
		/// </para><para>
		/// E.g. we could move all excess hours into days and all excess days into months and have the most readable form,
		/// or we could move everything into the ticks and have the form that allows for the easiest arithmetic) the form
		/// chosen has two important properties that make it the best choice.
		/// </para><para>First, it is closest two how
		/// <see cref="TimeSpan"/> objects are most often represented. Second, it is compatible with results of many
		/// PostgreSQL functions, particularly with age() and the results of subtracting one date, time or timestamp from
		/// another.
		/// </para>
		/// <para>Note that the results of casting a <see cref="TimeSpan"/> to <see cref="NpgsqlInterval"/> is
		/// canonicalised.</para>
		/// </remarks>
		/// <returns>An <see cref="NpgsqlInterval"/> based on this one, but with months converted to multiples of &#xB1;30days and with any hours outside of the range [-23, 23]
		/// converted into days.</return>
		public NpgsqlInterval Canonicalize()
		{
			return new NpgsqlInterval(0, Days + Months*DaysPerMonth + (int) (Ticks/TicksPerDay), Ticks%TicksPerDay);
		}

		#endregion

		#region Casts

		/// <summary>
		/// Implicit cast of a <see cref="TimeSpan"/> to an <see cref="NpgsqlInterval"/>
		/// </summary>
		/// <param name="timespan">A <see cref="TimeSpan"/></param>
		/// <returns>An eqivalent, canonical, <see cref="NpgsqlInterval"/>.</returns>
		public static implicit operator NpgsqlInterval(TimeSpan timespan)
		{
			return new NpgsqlInterval(timespan).Canonicalize();
		}

		/// <summary>
		/// Implicit cast of an <see cref="NpgsqlInterval"/> to a <see cref="TimeSpan"/>.
		/// </summary>
		/// <param name="interval">A <see cref="NpgsqlInterval"/>.</param>
		/// <returns>An equivalent <see cref="TimeSpan"/>.</returns>
		public static explicit operator TimeSpan(NpgsqlInterval interval)
		{
			return new TimeSpan(interval.Ticks + interval.Days*TicksPerDay + interval.Months*DaysPerMonth*TicksPerDay);
		}

		#endregion

		#region Comparison

		/// <summary>
		/// Returns true if another <see cref="NpgsqlInterval"/> is exactly the same as this instance.
		/// </summary>
		/// <param name="other">An <see cref="NpgsqlInterval"/> for comparison.</param>
		/// <returns>true if the two <see cref="NpgsqlInterval"/> instances are exactly the same,
		/// false otherwise.</returns>
		public bool Equals(NpgsqlInterval other)
		{
			return Ticks == other.Ticks && Days == other.Days && Months == other.Months;
		}

		/// <summary>
		/// Returns true if another object is an <see cref="NpgsqlInterval"/>, that is exactly the same as
		/// this instance
		/// </summary>
		/// <param name="obj">An <see cref="Object"/> for comparison.</param>
		/// <returns>true if the argument is an <see cref="NpgsqlInterval"/> and is exactly the same
		/// as this one, false otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is NpgsqlInterval)
			{
				return Equals((NpgsqlInterval) obj);
			}
			return false;
		}

		/// <summary>
		/// Compares two <see cref="NpgsqlInterval"/> instances.
		/// </summary>
		/// <param name="x">The first <see cref="NpgsqlInterval"/>.</param>
		/// <param name="y">The second <see cref="NpgsqlInterval"/>.</param>
		/// <returns>0 if the two are equal or equivalent. A value greater than zero if x is greater than y,
		/// a value less than zero if x is less than y.</returns>
		public static int Compare(NpgsqlInterval x, NpgsqlInterval y)
		{
			return x.CompareTo(y);
		}

		int IComparer<NpgsqlInterval>.Compare(NpgsqlInterval x, NpgsqlInterval y)
		{
			return x.CompareTo(y);
		}

		int IComparer.Compare(object x, object y)
		{
			if (x == null)
			{
				return y == null ? 0 : 1;
			}
			if (y == null)
			{
				return -1;
			}
			try
			{
				return ((IComparable) x).CompareTo(y);
			}
			catch (Exception)
			{
				throw new ArgumentException();
			}
		}

		/// <summary>
		/// A hash code suitable for uses with hashing algorithms.
		/// </summary>
		/// <returns>An signed integer.</returns>
		public override int GetHashCode()
		{
			return UnjustifyInterval().Ticks.GetHashCode();
		}

		/// <summary>
		/// Compares this instance with another/
		/// </summary>
		/// <param name="other">An <see cref="NpgsqlInterval"/> to compare this with.</param>
		/// <returns>0 if the instances are equal or equivalent. A value less than zero if
		/// this instance is less than the argument. A value greater than zero if this instance
		/// is greater than the instance.</returns>
		public int CompareTo(NpgsqlInterval other)
		{
			return UnjustifyInterval().Ticks.CompareTo(other.UnjustifyInterval().Ticks);
		}

		/// <summary>
		/// Compares this instance with another/
		/// </summary>
		/// <param name="other">An object to compare this with.</param>
		/// <returns>0 if the argument is an <see cref="NpgsqlInterval"/> and the instances are equal or equivalent.
		/// A value less than zero if the argument is an <see cref="NpgsqlInterval"/> and
		/// this instance is less than the argument.
		/// A value greater than zero if the argument is an <see cref="NpgsqlInterval"/> and this instance
		/// is greater than the instance.</returns>
		/// A value greater than zero if the argument is null.
		/// <exception cref="ArgumentException">The argument is not an <see cref="NpgsqlInterval"/>.</exception>
		public int CompareTo(object other)
		{
			if (other == null)
			{
				return 1;
			}
			else if (other is NpgsqlInterval)
			{
				return CompareTo((NpgsqlInterval) other);
			}
			else
			{
				throw new ArgumentException();
			}
		}

		#endregion

		#region To And From Strings

		/// <summary>
		/// Parses a <see cref="String"/> and returns a <see cref="NpgsqlInterval"/> instance.
		/// Designed to use the formats generally returned by PostgreSQL.
		/// </summary>
		/// <param name="str">The <see cref="String"/> to parse.</param>
		/// <returns>An <see cref="NpgsqlInterval"/> represented by the argument.</returns>
		/// <exception cref="ArgumentNullException">The string was null.</exception>
		/// <exception cref="OverflowException">A value obtained from parsing the string exceeded the values allowed for the relevant component.</exception>
		/// <exception cref="FormatException">The string was not in a format that could be parsed to produce an <see cref="NpgsqlInterval"/>.</exception>
		public static NpgsqlInterval Parse(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			str = str.Replace('s', ' '); //Quick and easy way to catch plurals.
			try
			{
				int years = 0;
				int months = 0;
				int days = 0;
				int hours = 0;
				int minutes = 0;
				decimal seconds = 0m;
				int idx = str.IndexOf("year");
				if (idx > 0)
				{
					years = int.Parse(str.Substring(0, idx));
					str = str.Substring(idx + 5);
				}
				idx = str.IndexOf("mon");
				if (idx > 0)
				{
					months = int.Parse(str.Substring(0, idx));
					str = str.Substring(idx + 4);
				}
				idx = str.IndexOf("day");
				if (idx > 0)
				{
					days = int.Parse(str.Substring(0, idx));
					str = str.Substring(idx + 4).Trim();
				}
				if (str.Length > 0)
				{
					string[] parts = str.Split(':');
					switch (parts.Length) //One of those times that fall-through would actually be good.
					{
						case 1:
							hours = int.Parse(parts[0]);
							break;
						case 2:
							hours = int.Parse(parts[0]);
							minutes = int.Parse(parts[1]);
							break;
						default:
							hours = int.Parse(parts[0]);
							minutes = int.Parse(parts[1]);
							seconds = decimal.Parse(parts[2]);
							break;
					}
				}
				long ticks = hours*TicksPerHour + minutes*TicksPerMinute + (long) (seconds*TicksPerSecond);
				return new NpgsqlInterval(years*MonthsPerYear + months, days, ticks);
			}
			catch (OverflowException)
			{
				throw;
			}
			catch (Exception)
			{
				throw new FormatException();
			}
		}

		/// <summary>
		/// Attempt to parse a <see cref="String"/> to produce an <see cref="NpgsqlInterval"/>.
		/// </summary>
		/// <param name="str">The <see cref="String"/> to parse.</param>
		/// <param name="result">(out) The <see cref="NpgsqlInterval"/> produced, or <see cref="Zero"/> if the parsing failed.</param>
		/// <returns>true if the parsing succeeded, false otherwise.</returns>
		public static bool TryParse(string str, out NpgsqlInterval result)
		{
			try
			{
				result = Parse(str);
				return true;
			}
			catch (Exception)
			{
				result = Zero;
				return false;
			}
		}

		/// <summary>
		/// Create a <see cref="String"/> representation of the <see cref="NpgsqlInterval"/> instance.
		/// The format returned is of the form:
		/// [M mon[s]] [d day[s]] [HH:mm:ss[.f[f[f[f[f[f[f[f[f]]]]]]]]]]
		/// A zero <see cref="NpgsqlInterval"/> is represented as 00:00:00
		/// <remarks>
		/// Ticks are 100ns, Postgress resolution is only to 1&#xb5;s at most. Hence we lose 1 or more decimal
		/// precision in storing values in the database. Despite this, this method will output that extra
		/// digit of precision. It's forward-compatible with any future increases in resolution up to 100ns,
		/// and also makes this ToString() more applicable to any other use-case.
		/// </remarks>
		/// </summary>
		/// <returns>The <see cref="String"/> representation.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			if (Months != 0)
			{
				sb.Append(Months).Append(Math.Abs(Months) == 1 ? " mon " : " mons ");
			}
			if (Days != 0)
			{
				if (Months < 0 && Days > 0)
				{
					sb.Append('+');
				}
				sb.Append(Days).Append(Math.Abs(Days) == 1 ? " day " : " days ");
			}
			if (Ticks != 0 || sb.Length == 0)
			{
				if (Days < 0 || (Days == 0 && Months < 0))
				{
					sb.Append('+');
				}
				TimeSpan time = Time;
				sb.Append(time.Hours.ToString("D2")).Append(':').Append(time.Minutes.ToString("D2")).Append(':').Append(
					time.Seconds.ToString("D2"));
				long remainingTicks = Math.Abs(Ticks)%TicksPerSecond;
				if (remainingTicks != 0)
				{
					while (remainingTicks%10 == 0)
					{
						remainingTicks /= 10;
					}
					sb.Append('.').Append(remainingTicks);
				}
			}
			if (sb[sb.Length - 1] == ' ')
			{
				sb.Remove(sb.Length - 1, 1);
			}
			return sb.ToString();
		}

		#endregion

		#region Common Operators

		/// <summary>
		/// Adds two <see cref="NpgsqlInterval"/> together.
		/// </summary>
		/// <param name="x">The first <see cref="NpgsqlInterval"/> to add.</param>
		/// <param name="y">The second <see cref="NpgsqlInterval"/> to add.</param>
		/// <returns>An <see cref="NpgsqlInterval"/> whose values are the sum of the arguments.</returns>
		public static NpgsqlInterval operator +(NpgsqlInterval x, NpgsqlInterval y)
		{
			return x.Add(y);
		}

		/// <summary>
		/// Subtracts one <see cref="NpgsqlInterval"/> from another.
		/// </summary>
		/// <param name="x">The <see cref="NpgsqlInterval"/> to subtract the other from.</param>
		/// <param name="y">The <see cref="NpgsqlInterval"/> to subtract from the other.</param>
		/// <returns>An <see cref="NpgsqlInterval"/> whose values are the difference of the arguments</returns>
		public static NpgsqlInterval operator -(NpgsqlInterval x, NpgsqlInterval y)
		{
			return x.Subtract(y);
		}

		/// <summary>
		/// Returns true if two <see cref="NpgsqlInterval"/> are exactly the same.
		/// </summary>
		/// <param name="x">The first <see cref="NpgsqlInterval"/> to compare.</param>
		/// <param name="y">The second <see cref="NpgsqlInterval"/> to compare.</param>
		/// <returns>true if the two arguments are exactly the same, false otherwise.</returns>
		public static bool operator ==(NpgsqlInterval x, NpgsqlInterval y)
		{
			return x.Equals(y);
		}

		/// <summary>
		/// Returns false if two <see cref="NpgsqlInterval"/> are exactly the same.
		/// </summary>
		/// <param name="x">The first <see cref="NpgsqlInterval"/> to compare.</param>
		/// <param name="y">The second <see cref="NpgsqlInterval"/> to compare.</param>
		/// <returns>false if the two arguments are exactly the same, true otherwise.</returns>
		public static bool operator !=(NpgsqlInterval x, NpgsqlInterval y)
		{
			return !(x == y);
		}

		/// <summary>
		/// Compares two <see cref="NpgsqlInterval"/> instances to see if the first is less than the second
		/// </summary>
		/// <param name="x">The first <see cref="NpgsqlInterval"/> to compare.</param>
		/// <param name="y">The second <see cref="NpgsqlInterval"/> to compare.</param>
		/// <returns>true if the first <see cref="NpgsqlInterval"/> is less than second, false otherwise.</returns>
		public static bool operator <(NpgsqlInterval x, NpgsqlInterval y)
		{
			return x.UnjustifyInterval().Ticks < y.UnjustifyInterval().Ticks;
		}

		/// <summary>
		/// Compares two <see cref="NpgsqlInterval"/> instances to see if the first is less than or equivalent to the second
		/// </summary>
		/// <param name="x">The first <see cref="NpgsqlInterval"/> to compare.</param>
		/// <param name="y">The second <see cref="NpgsqlInterval"/> to compare.</param>
		/// <returns>true if the first <see cref="NpgsqlInterval"/> is less than or equivalent to second, false otherwise.</returns>
		public static bool operator <=(NpgsqlInterval x, NpgsqlInterval y)
		{
			return x.UnjustifyInterval().Ticks <= y.UnjustifyInterval().Ticks;
		}

		/// <summary>
		/// Compares two <see cref="NpgsqlInterval"/> instances to see if the first is greater than the second
		/// </summary>
		/// <param name="x">The first <see cref="NpgsqlInterval"/> to compare.</param>
		/// <param name="y">The second <see cref="NpgsqlInterval"/> to compare.</param>
		/// <returns>true if the first <see cref="NpgsqlInterval"/> is greater than second, false otherwise.</returns>
		public static bool operator >(NpgsqlInterval x, NpgsqlInterval y)
		{
			return !(x <= y);
		}

		/// <summary>
		/// Compares two <see cref="NpgsqlInterval"/> instances to see if the first is greater than or equivalent the second
		/// </summary>
		/// <param name="x">The first <see cref="NpgsqlInterval"/> to compare.</param>
		/// <param name="y">The second <see cref="NpgsqlInterval"/> to compare.</param>
		/// <returns>true if the first <see cref="NpgsqlInterval"/> is greater than or equivalent to the second, false otherwise.</returns>
		public static bool operator >=(NpgsqlInterval x, NpgsqlInterval y)
		{
			return !(x < y);
		}

		/// <summary>
		/// Returns the instance.
		/// </summary>
		/// <param name="x">An <see cref="NpgsqlInterval"/>.</param>
		/// <returns>The argument.</returns>
		public static NpgsqlInterval operator +(NpgsqlInterval x)
		{
			return x;
		}

		/// <summary>
		/// Negates an <see cref="NpgsqlInterval"/> instance.
		/// </summary>
		/// <param name="x">An <see cref="NpgsqlInterval"/>.</param>
		/// <returns>The negation of the argument.</returns>
		public static NpgsqlInterval operator -(NpgsqlInterval x)
		{
			return x.Negate();
		}

		#endregion
	}

	[Serializable]
	public struct NpgsqlDate : IEquatable<NpgsqlDate>, IComparable<NpgsqlDate>, IComparable, IComparer<NpgsqlDate>,
	                           IComparer
	{
		private static readonly int[] CommonYearDays = new int[] {0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365};
		private static readonly int[] LeapYearDays = new int[] {0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366};
		private static readonly int[] CommonYearMaxes = new int[] {31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
		private static readonly int[] LeapYearMaxes = new int[] {31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
		public const int MaxYear = 5874897;
		public const int MinYear = -4714;
		public static readonly NpgsqlDate Epoch = new NpgsqlDate(1970, 1, 1);
		public static readonly NpgsqlDate MaxCalculableValue = new NpgsqlDate(MaxYear, 12, 31);
		public static readonly NpgsqlDate MinCalculableValue = new NpgsqlDate(MinYear, 11, 24);
		public static readonly NpgsqlDate Era = new NpgsqlDate(0);

		public static NpgsqlDate Now
		{
			get { return new NpgsqlDate(DateTime.Now); }
		}

		public static NpgsqlDate Today
		{
			get { return Now; }
		}

		public static NpgsqlDate Yesterday
		{
			get { return Now.AddDays(-1); }
		}

		public static NpgsqlDate Tomorrow
		{
			get { return Now.AddDays(1); }
		}

		public static NpgsqlDate Parse(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			str = str.Trim();
			try
			{
				int idx = str.IndexOf('-');
				if (idx == -1)
				{
					throw new FormatException();
				}
				int year = int.Parse(str.Substring(0, idx));
				int idxLast = idx + 1;
				if ((idx = str.IndexOf('-', idxLast)) == -1)
				{
					throw new FormatException();
				}
				int month = int.Parse(str.Substring(idxLast, idx - idxLast));
				idxLast = idx + 1;
				if ((idx = str.IndexOf(' ', idxLast)) == -1)
				{
					idx = str.Length;
				}
				int day = int.Parse(str.Substring(idxLast, idx - idxLast));
				if (str.Contains("BC"))
				{
					year = -year;
				}
				return new NpgsqlDate(year, month, day);
			}
			catch (OverflowException)
			{
				throw;
			}
			catch (Exception)
			{
				throw new FormatException();
			}
		}

		public static bool TryParse(string str, out NpgsqlDate date)
		{
			try
			{
				date = Parse(str);
				return true;
			}
			catch
			{
				date = Era;
				return false;
			}
		}

		//Number of days since January 1st CE (January 1st EV). 1 Jan 1 CE = 0, 2 Jan 1 CE = 1, 31 Dec 1 BCE = -1, etc.
		private readonly int _daysSinceEra;

		public NpgsqlDate(int days)
		{
			_daysSinceEra = days;
		}

		public NpgsqlDate(DateTime dateTime)
			: this((int) (dateTime.Ticks/TimeSpan.TicksPerDay))
		{
		}

		public NpgsqlDate(NpgsqlDate copyFrom)
			: this(copyFrom._daysSinceEra)
		{
		}

		public NpgsqlDate(int year, int month, int day)
		{
			if (year == 0 || year < MinYear || year > MaxYear || month < 1 || month > 12 || day < 1 ||
			    (day > (IsLeap(year) ? 366 : 365)))
			{
				throw new ArgumentOutOfRangeException();
			}

			_daysSinceEra = DaysForYears(year) + (IsLeap(year) ? LeapYearDays : CommonYearDays)[month - 1] + day - 1;
		}

		private const int DaysInYear = 365; //Common years
		private const int DaysIn4Years = 4*DaysInYear + 1; //Leap year every 4 years.
		private const int DaysInCentury = 25*DaysIn4Years - 1; //Except no leap year every 100.
		private const int DaysIn4Centuries = 4*DaysInCentury + 1; //Except leap year every 400.

		private static int DaysForYears(int years)
		{
			//Number of years after 1CE (0 for 1CE, -1 for 1BCE, 1 for 2CE).
			int calcYear = years < 1 ? years : years - 1;

			return calcYear/400*DaysIn4Centuries //Blocks of 400 years with their leap and common years
			       + calcYear%400/100*DaysInCentury //Remaining blocks of 100 years with their leap and common years
			       + calcYear%100/4*DaysIn4Years //Remaining blocks of 4 years with their leap and common years
			       + calcYear%4*DaysInYear //Remaining years, all common
			       + (calcYear < 0 ? -1 : 0); //And 1BCE is leap.
		}

		public int DayOfYear
		{
			get { return _daysSinceEra - DaysForYears(Year) + 1; }
		}

		public int Year
		{
			get
			{
				int guess = (int) Math.Round(_daysSinceEra/365.2425);
				int test = guess - 1;
				while (DaysForYears(++test) <= _daysSinceEra)
				{
					;
				}
				return test - 1;
			}
		}

		public int Month
		{
			get
			{
				int i = 1;
				int target = DayOfYear;
				int[] array = IsLeapYear ? LeapYearDays : CommonYearDays;
				while (target > array[i])
				{
					++i;
				}
				return i;
			}
		}

		public int Day
		{
			get { return DayOfYear - (IsLeapYear ? LeapYearDays : CommonYearDays)[Month - 1]; }
		}

		public DayOfWeek DayOfWeek
		{
			get { return (DayOfWeek) ((_daysSinceEra + 1)%7); }
		}

		internal int DaysSinceEra
		{
			get { return _daysSinceEra; }
		}

		public bool IsLeapYear
		{
			get { return IsLeap(Year); }
		}

		private static bool IsLeap(int year)
		{
			//Every 4 years is a leap year
			//Except every 100 years isn't a leap year.
			//Except every 400 years is.
			if (year < 1)
			{
				year = year + 1;
			}
			return (year%4 == 0) && ((year%100 != 0) || (year%400 == 0));
		}

		public NpgsqlDate AddDays(int days)
		{
			return new NpgsqlDate(_daysSinceEra + days);
		}

		public NpgsqlDate AddYears(int years)
		{
			int newYear = Year + years;
			if (newYear >= 0 && _daysSinceEra < 0) //cross 1CE/1BCE divide going up
			{
				++newYear;
			}
			else if (newYear <= 0 && _daysSinceEra >= 0) //cross 1CE/1BCE divide going down
			{
				--newYear;
			}
			return new NpgsqlDate(newYear, Month, Day);
		}

		public NpgsqlDate AddMonths(int months)
		{
			int newMonthOffset = Month - 1 + months;
			int newYear = Year + newMonthOffset/12;
			int maxDay = (IsLeap(newYear) ? LeapYearMaxes : CommonYearMaxes)[newMonthOffset];
			int newDay = Day > maxDay ? maxDay : Day;
			return new NpgsqlDate(newYear, newMonthOffset + 1, newDay);
		}

		public NpgsqlDate Add(NpgsqlInterval interval)
		{
			return AddMonths(interval.Months).AddDays(interval.Days);
		}

		internal NpgsqlDate Add(NpgsqlInterval interval, int carriedOverflow)
		{
			return AddMonths(interval.Months).AddDays(interval.Days + carriedOverflow);
		}

		public int Compare(NpgsqlDate x, NpgsqlDate y)
		{
			return x.CompareTo(y);
		}

		public int Compare(object x, object y)
		{
			if (x == null)
			{
				return y == null ? 0 : -1;
			}
			if (y == null)
			{
				return 1;
			}
			if (!(x is IComparable) || !(y is IComparable))
			{
				throw new ArgumentException();
			}
			return ((IComparable) x).CompareTo(y);
		}

		public bool Equals(NpgsqlDate other)
		{
			return _daysSinceEra == other._daysSinceEra;
		}

		public override bool Equals(object obj)
		{
			return obj != null && obj is NpgsqlDate && Equals((NpgsqlDate) obj);
		}

		public int CompareTo(NpgsqlDate other)
		{
			return _daysSinceEra.CompareTo(other._daysSinceEra);
		}

		public int CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}
			if (obj is NpgsqlDate)
			{
				return CompareTo((NpgsqlDate) obj);
			}
			throw new ArgumentException();
		}

		public override int GetHashCode()
		{
			return _daysSinceEra;
		}

		public override string ToString()
		{
			//Format of yyyy-MM-dd with " BC" for BCE and optional " AD" for CE which we omit here.
			return
				new StringBuilder(Math.Abs(Year).ToString("D4")).Append('-').Append(Month.ToString("D2")).Append('-').Append(
					Day.ToString("D2")).Append(_daysSinceEra < 0 ? " BC" : "").ToString();
		}

		public static bool operator ==(NpgsqlDate x, NpgsqlDate y)
		{
			return x.Equals(y);
		}

		public static bool operator !=(NpgsqlDate x, NpgsqlDate y)
		{
			return !(x == y);
		}

		public static bool operator <(NpgsqlDate x, NpgsqlDate y)
		{
			return x._daysSinceEra < y._daysSinceEra;
		}

		public static bool operator >(NpgsqlDate x, NpgsqlDate y)
		{
			return x._daysSinceEra > y._daysSinceEra;
		}

		public static bool operator <=(NpgsqlDate x, NpgsqlDate y)
		{
			return x._daysSinceEra <= y._daysSinceEra;
		}

		public static bool operator >=(NpgsqlDate x, NpgsqlDate y)
		{
			return x._daysSinceEra >= y._daysSinceEra;
		}

		public static explicit operator DateTime(NpgsqlDate date)
		{
			try
			{
				return new DateTime(date._daysSinceEra*NpgsqlInterval.TicksPerDay);
			}
			catch
			{
				throw new InvalidCastException();
			}
		}

		public static explicit operator NpgsqlDate(DateTime date)
		{
			return new NpgsqlDate((int) (date.Ticks/NpgsqlInterval.TicksPerDay));
		}

		public static NpgsqlDate operator +(NpgsqlDate date, NpgsqlInterval interval)
		{
			return date.Add(interval);
		}

		public static NpgsqlDate operator +(NpgsqlInterval interval, NpgsqlDate date)
		{
			return date.Add(interval);
		}

		public static NpgsqlDate operator -(NpgsqlDate date, NpgsqlInterval interval)
		{
			return date.Add(-interval);
		}

		public static NpgsqlInterval operator -(NpgsqlDate dateX, NpgsqlDate dateY)
		{
			return new NpgsqlInterval(0, dateX._daysSinceEra - dateY._daysSinceEra, 0);
		}
	}

	[Serializable]
	public struct NpgsqlTimeZone : IEquatable<NpgsqlTimeZone>, IComparable<NpgsqlTimeZone>, IComparable
	{
		public static NpgsqlTimeZone UTC = new NpgsqlTimeZone(0);
		private readonly int _totalSeconds;

		public NpgsqlTimeZone(TimeSpan ts)
			: this(ts.Ticks)
		{
		}

		private NpgsqlTimeZone(long ticks)
		{
			_totalSeconds = (int) (ticks/NpgsqlInterval.TicksPerSecond);
		}

		public NpgsqlTimeZone(NpgsqlInterval ni)
			: this(ni.Ticks)
		{
		}

		public NpgsqlTimeZone(NpgsqlTimeZone copyFrom)
		{
			_totalSeconds = copyFrom._totalSeconds;
		}

		public NpgsqlTimeZone(int hours, int minutes)
			: this(hours, minutes, 0)
		{
		}

		public NpgsqlTimeZone(int hours, int minutes, int seconds)
		{
			_totalSeconds = hours*60*60 + minutes*60 + seconds;
		}

		public static implicit operator NpgsqlTimeZone(NpgsqlInterval interval)
		{
			return new NpgsqlTimeZone(interval);
		}

		public static implicit operator NpgsqlInterval(NpgsqlTimeZone timeZone)
		{
			return new NpgsqlInterval(timeZone._totalSeconds*NpgsqlInterval.TicksPerSecond);
		}

		public static implicit operator NpgsqlTimeZone(TimeSpan interval)
		{
			return new NpgsqlTimeZone(interval);
		}

		public static implicit operator TimeSpan(NpgsqlTimeZone timeZone)
		{
			return new TimeSpan(timeZone._totalSeconds*NpgsqlInterval.TicksPerSecond);
		}

		public static NpgsqlTimeZone SolarTimeZone(decimal longitude)
		{
			return new NpgsqlTimeZone((long) (longitude/15m*NpgsqlInterval.TicksPerHour));
		}

		public int Hours
		{
			get { return _totalSeconds/60/60; }
		}

		public int Minutes
		{
			get { return (_totalSeconds/60)%60; }
		}

		public int Seconds
		{
			get { return _totalSeconds%60; }
		}

		public static NpgsqlTimeZone CurrentTimeZone
		{
			get { return new NpgsqlTimeZone(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now)); }
		}

		public static NpgsqlTimeZone LocalTimeZone(NpgsqlDate date)
		{
			DateTime dt;
			if (date.Year >= 1902 && date.Year <= 2038)
			{
				dt = (DateTime) date;
			}
			else
			{
				dt = new DateTime(2000, date.Month, date.Day);
			}
			return new NpgsqlTimeZone(TimeZone.CurrentTimeZone.GetUtcOffset(dt));
		}

		public bool Equals(NpgsqlTimeZone other)
		{
			return _totalSeconds == other._totalSeconds;
		}

		public override bool Equals(object obj)
		{
			return obj != null && obj is NpgsqlTimeZone && Equals((NpgsqlTimeZone) obj);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(_totalSeconds < 0 ? "-" : "+").Append(Math.Abs(Hours).ToString("D2"));
			if (Minutes != 0 || Seconds != 0)
			{
				sb.Append(':').Append(Math.Abs(Minutes).ToString("D2"));
				if (Seconds != 0)
				{
					sb.Append(":").Append(Math.Abs(Seconds).ToString("D2"));
				}
			}
			return sb.ToString();
		}

		public static NpgsqlTimeZone Parse(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException();
			}
			try
			{
				str = str.Trim();
				bool neg;
				switch (str[0])
				{
					case '+':
						neg = false;
						break;
					case '-':
						neg = true;
						break;
					default:
						throw new FormatException();
				}
				int hours;
				int minutes;
				int seconds;
				string[] parts = str.Substring(1).Split(':');
				switch (parts.Length) //One of those times that fall-through would actually be good.
				{
					case 1:
						hours = int.Parse(parts[0]);
						minutes = seconds = 0;
						break;
					case 2:
						hours = int.Parse(parts[0]);
						minutes = int.Parse(parts[1]);
						seconds = 0;
						break;
					default:
						hours = int.Parse(parts[0]);
						minutes = int.Parse(parts[1]);
						seconds = int.Parse(parts[2]);
						break;
				}
				int totalSeconds = hours*60*60 + minutes*60 + seconds*(neg ? -1 : 1);
				return new NpgsqlTimeZone(totalSeconds*NpgsqlInterval.TicksPerSecond);
			}
			catch (OverflowException)
			{
				throw;
			}
			catch
			{
				throw new FormatException();
			}
		}

		public static bool TryParse(string str, NpgsqlTimeZone tz)
		{
			try
			{
				tz = Parse(str);
				return true;
			}
			catch
			{
				tz = UTC;
				return false;
			}
		}

		public override int GetHashCode()
		{
			return _totalSeconds;
		}

		//Note, +01:00 is less than -01:00
		public int CompareTo(NpgsqlTimeZone other)
		{
			return -(_totalSeconds.CompareTo(other._totalSeconds));
		}

		public int CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}
			if (obj is NpgsqlTimeZone)
			{
				return CompareTo((NpgsqlTimeZone) obj);
			}
			throw new ArgumentException();
		}

		public static NpgsqlTimeZone operator -(NpgsqlTimeZone tz)
		{
			return new NpgsqlTimeZone(-tz._totalSeconds);
		}

		public static NpgsqlTimeZone operator +(NpgsqlTimeZone tz)
		{
			return tz;
		}

		public static bool operator ==(NpgsqlTimeZone x, NpgsqlTimeZone y)
		{
			return x.Equals(y);
		}

		public static bool operator !=(NpgsqlTimeZone x, NpgsqlTimeZone y)
		{
			return !(x == y);
		}

		public static bool operator <(NpgsqlTimeZone x, NpgsqlTimeZone y)
		{
			return x.CompareTo(y) < 0;
		}

		public static bool operator <=(NpgsqlTimeZone x, NpgsqlTimeZone y)
		{
			return x.CompareTo(y) <= 0;
		}

		public static bool operator >(NpgsqlTimeZone x, NpgsqlTimeZone y)
		{
			return x.CompareTo(y) > 0;
		}

		public static bool operator >=(NpgsqlTimeZone x, NpgsqlTimeZone y)
		{
			return x.CompareTo(y) >= 0;
		}
	}

	[Serializable]
	public struct NpgsqlTime : IEquatable<NpgsqlTime>, IComparable<NpgsqlTime>, IComparable, IComparer<NpgsqlTime>,
	                           IComparer
	{
		public static readonly NpgsqlTime AllBalls = new NpgsqlTime(0);

		public static NpgsqlTime Now
		{
			get { return new NpgsqlTime(DateTime.Now.TimeOfDay); }
		}

		private readonly long _ticks;

		public NpgsqlTime(long ticks)
		{
			if (ticks == NpgsqlInterval.TicksPerDay)
			{
				_ticks = ticks;
			}
			else
			{
				ticks %= NpgsqlInterval.TicksPerDay;
				_ticks = ticks < 0 ? ticks + NpgsqlInterval.TicksPerDay : ticks;
			}
		}

		public NpgsqlTime(TimeSpan time)
			: this(time.Ticks)
		{
		}

		public NpgsqlTime(NpgsqlInterval time)
			: this(time.Ticks)
		{
		}

		public NpgsqlTime(NpgsqlTime copyFrom)
			: this(copyFrom.Ticks)
		{
		}

		public NpgsqlTime(int hours, int minutes, int seconds)
			: this(hours, minutes, seconds, 0)
		{
		}

		public NpgsqlTime(int hours, int minutes, int seconds, int microseconds)
			: this(
				hours*NpgsqlInterval.TicksPerHour + minutes*NpgsqlInterval.TicksPerMinute + seconds*NpgsqlInterval.TicksPerSecond +
				microseconds*NpgsqlInterval.TicksPerMicrosecond)
		{
		}

		public NpgsqlTime(int hours, int minutes, decimal seconds)
			: this(
				hours*NpgsqlInterval.TicksPerHour + minutes*NpgsqlInterval.TicksPerMinute +
				(long) (seconds*NpgsqlInterval.TicksPerSecond))
		{
		}

		public NpgsqlTime(int hours, int minutes, double seconds)
			: this(hours, minutes, (decimal) seconds)
		{
		}

		/// <summary>
		/// The total number of ticks(100ns units) contained. This is the resolution of the
		/// <see cref="NpgsqlTime"/>  type.
		/// <remarks>The resolution of the PostgreSQL
		/// interval type is by default 1&#xb5;s = 1,000 ns. It may be smaller as follows:
		/// <list type="number">
		/// <item>
		/// <term>time(0)</term>
		/// <description>resolution of 1s (1 second)</description>
		/// </item>
		/// <item>
		/// <term>time(1)</term>
		/// <description>resolution of 100ms = 0.1s (100 milliseconds)</description>
		/// </item>
		/// <item>
		/// <term>time(2)</term>
		/// <description>resolution of 10ms = 0.01s (10 milliseconds)</description>
		/// </item>
		/// <item>
		/// <term>time(3)</term>
		/// <description>resolution of 1ms = 0.001s (1 millisecond)</description>
		/// </item>
		/// <item>
		/// <term>time(4)</term>
		/// <description>resolution of 100&#xb5;s = 0.0001s (100 microseconds)</description>
		/// </item>
		/// <item>
		/// <term>time(5)</term>
		/// <description>resolution of 10&#xb5;s = 0.00001s (10 microseconds)</description>
		/// </item>
		/// <item>
		/// <term>time(6) or interval</term>
		/// <description>resolution of 1&#xb5;s = 0.000001s (1 microsecond)</description>
		/// </item>
		/// </list>
		/// <para>As such, if the 100-nanosecond resolution is significant to an application, a PostgreSQL time will
		/// not suffice for those purposes.</para>
		/// <para>In more frequent cases though, the resolution of time suffices.
		/// <see cref="NpgsqlTime"/> will always suffice to handle the resolution of any time value, and upon
		/// writing to the database, will be rounded to the resolution used.</para>
		/// </remarks>
		/// <returns>The number of ticks in the instance.</returns>
		/// </summary>
		public long Ticks
		{
			get { return _ticks; }
		}

		/// <summary>
		/// Gets the number of whole microseconds held in the instance.
		/// <returns>An integer in the range [0, 999999].</returns>
		/// </summary>
		public int Microseconds
		{
			get { return (int) (_ticks/10)%1000000; }
		}

		/// <summary>
		/// Gets the number of whole milliseconds held in the instance.
		/// <returns>An integer in the range [0, 999].</returns>
		/// </summary>
		public int Milliseconds
		{
			get { return (int) ((_ticks/NpgsqlInterval.TicksPerMillsecond)%1000); }
		}

		/// <summary>
		/// Gets the number of whole seconds held in the instance.
		/// <returns>An interger in the range [0, 59].</returns>
		/// </summary>
		public int Seconds
		{
			get { return (int) ((_ticks/NpgsqlInterval.TicksPerSecond)%60); }
		}

		/// <summary>
		/// Gets the number of whole minutes held in the instance.
		/// <returns>An integer in the range [0, 59].</returns>
		/// </summary>
		public int Minutes
		{
			get { return (int) ((_ticks/NpgsqlInterval.TicksPerMinute)%60); }
		}

		/// <summary>
		/// Gets the number of whole hours held in the instance.
		/// <remarks>Note that the time 24:00:00 can be stored for roundtrip compatibility. Any calculations on such a
		/// value will normalised it to 00:00:00.</remarks>
		/// </summary>
		public int Hours
		{
			get { return (int) (_ticks/NpgsqlInterval.TicksPerHour); }
		}

		/// <summary>
		/// Normalise this time; if it is 24:00:00, convert it to 00:00:00
		/// </summary>
		/// <returns>This time, normalised</returns>
		public NpgsqlTime Normalize()
		{
			return new NpgsqlTime(_ticks%NpgsqlInterval.TicksPerDay);
		}

		public bool Equals(NpgsqlTime other)
		{
			return Ticks == other.Ticks;
		}

		public override bool Equals(object obj)
		{
			return obj != null && obj is NpgsqlTime && Equals((NpgsqlTime) obj);
		}

		public override int GetHashCode()
		{
			return Ticks.GetHashCode();
		}

		public override string ToString()
		{
			StringBuilder sb =
				new StringBuilder(Hours.ToString("D2")).Append(':').Append(Minutes.ToString("D2")).Append(':').Append(
					Seconds.ToString("D2"));
			long remainingTicks = Math.Abs(Ticks)%NpgsqlInterval.TicksPerSecond;
			if (remainingTicks != 0)
			{
				while (remainingTicks%10 == 0)
				{
					remainingTicks /= 10;
				}
				sb.Append('.').Append(remainingTicks);
			}
			return sb.ToString();
		}

		public static NpgsqlTime Parse(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException();
			}
			try
			{
				int hours = 0;
				int minutes = 0;
				decimal seconds = 0m;
				string[] parts = str.Split(':');
				switch (parts.Length) //One of those times that fall-through would actually be good.
				{
					case 1:
						hours = int.Parse(parts[0]);
						break;
					case 2:
						hours = int.Parse(parts[0]);
						minutes = int.Parse(parts[1]);
						break;
					default:
						hours = int.Parse(parts[0]);
						minutes = int.Parse(parts[1]);
						seconds = decimal.Parse(parts[2]);
						break;
				}
				if (hours < 0 || hours > 24 || minutes < 0 || minutes > 59 || seconds < 0m || seconds >= 60 ||
				    (hours == 24 && (minutes != 0 || seconds != 0m)))
				{
					throw new OverflowException();
				}
				return new NpgsqlTime(hours, minutes, seconds);
			}
			catch (OverflowException)
			{
				throw;
			}
			catch
			{
				throw new FormatException();
			}
		}

		public static bool TryParse(string str, out NpgsqlTime time)
		{
			try
			{
				time = Parse(str);
				return true;
			}
			catch
			{
				time = AllBalls;
				return false;
			}
		}

		public int CompareTo(NpgsqlTime other)
		{
			return Ticks.CompareTo(other.Ticks);
		}

		public int CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}
			if (obj is NpgsqlTime)
			{
				return CompareTo((NpgsqlTime) obj);
			}
			throw new ArgumentException();
		}

		public int Compare(NpgsqlTime x, NpgsqlTime y)
		{
			return x.CompareTo(y);
		}

		public int Compare(object x, object y)
		{
			if (x == null)
			{
				return y == null ? 0 : -1;
			}
			if (y == null)
			{
				return 1;
			}
			if (!(x is IComparable) || !(y is IComparable))
			{
				throw new ArgumentException();
			}
			return ((IComparable) x).CompareTo(y);
		}

		public static bool operator ==(NpgsqlTime x, NpgsqlTime y)
		{
			return x.Equals(y);
		}

		public static bool operator !=(NpgsqlTime x, NpgsqlTime y)
		{
			return !(x == y);
		}

		public static bool operator <(NpgsqlTime x, NpgsqlTime y)
		{
			return x.Ticks < y.Ticks;
		}

		public static bool operator >(NpgsqlTime x, NpgsqlTime y)
		{
			return x.Ticks > y.Ticks;
		}

		public static bool operator <=(NpgsqlTime x, NpgsqlTime y)
		{
			return x.Ticks <= y.Ticks;
		}

		public static bool operator >=(NpgsqlTime x, NpgsqlTime y)
		{
			return x.Ticks >= y.Ticks;
		}

		public static explicit operator NpgsqlInterval(NpgsqlTime time)
		{
			return new NpgsqlInterval(time.Ticks);
		}

		public static explicit operator NpgsqlTime(NpgsqlInterval interval)
		{
			return new NpgsqlTime(interval);
		}

		public static explicit operator TimeSpan(NpgsqlTime time)
		{
			return new TimeSpan(time.Ticks);
		}

		public static explicit operator NpgsqlTime(TimeSpan interval)
		{
			return new NpgsqlTime(interval);
		}

		public NpgsqlTime AddTicks(long ticksAdded)
		{
			return new NpgsqlTime((Ticks + ticksAdded)%NpgsqlInterval.TicksPerDay);
		}

		private NpgsqlTime AddTicks(long ticksAdded, out int overflow)
		{
			long result = Ticks + ticksAdded;
			overflow = (int) (result/NpgsqlInterval.TicksPerDay);
			result %= NpgsqlInterval.TicksPerDay;
			if (result < 0)
			{
				--overflow; //"carry the one"
			}
			return new NpgsqlTime(result);
		}

		public NpgsqlTime Add(NpgsqlInterval interval)
		{
			return AddTicks(interval.Ticks);
		}

		internal NpgsqlTime Add(NpgsqlInterval interval, out int overflow)
		{
			return AddTicks(interval.Ticks, out overflow);
		}

		public NpgsqlTime Subtract(NpgsqlInterval interval)
		{
			return AddTicks(-interval.Ticks);
		}

		public NpgsqlInterval Subtract(NpgsqlTime earlier)
		{
			return new NpgsqlInterval(Ticks - earlier.Ticks);
		}

		public NpgsqlTimeTZ AtTimeZone(NpgsqlTimeZone timeZone)
		{
			return new NpgsqlTimeTZ(this).AtTimeZone(timeZone);
		}

		public static NpgsqlTime operator +(NpgsqlTime time, NpgsqlInterval interval)
		{
			return time.Add(interval);
		}

		public static NpgsqlTime operator +(NpgsqlInterval interval, NpgsqlTime time)
		{
			return time + interval;
		}

		public static NpgsqlTime operator -(NpgsqlTime time, NpgsqlInterval interval)
		{
			return time.Subtract(interval);
		}

		public static NpgsqlInterval operator -(NpgsqlTime later, NpgsqlTime earlier)
		{
			return later.Subtract(earlier);
		}
	}

	[Serializable]
	public struct NpgsqlTimeTZ : IEquatable<NpgsqlTimeTZ>, IComparable<NpgsqlTimeTZ>, IComparable, IComparer<NpgsqlTimeTZ>,
	                             IComparer
	{
		public static readonly NpgsqlTimeTZ AllBalls = new NpgsqlTimeTZ(NpgsqlTime.AllBalls, NpgsqlTimeZone.UTC);

		public static NpgsqlTimeTZ Now
		{
			get { return new NpgsqlTimeTZ(NpgsqlTime.Now); }
		}

		public static NpgsqlTimeTZ LocalMidnight(NpgsqlDate date)
		{
			return new NpgsqlTimeTZ(NpgsqlTime.AllBalls, NpgsqlTimeZone.LocalTimeZone(date));
		}

		private readonly NpgsqlTime _localTime;
		private readonly NpgsqlTimeZone _timeZone;

		public NpgsqlTimeTZ(NpgsqlTime localTime, NpgsqlTimeZone timeZone)
		{
			_localTime = localTime;
			_timeZone = timeZone;
		}

		public NpgsqlTimeTZ(NpgsqlTime localTime)
			: this(localTime, NpgsqlTimeZone.CurrentTimeZone)
		{
		}

		public NpgsqlTimeTZ(long ticks)
			: this(new NpgsqlTime(ticks))
		{
		}

		public NpgsqlTimeTZ(TimeSpan time)
			: this(new NpgsqlTime(time))
		{
		}

		public NpgsqlTimeTZ(NpgsqlInterval time)
			: this(new NpgsqlTime(time))
		{
		}

		public NpgsqlTimeTZ(NpgsqlTimeTZ copyFrom)
			: this(copyFrom._localTime, copyFrom._timeZone)
		{
		}

		public NpgsqlTimeTZ(int hours, int minutes, int seconds)
			: this(new NpgsqlTime(hours, minutes, seconds))
		{
		}

		public NpgsqlTimeTZ(int hours, int minutes, int seconds, int microseconds)
			: this(new NpgsqlTime(hours, minutes, seconds, microseconds))
		{
		}

		public NpgsqlTimeTZ(int hours, int minutes, decimal seconds)
			: this(new NpgsqlTime(hours, minutes, seconds))
		{
		}

		public NpgsqlTimeTZ(int hours, int minutes, double seconds)
			: this(new NpgsqlTime(hours, minutes, seconds))
		{
		}

		public NpgsqlTimeTZ(long ticks, NpgsqlTimeZone timeZone)
			: this(new NpgsqlTime(ticks), timeZone)
		{
		}

		public NpgsqlTimeTZ(TimeSpan time, NpgsqlTimeZone timeZone)
			: this(new NpgsqlTime(time), timeZone)
		{
		}

		public NpgsqlTimeTZ(NpgsqlInterval time, NpgsqlTimeZone timeZone)
			: this(new NpgsqlTime(time), timeZone)
		{
		}

		public NpgsqlTimeTZ(int hours, int minutes, int seconds, NpgsqlTimeZone timeZone)
			: this(new NpgsqlTime(hours, minutes, seconds), timeZone)
		{
		}

		public NpgsqlTimeTZ(int hours, int minutes, int seconds, int microseconds, NpgsqlTimeZone timeZone)
			: this(new NpgsqlTime(hours, minutes, seconds, microseconds), timeZone)
		{
		}

		public NpgsqlTimeTZ(int hours, int minutes, decimal seconds, NpgsqlTimeZone timeZone)
			: this(new NpgsqlTime(hours, minutes, seconds), timeZone)
		{
		}

		public NpgsqlTimeTZ(int hours, int minutes, double seconds, NpgsqlTimeZone timeZone)
			: this(new NpgsqlTime(hours, minutes, seconds), timeZone)
		{
		}

		public override string ToString()
		{
			return string.Format("{0}{1}", _localTime, _timeZone);
		}

		public static NpgsqlTimeTZ Parse(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException();
			}
			try
			{
				int idx = Math.Max(str.IndexOf('+'), str.IndexOf('-'));
				if (idx == -1)
				{
					throw new FormatException();
				}
				return new NpgsqlTimeTZ(NpgsqlTime.Parse(str.Substring(0, idx)), NpgsqlTimeZone.Parse(str.Substring(idx)));
			}
			catch (OverflowException)
			{
				throw;
			}
			catch
			{
				throw new FormatException();
			}
		}

		public NpgsqlTime LocalTime
		{
			get { return _localTime; }
		}

		public NpgsqlTimeZone TimeZone
		{
			get { return _timeZone; }
		}

		public NpgsqlTime UTCTime
		{
			get { return AtTimeZone(NpgsqlTimeZone.UTC).LocalTime; }
		}

		public NpgsqlTimeTZ AtTimeZone(NpgsqlTimeZone timeZone)
		{
			return new NpgsqlTimeTZ(LocalTime - _timeZone + timeZone, timeZone);
		}

		internal NpgsqlTimeTZ AtTimeZone(NpgsqlTimeZone timeZone, out int overflow)
		{
			return
				new NpgsqlTimeTZ(LocalTime.Add(timeZone - (NpgsqlInterval) (_timeZone), out overflow), timeZone);
		}

		public long Ticks
		{
			get { return _localTime.Ticks; }
		}

		/// <summary>
		/// Gets the number of whole microseconds held in the instance.
		/// <returns>An integer in the range [0, 999999].</returns>
		/// </summary>
		public int Microseconds
		{
			get { return _localTime.Microseconds; }
		}

		/// <summary>
		/// Gets the number of whole milliseconds held in the instance.
		/// <returns>An integer in the range [0, 999].</returns>
		/// </summary>
		public int Milliseconds
		{
			get { return _localTime.Milliseconds; }
		}

		/// <summary>
		/// Gets the number of whole seconds held in the instance.
		/// <returns>An interger in the range [0, 59].</returns>
		/// </summary>
		public int Seconds
		{
			get { return _localTime.Seconds; }
		}

		/// <summary>
		/// Gets the number of whole minutes held in the instance.
		/// <returns>An integer in the range [0, 59].</returns>
		/// </summary>
		public int Minutes
		{
			get { return _localTime.Minutes; }
		}

		/// <summary>
		/// Gets the number of whole hours held in the instance.
		/// <remarks>Note that the time 24:00:00 can be stored for roundtrip compatibility. Any calculations on such a
		/// value will normalised it to 00:00:00.</remarks>
		/// </summary>
		public int Hours
		{
			get { return _localTime.Hours; }
		}

		/// <summary>
		/// Normalise this time; if it is 24:00:00, convert it to 00:00:00
		/// </summary>
		/// <returns>This time, normalised</returns>
		public NpgsqlTimeTZ Normalize()
		{
			return new NpgsqlTimeTZ(_localTime.Normalize(), _timeZone);
		}

		public bool Equals(NpgsqlTimeTZ other)
		{
			return _localTime.Equals(other._localTime) && _timeZone.Equals(other._timeZone);
		}

		public override bool Equals(object obj)
		{
			return obj != null && obj is NpgsqlTimeTZ && Equals((NpgsqlTimeTZ) obj);
		}

		public override int GetHashCode()
		{
			return _localTime.GetHashCode() ^ PGUtil.RotateShift(_timeZone.GetHashCode(), 24);
		}

		/// <summary>
		/// Compares this with another <see cref="NpgsqlTimeTZ"/>. As per postgres' rules,
		/// first the times are compared as if they were both in the same timezone. If they are equal then
		/// then timezones are compared (+01:00 being "smaller" than -01:00).
		/// </summary>
		/// <param name="other">the <see cref="NpgsqlTimeTZ"/> to compare with.</param>
		/// <returns>An integer which is 0 if they are equal, &lt; 0 if this is the smaller and &gt; 0 if this is the larger.</returns>
		public int CompareTo(NpgsqlTimeTZ other)
		{
			int cmp = AtTimeZone(NpgsqlTimeZone.UTC).LocalTime.CompareTo(other.AtTimeZone(NpgsqlTimeZone.UTC).LocalTime);
			return cmp == 0 ? _timeZone.CompareTo(other._timeZone) : cmp;
		}

		public int CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}
			if (obj is NpgsqlTimeTZ)
			{
				return CompareTo((NpgsqlTimeTZ) obj);
			}
			throw new ArgumentException();
		}

		public int Compare(NpgsqlTimeTZ x, NpgsqlTimeTZ y)
		{
			return x.CompareTo(y);
		}

		public int Compare(object x, object y)
		{
			if (x == null)
			{
				return y == null ? 0 : -1;
			}
			if (y == null)
			{
				return 1;
			}
			if (!(x is IComparable) || !(y is IComparable))
			{
				throw new ArgumentException();
			}
			return ((IComparable) x).CompareTo(y);
		}

		public static bool operator ==(NpgsqlTimeTZ x, NpgsqlTimeTZ y)
		{
			return x.Equals(y);
		}

		public static bool operator !=(NpgsqlTimeTZ x, NpgsqlTimeTZ y)
		{
			return !(x == y);
		}

		public static bool operator <(NpgsqlTimeTZ x, NpgsqlTimeTZ y)
		{
			return x.CompareTo(y) < 0;
		}

		public static bool operator >(NpgsqlTimeTZ x, NpgsqlTimeTZ y)
		{
			return x.CompareTo(y) > 0;
		}

		public static bool operator <=(NpgsqlTimeTZ x, NpgsqlTimeTZ y)
		{
			return x.CompareTo(y) <= 0;
		}

		public static bool operator >=(NpgsqlTimeTZ x, NpgsqlTimeTZ y)
		{
			return x.CompareTo(y) >= 0;
		}

		public NpgsqlTimeTZ Add(NpgsqlInterval interval)
		{
			return new NpgsqlTimeTZ(_localTime.Add(interval), _timeZone);
		}

		internal NpgsqlTimeTZ Add(NpgsqlInterval interval, out int overflow)
		{
			return new NpgsqlTimeTZ(_localTime.Add(interval, out overflow), _timeZone);
		}

		public NpgsqlTimeTZ Subtract(NpgsqlInterval interval)
		{
			return new NpgsqlTimeTZ(_localTime.Subtract(interval), _timeZone);
		}

		public NpgsqlInterval Subtract(NpgsqlTimeTZ earlier)
		{
			return _localTime.Subtract(earlier.AtTimeZone(_timeZone)._localTime);
		}

		public static NpgsqlTimeTZ operator +(NpgsqlTimeTZ time, NpgsqlInterval interval)
		{
			return time.Add(interval);
		}

		public static NpgsqlTimeTZ operator +(NpgsqlInterval interval, NpgsqlTimeTZ time)
		{
			return time + interval;
		}

		public static NpgsqlTimeTZ operator -(NpgsqlTimeTZ time, NpgsqlInterval interval)
		{
			return time.Subtract(interval);
		}

		public static NpgsqlInterval operator -(NpgsqlTimeTZ later, NpgsqlTimeTZ earlier)
		{
			return later.Subtract(earlier);
		}

		public static explicit operator NpgsqlTimeTZ(TimeSpan time)
		{
			return new NpgsqlTimeTZ(new NpgsqlTime(time));
		}

		public static explicit operator TimeSpan(NpgsqlTimeTZ time)
		{
			return (TimeSpan) time.LocalTime;
		}
	}

	[Serializable]
	public struct NpgsqlTimeStamp : IEquatable<NpgsqlTimeStamp>, IComparable<NpgsqlTimeStamp>, IComparable,
	                                IComparer<NpgsqlTimeStamp>, IComparer
	{
		private enum TimeType
		{
			Finite,
			Infinity,
			MinusInfinity
		}

		public static readonly NpgsqlTimeStamp Epoch = new NpgsqlTimeStamp(NpgsqlDate.Epoch);
		public static readonly NpgsqlTimeStamp Era = new NpgsqlTimeStamp(NpgsqlDate.Era);

		public static readonly NpgsqlTimeStamp Infinity =
			new NpgsqlTimeStamp(TimeType.Infinity, NpgsqlDate.Era, NpgsqlTime.AllBalls);

		public static readonly NpgsqlTimeStamp MinusInfinity =
			new NpgsqlTimeStamp(TimeType.MinusInfinity, NpgsqlDate.Era, NpgsqlTime.AllBalls);

		public static NpgsqlTimeStamp Now
		{
			get { return new NpgsqlTimeStamp(NpgsqlDate.Now, NpgsqlTime.Now); }
		}

		public static NpgsqlTimeStamp Today
		{
			get { return new NpgsqlTimeStamp(NpgsqlDate.Now); }
		}

		public static NpgsqlTimeStamp Yesterday
		{
			get { return new NpgsqlTimeStamp(NpgsqlDate.Yesterday); }
		}

		public static NpgsqlTimeStamp Tomorrow
		{
			get { return new NpgsqlTimeStamp(NpgsqlDate.Tomorrow); }
		}

		private readonly NpgsqlDate _date;
		private readonly NpgsqlTime _time;
		private readonly TimeType _type;

		private NpgsqlTimeStamp(TimeType type, NpgsqlDate date, NpgsqlTime time)
		{
			_type = type;
			_date = date;
			_time = time;
		}

		public NpgsqlTimeStamp(NpgsqlDate date, NpgsqlTime time)
			: this(TimeType.Finite, date, time)
		{
		}

		public NpgsqlTimeStamp(NpgsqlDate date)
			: this(date, NpgsqlTime.AllBalls)
		{
		}

		public NpgsqlTimeStamp(int year, int month, int day, int hours, int minutes, int seconds)
			: this(new NpgsqlDate(year, month, day), new NpgsqlTime(hours, minutes, seconds))
		{
		}

		public NpgsqlDate Date
		{
			get { return _date; }
		}

		public NpgsqlTime Time
		{
			get { return _time; }
		}

		public int DayOfYear
		{
			get { return _date.DayOfYear; }
		}

		public int Year
		{
			get { return _date.Year; }
		}

		public int Month
		{
			get { return _date.Month; }
		}

		public int Day
		{
			get { return _date.Day; }
		}

		public DayOfWeek DayOfWeek
		{
			get { return _date.DayOfWeek; }
		}

		public bool IsLeapYear
		{
			get { return _date.IsLeapYear; }
		}

		public NpgsqlTimeStamp AddDays(int days)
		{
			switch (_type)
			{
				case TimeType.Infinity:
				case TimeType.MinusInfinity:
					return this;
				default:
					return new NpgsqlTimeStamp(_date.AddDays(days), _time);
			}
		}

		public NpgsqlTimeStamp AddYears(int years)
		{
			switch (_type)
			{
				case TimeType.Infinity:
				case TimeType.MinusInfinity:
					return this;
				default:
					return new NpgsqlTimeStamp(_date.AddYears(years), _time);
			}
		}

		public NpgsqlTimeStamp AddMonths(int months)
		{
			switch (_type)
			{
				case TimeType.Infinity:
				case TimeType.MinusInfinity:
					return this;
				default:
					return new NpgsqlTimeStamp(_date.AddMonths(months), _time);
			}
		}

		public long Ticks
		{
			get { return _date.DaysSinceEra*NpgsqlInterval.TicksPerDay + _time.Ticks; }
		}

		public int Microseconds
		{
			get { return _time.Microseconds; }
		}

		public int Milliseconds
		{
			get { return _time.Milliseconds; }
		}

		public int Seconds
		{
			get { return _time.Seconds; }
		}

		public int Minutes
		{
			get { return _time.Minutes; }
		}

		public int Hours
		{
			get { return _time.Hours; }
		}

		public bool IsFinite
		{
			get { return _type == TimeType.Finite; }
		}

		public bool IsInfinity
		{
			get { return _type == TimeType.Infinity; }
		}

		public bool IsMinusInfinity
		{
			get { return _type == TimeType.MinusInfinity; }
		}

		public NpgsqlTimeStamp Normalize()
		{
			return Add(NpgsqlInterval.Zero);
		}

		public override string ToString()
		{
			switch (_type)
			{
				case TimeType.Infinity:
					return "infinity";
				case TimeType.MinusInfinity:
					return "-infinity";
				default:
					return string.Format("{0} {1}", _date, _time);
			}
		}

		public static NpgsqlTimeStamp Parse(string str)
		{
			if (str == null)
			{
				throw new NullReferenceException();
			}
			switch (str = str.Trim().ToLowerInvariant())
			{
				case "infinity":
					return Infinity;
				case "-infinity":
					return MinusInfinity;
				default:
					try
					{
						int idxSpace = str.IndexOf(' ');
						string datePart = str.Substring(0, idxSpace);
						if (str.Contains("bc"))
						{
							datePart += " BC";
						}
						int idxSecond = str.IndexOf(' ', idxSpace + 1);
						if (idxSecond == -1)
						{
							idxSecond = str.Length;
						}
						string timePart = str.Substring(idxSpace + 1, idxSecond - idxSpace - 1);
						return new NpgsqlTimeStamp(NpgsqlDate.Parse(datePart), NpgsqlTime.Parse(timePart));
					}
					catch (OverflowException)
					{
						throw;
					}
					catch
					{
						throw new FormatException();
					}
			}
		}

		public bool Equals(NpgsqlTimeStamp other)
		{
			switch (_type)
			{
				case TimeType.Infinity:
					return other._type == TimeType.Infinity;
				case TimeType.MinusInfinity:
					return other._type == TimeType.MinusInfinity;
				default:
					return other._type == TimeType.Finite && _date.Equals(other._date) && _time.Equals(other._time);
			}
		}

		public override bool Equals(object obj)
		{
			return obj != null && obj is NpgsqlTimeStamp && Equals((NpgsqlTimeStamp) obj);
		}

		public override int GetHashCode()
		{
			switch (_type)
			{
				case TimeType.Infinity:
					return int.MaxValue;
				case TimeType.MinusInfinity:
					return int.MinValue;
				default:
					return _date.GetHashCode() ^ PGUtil.RotateShift(_time.GetHashCode(), 16);
			}
		}

		public int CompareTo(NpgsqlTimeStamp other)
		{
			switch (_type)
			{
				case TimeType.Infinity:
					return other._type == TimeType.Infinity ? 0 : 1;
				case TimeType.MinusInfinity:
					return other._type == TimeType.MinusInfinity ? 0 : -1;
				default:
					switch (other._type)
					{
						case TimeType.Infinity:
							return -1;
						case TimeType.MinusInfinity:
							return 1;
						default:
							int cmp = _date.CompareTo(other._date);
							return cmp == 0 ? _time.CompareTo(_time) : cmp;
					}
			}
		}

		public int CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}
			if (obj is NpgsqlTimeStamp)
			{
				return CompareTo((NpgsqlTimeStamp) obj);
			}
			throw new ArgumentException();
		}

		public int Compare(NpgsqlTimeStamp x, NpgsqlTimeStamp y)
		{
			return x.CompareTo(y);
		}

		public int Compare(object x, object y)
		{
			if (x == null)
			{
				return y == null ? 0 : -1;
			}
			if (y == null)
			{
				return 1;
			}
			if (!(x is IComparable) || !(y is IComparable))
			{
				throw new ArgumentException();
			}
			return ((IComparable) x).CompareTo(y);
		}

		public NpgsqlTimeStampTZ AtTimeZone(NpgsqlTimeZone timeZoneFrom, NpgsqlTimeZone timeZoneTo)
		{
			int overflow;
			NpgsqlTimeTZ adjusted = new NpgsqlTimeTZ(_time, timeZoneFrom).AtTimeZone(timeZoneTo, out overflow);
			return new NpgsqlTimeStampTZ(_date.AddDays(overflow), adjusted);
		}

		public NpgsqlTimeStampTZ AtTimeZone(NpgsqlTimeZone timeZone)
		{
			return AtTimeZone(timeZone, NpgsqlTimeZone.LocalTimeZone(_date));
		}

		public NpgsqlTimeStamp Add(NpgsqlInterval interval)
		{
			switch (_type)
			{
				case TimeType.Infinity:
				case TimeType.MinusInfinity:
					return this;
				default:
					int overflow;
					NpgsqlTime time = _time.Add(interval, out overflow);
					return new NpgsqlTimeStamp(_date.Add(interval, overflow), time);
			}
		}

		public NpgsqlTimeStamp Subtract(NpgsqlInterval interval)
		{
			return Add(-interval);
		}

		public NpgsqlInterval Subtract(NpgsqlTimeStamp timestamp)
		{
			switch (_type)
			{
				case TimeType.Infinity:
				case TimeType.MinusInfinity:
					throw new ArgumentOutOfRangeException("You cannot subtract infinity timestamps");
			}
			switch (timestamp._type)
			{
				case TimeType.Infinity:
				case TimeType.MinusInfinity:
					throw new ArgumentOutOfRangeException("You cannot subtract infinity timestamps");
			}
			return new NpgsqlInterval(0, _date.DaysSinceEra - timestamp._date.DaysSinceEra, _time.Ticks - timestamp._time.Ticks);
		}

		public static implicit operator NpgsqlTimeStamp(DateTime datetime)
		{
			if (datetime == DateTime.MaxValue)
			{
				return Infinity;
			}
			else if (datetime == DateTime.MinValue)
			{
				return MinusInfinity;
			}
			else
			{
				return new NpgsqlTimeStamp(new NpgsqlDate(datetime), new NpgsqlTime(datetime.TimeOfDay));
			}
		}

		public static implicit operator DateTime(NpgsqlTimeStamp timestamp)
		{
			switch (timestamp._type)
			{
				case TimeType.Infinity:
					return DateTime.MaxValue;
				case TimeType.MinusInfinity:
					return DateTime.MinValue;
				default:
					try
					{
						return
							new DateTime(timestamp.Date.DaysSinceEra*NpgsqlInterval.TicksPerDay + timestamp._time.Ticks,
							             DateTimeKind.Unspecified);
					}
					catch
					{
						throw new InvalidCastException();
					}
			}
		}

		public static NpgsqlTimeStamp operator +(NpgsqlTimeStamp timestamp, NpgsqlInterval interval)
		{
			return timestamp.Add(interval);
		}

		public static NpgsqlTimeStamp operator +(NpgsqlInterval interval, NpgsqlTimeStamp timestamp)
		{
			return timestamp.Add(interval);
		}

		public static NpgsqlTimeStamp operator -(NpgsqlTimeStamp timestamp, NpgsqlInterval interval)
		{
			return timestamp.Subtract(interval);
		}

		public static NpgsqlInterval operator -(NpgsqlTimeStamp x, NpgsqlTimeStamp y)
		{
			return x.Subtract(y);
		}

		public static bool operator ==(NpgsqlTimeStamp x, NpgsqlTimeStamp y)
		{
			return x.Equals(y);
		}

		public static bool operator !=(NpgsqlTimeStamp x, NpgsqlTimeStamp y)
		{
			return !(x == y);
		}

		public static bool operator <(NpgsqlTimeStamp x, NpgsqlTimeStamp y)
		{
			return x.CompareTo(y) < 0;
		}

		public static bool operator >(NpgsqlTimeStamp x, NpgsqlTimeStamp y)
		{
			return x.CompareTo(y) > 0;
		}

		public static bool operator <=(NpgsqlTimeStamp x, NpgsqlTimeStamp y)
		{
			return x.CompareTo(y) <= 0;
		}

		public static bool operator >=(NpgsqlTimeStamp x, NpgsqlTimeStamp y)
		{
			return x.CompareTo(y) >= 0;
		}
	}

	[Serializable]
	public struct NpgsqlTimeStampTZ : IEquatable<NpgsqlTimeStampTZ>, IComparable<NpgsqlTimeStampTZ>, IComparable,
	                                  IComparer<NpgsqlTimeStampTZ>, IComparer
	{
		private enum TimeType
		{
			Finite,
			Infinity,
			MinusInfinity
		}

		public static readonly NpgsqlTimeStampTZ Epoch = new NpgsqlTimeStampTZ(NpgsqlDate.Epoch, NpgsqlTimeTZ.AllBalls);
		public static readonly NpgsqlTimeStampTZ Era = new NpgsqlTimeStampTZ(NpgsqlDate.Era, NpgsqlTimeTZ.AllBalls);

		public static readonly NpgsqlTimeStampTZ Infinity =
			new NpgsqlTimeStampTZ(TimeType.Infinity, NpgsqlDate.Era, NpgsqlTimeTZ.AllBalls);

		public static readonly NpgsqlTimeStampTZ MinusInfinity =
			new NpgsqlTimeStampTZ(TimeType.MinusInfinity, NpgsqlDate.Era, NpgsqlTimeTZ.AllBalls);

		public static NpgsqlTimeStampTZ Now
		{
			get { return new NpgsqlTimeStampTZ(NpgsqlDate.Now, NpgsqlTimeTZ.Now); }
		}

		public static NpgsqlTimeStampTZ Today
		{
			get { return new NpgsqlTimeStampTZ(NpgsqlDate.Now); }
		}

		public static NpgsqlTimeStampTZ Yesterday
		{
			get { return new NpgsqlTimeStampTZ(NpgsqlDate.Yesterday); }
		}

		public static NpgsqlTimeStampTZ Tomorrow
		{
			get { return new NpgsqlTimeStampTZ(NpgsqlDate.Tomorrow); }
		}

		private readonly NpgsqlDate _date;
		private readonly NpgsqlTimeTZ _time;
		private readonly TimeType _type;

		private NpgsqlTimeStampTZ(TimeType type, NpgsqlDate date, NpgsqlTimeTZ time)
		{
			_type = type;
			_date = date;
			_time = time;
		}

		public NpgsqlTimeStampTZ(NpgsqlDate date, NpgsqlTimeTZ time)
			: this(TimeType.Finite, date, time)
		{
		}

		public NpgsqlTimeStampTZ(NpgsqlDate date)
			: this(date, NpgsqlTimeTZ.LocalMidnight(date))
		{
		}

		public NpgsqlTimeStampTZ(int year, int month, int day, int hours, int minutes, int seconds, NpgsqlTimeZone? timezone)
			: this(
				new NpgsqlDate(year, month, day),
				new NpgsqlTimeTZ(hours, minutes, seconds,
				                 timezone.HasValue ? timezone.Value : NpgsqlTimeZone.LocalTimeZone(new NpgsqlDate(year, month, day)))
				)
		{
		}

		public NpgsqlDate Date
		{
			get { return _date; }
		}

		public NpgsqlTimeTZ Time
		{
			get { return _time; }
		}

		public int DayOfYear
		{
			get { return _date.DayOfYear; }
		}

		public int Year
		{
			get { return _date.Year; }
		}

		public int Month
		{
			get { return _date.Month; }
		}

		public int Day
		{
			get { return _date.Day; }
		}

		public DayOfWeek DayOfWeek
		{
			get { return _date.DayOfWeek; }
		}

		public bool IsLeapYear
		{
			get { return _date.IsLeapYear; }
		}

		public NpgsqlTimeStampTZ AddDays(int days)
		{
			switch (_type)
			{
				case TimeType.Infinity:
				case TimeType.MinusInfinity:
					return this;
				default:
					return new NpgsqlTimeStampTZ(_date.AddDays(days), _time);
			}
		}

		public NpgsqlTimeStampTZ AddYears(int years)
		{
			switch (_type)
			{
				case TimeType.Infinity:
				case TimeType.MinusInfinity:
					return this;
				default:
					return new NpgsqlTimeStampTZ(_date.AddYears(years), _time);
			}
		}

		public NpgsqlTimeStampTZ AddMonths(int months)
		{
			switch (_type)
			{
				case TimeType.Infinity:
				case TimeType.MinusInfinity:
					return this;
				default:
					return new NpgsqlTimeStampTZ(_date.AddMonths(months), _time);
			}
		}

		public NpgsqlTime LocalTime
		{
			get { return _time.LocalTime; }
		}

		public NpgsqlTimeZone TimeZone
		{
			get { return _time.TimeZone; }
		}

		public NpgsqlTime UTCTime
		{
			get { return _time.UTCTime; }
		}

		public long Ticks
		{
			get { return _date.DaysSinceEra*NpgsqlInterval.TicksPerDay + _time.Ticks; }
		}

		public int Microseconds
		{
			get { return _time.Microseconds; }
		}

		public int Milliseconds
		{
			get { return _time.Milliseconds; }
		}

		public int Seconds
		{
			get { return _time.Seconds; }
		}

		public int Minutes
		{
			get { return _time.Minutes; }
		}

		public int Hours
		{
			get { return _time.Hours; }
		}

		public bool IsFinite
		{
			get { return _type == TimeType.Finite; }
		}

		public bool IsInfinity
		{
			get { return _type == TimeType.Infinity; }
		}

		public bool IsMinusInfinity
		{
			get { return _type == TimeType.MinusInfinity; }
		}

		public NpgsqlTimeStampTZ Normalize()
		{
			return Add(NpgsqlInterval.Zero);
		}

		public override string ToString()
		{
			switch (_type)
			{
				case TimeType.Infinity:
					return "infinity";
				case TimeType.MinusInfinity:
					return "-infinity";
				default:
					return string.Format("{0} {1}", _date, _time);
			}
		}

		public static NpgsqlTimeStampTZ Parse(string str)
		{
			if (str == null)
			{
				throw new NullReferenceException();
			}
			switch (str = str.Trim().ToLowerInvariant())
			{
				case "infinity":
					return Infinity;
				case "-infinity":
					return MinusInfinity;
				default:
					try
					{
						int idxSpace = str.IndexOf(' ');
						string datePart = str.Substring(0, idxSpace);
						if (str.Contains("bc"))
						{
							datePart += " BC";
						}
						int idxSecond = str.IndexOf(' ', idxSpace + 1);
						if (idxSecond == -1)
						{
							idxSecond = str.Length;
						}
						string timePart = str.Substring(idxSpace + 1, idxSecond - idxSpace - 1);
						return new NpgsqlTimeStampTZ(NpgsqlDate.Parse(datePart), NpgsqlTimeTZ.Parse(timePart));
					}
					catch (OverflowException)
					{
						throw;
					}
					catch
					{
						throw new FormatException();
					}
			}
		}

		public bool Equals(NpgsqlTimeStampTZ other)
		{
			switch (_type)
			{
				case TimeType.Infinity:
					return other._type == TimeType.Infinity;
				case TimeType.MinusInfinity:
					return other._type == TimeType.MinusInfinity;
				default:
					return other._type == TimeType.Finite && _date.Equals(other._date) && _time.Equals(other._time);
			}
		}

		public override bool Equals(object obj)
		{
			return obj != null && obj is NpgsqlTimeStamp && Equals((NpgsqlTimeStampTZ) obj);
		}

		public override int GetHashCode()
		{
			switch (_type)
			{
				case TimeType.Infinity:
					return int.MaxValue;
				case TimeType.MinusInfinity:
					return int.MinValue;
				default:
					return _date.GetHashCode() ^ PGUtil.RotateShift(_time.GetHashCode(), 16);
			}
		}

		public int CompareTo(NpgsqlTimeStampTZ other)
		{
			switch (_type)
			{
				case TimeType.Infinity:
					return other._type == TimeType.Infinity ? 0 : 1;
				case TimeType.MinusInfinity:
					return other._type == TimeType.MinusInfinity ? 0 : -1;
				default:
					switch (other._type)
					{
						case TimeType.Infinity:
							return -1;
						case TimeType.MinusInfinity:
							return 1;
						default:
							int cmp = _date.CompareTo(other._date);
							return cmp == 0 ? _time.CompareTo(_time) : cmp;
					}
			}
		}

		public int CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}
			if (obj is NpgsqlTimeStamp)
			{
				return CompareTo((NpgsqlTimeStamp) obj);
			}
			throw new ArgumentException();
		}

		public int Compare(NpgsqlTimeStampTZ x, NpgsqlTimeStampTZ y)
		{
			return x.CompareTo(y);
		}

		public int Compare(object x, object y)
		{
			if (x == null)
			{
				return y == null ? 0 : -1;
			}
			if (y == null)
			{
				return 1;
			}
			if (!(x is IComparable) || !(y is IComparable))
			{
				throw new ArgumentException();
			}
			return ((IComparable) x).CompareTo(y);
		}

		public NpgsqlTimeStamp AtTimeZone(NpgsqlTimeZone timeZone)
		{
			int overflow;
			NpgsqlTimeTZ adjusted = _time.AtTimeZone(timeZone, out overflow);
			return new NpgsqlTimeStamp(_date.AddDays(overflow), adjusted.LocalTime);
		}

		public NpgsqlTimeStampTZ Add(NpgsqlInterval interval)
		{
			switch (_type)
			{
				case TimeType.Infinity:
				case TimeType.MinusInfinity:
					return this;
				default:
					int overflow;
					NpgsqlTimeTZ time = _time.Add(interval, out overflow);
					return new NpgsqlTimeStampTZ(_date.Add(interval, overflow), time);
			}
		}

		public NpgsqlTimeStampTZ Subtract(NpgsqlInterval interval)
		{
			return Add(-interval);
		}

		public NpgsqlInterval Subtract(NpgsqlTimeStampTZ timestamp)
		{
			switch (_type)
			{
				case TimeType.Infinity:
				case TimeType.MinusInfinity:
					throw new ArgumentOutOfRangeException("You cannot subtract infinity timestamps");
			}
			switch (timestamp._type)
			{
				case TimeType.Infinity:
				case TimeType.MinusInfinity:
					throw new ArgumentOutOfRangeException("You cannot subtract infinity timestamps");
			}
			return new NpgsqlInterval(0, _date.DaysSinceEra - timestamp._date.DaysSinceEra, (_time - timestamp._time).Ticks);
		}

		public static implicit operator NpgsqlTimeStampTZ(DateTime datetime)
		{
			if (datetime == DateTime.MaxValue)
			{
				return Infinity;
			}
			else if (datetime == DateTime.MinValue)
			{
				return MinusInfinity;
			}
			else
			{
				NpgsqlDate newDate = new NpgsqlDate(datetime);
				return
					new NpgsqlTimeStampTZ(newDate,
					                      new NpgsqlTimeTZ(datetime.TimeOfDay,
					                                       datetime.Kind == DateTimeKind.Utc
					                                       	? NpgsqlTimeZone.UTC
					                                       	: NpgsqlTimeZone.LocalTimeZone(newDate)));
			}
		}

		public static explicit operator DateTime(NpgsqlTimeStampTZ timestamp)
		{
			switch (timestamp._type)
			{
				case TimeType.Infinity:
					return DateTime.MaxValue;
				case TimeType.MinusInfinity:
					return DateTime.MinValue;
				default:
					try
					{
						NpgsqlTimeStamp utc = timestamp.AtTimeZone(NpgsqlTimeZone.UTC);
						return new DateTime(utc.Date.DaysSinceEra*NpgsqlInterval.TicksPerDay + utc.Time.Ticks, DateTimeKind.Utc);
					}
					catch
					{
						throw new InvalidCastException();
					}
			}
		}

		public static NpgsqlTimeStampTZ operator +(NpgsqlTimeStampTZ timestamp, NpgsqlInterval interval)
		{
			return timestamp.Add(interval);
		}

		public static NpgsqlTimeStampTZ operator +(NpgsqlInterval interval, NpgsqlTimeStampTZ timestamp)
		{
			return timestamp.Add(interval);
		}

		public static NpgsqlTimeStampTZ operator -(NpgsqlTimeStampTZ timestamp, NpgsqlInterval interval)
		{
			return timestamp.Subtract(interval);
		}

		public static NpgsqlInterval operator -(NpgsqlTimeStampTZ x, NpgsqlTimeStampTZ y)
		{
			return x.Subtract(y);
		}

		public static bool operator ==(NpgsqlTimeStampTZ x, NpgsqlTimeStampTZ y)
		{
			return x.Equals(y);
		}

		public static bool operator !=(NpgsqlTimeStampTZ x, NpgsqlTimeStampTZ y)
		{
			return !(x == y);
		}

		public static bool operator <(NpgsqlTimeStampTZ x, NpgsqlTimeStampTZ y)
		{
			return x.CompareTo(y) < 0;
		}

		public static bool operator >(NpgsqlTimeStampTZ x, NpgsqlTimeStampTZ y)
		{
			return x.CompareTo(y) > 0;
		}

		public static bool operator <=(NpgsqlTimeStampTZ x, NpgsqlTimeStampTZ y)
		{
			return x.CompareTo(y) <= 0;
		}

		public static bool operator >=(NpgsqlTimeStampTZ x, NpgsqlTimeStampTZ y)
		{
			return x.CompareTo(y) >= 0;
		}
	}
}