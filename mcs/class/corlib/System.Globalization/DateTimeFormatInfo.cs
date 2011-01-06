// System.Globalization.DateTimeFormatInfo
//
// Some useful functions are missing in the ECMA specs.
// They have been added following MS SDK Beta2
//
// Martin Weindel (martin.weindel@t-online.de)
//
// (C) Martin Weindel (martin.weindel@t-online.de)

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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Globalization
{
	[Flags]
	enum DateTimeFormatFlags {
		Unused,
		But,
		Serialized,
		By,
		Microsoft
	}

	[Serializable]
	[ComVisible (true)]
	public sealed class DateTimeFormatInfo : ICloneable, IFormatProvider {
		private static readonly string MSG_READONLY = "This instance is read only";
		private static readonly string MSG_ARRAYSIZE_MONTH = "An array with exactly 13 elements is needed";
		private static readonly string MSG_ARRAYSIZE_DAY = "An array with exactly 7 elements is needed";
		private static readonly string[] INVARIANT_ABBREVIATED_DAY_NAMES
			= new string[7] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"};
		private static readonly string[] INVARIANT_DAY_NAMES
			= new string[7] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"};
		private static readonly string[] INVARIANT_ABBREVIATED_MONTH_NAMES
			= new string[13] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", ""};
		private static readonly string[] INVARIANT_MONTH_NAMES
			= new string[13] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December", ""};
//		private static readonly string[] INVARIANT_ERA_NAMES = {"A.D."};
		static readonly string [] INVARIANT_SHORT_DAY_NAMES =
			new string [7] {"Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"};
		private static DateTimeFormatInfo theInvariantDateTimeFormatInfo;

		private const string _RoundtripPattern = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK";

#pragma warning disable 169
		#region Sync with object-internals.h
		private bool m_isReadOnly;
		private string amDesignator;
		private string pmDesignator;
		private string dateSeparator;
		private string timeSeparator;
		private string shortDatePattern;
		private string longDatePattern;
		private string shortTimePattern;
		private string longTimePattern;
		private string monthDayPattern;
		private string yearMonthPattern;
		private string fullDateTimePattern;
		private string _RFC1123Pattern;
		private string _SortableDateTimePattern;
		private string _UniversalSortableDateTimePattern;
		private int firstDayOfWeek;
		private Calendar calendar;
		private int calendarWeekRule;
		private string[] abbreviatedDayNames;
		private string[] dayNames;
		private string[] monthNames;
		private string[] abbreviatedMonthNames;

		// FIXME: not supported other than invariant
		private string [] allShortDatePatterns;
		private string [] allLongDatePatterns;
		private string [] allShortTimePatterns;
		private string [] allLongTimePatterns;
		private string [] monthDayPatterns;
		private string [] yearMonthPatterns;
		private string [] shortDayNames;
		#endregion

		// MS Serialization needs this
		private int nDataItem;
		private bool m_useUserOverride;
		private bool m_isDefaultCalendar;
		private int CultureID;
		private bool bUseCalendarInfo;
		private string generalShortTimePattern;
		private string generalLongTimePattern;
		private string[] m_eraNames;
		private string[] m_abbrevEraNames;
		private string[] m_abbrevEnglishEraNames;
		private string[] m_dateWords;
		private int[] optionalCalendars;
		private string[] m_superShortDayNames;
		private string[] genitiveMonthNames;
		private string[] m_genitiveAbbreviatedMonthNames;
		private string[] leapYearMonthNames;
		private DateTimeFormatFlags formatFlags;
		private string m_name; // Unused, but MS.NET serializes this
#pragma warning restore 169

		internal DateTimeFormatInfo(bool read_only)
		{
			m_isReadOnly = read_only;
			amDesignator = "AM";
			pmDesignator = "PM";
			dateSeparator = "/";
			timeSeparator = ":";
			shortDatePattern = "MM/dd/yyyy";
			longDatePattern = "dddd, dd MMMM yyyy";
			shortTimePattern = "HH:mm";
			longTimePattern = "HH:mm:ss";
			monthDayPattern = "MMMM dd";
			yearMonthPattern = "yyyy MMMM";
			fullDateTimePattern = "dddd, dd MMMM yyyy HH:mm:ss";

			// FIXME: for the following three pattern: "The
			// default value of this property is derived
			// from the calendar that is set for
			// CultureInfo.CurrentCulture or the default
			// calendar of CultureInfo.CurrentCulture."

			// Actually, no predefined culture has different values
			// than those default values.

			_RFC1123Pattern = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'"; 
			_SortableDateTimePattern = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";
			_UniversalSortableDateTimePattern = "yyyy'-'MM'-'dd HH':'mm':'ss'Z'";

			firstDayOfWeek = (int)DayOfWeek.Sunday;
			calendar = new GregorianCalendar();
			calendarWeekRule = (int)CalendarWeekRule.FirstDay;

			abbreviatedDayNames = INVARIANT_ABBREVIATED_DAY_NAMES;
			dayNames = INVARIANT_DAY_NAMES;
			abbreviatedMonthNames = INVARIANT_ABBREVIATED_MONTH_NAMES;
			monthNames = INVARIANT_MONTH_NAMES;
			m_genitiveAbbreviatedMonthNames = INVARIANT_ABBREVIATED_MONTH_NAMES;
			genitiveMonthNames = INVARIANT_MONTH_NAMES;
			shortDayNames = INVARIANT_SHORT_DAY_NAMES;
		}

		public DateTimeFormatInfo() : this (false)
		{
		}
				
		public static DateTimeFormatInfo GetInstance(IFormatProvider provider)
		{
			if (provider != null) {
				DateTimeFormatInfo dtfi;
				dtfi = (DateTimeFormatInfo)provider.GetFormat(typeof(DateTimeFormatInfo));
				if (dtfi != null)
					return dtfi;
			}
			
			return CurrentInfo;
		}

		public bool IsReadOnly {
			get {
				return m_isReadOnly;
			}
		}

		public static DateTimeFormatInfo ReadOnly(DateTimeFormatInfo dtfi)
		{
			DateTimeFormatInfo copy = (DateTimeFormatInfo)dtfi.Clone();
			copy.m_isReadOnly = true;
			return copy;
		}			

		public object Clone () 
		{
			DateTimeFormatInfo clone = (DateTimeFormatInfo) MemberwiseClone();
			// clone is not read only
			clone.m_isReadOnly = false;
			return clone;
		}

		public object GetFormat(Type formatType)
		{
			return (formatType == GetType()) ? this : null;
		}

		public string GetAbbreviatedEraName (int era)
		{
			if (era < 0 || era >= calendar.AbbreviatedEraNames.Length)
				throw new ArgumentOutOfRangeException ("era", era.ToString ());
			return calendar.AbbreviatedEraNames [era];
		}

		public string GetAbbreviatedMonthName(int month)
		{
			if (month < 1 || month > 13) throw new ArgumentOutOfRangeException();
			return abbreviatedMonthNames[month-1];
		}

		public int GetEra (string eraName)
		{
			if (eraName == null)
				throw new ArgumentNullException ();
			string [] eras = calendar.EraNames;
			for (int i = 0; i < eras.Length; i++)
				if (CultureInfo.InvariantCulture.CompareInfo
					.Compare (eraName, eras [i],
					CompareOptions.IgnoreCase) == 0)
					return calendar.Eras [i];
			
			eras = calendar.AbbreviatedEraNames;
			for (int i = 0; i < eras.Length; i++)
				if (CultureInfo.InvariantCulture.CompareInfo
					.Compare (eraName, eras [i],
					CompareOptions.IgnoreCase) == 0)
					return calendar.Eras [i];
			
			return -1;
		}

		public string GetEraName (int era)
		{
			if (era < 0 || era > calendar.EraNames.Length)
				throw new ArgumentOutOfRangeException ("era", era.ToString ());
			return calendar.EraNames [era - 1];
		}

		public string GetMonthName(int month)
		{
			if (month < 1 || month > 13) throw new ArgumentOutOfRangeException();
			return monthNames[month-1];
		}

		public string[] AbbreviatedDayNames
		{
			get { return (string[]) RawAbbreviatedDayNames.Clone (); }
			set { RawAbbreviatedDayNames = value; }
		}

		internal string[] RawAbbreviatedDayNames
		{
			get
			{
				return abbreviatedDayNames;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				if (value.GetLength(0) != 7) throw new ArgumentException(MSG_ARRAYSIZE_DAY);
				abbreviatedDayNames = (string[]) value.Clone();
			}
		}

		public string[] AbbreviatedMonthNames
		{
			get { return (string[]) RawAbbreviatedMonthNames.Clone (); }
			set { RawAbbreviatedMonthNames = value; }
		}

		internal string[] RawAbbreviatedMonthNames
		{
			get
			{
				return abbreviatedMonthNames;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				if (value.GetLength(0) != 13) throw new ArgumentException(MSG_ARRAYSIZE_MONTH);
				abbreviatedMonthNames = (string[]) value.Clone();
			}
		}

		public string[] DayNames
		{
			get { return (string[]) RawDayNames.Clone (); }
			set { RawDayNames = value; }
		}

		internal string[] RawDayNames
		{
			get
			{
				return dayNames;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				if (value.GetLength(0) != 7) throw new ArgumentException(MSG_ARRAYSIZE_DAY);
				dayNames = (string[]) value.Clone();
			}
		}

		public string[] MonthNames
		{
			get { return (string[]) RawMonthNames.Clone (); }
			set { RawMonthNames = value; }
		}

		internal string[] RawMonthNames
		{
			get
			{
				return monthNames;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				if (value.GetLength(0) != 13) throw new ArgumentException(MSG_ARRAYSIZE_MONTH);
				monthNames = (string[]) value.Clone();
			}
		}

		public string AMDesignator
		{
			get
			{
				return amDesignator;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				amDesignator = value;
			}
		}

		public string PMDesignator
		{
			get
			{
				return pmDesignator;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				pmDesignator = value;
			}
		}

		public string DateSeparator
		{
			get
			{
				return dateSeparator;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				dateSeparator = value;
			}
		}

		public string TimeSeparator
		{
			get
			{
				return timeSeparator;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				timeSeparator = value;
			}
		}

		public string LongDatePattern
		{
			get
			{
				return longDatePattern;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				longDatePattern = value;
			}
		}

		public string ShortDatePattern
		{
			get
			{
				return shortDatePattern;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				shortDatePattern = value;
			}
		}

		public string ShortTimePattern
		{
			get
			{
				return shortTimePattern;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				shortTimePattern = value;
			}
		}

		public string LongTimePattern
		{
			get
			{
				return longTimePattern;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				longTimePattern = value;
			}
		}

		public string MonthDayPattern
		{
			get
			{
				return monthDayPattern;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				monthDayPattern = value;
			}
		}

		public string YearMonthPattern
		{
			get
			{
				return yearMonthPattern;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				yearMonthPattern = value;
			}
		}

		public string FullDateTimePattern
		{
			get
			{
				if(fullDateTimePattern!=null) {
					return fullDateTimePattern;
				} else {
					return(longDatePattern + " " + longTimePattern);
				}
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				fullDateTimePattern = value;
			}
		}

		public static DateTimeFormatInfo CurrentInfo
		{
			get
			{
				return Thread.CurrentThread.CurrentCulture.DateTimeFormat;
			}
		}

		public static DateTimeFormatInfo InvariantInfo
		{
			get
			{
				if (theInvariantDateTimeFormatInfo == null) {
					theInvariantDateTimeFormatInfo = 
						DateTimeFormatInfo.ReadOnly(new DateTimeFormatInfo());
					theInvariantDateTimeFormatInfo.FillInvariantPatterns ();
				}
				return theInvariantDateTimeFormatInfo;
			}
		}

		public DayOfWeek FirstDayOfWeek
		{
			get
			{
				return (DayOfWeek)firstDayOfWeek;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if ((int) value < 0 || (int) value > 6) throw new ArgumentOutOfRangeException();
				firstDayOfWeek = (int)value;
			}
		}

		public Calendar Calendar
		{
			get
			{
				return calendar;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				calendar = value;
			}
		}

		public CalendarWeekRule CalendarWeekRule
		{
			get
			{
				return (CalendarWeekRule)calendarWeekRule;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				calendarWeekRule = (int)value;
			}
		}

		public string RFC1123Pattern
		{
			get
			{
				return _RFC1123Pattern;
			}
		}

		internal string RoundtripPattern
		{
			get
			{
				return _RoundtripPattern;
			}
		}

		public string SortableDateTimePattern
		{
			get
			{
				return _SortableDateTimePattern;
			}
		}

		public string UniversalSortableDateTimePattern
		{
			get
			{
				return _UniversalSortableDateTimePattern;
			}
		}
		
		// FIXME: Not complete depending on GetAllDateTimePatterns(char)")]
		public string[] GetAllDateTimePatterns () 
		{
			return (string[]) GetAllDateTimePatternsInternal ().Clone ();
		}

		// Same as above, but with no cloning, because we know that
		// clients are friendly
		internal string [] GetAllDateTimePatternsInternal ()
		{
			FillAllDateTimePatterns ();
			return all_date_time_patterns;
		}
		
		// Prevent write reordering
		volatile string [] all_date_time_patterns;
		
		void FillAllDateTimePatterns (){

			if (all_date_time_patterns != null)
				return;
			
			var al = new List<string> ();
			al.AddRange (GetAllRawDateTimePatterns ('d'));
			al.AddRange (GetAllRawDateTimePatterns ('D'));
			al.AddRange (GetAllRawDateTimePatterns ('g'));
			al.AddRange (GetAllRawDateTimePatterns ('G'));
			al.AddRange (GetAllRawDateTimePatterns ('f'));
			al.AddRange (GetAllRawDateTimePatterns ('F'));
			// Yes, that is very meaningless, but that is what MS
			// is doing (LAMESPEC: Since it is documented that
			// 'M' and 'm' are equal, they should not cosider
			// that there is a possibility that 'M' and 'm' are
			// different.)
			al.AddRange (GetAllRawDateTimePatterns ('m'));
			al.AddRange (GetAllRawDateTimePatterns ('M'));
			al.AddRange (GetAllRawDateTimePatterns ('r'));
			al.AddRange (GetAllRawDateTimePatterns ('R'));
			al.AddRange (GetAllRawDateTimePatterns ('s'));
			al.AddRange (GetAllRawDateTimePatterns ('t'));
			al.AddRange (GetAllRawDateTimePatterns ('T'));
			al.AddRange (GetAllRawDateTimePatterns ('u'));
			al.AddRange (GetAllRawDateTimePatterns ('U'));
			al.AddRange (GetAllRawDateTimePatterns ('y'));
			al.AddRange (GetAllRawDateTimePatterns ('Y'));

			// all_date_time_patterns needs to be volatile to prevent
			// reordering of writes here and still avoid any locking.
			all_date_time_patterns = al.ToArray ();
		}

		//
		// FIXME: We need more culture data in locale-builder
		//   Whoever put that comment, please expand.
		//
		public string[] GetAllDateTimePatterns (char format)
		{
			return (string[]) GetAllRawDateTimePatterns (format).Clone ();
		}

		internal string[] GetAllRawDateTimePatterns (char format)
		{
			string [] list;
			switch (format) {
			// Date
			case 'D':
				if (allLongDatePatterns != null && allLongDatePatterns.Length > 0)
					return allLongDatePatterns;
				return new string [] {LongDatePattern};
			case 'd':
				if (allShortDatePatterns != null && allShortDatePatterns.Length > 0)
					return allShortDatePatterns;
				return new string [] {ShortDatePattern};
			// Time
			case 'T':
				if (allLongTimePatterns != null && allLongTimePatterns.Length > 0)
					return allLongTimePatterns;
				return new string [] {LongTimePattern};
			case 't':
				if (allShortTimePatterns != null && allShortTimePatterns.Length > 0)
					return allShortTimePatterns;
				return new string [] {ShortTimePattern};
			// {Short|Long}Date + {Short|Long}Time
			// FIXME: they should be the agglegation of the
			// combination of the Date patterns and Time patterns.
			case 'G':
				list = PopulateCombinedList (allShortDatePatterns, allLongTimePatterns);
				if (list != null && list.Length > 0)
					return list;
				return new string [] {ShortDatePattern + " " + LongTimePattern};
			case 'g':
				list = PopulateCombinedList (allShortDatePatterns, allShortTimePatterns);
				if (list != null && list.Length > 0)
					return list;
				return new string [] {ShortDatePattern + " " + ShortTimePattern};
			// The 'U' pattern strings are always the same as 'F'.
			// (only differs in assuming UTC or not.)
			case 'U':
			case 'F':
				list = PopulateCombinedList (allLongDatePatterns, allLongTimePatterns);
				if (list != null && list.Length > 0)
					return list;
				return new string [] {LongDatePattern + " " + LongTimePattern};
			case 'f':
				list = PopulateCombinedList (allLongDatePatterns, allShortTimePatterns);
				if (list != null && list.Length > 0)
					return list;
				return new string [] {LongDatePattern + " " + ShortTimePattern};
			// MonthDay
			case 'm':
			case 'M':
				if (monthDayPatterns != null && monthDayPatterns.Length > 0)
					return monthDayPatterns;
				return new string [] {MonthDayPattern};
			// YearMonth
			case 'Y':
			case 'y':
				if (yearMonthPatterns != null && yearMonthPatterns.Length > 0)
					return yearMonthPatterns;
				return new string [] {YearMonthPattern};
			// RFC1123
			case 'r':
			case 'R':
				return new string [] {RFC1123Pattern};
			case 's':
				return new string [] {SortableDateTimePattern};
			case 'u':
				return new string [] {UniversalSortableDateTimePattern};
			}
			throw new ArgumentException ("Format specifier was invalid.");
		}

		public string GetDayName(DayOfWeek dayofweek)
		{
			int index = (int) dayofweek;
			if (index < 0 || index > 6) throw new ArgumentOutOfRangeException();
			return dayNames[index];
		}

		public string GetAbbreviatedDayName(DayOfWeek dayofweek)
		{
			int index = (int) dayofweek;
			if (index < 0 || index > 6) throw new ArgumentOutOfRangeException();
			return abbreviatedDayNames[index];
		}

		private void FillInvariantPatterns ()
		{
			allShortDatePatterns = new string [] {"MM/dd/yyyy"};
			allLongDatePatterns = new string [] {"dddd, dd MMMM yyyy"};
			allLongTimePatterns = new string [] {"HH:mm:ss"};
			allShortTimePatterns = new string [] {
				"HH:mm",
				"hh:mm tt",
				"H:mm",
				"h:mm tt"
			};
			monthDayPatterns = new string [] {"MMMM dd"};
			yearMonthPatterns = new string [] {"yyyy MMMM"};
		}

		private string [] PopulateCombinedList (string [] dates, string [] times)
		{
			if (dates != null && times != null) {
				string [] list = new string [dates.Length * times.Length];
				int i = 0;
				foreach (string d in dates)
					foreach (string t in times)
						list [i++] = d + " " + t;
				return list;
			}
			return null;
		}

		[MonoLimitation ("Returns only the English month abbreviated names")]
		[ComVisible (false)]
		public string [] AbbreviatedMonthGenitiveNames {
			get { return m_genitiveAbbreviatedMonthNames; }
			set { m_genitiveAbbreviatedMonthNames = value; }
		}

		[MonoLimitation ("Returns only the English month names")]
		[ComVisible (false)]
		public string [] MonthGenitiveNames {
			get { return genitiveMonthNames; }
			set { genitiveMonthNames = value; }
		}

		[MonoLimitation ("Returns an empty string as if the calendar name wasn't available")]
		[ComVisible (false)]
		public string NativeCalendarName {
			get { return String.Empty; }
		}

		[ComVisible (false)]
		public string [] ShortestDayNames {
			get {
				return shortDayNames;
			}

			set {
				if (value == null)
					throw new ArgumentNullException ();

				if (value.Length != 7)
					throw new ArgumentException ("Array must have 7 entries");
				
				for (int i = 0; i < 7; i++)
					if (value [i] == null)
						throw new ArgumentNullException (String.Format ("Element {0} is null", i));
				
				shortDayNames = value;
			}
		}

		[ComVisible (false)]
		public string GetShortestDayName (DayOfWeek dayOfWeek)
		{
			int index = (int) dayOfWeek;
			if (index < 0 || index > 6)
				throw new ArgumentOutOfRangeException();

			return shortDayNames [index];
		}

		[ComVisible (false)]
		public void SetAllDateTimePatterns (string [] patterns, char format)
		{
			if (patterns == null)
				throw new ArgumentNullException ("patterns");
			if (patterns.Length == 0)
				throw new ArgumentException ("patterns", "The argument patterns must not be of zero-length");

			switch (format) {
			// YearMonth
			case 'Y':
			case 'y':
				yearMonthPatterns = patterns;
				break;
			// MonthDay
			case 'm':
			case 'M':
				monthDayPatterns = patterns;
				break;
			// Date
			case 'D':
				allLongDatePatterns = patterns;
				break;
			case 'd':
				allShortDatePatterns = patterns;
				break;
			// Time
			case 'T':
				allLongTimePatterns = patterns;
				break;
			case 't':
				allShortTimePatterns = patterns;
				break;
			default:
				// note that any other formats are invalid (such as 'r', 'g', 'U')
				throw new ArgumentException ("format", "Format specifier is invalid");
			}
		}
	}
}
