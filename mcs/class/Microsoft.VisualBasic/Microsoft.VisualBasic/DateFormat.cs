//
// DateFormat.cs
//
// Author:
//   Martin Adoue (martin@cwanet.com)
//
// (C) 2002 Martin Adoue
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
namespace Microsoft.VisualBasic {

	/// <summary>
	/// When you call the DateValue function, you can use the following 
	/// enumeration members in your code in place of the actual values.
	/// </summary>
	[System.SerializableAttribute]
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
