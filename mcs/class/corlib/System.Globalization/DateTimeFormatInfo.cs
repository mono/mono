// System.Globalization.DateTimeFormatInfo
//
// Some useful functions are missing in the ECMA specs.
// They have been added following MS SDK Beta2
//
// Martin Weindel (martin.weindel@t-online.de)
//
// (C) Martin Weindel (martin.weindel@t-online.de)

using System;

using System.Threading;


namespace System.Globalization 
{

	[Serializable]
	
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

		internal static DateTimeFormatInfo theInvariantDateTimeFormatInfo;

		private bool readOnly;
		
		private string _AMDesignator;
		
		private string _PMDesignator;
		
		private string _DateSeparator;
		
		private string _TimeSeparator;
		
		private string _ShortDatePattern;
		
		private string _LongDatePattern;
		
		private string _ShortTimePattern;
		
		private string _LongTimePattern;
		
		private string _MonthDayPattern;
		
		private string _YearMonthPattern;
		
		private string _FullDateTimePattern;
		
		private string _RFC1123Pattern;
		
		private string _SortableDateTimePattern;
		
		private string _UniversalSortableDateTimePattern;
		
		private DayOfWeek _FirstDayOfWeek;
		
		private Calendar _Calendar;
		
		private CalendarWeekRule _CalendarWeekRule;
		
		private string[] _AbbreviatedDayNames;
		
		private string[] _DayNames;
		
		private string[] _MonthNames;
		
		private string[] _AbbreviatedMonthNames;

		
		public DateTimeFormatInfo()
		
		{
			
			readOnly = false;
			
			_AMDesignator = "AM";
			
			_PMDesignator = "PM";
			
			_DateSeparator = "/";
			
			_TimeSeparator = ":";
			
			_ShortDatePattern = "MM/dd/yyyy";
			
			_LongDatePattern = "dddd, dd MMMM yyyy";
			
			_ShortTimePattern = "HH:mm";
			
			_LongTimePattern = "HH:mm:ss";
			
			_MonthDayPattern = "MMMM dd";
			
			_YearMonthPattern = "yyyy MMMM";
			
			_FullDateTimePattern = "dddd, dd MMMM yyyy HH:mm:ss";

			// FIXME for the following three pattern:  "The default value of this property is 
			//derived from the calendar that is set for CultureInfo.CurrentCulture or the default 
			//calendar of CultureInfo.CurrentCulture."
			
			_RFC1123Pattern = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'"; 
			
			_SortableDateTimePattern = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";
			
			_UniversalSortableDateTimePattern = "yyyy'-'MM'-'dd HH':'mm':'ss'Z'";

			

			_FirstDayOfWeek = DayOfWeek.Sunday;
			
			_Calendar = new GregorianCalendar();
			
			_CalendarWeekRule = CalendarWeekRule.FirstDay;

			
			_AbbreviatedDayNames = INVARIANT_ABBREVIATED_DAY_NAMES;
			
			_DayNames = INVARIANT_DAY_NAMES;
			
			_AbbreviatedMonthNames = INVARIANT_ABBREVIATED_MONTH_NAMES;
			
			_MonthNames = INVARIANT_MONTH_NAMES;
		
		}
				
		
		
		// LAMESPEC: this is not in ECMA specs
		
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
				
