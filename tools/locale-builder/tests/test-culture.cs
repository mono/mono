using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Collections.Generic;

class TestCulture
{
	public static void Main ()
	{
		var all = CultureInfo.GetCultures (CultureTypes.SpecificCultures | CultureTypes.NeutralCultures).OrderBy (l => l.LCID);

		Console.OutputEncoding = Encoding.UTF8;
		var writer = Console.Out;
	
		foreach (var c in all) {
			if (c.LCID == CultureInfo.InvariantCulture.LCID)
				continue;

			writer.WriteLine ("Name: {0}, LCID 0x{1}", c.Name , c.LCID.ToString ("X4"));

			writer.WriteLine ("{0}: {1}", "DisplayName", c.DisplayName);
			writer.WriteLine ("{0}: {1}", "EnglishName", c.EnglishName);
			writer.WriteLine ("{0}: {1}", "NativeName", c.NativeName);
			// writer.WriteLine ("{0}: {1}", "OptionalCalendars", c.OptionalCalendars);
			writer.WriteLine ("{0}: {1}", "ThreeLetterISOLanguageName", c.ThreeLetterISOLanguageName);
			writer.WriteLine ("{0}: {1}", "ThreeLetterWindowsLanguageName", c.ThreeLetterWindowsLanguageName);
			writer.WriteLine ("{0}: {1}", "TwoLetterISOLanguageName", c.TwoLetterISOLanguageName);
			writer.WriteLine ("{0}: {1}", "Calendar", c.Calendar.GetType ());

			var df = c.DateTimeFormat;
			writer.WriteLine ("-- DateTimeFormat --");
			Dump (writer, df.AbbreviatedDayNames, "AbbreviatedDayNames");
			Dump (writer, df.AbbreviatedMonthGenitiveNames, "AbbreviatedMonthGenitiveNames");
			Dump (writer, df.AbbreviatedMonthNames, "AbbreviatedMonthNames");
			writer.WriteLine ("{0}: {1}", "AMDesignator", df.AMDesignator);
			writer.WriteLine ("{0}: {1}", "CalendarWeekRule", df.CalendarWeekRule);
			writer.WriteLine ("{0}: {1}", "DateSeparator", df.DateSeparator);
			Dump (writer, df.DayNames, "DayNames");
			writer.WriteLine ("{0}: {1}", "FirstDayOfWeek", df.FirstDayOfWeek);
			Dump (writer, df.GetAllDateTimePatterns (), "GetAllDateTimePatterns");
			// df.GetAbbreviatedEraName
			// df.GetEra
			// df.GetEraName
			writer.WriteLine ("{0}: {1}", "LongDatePattern", df.LongDatePattern);
			writer.WriteLine ("{0}: {1}", "LongTimePattern", df.LongTimePattern);
			writer.WriteLine ("{0}: {1}", "MonthDayPattern", df.MonthDayPattern);
			Dump (writer, df.MonthGenitiveNames, "MonthGenitiveNames");
			Dump (writer, df.MonthNames, "MonthNames");
			writer.WriteLine ("{0}: {1}", "NativeCalendarName", df.NativeCalendarName);
			writer.WriteLine ("{0}: {1}", "PMDesignator", df.PMDesignator);
			writer.WriteLine ("{0}: {1}", "ShortDatePattern", df.ShortDatePattern);
			Dump (writer, df.ShortestDayNames, "ShortestDayNames");
			writer.WriteLine ("{0}: {1}", "ShortTimePattern", df.ShortTimePattern);
			writer.WriteLine ("{0}: {1}", "TimeSeparator", df.TimeSeparator);
			writer.WriteLine ("{0}: {1}", "YearMonthPattern", df.YearMonthPattern);

			var ti = c.TextInfo;
			writer.WriteLine ("-- TextInfo --");
			writer.WriteLine ("{0}: {1}", "ANSICodePage", ti.ANSICodePage);
			writer.WriteLine ("{0}: {1}", "EBCDICCodePage", ti.EBCDICCodePage);
			writer.WriteLine ("{0}: {1}", "IsRightToLeft", ti.IsRightToLeft);
			writer.WriteLine ("{0}: {1}", "ListSeparator", ti.ListSeparator);
			writer.WriteLine ("{0}: {1}", "MacCodePage", ti.MacCodePage);
			writer.WriteLine ("{0}: {1}", "OEMCodePage", ti.OEMCodePage);

			var nf = c.NumberFormat;
			writer.WriteLine ("-- NumberFormat --");
			writer.WriteLine ("{0}: {1}", "CurrencyDecimalDigits", nf.CurrencyDecimalDigits);
			writer.WriteLine ("{0}: {1}", "CurrencyDecimalSeparator", nf.CurrencyDecimalSeparator);
			writer.WriteLine ("{0}: {1}", "CurrencyGroupSeparator", nf.CurrencyGroupSeparator);
			Dump (writer, nf.CurrencyGroupSizes, "CurrencyGroupSizes");
			writer.WriteLine ("{0}: {1}", "CurrencyNegativePattern", nf.CurrencyNegativePattern);
			writer.WriteLine ("{0}: {1}", "CurrencyPositivePattern", nf.CurrencyPositivePattern);
			writer.WriteLine ("{0}: {1}", "CurrencySymbol", nf.CurrencySymbol);
			writer.WriteLine ("{0}: {1}", "DigitSubstitution", nf.DigitSubstitution);
			writer.WriteLine ("{0}: {1}", "NaNSymbol", nf.NaNSymbol);
			Dump (writer, nf.NativeDigits, "NativeDigits");
			writer.WriteLine ("{0}: {1}", "NegativeInfinitySymbol", nf.NegativeInfinitySymbol);
			writer.WriteLine ("{0}: {1}", "NegativeSign", nf.NegativeSign);
			writer.WriteLine ("{0}: {1}", "NumberDecimalDigits", nf.NumberDecimalDigits);
			writer.WriteLine ("{0}: {1}", "NumberDecimalSeparator", nf.NumberDecimalSeparator);
			writer.WriteLine ("{0}: {1}", "NumberGroupSeparator", nf.NumberGroupSeparator);
			Dump (writer, nf.NumberGroupSizes, "NumberGroupSizes");
			writer.WriteLine ("{0}: {1}", "NumberNegativePattern", nf.NumberNegativePattern);
			writer.WriteLine ("{0}: {1}", "PercentDecimalDigits", nf.PercentDecimalDigits);
			writer.WriteLine ("{0}: {1}", "PercentDecimalSeparator", nf.PercentDecimalSeparator);
			writer.WriteLine ("{0}: {1}", "PercentGroupSeparator", nf.PercentGroupSeparator);
			Dump (writer, nf.PercentGroupSizes, "PercentGroupSizes");
			writer.WriteLine ("{0}: {1}", "PercentNegativePattern", nf.PercentNegativePattern);
			writer.WriteLine ("{0}: {1}", "PercentPositivePattern", nf.PercentPositivePattern);
			writer.WriteLine ("{0}: {1}", "PercentSymbol", nf.PercentSymbol);
			writer.WriteLine ("{0}: {1}", "PerMilleSymbol", nf.PerMilleSymbol);
			writer.WriteLine ("{0}: {1}", "PositiveInfinitySymbol", nf.PositiveInfinitySymbol);
			writer.WriteLine ("{0}: {1}", "PositiveSign", nf.PositiveSign);

			if (!c.IsNeutralCulture) {
				var ri = new RegionInfo (c.LCID);
				writer.WriteLine ("-- RegionInfo --");
				writer.WriteLine ("{0}: {1}", "CurrencyEnglishName", ri.CurrencyEnglishName);
				writer.WriteLine ("{0}: {1}", "CurrencyNativeName", ri.CurrencyNativeName);
				writer.WriteLine ("{0}: {1}", "CurrencySymbol", ri.CurrencySymbol);
				writer.WriteLine ("{0}: {1}", "DisplayName", ri.DisplayName);
				writer.WriteLine ("{0}: {1}", "EnglishName", ri.EnglishName);
				writer.WriteLine ("{0}: {1}", "GeoId", ri.GeoId);
				writer.WriteLine ("{0}: {1}", "IsMetric", ri.IsMetric);
				writer.WriteLine ("{0}: {1}", "ISOCurrencySymbol", ri.ISOCurrencySymbol);
				writer.WriteLine ("{0}: {1}", "Name", ri.Name);
				writer.WriteLine ("{0}: {1}", "NativeName", ri.NativeName);
				writer.WriteLine ("{0}: {1}", "ThreeLetterISORegionName", ri.ThreeLetterISORegionName);
				writer.WriteLine ("{0}: {1}", "ThreeLetterWindowsRegionName", ri.ThreeLetterWindowsRegionName);
				writer.WriteLine ("{0}: {1}", "TwoLetterISORegionName", ri.TwoLetterISORegionName);
			}

			CompareInfo ci = CompareInfo.GetCompareInfo (c.LCID);
			writer.WriteLine ("-- CompareInfo --");
			writer.WriteLine ("{0}: {1}", "Name", ci.Name);
			//writer.WriteLine ("{0}: {1}", "Version", ci.Version);

			writer.WriteLine ();
		}
	}

	static void Dump<T> (TextWriter tw, T[] values, string name)
	{
		tw.Write (name);
		tw.Write (": ");

		for (int i = 0; i < values.Length; ++i) {
			if (i > 0)
				tw.Write (", ");

			tw.Write (values[i]);
		}

		tw.WriteLine ();
	}
}
