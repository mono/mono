//
// DateAndTime.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//
// (C) 2002 Chris J Breisch
//

using System;

namespace Microsoft.VisualBasic 
{
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	sealed public class DateAndTime {
		// Declarations
		// Constructors
		// Properties
		[MonoTODO]
		public static System.String DateString {  get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
		[MonoTODO]
		public static System.DateTime Today {  get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
		[MonoTODO]
		public static System.Double Timer {  get { throw new NotImplementedException (); } }
		[MonoTODO]
		public static System.DateTime Now {  get { throw new NotImplementedException (); } }
		[MonoTODO]
		public static System.DateTime TimeOfDay {  get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
		[MonoTODO]
		public static System.String TimeString {  get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
		// Methods
		[MonoTODO]
		public static System.DateTime DateAdd (Microsoft.VisualBasic.DateInterval Interval, System.Double Number, System.DateTime DateValue) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int64 DateDiff (Microsoft.VisualBasic.DateInterval Interval, System.DateTime Date1, System.DateTime Date2, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] Microsoft.VisualBasic.FirstDayOfWeek DayOfWeek, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] Microsoft.VisualBasic.FirstWeekOfYear WeekOfYear) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int32 DatePart (Microsoft.VisualBasic.DateInterval Interval, System.DateTime DateValue, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] Microsoft.VisualBasic.FirstDayOfWeek FirstDayOfWeekValue, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] Microsoft.VisualBasic.FirstWeekOfYear FirstWeekOfYearValue) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.DateTime DateAdd (System.String Interval, System.Double Number, System.Object DateValue) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int64 DateDiff (System.String Interval, System.Object Date1, System.Object Date2, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] Microsoft.VisualBasic.FirstDayOfWeek DayOfWeek, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] Microsoft.VisualBasic.FirstWeekOfYear WeekOfYear) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int32 DatePart (System.String Interval, System.Object DateValue, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] Microsoft.VisualBasic.FirstDayOfWeek DayOfWeek, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] Microsoft.VisualBasic.FirstWeekOfYear WeekOfYear) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.DateTime DateSerial (System.Int32 Year, System.Int32 Month, System.Int32 Day) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.DateTime TimeSerial (System.Int32 Hour, System.Int32 Minute, System.Int32 Second) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.DateTime DateValue (System.String StringDate) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.DateTime TimeValue (System.String StringTime) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int32 Year (System.DateTime DateValue) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int32 Month (System.DateTime DateValue) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int32 Day (System.DateTime DateValue) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int32 Hour (System.DateTime TimeValue) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int32 Minute (System.DateTime TimeValue) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int32 Second (System.DateTime TimeValue) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int32 Weekday (System.DateTime DateValue, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] Microsoft.VisualBasic.FirstDayOfWeek DayOfWeek) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String MonthName (System.Int32 Month, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(false)] System.Boolean Abbreviate) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String WeekdayName (System.Int32 Weekday, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(false)] System.Boolean Abbreviate, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.FirstDayOfWeek FirstDayOfWeekValue) { throw new NotImplementedException (); }
		// Events
	};
}
