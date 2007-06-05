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
		public int RegionId; // it is GeoId in 2.0.
		// public byte MeasurementSystem;
		public string ISO2Name = String.Empty; // supplementalData.xml
		public string ISO3Name = String.Empty;
		public string Win3Name = String.Empty;
		public string EnglishName = String.Empty; // langs/en.xml
		public string CurrencySymbol = String.Empty;
		public string ISOCurrencySymbol = String.Empty; // supplementalData.xml
		public string CurrencyEnglishName = String.Empty; // langs/en.xml

		// NativeName and CurrencyNativeName are language dependent.

		public void AppendTableRow (StringBuilder builder)
		{
			builder.Append ("\t{ 0, "); // 0 is a slot for LCID (stored at managed code)
			builder.Append (RegionId);
			builder.Append (',');
			// builder.Append (MeasurementSystem);
			// builder.Append (',');
			builder.Append (EncodeStringIdx (ISO2Name));
			builder.Append (',');
			builder.Append (EncodeStringIdx (ISO3Name));
			builder.Append (',');
			builder.Append (EncodeStringIdx (Win3Name));
			builder.Append (',');
			builder.Append (EncodeStringIdx (EnglishName));
			builder.Append (',');
			builder.Append (EncodeStringIdx (CurrencySymbol));
			builder.Append (',');
			builder.Append (EncodeStringIdx (ISOCurrencySymbol));
			builder.Append (',');
			builder.Append (EncodeStringIdx (CurrencyEnglishName));
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


