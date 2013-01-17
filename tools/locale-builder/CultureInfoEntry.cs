//
// Mono.Tools.LocaleBuilder.CultureInfoEntry
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//


using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;

namespace Mono.Tools.LocaleBuilder
{
	public class CultureInfoEntry : Entry
	{
		string language;

		public string Script;
		public string Territory;

		public string EnglishName;
		public string DisplayName;
		public string NativeName;
		public string ThreeLetterWindowsLanguageName;
		public string TwoLetterISOLanguageName;
		public string ThreeLetterISOLanguageName;
		public string LCID;
		public string ParentLcid;
		public string SpecificLcid;
		public RegionInfoEntry RegionInfoEntry;
		public DateTimeFormatEntry DateTimeFormatEntry;
		public NumberFormatEntry NumberFormatEntry;
		public TextInfoEntry TextInfoEntry;
		public int DateTimeIndex;
		public int NumberIndex;
		public string NativeCurrencyName;
		public string NativeTerritoryName;
		public string[] NativeCalendarNames = new string[Constants.NUM_CALENDARS];

		public CalendarType CalendarType;
		public GregorianCalendarTypes GregorianCalendarType;

		public List<CultureInfoEntry> Children = new List<CultureInfoEntry> ();

		public int Row;

		public CultureInfoEntry ()
		{
			DateTimeFormatEntry = new DateTimeFormatEntry ();
			NumberFormatEntry = new NumberFormatEntry ();
		}

		public string Language {
			get {
				return language;
			}
			set {
				language = value;
			}
		}

		public bool HasMissingLocale { get; set; }

		public string OriginalName { get; set; }

		public CultureInfoEntry Parent { get; set; }

		public string Name {
			get {
				string s = language;
				if (Script != null)
					s = s + "-" + Script;
				if (Territory != null)
					s = s + "-" + Territory;

				return s;
			}
		}

		public string GetExportName ()
		{
			return OriginalName.Replace ('_', '-');
		}

		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			AppendTableRow (builder);
			return builder.ToString ();
		}

		public void AppendTableRow (StringBuilder builder)
		{
			builder.Append ("\t{");
			builder.Append (LCID).Append (", ");
			builder.Append (ParentLcid).Append (", ");

			int calendar_type = (int) CalendarType;
			calendar_type <<= 8;
			if (CalendarType == CalendarType.Gregorian)
				calendar_type |= (int) GregorianCalendarType;

			builder.Append (calendar_type).Append (", ");
			builder.Append (RegionInfoEntry == null ? -1 : RegionInfoEntry.Index).Append (", ");
			builder.Append (EncodeStringIdx (GetExportName ())).Append (", ");
			builder.Append (EncodeStringIdx (EnglishName)).Append (", ");
			builder.Append (EncodeStringIdx (NativeName)).Append (", ");
			builder.Append (EncodeStringIdx (ThreeLetterWindowsLanguageName)).Append (", ");
			builder.Append (EncodeStringIdx (ThreeLetterISOLanguageName)).Append (", ");
			builder.Append (EncodeStringIdx (TwoLetterISOLanguageName)).Append (", ");
			builder.Append (EncodeStringIdx (Territory)).Append (", ");
			AppendNames (builder, NativeCalendarNames).Append (", ");
			builder.Append (DateTimeFormatEntry.Row).Append (", ");
			builder.Append (NumberFormatEntry.Row).Append (", ");
			builder.Append (TextInfoEntry.ToString ());
			builder.Append ('}');
		}

		private string ValuesString (int[] values)
		{
			StringBuilder builder = new StringBuilder ();
			builder.Append ('{');
			for (int i = 0; i < values.Length; i++) {
				builder.Append (values[i].ToString ());
				if (i + 1 < values.Length)
					builder.Append (", ");
			}
			builder.Append ("}");
			return builder.ToString ();
		}
	}

}