				return readOnly;
			
			}
		
		}

		
		
		public static DateTimeFormatInfo ReadOnly(DateTimeFormatInfo dtfi)
		
		{
			
			DateTimeFormatInfo copy = (DateTimeFormatInfo)dtfi.Clone();
			
			copy.readOnly = true;
			
			return copy;
		
		}			

		
		
		public object Clone () 
		
		{
			
			DateTimeFormatInfo clone = (DateTimeFormatInfo) MemberwiseClone();
			
			// clone is not read only
			
			clone.readOnly = false;
			
			return clone;
		
		}

		
		
		public object GetFormat(Type formatType)
		{
			return (formatType == GetType()) ? this : null;
		}

		public string GetAbbreviatedEraName(int era) 
		{
			if (era < _Calendar.Eras.Length || era >= _Calendar.Eras.Length)
				throw new ArgumentOutOfRangeException();
			notImplemented();
			//FIXME: implement me
			return null;
		}

		public string GetAbbreviatedMonthName(int month) 
		{
			if (month < 1 || month > 13) throw new ArgumentOutOfRangeException();
			return _AbbreviatedMonthNames[month-1];
		}

		public int GetEra(string eraName)
		{
			if (eraName == null) throw new ArgumentNullException();
			eraName = eraName.ToUpper();
			notImplemented();
			//FIXME: implement me
			return -1;
		}

		public string GetEraName(int era)
		{
			if (era < _Calendar.Eras.Length || era >= _Calendar.Eras.Length)
				throw new ArgumentOutOfRangeException();
			notImplemented();
			//FIXME: implement me
			return null;
		}

		public string GetMonthName(int month)
		{
			if (month < 1 || month > 13) throw new ArgumentOutOfRangeException();
			return _MonthNames[month-1];
		}

		public string[] AbbreviatedDayNames
		{
			get 
			{
				return (string[]) _AbbreviatedDayNames.Clone();
			}
			
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				if (value.GetLength(0) != 7) throw new ArgumentException(MSG_ARRAYSIZE_DAY);
				_AbbreviatedDayNames = (string[]) value.Clone();
			}
		} 

		public string[] AbbreviatedMonthNames
		{
			get 
			{
				return (string[]) _AbbreviatedMonthNames.Clone();
			}
			
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				if (value.GetLength(0) != 13) throw new ArgumentException(MSG_ARRAYSIZE_MONTH);
				_AbbreviatedMonthNames = (string[]) value.Clone();
			}
		} 

		public string[] DayNames
		{
			get 
			{
				return (string[]) _DayNames.Clone();
			}
			
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				if (value.GetLength(0) != 7) throw new ArgumentException(MSG_ARRAYSIZE_DAY);
				_DayNames = (string[]) value.Clone();
			}
		} 

		public string[] MonthNames
		{
			get 
			{
				return (string[]) _MonthNames.Clone();
			}
			
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				if (value.GetLength(0) != 13) throw new ArgumentException(MSG_ARRAYSIZE_MONTH);
				_MonthNames = (string[]) value.Clone();
			}
		} 

		public string AMDesignator
		{
			get 
			{
				return _AMDesignator;
			}
			
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_AMDesignator = value;
			}
		} 

		public string PMDesignator
		{
			get 
			{
				return _PMDesignator;
			}
			
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_PMDesignator = value;
			}
		} 

		public string DateSeparator
		{
			get 
			{
				return _DateSeparator;
			}
			
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_DateSeparator = value;
			}
		} 

		public string TimeSeparator
		{
			get 
			{
				return _TimeSeparator;
			}
			
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_TimeSeparator = value;
			}
		} 

		public string LongDatePattern
		{
			get 
			{
				return _LongDatePattern;
			}
			
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_LongDatePattern = value;
			}
		} 

		public string ShortDatePattern
		{
			get 
			{
				return _ShortDatePattern;
			}
			
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_ShortDatePattern = value;
			}
		} 

		public string ShortTimePattern
		{
			get 
			{
				return _ShortTimePattern;
			}
			
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_ShortTimePattern = value;
			}
		} 

		public string LongTimePattern
		{
			get 
			{
				return _LongTimePattern;
			}
			
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_LongTimePattern = value;
			}
		} 

		public string MonthDayPattern 
		{
			get 
			{
				return _MonthDayPattern;
			}
			
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_MonthDayPattern = value;
			}
		} 

		public string YearMonthPattern 
		{
			get 
			{
				return _YearMonthPattern;
			}
			
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_YearMonthPattern = value;
			}
		} 

		public string FullDateTimePattern
		{
			get 
			{
				return _FullDateTimePattern;
			}
			
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_FullDateTimePattern = value;
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
				if (theInvariantDateTimeFormatInfo == null)
				{
					theInvariantDateTimeFormatInfo = 
						DateTimeFormatInfo.ReadOnly(new DateTimeFormatInfo());
				}
				return theInvariantDateTimeFormatInfo;
			}
		} 

		// LAMESPEC: this is not in ECMA specs
		public DayOfWeek FirstDayOfWeek
		{
			get
			{
				return _FirstDayOfWeek;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if ((int) value < 0 || (int) value > 6) throw new ArgumentOutOfRangeException();
				_FirstDayOfWeek = value;
			}
		}

		// LAMESPEC: this is not in ECMA specs
		public Calendar Calendar 
		{
			get
			{
				return _Calendar;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_Calendar = value;
			}
		}

		public CalendarWeekRule CalendarWeekRule 
		{
			get
			{
				return _CalendarWeekRule;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				_CalendarWeekRule = value;
			}
		}

		// LAMESPEC: this is not in ECMA specs
		public string RFC1123Pattern
		{
			get
			{
				return _RFC1123Pattern;
			}
		}

		// LAMESPEC: this is not in ECMA specs
		public string SortableDateTimePattern
		{
			get
			{
				return _SortableDateTimePattern;
			}
		}

		// LAMESPEC: this is not in ECMA specs
		public string UniversalSortableDateTimePattern
		{
			get
			{
				return _UniversalSortableDateTimePattern;
			}
		}

		// LAMESPEC: this is not in ECMA specs
		public string[] GetAllDateTimePatterns()
		{
			notImplemented();
			//FIXME: implement me
			return null;
		}

		// LAMESPEC: this is not in ECMA specs
		public string[] GetAllDateTimePatterns(char format)
		{
			notImplemented();
			//FIXME: implement me
			return null;
		}

		// LAMESPEC: this is not in ECMA specs
		public string GetDayName(DayOfWeek dayofweek)
		{
			int index = (int) dayofweek;
			if (index < 0 || index > 6) throw new ArgumentOutOfRangeException();
			return _DayNames[index];
		}

		// LAMESPEC: this is not in ECMA specs
		public string GetAbbreviatedDayName(DayOfWeek dayofweek)
		{
			int index = (int) dayofweek;
			if (index < 0 || index > 6) throw new ArgumentOutOfRangeException();
			return _AbbreviatedDayNames[index];
		}

		private static void notImplemented()
		{
			throw new Exception("Not implemented");
		}
	}

}
