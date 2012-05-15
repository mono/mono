//
// Mono.Tools.LocaleBuilder.RegionInfoEntry
//
// Author(s):
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// (C) 2005, Novell, Inc (http://www.novell.com)
//


using System;
using System.Text;
using System.Collections;

namespace Mono.Tools.LocaleBuilder
{
	public class RegionInfoEntry : Entry
	{
		public int Index; // Used to link region from culture, it must be 0-based index from region_name_entries

		public string GeoId;
		public string TwoLetterISORegionName;
		public string ThreeLetterISORegionName;
		public string ThreeLetterWindowsRegionName;
		public string EnglishName;
		public string CurrencySymbol;
		public string ISOCurrencySymbol;
		public string CurrencyEnglishName;
		public string Name;
		public string DisplayName;
		public string NativeName;
		public string CurrencyNativeName;
		public bool IsMetric = true;

		public void AppendTableRow (StringBuilder builder)
		{
			builder.Append ("\t{ ");
			builder.Append (GeoId).Append (',');
			builder.Append (EncodeStringIdx (TwoLetterISORegionName)).Append (',');
			builder.Append (EncodeStringIdx (ThreeLetterISORegionName)).Append (',');
			builder.Append (EncodeStringIdx (ThreeLetterWindowsRegionName)).Append (',');
			builder.Append (EncodeStringIdx (EnglishName)).Append (',');
			builder.Append (EncodeStringIdx (NativeName)).Append (',');
			builder.Append (EncodeStringIdx (CurrencySymbol)).Append (',');
			builder.Append (EncodeStringIdx (ISOCurrencySymbol)).Append (',');
			builder.Append (EncodeStringIdx (CurrencyEnglishName)).Append (',');
			builder.Append (EncodeStringIdx (CurrencyNativeName));
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


