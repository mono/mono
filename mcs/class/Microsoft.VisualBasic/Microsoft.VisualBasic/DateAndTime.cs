//
// DateAndTime.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic {
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Auto)] 
	sealed public class DateAndTime {
		// Declarations
		// Constructors
		// Properties
		public static System.String DateString { get {return "";} set {} }
		public static System.DateTime Today { get {return System.DateTime.MinValue;} set {} }
		public static System.Double Timer { get {return 0;} }
		public static System.DateTime Now { get {return System.DateTime.MinValue;} }
		public static System.DateTime TimeOfDay { get {return System.DateTime.MinValue;} set {} }
		public static System.String TimeString { get {return "";} set {} }
		// Methods
		public static System.DateTime DateAdd (Microsoft.VisualBasic.DateInterval Interval, System.Double Number, System.DateTime DateValue) { return System.DateTime.MinValue;}
		public static System.Int64 DateDiff (Microsoft.VisualBasic.DateInterval Interval, System.DateTime Date1, System.DateTime Date2, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] Microsoft.VisualBasic.FirstDayOfWeek DayOfWeek, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] Microsoft.VisualBasic.FirstWeekOfYear WeekOfYear) { return 0;}
		public static System.Int32 DatePart (Microsoft.VisualBasic.DateInterval Interval, System.DateTime DateValue, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] Microsoft.VisualBasic.FirstDayOfWeek FirstDayOfWeekValue, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] Microsoft.VisualBasic.FirstWeekOfYear FirstWeekOfYearValue) { return 0;}
		public static System.DateTime DateAdd (System.String Interval, System.Double Number, System.Object DateValue) { return System.DateTime.MinValue;}
		public static System.Int64 DateDiff (System.String Interval, System.Object Date1, System.Object Date2, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] Microsoft.VisualBasic.FirstDayOfWeek DayOfWeek, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] Microsoft.VisualBasic.FirstWeekOfYear WeekOfYear) { return 0;}
		public static System.Int32 DatePart (System.String Interval, System.Object DateValue, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] Microsoft.VisualBasic.FirstDayOfWeek DayOfWeek, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] Microsoft.VisualBasic.FirstWeekOfYear WeekOfYear) { return 0;}
		public static System.DateTime DateSerial (System.Int32 Year, System.Int32 Month, System.Int32 Day) { return System.DateTime.MinValue;}
		public static System.DateTime TimeSerial (System.Int32 Hour, System.Int32 Minute, System.Int32 Second) { return System.DateTime.MinValue;}
		public static System.DateTime DateValue (System.String StringDate) { return System.DateTime.MinValue;}
		public static System.DateTime TimeValue (System.String StringTime) { return System.DateTime.MinValue;}
		public static System.Int32 Year (System.DateTime DateValue) { return 0;}
		public static System.Int32 Month (System.DateTime DateValue) { return 0;}
		public static System.Int32 Day (System.DateTime DateValue) { return 0;}
		public static System.Int32 Hour (System.DateTime TimeValue) { return 0;}
		public static System.Int32 Minute (System.DateTime TimeValue) { return 0;}
		public static System.Int32 Second (System.DateTime TimeValue) { return 0;}
		public static System.Int32 Weekday (System.DateTime DateValue, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] Microsoft.VisualBasic.FirstDayOfWeek DayOfWeek) { return 0;}
		public static System.String MonthName (System.Int32 Month, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(false)] System.Boolean Abbreviate) { return "";}
		public static System.String WeekdayName (System.Int32 Weekday, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(false)] System.Boolean Abbreviate, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.FirstDayOfWeek FirstDayOfWeekValue) { return "";}
		// Events
	};
}
