//
// Driver.cs
//
// Authors:
//  Jackson Harper (jackson@ximian.com)
//  Atsushi Enomoto (atsushi@ximian.com)
//	Marek Safar  <marek.safar@gmail.com>
//
// (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Text;
using System.Xml;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace Mono.Tools.LocaleBuilder
{
	public class Driver
	{
		static readonly string data_root = Path.Combine ("CLDR", "common");

		public static void Main (string[] args)
		{
			Driver d = new Driver ();
			ParseArgs (args, d);
			d.Run ();
		}

		private static void ParseArgs (string[] args, Driver d)
		{
			for (int i = 0; i < args.Length; i++) {
				if (args[i] == "--lang" && i + 1 < args.Length)
					d.Lang = args[++i];
				else if (args[i] == "--locales" && i + 1 < args.Length)
					d.Locales = args[++i];
				else if (args[i] == "--header" && i + 1 < args.Length)
					d.HeaderFileName = args[++i];
				else if (args[i] == "--compare")
					d.OutputCompare = true;
			}
		}

		private string lang;
		private string locales;
		private string header_name;
		List<CultureInfoEntry> cultures;
		Dictionary<string, string> region_currency;
		Dictionary<string, string> currency_fractions;

		// The lang is the language that display names will be displayed in
		public string Lang
		{
			get
			{
				if (lang == null)
					lang = "en";
				return lang;
			}
			set { lang = value; }
		}

		public string Locales
		{
			get { return locales; }
			set { locales = value; }
		}

		public string HeaderFileName
		{
			get
			{
				if (header_name == null)
					return "culture-info-tables.h";
				return header_name;
			}
			set { header_name = value; }
		}

		public bool OutputCompare { get; set; }

		void Print ()
		{
			cultures.Sort ((a, b) => int.Parse (a.LCID.Substring (2), NumberStyles.HexNumber).CompareTo (int.Parse (b.LCID.Substring (2), NumberStyles.HexNumber)));

			var writer = Console.Out;

			foreach (var c in cultures) {
				writer.WriteLine ("Name: {0}, LCID {1}", c.OriginalName, c.LCID);

				writer.WriteLine ("{0}: {1}", "DisplayName", c.DisplayName);
				writer.WriteLine ("{0}: {1}", "EnglishName", c.EnglishName);
				writer.WriteLine ("{0}: {1}", "NativeName", c.NativeName);
				// writer.WriteLine ("{0}: {1}", "OptionalCalendars", c.OptionalCalendars);
				writer.WriteLine ("{0}: {1}", "ThreeLetterISOLanguageName", c.ThreeLetterISOLanguageName);
				writer.WriteLine ("{0}: {1}", "ThreeLetterWindowsLanguageName", c.ThreeLetterWindowsLanguageName);
				writer.WriteLine ("{0}: {1}", "TwoLetterISOLanguageName", c.TwoLetterISOLanguageName);
				writer.WriteLine ("{0}: {1}", "Calendar", GetCalendarType (c.CalendarType));

				var df = c.DateTimeFormatEntry;
				writer.WriteLine ("-- DateTimeFormat --");
				Dump (writer, df.AbbreviatedDayNames, "AbbreviatedDayNames");
				Dump (writer, df.AbbreviatedMonthGenitiveNames, "AbbreviatedMonthGenitiveNames");
				Dump (writer, df.AbbreviatedMonthNames, "AbbreviatedMonthNames");
				writer.WriteLine ("{0}: {1}", "AMDesignator", df.AMDesignator);
				writer.WriteLine ("{0}: {1}", "CalendarWeekRule", (CalendarWeekRule) df.CalendarWeekRule);
				writer.WriteLine ("{0}: {1}", "DateSeparator", df.DateSeparator);
				Dump (writer, df.DayNames, "DayNames");
				writer.WriteLine ("{0}: {1}", "FirstDayOfWeek", (DayOfWeek) df.FirstDayOfWeek);
//				Dump (writer, df.GetAllDateTimePatterns (), "GetAllDateTimePatterns");
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

				var ti = c.TextInfoEntry;
				writer.WriteLine ("-- TextInfo --");
				writer.WriteLine ("{0}: {1}", "ANSICodePage", ti.ANSICodePage);
				writer.WriteLine ("{0}: {1}", "EBCDICCodePage", ti.EBCDICCodePage);
				writer.WriteLine ("{0}: {1}", "IsRightToLeft", ti.IsRightToLeft);
				writer.WriteLine ("{0}: {1}", "ListSeparator", ti.ListSeparator);
				writer.WriteLine ("{0}: {1}", "MacCodePage", ti.MacCodePage);
				writer.WriteLine ("{0}: {1}", "OEMCodePage", ti.OEMCodePage);

				var nf = c.NumberFormatEntry;
				writer.WriteLine ("-- NumberFormat --");
				writer.WriteLine ("{0}: {1}", "CurrencyDecimalDigits", nf.CurrencyDecimalDigits);
				writer.WriteLine ("{0}: {1}", "CurrencyDecimalSeparator", nf.CurrencyDecimalSeparator);
				writer.WriteLine ("{0}: {1}", "CurrencyGroupSeparator", nf.CurrencyGroupSeparator);
				Dump (writer, nf.CurrencyGroupSizes, "CurrencyGroupSizes", true);
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
				Dump (writer, nf.NumberGroupSizes, "NumberGroupSizes", true);
				writer.WriteLine ("{0}: {1}", "NumberNegativePattern", nf.NumberNegativePattern);
				writer.WriteLine ("{0}: {1}", "PercentDecimalDigits", nf.PercentDecimalDigits);
				writer.WriteLine ("{0}: {1}", "PercentDecimalSeparator", nf.PercentDecimalSeparator);
				writer.WriteLine ("{0}: {1}", "PercentGroupSeparator", nf.PercentGroupSeparator);
				Dump (writer, nf.PercentGroupSizes, "PercentGroupSizes", true);
				writer.WriteLine ("{0}: {1}", "PercentNegativePattern", nf.PercentNegativePattern);
				writer.WriteLine ("{0}: {1}", "PercentPositivePattern", nf.PercentPositivePattern);
				writer.WriteLine ("{0}: {1}", "PercentSymbol", nf.PercentSymbol);
				writer.WriteLine ("{0}: {1}", "PerMilleSymbol", nf.PerMilleSymbol);
				writer.WriteLine ("{0}: {1}", "PositiveInfinitySymbol", nf.PositiveInfinitySymbol);
				writer.WriteLine ("{0}: {1}", "PositiveSign", nf.PositiveSign);

				if (c.RegionInfoEntry != null) {
					var ri = c.RegionInfoEntry;
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

				writer.WriteLine ();
			}
		}

		static Type GetCalendarType (CalendarType ct)
		{
			switch (ct) {
			case CalendarType.Gregorian:
				return typeof (GregorianCalendar);
			case CalendarType.HijriCalendar:
				return typeof (HijriCalendar);
			case CalendarType.ThaiBuddhist:
				return typeof (ThaiBuddhistCalendar);
			case CalendarType.UmAlQuraCalendar:
				return typeof (UmAlQuraCalendar);
			default:
				throw new NotImplementedException ();
			}
		}

		static void Dump<T> (TextWriter tw, IList<T> values, string name, bool stopOnNull = false) where T : class
		{
			tw.Write (name);
			tw.Write (": ");

			for (int i = 0; i < values.Count; ++i) {
				var v = values[i];

				if (stopOnNull && v == null)
					break;

				if (i > 0)
					tw.Write (", ");

				tw.Write (v);
			}

			tw.WriteLine ();
		}

		void Run ()
		{
			Regex locales_regex = null;
			if (Locales != null)
				locales_regex = new Regex (Locales);

			cultures = new List<CultureInfoEntry> ();
			var regions = new List<RegionInfoEntry> ();


			var supplemental = GetXmlDocument (Path.Combine (data_root, "supplemental", "supplementalData.xml"));

			// Read currencies info
			region_currency = new Dictionary<string, string> (StringComparer.OrdinalIgnoreCase);
			foreach (XmlNode entry in supplemental.SelectNodes ("supplementalData/currencyData/region")) {
				var child = entry.SelectSingleNode ("currency");
				region_currency.Add (entry.Attributes["iso3166"].Value, child.Attributes["iso4217"].Value);
			}

			var lcdids = GetXmlDocument ("lcids.xml");
			foreach (XmlNode lcid in lcdids.SelectNodes ("lcids/lcid")) {
				var name = lcid.Attributes["name"].Value;

				if (locales_regex != null && !locales_regex.IsMatch (name))
					continue;

				var ci = new CultureInfoEntry ();
				ci.LCID = lcid.Attributes["id"].Value;
				ci.ParentLcid = lcid.Attributes["parent"].Value;
				ci.TwoLetterISOLanguageName = lcid.Attributes["iso2"].Value;
				ci.ThreeLetterISOLanguageName = lcid.Attributes["iso3"].Value;
				ci.ThreeLetterWindowsLanguageName = lcid.Attributes["win"].Value;
				ci.OriginalName = name.Replace ('_', '-');
				ci.TextInfoEntry = new TextInfoEntry ();
				ci.NumberFormatEntry = new NumberFormatEntry ();

				if (!Import (ci, name))
					continue;

				cultures.Add (ci);
			}

			var doc_english = GetXmlDocument (Path.Combine (data_root, "main", "en.xml"));

			//
			// Fill all EnglishName values from en.xml language file
			//
			foreach (var ci in cultures) {
				var el = doc_english.SelectSingleNode (string.Format ("ldml/localeDisplayNames/languages/language[@type='{0}']", ci.Language));
				if (el != null)
					ci.EnglishName = el.InnerText;

				string s = null;
				if (ci.Script != null) {
					el = doc_english.SelectSingleNode (string.Format ("ldml/localeDisplayNames/scripts/script[@type='{0}']", ci.Script));
					if (el != null)
						s = el.InnerText;
				}

				if (ci.Territory != null) {
					el = doc_english.SelectSingleNode (string.Format ("ldml/localeDisplayNames/territories/territory[@type='{0}']", ci.Territory));
					if (el != null) {
						if (s == null)
							s = el.InnerText;
						else
							s = string.Join (", ", s, el.InnerText);
					}
				}

				switch (ci.ThreeLetterWindowsLanguageName) {
				case "CHT":
					s = "Traditional";
					break;
				case "CHS":
					s = "Simplified";
					break;
				}

				if (s != null)
					ci.EnglishName = string.Format ("{0} ({1})", ci.EnglishName, s);

				// Special case legacy chinese
				if (ci.OriginalName == "zh-CHS" || ci.OriginalName == "zh-CHT")
					ci.EnglishName += " Legacy";

				// Mono is not localized and supports english only, hence the name will always be same
				ci.DisplayName = ci.EnglishName;
			}

			//
			// Fill culture hierarchy for easier data manipulation
			//
			foreach (var ci in cultures) {
				foreach (var p in cultures.Where (l => ci.LCID == l.ParentLcid)) {
					ci.Children.Add (p);
				}
			}

			currency_fractions = new Dictionary<string, string> (StringComparer.OrdinalIgnoreCase);
			foreach (XmlNode entry in supplemental.SelectNodes ("supplementalData/currencyData/fractions/info")) {
				currency_fractions.Add (entry.Attributes["iso4217"].Value, entry.Attributes["digits"].Value);
			}

			var territory2dayofweek = new Dictionary<string, DayOfWeek> (StringComparer.OrdinalIgnoreCase);
			foreach (XmlNode entry in supplemental.SelectNodes ("supplementalData/weekData/firstDay")) {
				DayOfWeek dow;

				switch (entry.Attributes["day"].Value) {
				case "mon":
					dow = DayOfWeek.Monday;
					break;
				case "fri":
					dow = DayOfWeek.Friday;
					break;
				case "sat":
					dow = DayOfWeek.Saturday;
					break;
				case "sun":
					dow = DayOfWeek.Sunday;
					break;
				default:
					throw new NotImplementedException ();
				}

				var territories = entry.Attributes["territories"].Value.Split ();
				foreach (var t in territories)
					territory2dayofweek[t] = dow;
			}

			var territory2wr = new Dictionary<string, CalendarWeekRule> (StringComparer.OrdinalIgnoreCase);
			foreach (XmlNode entry in supplemental.SelectNodes ("supplementalData/weekData/minDays")) {
				CalendarWeekRule rule;

				switch (entry.Attributes["count"].InnerText) {
				case "1":
					rule = CalendarWeekRule.FirstDay;
					break;
				case "4":
					rule = CalendarWeekRule.FirstFourDayWeek;
					break;
				default:
					throw new NotImplementedException ();
				}

				var territories = entry.Attributes["territories"].InnerText.Split ();
				foreach (var t in territories)
					territory2wr[t] = rule;
			}

			//
			// Fill all territory speficic data where territory is available
			//
			var non_metric = new HashSet<string> ();
			foreach (XmlNode entry in supplemental.SelectNodes ("supplementalData/measurementData/measurementSystem[@type='US']")) {
				var territories = entry.Attributes["territories"].InnerText.Split ();
				foreach (var t in territories)
					non_metric.Add (t);
			}

			foreach (var ci in cultures) {
				if (ci.Territory == null)
					continue;

				DayOfWeek value;
				if (territory2dayofweek.TryGetValue (ci.Territory, out value)) {
					ci.DateTimeFormatEntry.FirstDayOfWeek = (int) value;
				}

				CalendarWeekRule rule;
				if (territory2wr.TryGetValue (ci.Territory, out rule)) {
					ci.DateTimeFormatEntry.CalendarWeekRule = (int) rule;
				}

				string fraction_value;
				if (currency_fractions.TryGetValue (ci.Territory, out fraction_value)) {
					ci.NumberFormatEntry.CurrencyDecimalDigits = fraction_value;
				}

				RegionInfoEntry region = regions.Where (l => l.Name == ci.Territory).FirstOrDefault ();
				if (region == null) {
					region = new RegionInfoEntry () {
						CurrencySymbol = ci.NumberFormatEntry.CurrencySymbol,
						EnglishName = ci.EnglishName,
						NativeName = ci.NativeTerritoryName,
						Name = ci.Territory,
						TwoLetterISORegionName = ci.Territory,
						CurrencyNativeName = ci.NativeCurrencyName
					};

					var tc = supplemental.SelectSingleNode (string.Format ("supplementalData/codeMappings/territoryCodes[@type='{0}']", ci.Territory));
					region.ThreeLetterISORegionName = tc.Attributes["alpha3"].Value;
					region.ThreeLetterWindowsRegionName = region.ThreeLetterISORegionName;

					var el = doc_english.SelectSingleNode (string.Format ("ldml/localeDisplayNames/territories/territory[@type='{0}']", ci.Territory));
					region.EnglishName = el.InnerText;
					region.DisplayName = region.EnglishName;

					region.ISOCurrencySymbol = region_currency[ci.Territory];

					el = doc_english.SelectSingleNode (string.Format ("ldml/numbers/currencies/currency[@type='{0}']/displayName", region.ISOCurrencySymbol));
					region.CurrencyEnglishName = el.InnerText;

					if (non_metric.Contains (ci.Territory))
						region.IsMetric = false;

					var lcdid_value = int.Parse (ci.LCID.Substring (2), NumberStyles.HexNumber);
					Patterns.FillValues (lcdid_value, region);
					regions.Add (region);
				}

				ci.RegionInfoEntry = region;
			}

			//
			// Fill neutral cultures territory data
			//
			foreach (var ci in cultures) {
				var dtf = ci.DateTimeFormatEntry;
				if (dtf.FirstDayOfWeek == null) {
					switch (ci.Name) {
					case "ar":
						dtf.FirstDayOfWeek = (int) DayOfWeek.Saturday;
						break;
					case "en":
					case "pt":
					case "zh-Hans":
						dtf.FirstDayOfWeek = (int) DayOfWeek.Sunday;
						break;
					case "es":
					case "fr":
					case "bn":
					case "sr-Cyrl":
					case "sr-Latn":
						dtf.FirstDayOfWeek = (int) DayOfWeek.Monday;
						break;
					default:
						List<int?> all_fdow = new List<int?> ();
						GetAllChildrenValues (ci, all_fdow, l => l.DateTimeFormatEntry.FirstDayOfWeek);
						var children = all_fdow.Where (l => l != null).Distinct ().ToList ();

						if (children.Count == 1) {
							dtf.FirstDayOfWeek = children[0];
						} else if (children.Count == 0) {
							if (!ci.HasMissingLocale)
								Console.WriteLine ("No week data for `{0}'", ci.Name);

							// Default to Sunday
							dtf.FirstDayOfWeek = (int) DayOfWeek.Sunday;
						} else {
							// .NET has weird concept of territory data available for neutral cultures (e.g. en, es, pt)
							// We have to manually disambiguate the correct entry (which is artofficial anyway)
							throw new ApplicationException (string.Format ("Ambiguous week data for `{0}'", ci.Name));
						}

						break;
					}
				}

				if (dtf.CalendarWeekRule == null) {
					switch (ci.Name) {
					case "ar":
					case "en":
					case "es":
					case "zh-Hans":
					case "pt":
					case "fr":
					case "bn":
						dtf.CalendarWeekRule = (int) CalendarWeekRule.FirstDay;
						break;
					default:
						List<int?> all_cwr = new List<int?> ();
						GetAllChildrenValues (ci, all_cwr, l => l.DateTimeFormatEntry.CalendarWeekRule);
						var children = all_cwr.Where (l => l != null).Distinct ().ToList ();

						if (children.Count == 1) {
							dtf.CalendarWeekRule = children[0];
						} else if (children.Count == 0) {
							if (!ci.HasMissingLocale)
								Console.WriteLine ("No calendar week data for `{0}'", ci.Name);


							// Default to FirstDay
							dtf.CalendarWeekRule = (int) CalendarWeekRule.FirstDay;
						} else {
							// .NET has weird concept of territory data available for neutral cultures (e.g. en, es, pt)
							// We have to manually disambiguate the correct entry (which is artofficial anyway)
							throw new ApplicationException (string.Format ("Ambiguous calendar data for `{0}'", ci.Name));
						}

						break;
					}
				}

				var nfe = ci.NumberFormatEntry;
				if (nfe.CurrencySymbol == null) {
					switch (ci.Name) {
					case "ar":
						nfe.CurrencySymbol = "ر.س.‏";
						break;
					case "en":
						nfe.CurrencySymbol = "$";
						break;
					case "es":
					case "fr":
						nfe.CurrencySymbol = "€";
						break;
					case "pt":
						nfe.CurrencySymbol = "R$";
						break;
					case "sv":
						nfe.CurrencySymbol = "kr";
						break;
					case "ms":
						nfe.CurrencySymbol = "RM";
						break;
					case "bn":
						nfe.CurrencySymbol = "টা";
						break;
					case "sr-Cyrl":
						nfe.CurrencySymbol = "Дин.";
						break;
					case "sr-Latn":
					case "sr":
						nfe.CurrencySymbol = "Din.";
						break;
					case "zh":
						nfe.CurrencySymbol = "¥";
						break;
					case "zh-Hant":
						nfe.CurrencySymbol = "HK$";
						break;
						
					default:
						var all_currencies = new List<string> ();
						GetAllChildrenValues (ci, all_currencies, l => l.NumberFormatEntry.CurrencySymbol);
						var children = all_currencies.Where (l => l != null).Distinct ().ToList ();

						if (children.Count == 1) {
							nfe.CurrencySymbol = children[0];
						} else if (children.Count == 0) {
							if (!ci.HasMissingLocale)
								Console.WriteLine ("No currency data for `{0}'", ci.Name);


						} else {
							// .NET has weird concept of territory data available for neutral cultures (e.g. en, es, pt)
							// We have to manually disambiguate the correct entry (which is artofficial anyway)
							throw new ApplicationException (string.Format ("Ambiguous currency data for `{0}'", ci.Name));
						}

						break;
					}
				}

				if (nfe.CurrencyDecimalDigits == null) {
					var all_digits = new List<string> ();
					GetAllChildrenValues (ci, all_digits, l => l.NumberFormatEntry.CurrencyDecimalDigits);
					var children = all_digits.Where (l => l != null).Distinct ().ToList ();

					if (children.Count == 1) {
						nfe.CurrencyDecimalDigits = children[0];
					} else if (children.Count == 0) {
						if (!ci.HasMissingLocale)
							Console.WriteLine ("No currency decimal digits data for `{0}'", ci.Name);

						nfe.CurrencyDecimalDigits = "2";
					} else {
						// .NET has weird concept of territory data available for neutral cultures (e.g. en, es, pt)
						// We have to manually disambiguate the correct entry (which is artofficial anyway)
						throw new ApplicationException (string.Format ("Ambiguous currency decimal digits data for `{0}'", ci.Name));
					}
				}
			}

			if (OutputCompare)
				Print ();

			regions.Sort (new RegionComparer ());
			for (int i = 0; i < regions.Count; ++i)
				regions[i].Index = i;

			/**
			 * Dump each table individually. Using StringBuilders
			 * because it is easier to debug, should switch to just
			 * writing to streams eventually.
			 */
			using (StreamWriter writer = new StreamWriter (HeaderFileName, false, new UTF8Encoding (false, true))) {
				writer.NewLine = "\n";
				writer.WriteLine ();
				writer.WriteLine ("/* This is a generated file. Do not edit. See tools/locale-builder. */");
				writer.WriteLine ("#ifndef MONO_METADATA_CULTURE_INFO_TABLES");
				writer.WriteLine ("#define MONO_METADATA_CULTURE_INFO_TABLES 1");
				writer.WriteLine ("\n");

				writer.WriteLine ("#define NUM_CULTURE_ENTRIES {0}", cultures.Count);
				writer.WriteLine ("#define NUM_REGION_ENTRIES {0}", regions.Count);

				writer.WriteLine ("\n");

				// Sort the cultures by lcid
				cultures.Sort (new LcidComparer ());

				StringBuilder builder = new StringBuilder ();
				int row = 0;
				int count = cultures.Count;
				for (int i = 0; i < count; i++) {
					CultureInfoEntry ci = cultures[i];
					if (ci.DateTimeFormatEntry == null)
						continue;
					ci.DateTimeFormatEntry.AppendTableRow (builder);
					ci.DateTimeFormatEntry.Row = row++;
					if (i + 1 < count)
						builder.Append (',');
					builder.Append ('\n');
				}

				writer.WriteLine ("static const DateTimeFormatEntry datetime_format_entries [] = {");
				writer.Write (builder);
				writer.WriteLine ("};\n\n");

				builder = new StringBuilder ();
				row = 0;
				for (int i = 0; i < count; i++) {
					CultureInfoEntry ci = cultures[i];
					if (ci.NumberFormatEntry == null)
						continue;
					ci.NumberFormatEntry.AppendTableRow (builder);
					ci.NumberFormatEntry.Row = row++;
					if (i + 1 < count)
						builder.Append (',');
					builder.Append ('\n');
				}

				writer.WriteLine ("static const NumberFormatEntry number_format_entries [] = {");
				writer.Write (builder);
				writer.WriteLine ("};\n\n");

				builder = new StringBuilder ();
				row = 0;
				for (int i = 0; i < count; i++) {
					CultureInfoEntry ci = cultures[i];
					ci.AppendTableRow (builder);
					ci.Row = row++;
					if (i + 1 < count)
						builder.Append (',');
					builder.Append ('\n');
				}

				writer.WriteLine ("static const CultureInfoEntry culture_entries [] = {");
				writer.Write (builder);
				writer.WriteLine ("};\n\n");

				cultures.Sort (new ExportNameComparer ()); // Sort based on name
				builder = new StringBuilder ();
				for (int i = 0; i < count; i++) {
					CultureInfoEntry ci = cultures[i];
					var name = ci.GetExportName ().ToLowerInvariant ();
					builder.Append ("\t{" + Entry.EncodeStringIdx (name) + ", ");
					builder.Append (ci.Row + "}");
					if (i + 1 < count)
						builder.Append (',');

					builder.AppendFormat ("\t /* {0} */", name);
					builder.Append ('\n');
				}

				writer.WriteLine ("static const CultureInfoNameEntry culture_name_entries [] = {");
				writer.Write (builder);
				writer.WriteLine ("};\n\n");

				builder = new StringBuilder ();
				int rcount = 0;
				foreach (RegionInfoEntry r in regions) {
					r.AppendTableRow (builder);
					if (++rcount != regions.Count)
						builder.Append (',');

					builder.Append ('\n');
				}
				writer.WriteLine ("static const RegionInfoEntry region_entries [] = {");
				writer.Write (builder);
				writer.WriteLine ("};\n\n");

				builder = new StringBuilder ();
				rcount = 0;
				foreach (RegionInfoEntry ri in regions) {
					builder.Append ("\t{" + Entry.EncodeStringIdx (ri.TwoLetterISORegionName) + ", ");
					builder.Append (ri.Index + "}");
					if (++rcount != regions.Count)
						builder.Append (',');
					
					builder.AppendFormat ("\t /* {0} */", ri.TwoLetterISORegionName);
					builder.Append ('\n');
				}

				writer.WriteLine ("static const RegionInfoNameEntry region_name_entries [] = {");
				writer.Write (builder);
				writer.WriteLine ("};\n\n");

				writer.WriteLine ("static const char locale_strings [] = {");
				writer.Write (Entry.GetStrings ());
				writer.WriteLine ("};\n\n");

				writer.WriteLine ("#endif\n");
			}
		}

		static void GetAllChildrenValues<T> (CultureInfoEntry entry, List<T> values, Func<CultureInfoEntry, T> selector)
		{
			foreach (var e in entry.Children) {
				if (e == entry)
					continue;

				values.Add (selector (e));

				foreach (var e2 in e.Children) {
					GetAllChildrenValues (e2, values, selector);
				}
			}
		}

		static XmlDocument GetXmlDocument (string path)
		{
			var doc = new XmlDocument ();
			doc.Load (new XmlTextReader (path) { /*DtdProcessing = DtdProcessing.Ignore*/ } );
			return doc;
		}

		bool Import (CultureInfoEntry data, string locale)
		{
			string fname = null;
			var sep = locale.Split ('_');
			data.Language = sep[0];

			// CLDR strictly follow ISO names, .NET does not
			// Replace names where non-iso2 is used, e.g. Norway
			if (data.Language != data.TwoLetterISOLanguageName) {
				locale = data.TwoLetterISOLanguageName;
				if (sep.Length > 1) {
					locale += string.Join ("_", sep.Skip (1));
				}
			}

			// Convert broken Chinese names to correct one
			switch (locale) {
			case "zh_CHS":
				locale = "zh_Hans";
				break;
			case "zh_CHT":
				locale = "zh_Hant";
				break;
			case "zh_CN":
				locale = "zh_Hans_CN";
				break;
			case "zh_HK":
				locale = "zh_Hant_HK";
				break;
			case "zh_SG":
				locale = "zh_Hans_SG";
				break;
			case "zh_TW":
				locale = "zh_Hant_TW";
				break;
			case "zh_MO":
				locale = "zh_Hant_MO";
				break;
			}

			sep = locale.Split ('_');

			string full_name = Path.Combine (data_root, "main", locale + ".xml");
			if (!File.Exists (full_name)) {
				Console.WriteLine ("Missing locale file for `{0}'", locale);

				// We could fill default values but that's not as simple as it seems. For instance for non-neutral
				// cultures the next part could be territory or not.
				return false;
			} else {
				XmlDocument doc = null;

				/*
				 * Locale generation is done in several steps, first we
				 * read the root file which is the base invariant data
				 * then the supplemental root data, 
				 * then the language file, the supplemental languages
				 * file then the locale file, then the supplemental
				 * locale file. Values in each descending file can
				 * overwrite previous values.
				 */
				foreach (var part in sep) {
					if (fname != null)
						fname += "_";

					fname += part;

					var xml = GetXmlDocument (Path.Combine (data_root, "main", fname + ".xml"));
					if (doc == null)
						doc = xml;

					Import (xml, data);
				}

				//
				// Extract localized locale name from language xml file. Have to do it after both language and territory are read
				//
				var el = doc.SelectSingleNode (string.Format ("ldml/localeDisplayNames/languages/language[@type='{0}']", data.Language));
				if (el != null)
					data.NativeName = el.InnerText;

				if (data.Territory != null) {
					el = doc.SelectSingleNode (string.Format ("ldml/localeDisplayNames/territories/territory[@type='{0}']", data.Territory));
					if (el != null) {
						// TODO: Should read <localePattern>
						data.NativeName = string.Format ("{0} ({1})", data.NativeName, el.InnerText);
						data.NativeTerritoryName = el.InnerText;
					}

					string currency;
					// We have territory now we have to run the process again to extract currency symbol
					if (region_currency.TryGetValue (data.Territory, out currency)) {
						fname = null;

						var xml = GetXmlDocument (Path.Combine (data_root, "main", "root.xml"));
						el = xml.SelectSingleNode (string.Format ("ldml/numbers/currencies/currency[@type='{0}']/symbol", currency));
						if (el != null)
							data.NumberFormatEntry.CurrencySymbol = el.InnerText;

						foreach (var part in sep) {
							if (fname != null)
								fname += "_";

							fname += part;

							xml = GetXmlDocument (Path.Combine (data_root, "main", fname + ".xml"));
							el = xml.SelectSingleNode (string.Format ("ldml/numbers/currencies/currency[@type='{0}']/symbol", currency));
							if (el != null)
								data.NumberFormatEntry.CurrencySymbol = el.InnerText;

							el = xml.SelectSingleNode (string.Format ("ldml/numbers/currencies/currency[@type='{0}']/displayName", currency));
							if (el != null)
								data.NativeCurrencyName = el.InnerText;
						}
					}
				}

				if (data.DateTimeFormatEntry.MonthGenitiveNames[0] == null)
					data.DateTimeFormatEntry.MonthGenitiveNames = data.DateTimeFormatEntry.MonthNames;

				if (data.DateTimeFormatEntry.AbbreviatedMonthGenitiveNames[0] == null)
					data.DateTimeFormatEntry.AbbreviatedMonthGenitiveNames = data.DateTimeFormatEntry.AbbreviatedMonthNames;


			}

			// It looks like it never changes
			data.DateTimeFormatEntry.TimeSeparator = ":";

			// TODO: Don't have input data available but most values are 2 with few exceptions for 1 and 3
			// We don't add 3 as it's for some arabic states only
			switch (data.ThreeLetterISOLanguageName) {
			case "amh":
				data.NumberFormatEntry.NumberDecimalDigits =
				data.NumberFormatEntry.PercentDecimalDigits = 1;
				break;
			default:
				data.NumberFormatEntry.NumberDecimalDigits =
				data.NumberFormatEntry.PercentDecimalDigits = 2;
				break;
			}

			// TODO: For now we capture only native name for default calendar
			data.NativeCalendarNames[((int) data.CalendarType & 0xFF) - 1] = data.DateTimeFormatEntry.NativeCalendarName;

			var lcdid_value = int.Parse (data.LCID.Substring (2), NumberStyles.HexNumber);
			Patterns.FillValues (lcdid_value, data);

			return true;
		}

		void Import (XmlDocument doc, CultureInfoEntry ci)
		{
			XmlNodeList nodes;
			XmlNode el;

			//
			// Extract script & teritory
			//
			el = doc.SelectSingleNode ("ldml/identity/script");
			if (el != null)
				ci.Script = el.Attributes["type"].Value;

			el = doc.SelectSingleNode ("ldml/identity/territory");
			if (el != null)
				ci.Territory = el.Attributes["type"].Value;

			var df = ci.DateTimeFormatEntry;

			string calendar;
			// Default calendar is for now always "gregorian"
			switch (ci.Name) {
			case "th": case "th-TH":
				calendar = "buddhist";
				ci.CalendarType = CalendarType.ThaiBuddhist; // typeof (ThaiBuddhistCalendar);
				break;
			case "ar": case "ar-SA":
				calendar = "islamic";
				ci.CalendarType = CalendarType.UmAlQuraCalendar; // typeof (UmAlQuraCalendar);
				break;
			case "ps": case "ps-AF": case "prs": case "prs-AF": case "dv": case "dv-MV":
				calendar = "persian";
				ci.CalendarType = CalendarType.HijriCalendar; // typeof (HijriCalendar);
				break;
			default:
				calendar = "gregorian";
				ci.CalendarType = CalendarType.Gregorian; // typeof (GregorianCalendar);
				ci.GregorianCalendarType = GregorianCalendarTypes.Localized;
				break;
			}

			var node = doc.SelectSingleNode (string.Format ("ldml/dates/calendars/calendar[@type='{0}']", calendar));
			if (node != null) {
				el = doc.SelectSingleNode (string.Format ("ldml/localeDisplayNames/types/type[@type='{0}']", calendar));
				if (el != null)
					df.NativeCalendarName = el.InnerText;


				// Apply global rule first <alias source="locale" path="../../monthContext[@type='format']/monthWidth[@type='wide']"/>
				nodes = node.SelectNodes ("months/monthContext[@type='format']/monthWidth[@type='wide']/month");
				ProcessAllNodes (nodes, df.MonthNames, AddOrReplaceValue);
				nodes = node.SelectNodes ("months/monthContext[@type='stand-alone']/monthWidth[@type='wide']/month");
				ProcessAllNodes (nodes, df.MonthNames, AddOrReplaceValue);

				// Apply global rule first <alias source="locale" path="../../monthContext[@type='format']/monthWidth[@type='abbreviated']"/>
				nodes = node.SelectNodes ("months/monthContext[@type='format']/monthWidth[@type='abbreviated']/month");
				ProcessAllNodes (nodes, df.AbbreviatedMonthNames, AddOrReplaceValue);
				nodes = node.SelectNodes ("months/monthContext[@type='stand-alone']/monthWidth[@type='abbreviated']/month");
				ProcessAllNodes (nodes, df.AbbreviatedMonthNames, AddOrReplaceValue);

				nodes = node.SelectNodes ("months/monthContext[@type='format']/monthWidth[@type='wide']/month");
				if (nodes != null)
					ProcessAllNodes (nodes, df.MonthGenitiveNames, AddOrReplaceValue);

				nodes = node.SelectNodes ("days/dayContext[@type='format']/dayWidth[@type='wide']/day");
				ProcessAllNodes (nodes, df.DayNames, AddOrReplaceDayValue);

				// Apply global rule first <alias source="locale" path="../../dayContext[@type='format']/dayWidth[@type='abbreviated']"/>
				nodes = node.SelectNodes ("days/dayContext[@type='format']/dayWidth[@type='abbreviated']/day");
				ProcessAllNodes (nodes, df.AbbreviatedDayNames, AddOrReplaceDayValue);
				nodes = node.SelectNodes ("days/dayContext[@type='stand-alone']/dayWidth[@type='abbreviated']/day");
				ProcessAllNodes (nodes, df.AbbreviatedDayNames, AddOrReplaceDayValue);

				// TODO: This is not really ShortestDayNames as .NET uses it
				// Apply global rules first <alias source="locale" path="../../dayContext[@type='stand-alone']/dayWidth[@type='narrow']"/>
				nodes = node.SelectNodes ("days/dayContext[@type='format']/dayWidth[@type='narrow']/day");
				ProcessAllNodes (nodes, df.ShortestDayNames, AddOrReplaceDayValue);
				nodes = node.SelectNodes ("days/dayContext[@type='stand-alone']/dayWidth[@type='narrow']/day");
				ProcessAllNodes (nodes, df.ShortestDayNames, AddOrReplaceDayValue);
/*
				Cannot really be used it's too different to .NET and most app rely on it
 
				el = node.SelectSingleNode ("dateFormats/dateFormatLength[@type='full']/dateFormat/pattern");
				if (el != null)
					df.LongDatePattern = ConvertDatePatternFormat (el.InnerText);

				// Medium is our short
				el = node.SelectSingleNode ("dateFormats/dateFormatLength[@type='medium']/dateFormat/pattern");
				if (el != null)
					df.ShortDatePattern = ConvertDatePatternFormat (el.InnerText);

				// Medium is our Long
				el = node.SelectSingleNode ("timeFormats/timeFormatLength[@type='medium']/timeFormat/pattern");
				if (el != null)
					df.LongTimePattern = ConvertTimePatternFormat (el.InnerText);

				el = node.SelectSingleNode ("timeFormats/timeFormatLength[@type='short']/timeFormat/pattern");
				if (el != null)
					df.ShortTimePattern = ConvertTimePatternFormat (el.InnerText);

				el = node.SelectSingleNode ("dateTimeFormats/availableFormats/dateFormatItem[@id='yyyyMMMM']");
				if (el != null)
					df.YearMonthPattern = ConvertDatePatternFormat (el.InnerText);

				el = node.SelectSingleNode ("dateTimeFormats/availableFormats/dateFormatItem[@id='MMMMdd']");
				if (el != null)
					df.MonthDayPattern = ConvertDatePatternFormat (el.InnerText);
*/
				el = node.SelectSingleNode ("dayPeriods/dayPeriodContext/dayPeriodWidth[@type='abbreviated']/dayPeriod[@type='am']");
				if (el == null)
					// Apply global rule first <alias source="locale" path="../dayPeriodWidth[@type='wide']"/>
					el = node.SelectSingleNode ("dayPeriods/dayPeriodContext/dayPeriodWidth[@type='wide']/dayPeriod[@type='am']");

				if (el != null)
					df.AMDesignator = el.InnerText;

				el = node.SelectSingleNode ("dayPeriods/dayPeriodContext/dayPeriodWidth[@type='abbreviated']/dayPeriod[@type='pm']");
				if (el == null)
					// Apply global rule first <alias source="locale" path="../dayPeriodWidth[@type='wide']"/>
					el = node.SelectSingleNode ("dayPeriods/dayPeriodContext/dayPeriodWidth[@type='wide']/dayPeriod[@type='pm']");

				// No data
				if (el != null)
					df.PMDesignator = el.InnerText;
			}

			var ni = ci.NumberFormatEntry;

			node = doc.SelectSingleNode ("ldml/numbers/symbols");
			if (node != null) {
				el = node.SelectSingleNode ("decimal");
				if (el != null) {
					ni.NumberDecimalSeparator =
					ni.PercentDecimalSeparator =
					ni.CurrencyDecimalSeparator = el.InnerText;
				}

				el = node.SelectSingleNode ("plusSign");
				if (el != null)
					ni.PositiveSign = el.InnerText;

				el = node.SelectSingleNode ("minusSign");
				if (el != null)
					ni.NegativeSign = el.InnerText;

				el = node.SelectSingleNode ("infinity");

				// We cannot use the value from CLDR because many broken
				// .NET serializers (e.g. JSON) use text value of NegativeInfinity
				// and different value would break interoperability with .NET
				if (el != null && el.InnerText != "∞") {
					ni.InfinitySymbol = el.InnerText;
				}

				el = node.SelectSingleNode ("perMille");
				if (el != null)
					ni.PerMilleSymbol = el.InnerText;

				el = node.SelectSingleNode ("nan");
				if (el != null)
					ni.NaNSymbol = el.InnerText;

				el = node.SelectSingleNode ("percentSign");
				if (el != null)
					ni.PercentSymbol = el.InnerText;

				el = node.SelectSingleNode ("group");
				if (el != null) {
					ni.NumberGroupSeparator =
					ni.PercentGroupSeparator =
					ni.CurrencyGroupSeparator = el.InnerText;
				}
			}
		}

		static string ConvertDatePatternFormat (string format)
		{
			//
			// LDMR uses different characters for some fields
			// http://unicode.org/reports/tr35/#Date_Format_Patterns
			//
			format = format.Replace ("EEEE", "dddd"); // The full name of the day of the week
			format = format.Replace ("LLLL", "MMMM"); // The full month name

			if (format.EndsWith (" y", StringComparison.Ordinal))
				format += "yyy";

			return format;
		}

		static string ConvertTimePatternFormat (string format)
		{
			format = format.Replace ("a", "tt"); // AM or PM
			return format;
		}

		static void ProcessAllNodes (XmlNodeList list, IList<string> values, Action<IList<string>, string, string> convertor)
		{
			foreach (XmlNode entry in list) {
				var index = entry.Attributes["type"].Value;
				var value = entry.InnerText;
				convertor (values, index, value);
			}
		}

		// All text indexes are 1-based
		static void AddOrReplaceValue (IList<string> list, string oneBasedIndex, string value)
		{
			int index = int.Parse (oneBasedIndex);
			AddOrReplaceValue (list, index - 1, value);
		}

		static readonly string[] day_types = new string[] { "sun", "mon", "tue", "wed", "thu", "fri", "sat" };

		static void AddOrReplaceDayValue (IList<string> list, string dayType, string value)
		{
			int index = Array.IndexOf (day_types, dayType);
			AddOrReplaceValue (list, index, value);
		}

		static void AddOrReplaceValue (IList<string> list, int index, string value)
		{
			if (list.Count <= index)
				((List<string>) list).AddRange (new string[index - list.Count + 1]);

			list[index] = value;
		}

		sealed class LcidComparer : IComparer<CultureInfoEntry>
		{
			public int Compare (CultureInfoEntry x, CultureInfoEntry y)
			{
				return x.LCID.CompareTo (y.LCID);
			}
		}

		sealed class ExportNameComparer : IComparer<CultureInfoEntry>
		{
			public int Compare (CultureInfoEntry x, CultureInfoEntry y)
			{
				return String.Compare (x.GetExportName (), y.GetExportName (), StringComparison.OrdinalIgnoreCase);
			}
		}

		class RegionComparer : IComparer<RegionInfoEntry>
		{
			public int Compare (RegionInfoEntry x, RegionInfoEntry y)
			{
				return x.TwoLetterISORegionName.CompareTo (y.TwoLetterISORegionName);
			}
		}
	}
}
