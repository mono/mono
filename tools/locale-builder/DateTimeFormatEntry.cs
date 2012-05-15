//
// DateTimeFormatEntry.cs
//
// Authors:
//  Jackson Harper (jackson@ximian.com)
//	Marek Safar  <marek.safar@gmail.com>
//
// (C) 2004, Novell, Inc (http://www.novell.com)
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


using System.Text;
using System.Collections.Generic;

namespace Mono.Tools.LocaleBuilder
{
	public class DateTimeFormatEntry : Entry
	{
		public string NativeCalendarName;
		public string[] AbbreviatedDayNames = new string[Constants.NUM_DAYS];
		// Input data are mostly missing for abbreviated month but datetime 'MMM' parse depends on them
		// we pre-fill them the most common ones
		public string[] AbbreviatedMonthGenitiveNames = new string[Constants.NUM_MONTHS] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", null };
		public string[] AbbreviatedMonthNames = new string[Constants.NUM_MONTHS] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", null };
		public string AMDesignator;
		public int? CalendarWeekRule;
		public string DateSeparator;
		public string[] DayNames = new string[Constants.NUM_DAYS];
		public int? FirstDayOfWeek;
		public string RawFullDateTimePattern;
		public string LongDatePattern;
		public string LongTimePattern;
		public string MonthDayPattern;
		public string[] MonthGenitiveNames = new string[Constants.NUM_MONTHS];
		public string[] MonthNames = new string[Constants.NUM_MONTHS];
		public string PMDesignator;
		public string ShortDatePattern;
		public string ShortTimePattern;
		public string TimeSeparator;
		public string YearMonthPattern;
		public string[] ShortDatePatterns = new string[Constants.NUM_SHORT_DATE_PATTERNS];
		public string[] LongDatePatterns = new string[Constants.NUM_LONG_DATE_PATTERNS];
		public string[] ShortTimePatterns = new string[Constants.NUM_SHORT_TIME_PATTERNS];
		public string[] LongTimePatterns = new string[Constants.NUM_LONG_TIME_PATTERNS];
		public string[] ShortestDayNames = new string[Constants.NUM_DAYS];

		public int Row;

		public void AppendTableRow (StringBuilder builder)
		{
			builder.Append ("\t{");
			builder.Append (EncodeStringIdx (LongDatePattern) + ", ");
			builder.Append (EncodeStringIdx (ShortDatePattern) + ", ");

			builder.Append (EncodeStringIdx (LongTimePattern) + ", ");
			builder.Append (EncodeStringIdx (ShortTimePattern) + ", ");

			builder.Append (EncodeStringIdx (YearMonthPattern) + ", ");
			builder.Append (EncodeStringIdx (MonthDayPattern) + ", ");

			builder.Append (EncodeStringIdx (AMDesignator) + ", ");
			builder.Append (EncodeStringIdx (PMDesignator) + ", ");

			AppendNames (builder, DayNames).Append (", ");
			AppendNames (builder, AbbreviatedDayNames).Append (", ");
			AppendNames (builder, ShortestDayNames).Append (", ");

			AppendNames (builder, MonthNames).Append (", ");
			AppendNames (builder, MonthGenitiveNames).Append (", ");
			AppendNames (builder, AbbreviatedMonthNames).Append (", ");
			AppendNames (builder, AbbreviatedMonthGenitiveNames).Append (", ");

			// TODO:
			builder.Append ((CalendarWeekRule ?? 0) + ", ");
			builder.Append ((FirstDayOfWeek ?? 0) + ", ");

			builder.Append (EncodeStringIdx (DateSeparator) + ", ");
			builder.Append (EncodeStringIdx (TimeSeparator) + ", ");

			AppendPatterns (builder, ShortDatePatterns);
			builder.Append (',');
			AppendPatterns (builder, LongDatePatterns);
			builder.Append (',');
			AppendPatterns (builder, ShortTimePatterns);
			builder.Append (',');
			AppendPatterns (builder, LongTimePatterns);

			builder.Append ('}');
		}

		private void AppendPatterns (StringBuilder builder, IList<string> patterns)
		{
			builder.Append ('{');
			for (int i = 0; i < patterns.Count; i++) {
				if (i > 0)
					builder.Append (',');

				string s = EncodeStringIdx (patterns[i]);
				builder.Append (s);
			}
			if (patterns.Count == 0)
				builder.Append ('0');
			builder.Append ('}');
		}

		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			AppendTableRow (builder);
			return builder.ToString ();
		}
	}
}


