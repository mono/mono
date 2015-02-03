//
// System.Globalization.DateTimeFormatInfo.cs
//
// Authors:
//   Martin Weindel (martin.weindel@t-online.de)
//   Marek Safar (marek.safar@gmail.com)
//
// (C) Martin Weindel (martin.weindel@t-online.de)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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

namespace System.Globalization
{
	public sealed partial class DateTimeFormatInfo : ICloneable, IFormatProvider
	{
		internal string GetMonthGenitiveName (int month)
		{
			return internalGetGenitiveMonthNames (false) [month - 1];
		}

		internal string[] RawAbbreviatedDayNames
		{
			get {
				return internalGetAbbreviatedDayOfWeekNames ();
			}
		}

		internal string[] RawAbbreviatedMonthNames
		{
			get
			{
				return internalGetAbbreviatedMonthNames ();
			}
		}

		internal string[] RawDayNames  {
			get {
				return internalGetDayOfWeekNames ();
			}
		}

		internal string[] RawMonthNames {
			get {
				return internalGetMonthNames ();
			}
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
			
			var al = new List<string> (16);
			al.AddRange (GetAllRawDateTimePatterns ('d'));
			al.AddRange (GetAllRawDateTimePatterns ('D'));
			al.AddRange (GetAllRawDateTimePatterns ('f'));
			al.AddRange (GetAllRawDateTimePatterns ('F'));
			al.AddRange (GetAllRawDateTimePatterns ('g'));
			al.AddRange (GetAllRawDateTimePatterns ('G'));
			al.AddRange (GetAllRawDateTimePatterns ('m'));
			al.AddRange (GetAllRawDateTimePatterns ('M'));
			al.AddRange (GetAllRawDateTimePatterns ('o'));
			al.AddRange (GetAllRawDateTimePatterns ('O'));
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

		string[] GetAllRawDateTimePatterns (char format)
		{
			return GetAllDateTimePatterns (format);
		}

		internal CompareInfo CompareInfo {
			get {
				throw new NotImplementedException ("CompareInfo");
			}
		}

		internal String FullTimeSpanPositivePattern {
			get {
				throw new NotImplementedException ("FullTimeSpanPositivePattern");
			}
		}
	}
}
