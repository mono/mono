//
// DateFormat.cs
//
// Author:
//   Martin Adoue (martin@cwanet.com)
//
// (C) 2002 Martin Adoue
//
namespace Microsoft.VisualBasic {

	/// <summary>
	/// When you call the DateValue function, you can use the following 
	/// enumeration members in your code in place of the actual values.
	/// </summary>
	public enum DateFormat : int {
		/// <summary>
		/// For real numbers, displays a date and time. If the number has no fractional part, displays only a date. If the number has no integer part, displays time only. Date and time display is determined by your computer's regional settings.
		/// </summary>
		GeneralDate = 0,
		/// <summary>
		/// Displays a date using the long-date format specified in your computer's regional settings.
		/// </summary>
		LongDate = 1,
		/// <summary>
		/// Displays a date using the short-date format specified in your computer's regional settings.
		/// </summary>
		ShortDate = 2,	
		/// <summary>
		/// Displays a time using the long-time format specified in your computer's regional settings.
		/// </summary>
		LongTime = 3,		
		/// <summary>
		/// Displays a time using the short-time format specified in your computer's regional settings.
		/// </summary>
		ShortTime = 4		
	};
}
