//
// Mono.Tools.LocalBuilder.Driver
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//


using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Mono.Tools.LocaleBuilder {

	public class Driver {

		public static void Main (string [] args)
		{
			Driver d = new Driver ();
			ParseArgs (args, d);
			d.Run ();
		}

		private static void ParseArgs (string [] args, Driver d)
		{
			for (int i = 0; i < args.Length; i++) {
				if (args [i] == "--lang" && i+1 < args.Length)
					d.Lang = args [++i];
				else if (args [i] == "--locales" && i+1 < args.Length)
					d.Locales = args [++i];
                                else if (args [i] == "--header" && i + 1 < args.Length)
                                        d.HeaderFileName = args [++i];
			}
		}

		private string lang;
		private string locales;
                private string header_name;
                private ArrayList cultures;
                private Hashtable langs;
                private Hashtable currency_types;
                
		// The lang is the language that display names will be displayed in
		public string Lang {
			get {
				if (lang == null)
					lang = "en";
				return lang;
			}
			set { lang = value; }
		}

		public string Locales {
			get { return locales; }
			set { locales = value; }
		}

                public string HeaderFileName {
                        get {
                                if (header_name == null)
                                        return "culture-info-tables.h";
                                return header_name;
                        }
                        set { header_name = value; }
                }

		public void Run ()
		{
			Regex locales_regex = null;
			if (Locales != null)
				locales_regex = new Regex (Locales);

                        langs = new Hashtable ();
                        cultures = new ArrayList ();

                        LookupCurrencyTypes ();

			foreach (string file in Directory.GetFiles ("locales", "*.xml")) {
				string fn = Path.GetFileNameWithoutExtension (file);
				if (locales_regex == null || locales_regex.IsMatch (fn)) {
					ParseLocale (fn);
                                }
			}

                        /**
                         * Dump each table individually. Using StringBuilders
                         * because it is easier to debug, should switch to just
                         * writing to streams eventually.
                         */
                        using (StreamWriter writer = new StreamWriter (HeaderFileName, false, new UTF8Encoding (false, true))) {

                                writer.WriteLine ();
                                writer.WriteLine ("/* This is a generated file. Do not edit. See tools/locale-builder. */");
                                writer.WriteLine ("#ifndef MONO_METADATA_CULTURE_INFO_TABLES");
                                writer.WriteLine ("#define MONO_METADATA_CULTURE_INFO_TABLES 1");
                                writer.WriteLine ("\n");

                                writer.WriteLine ("#define NUM_CULTURE_ENTRIES " + cultures.Count);
                                writer.WriteLine ("\n");

                                // Sort the cultures by lcid
                                cultures.Sort (new LcidComparer ());

                                StringBuilder builder = new StringBuilder ();
                                int row = 0;
                                int count = cultures.Count;
                                for (int i = 0; i < count; i++) {
                                        CultureInfoEntry ci = (CultureInfoEntry) cultures [i];
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
                                for (int i=0; i < count; i++) {
                                        CultureInfoEntry ci = (CultureInfoEntry) cultures [i];
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
                                        CultureInfoEntry ci = (CultureInfoEntry) cultures [i];
                                        ci.AppendTableRow (builder);
                                        ci.Row = row++;
                                        if (i + 1 < count)
                                                builder.Append (',');
                                        builder.Append ('\n');
                                }
                                
                                writer.WriteLine ("static const CultureInfoEntry culture_entries [] = {");
                                writer.Write (builder);
                                writer.WriteLine ("};\n\n");

                                cultures.Sort (new NameComparer ()); // Sort based on name
                                builder = new StringBuilder ();
                                for (int i = 0; i < count; i++) {
                                        CultureInfoEntry ci = (CultureInfoEntry) cultures [i];
                                        builder.Append ("\t{" + Entry.EncodeStringIdx (ci.Name.ToLower ()) + ", ");
                                        builder.Append (ci.Row + "}");
                                        if (i + 1 < count)
                                                builder.Append (',');
                                        builder.Append ('\n');
                                }

                                writer.WriteLine ("static const CultureInfoNameEntry culture_name_entries [] = {");
                                writer.Write (builder);
                                writer.WriteLine ("};\n\n");

                                writer.WriteLine ("static const char locale_strings [] = {");
                                writer.Write (Entry.GetStrings ());
                                writer.WriteLine ("};\n\n");

                                writer.WriteLine ("#endif\n");
                        }
		}

		private XPathDocument GetXPathDocument (string path)
		{
			XmlTextReader xtr = new XmlTextReader (path);
			xtr.XmlResolver = null;
			return new XPathDocument (xtr);
		}

                private bool ParseLang (string lang)
		{
                        XPathDocument doc = GetXPathDocument (Path.Combine ("langs", lang + ".xml"));
			XPathNavigator nav = doc.CreateNavigator ();
                        CultureInfoEntry ci = new CultureInfoEntry ();
                        string lang_type, terr_type;

//                        ci.Name = lang; // TODO: might need to be mapped.

                        lang_type = nav.Evaluate ("string (ldml/identity/language/@type)").ToString ();
			terr_type = nav.Evaluate ("string (ldml/identity/territory/@type)").ToString ();

                        ci.Language = (lang_type == String.Empty ? null : lang_type);
                        ci.Territory = (terr_type == String.Empty ? null : terr_type);

			if (!LookupLcids (ci))
                                return false;

                        doc = GetXPathDocument (Path.Combine ("langs", Lang + ".xml"));
			nav = doc.CreateNavigator ();
			ci.DisplayName = LookupFullName (ci, nav);
                        
			if (Lang == "en") {
				ci.EnglishName = ci.DisplayName;
			} else {
				doc = GetXPathDocument (Path.Combine ("langs", Lang + ".xml"));
				nav = doc.CreateNavigator ();
				ci.EnglishName = LookupFullName (ci, nav);
			}

			if (ci.Language == Lang) {
				ci.NativeName = ci.DisplayName;
			} else {
				doc = GetXPathDocument (Path.Combine ("langs", lang + ".xml"));
				nav = doc.CreateNavigator ();
				ci.NativeName = LookupFullName (ci, nav);
			}

                        // Null these out because langs dont have them
                        ci.DateTimeFormatEntry = null;
                        ci.NumberFormatEntry = null;

                        langs [lang] = ci;
                        cultures.Add (ci);

                        return true;
                }

                private void ParseLocale (string locale)
                {
                        CultureInfoEntry ci;

                        ci = LookupCulture (locale);

                        if (ci == null)
                                return;

                        if (langs [ci.Language] == null) {
                                if (!ParseLang (ci.Language)) // If we can't parse the lang we cant have the locale
                                        return;
                        }

                        cultures.Add (ci);
                }

		private CultureInfoEntry LookupCulture (string locale)
		{
                        XPathDocument doc = GetXPathDocument (Path.Combine ("locales", locale + ".xml"));
                        XPathNavigator nav = doc.CreateNavigator ();
			CultureInfoEntry ci = new CultureInfoEntry ();
			string supp;

//                        ci.Name = locale; // TODO: Some of these need to be mapped.

			// First thing we do is get the lang-territory combo, lcid, and full names
			ci.Language = nav.Evaluate ("string (ldml/identity/language/@type)").ToString ();
			ci.Territory = nav.Evaluate ("string (ldml/identity/territory/@type)").ToString ();

                        if (!LookupLcids (ci))
                                return null;
			LookupNames (ci);

			/**
			 * Locale generation is done in six steps, first we
                         * read the root file which is the base invariant data
                         * then the supplemental root data, 
			 * then the language file, the supplemental languages
			 * file then the locale file, then the supplemental
			 * locale file. Values in each descending file can
			 * overwrite previous values.
			 */
                        doc = GetXPathDocument (Path.Combine ("langs", "root.xml"));
                        nav = doc.CreateNavigator ();
                        Lookup (nav, ci);

                        doc = GetXPathDocument (Path.Combine ("supp", "root.xml"));
                        nav = doc.CreateNavigator ();
                        Lookup (nav, ci);

			doc = GetXPathDocument (Path.Combine ("langs", ci.Language + ".xml"));
			nav = doc.CreateNavigator ();
			Lookup (nav, ci);

			supp = Path.Combine ("supp", ci.Language + ".xml");
			if (File.Exists (supp)) {
				doc = GetXPathDocument (supp);
				nav = doc.CreateNavigator ();
				Lookup (nav, ci);
			}
			
			doc = GetXPathDocument (Path.Combine ("locales", locale + ".xml"));
			nav = doc.CreateNavigator ();
			Lookup (nav, ci);

			supp = Path.Combine ("supp", locale + ".xml");
			if (File.Exists (supp)) {
				doc = GetXPathDocument (supp);
				nav = doc.CreateNavigator ();
				Lookup (nav, ci);
			}

                        return ci;
		}

		private void Lookup (XPathNavigator nav, CultureInfoEntry ci)
		{
			LookupDateTimeInfo (nav, ci);
			LookupNumberInfo (nav, ci);
		}

		private void LookupNames (CultureInfoEntry ci)
		{
			XPathDocument doc = GetXPathDocument (Path.Combine ("langs", Lang + ".xml"));
			XPathNavigator nav = doc.CreateNavigator ();

			ci.DisplayName = LookupFullName (ci, nav);
			
			if (Lang == "en") {
				ci.EnglishName = ci.DisplayName;
			} else {
				doc = GetXPathDocument (Path.Combine ("langs", "en.xml"));
				nav = doc.CreateNavigator ();
				ci.EnglishName = LookupFullName (ci, nav);
			}

			if (ci.Language == Lang) {
				ci.NativeName = ci.DisplayName;
			} else {
				doc = GetXPathDocument (Path.Combine ("langs", ci.Language + ".xml"));
				nav = doc.CreateNavigator ();
				ci.NativeName = LookupFullName (ci, nav);
			}
		}

		private void LookupDateTimeInfo (XPathNavigator nav, CultureInfoEntry ci)
		{
			/**
			 * TODO: Does anyone have multiple calendars?
			 */
			XPathNodeIterator ni =(XPathNodeIterator) nav.Evaluate ("ldml/dates/calendars/calendar");

			while (ni.MoveNext ()) {
				DateTimeFormatEntry df = ci.DateTimeFormatEntry;
				string cal_type = ni.Current.GetAttribute ("type", String.Empty);

				if (cal_type != String.Empty)
					df.CalendarType = cal_type;

				XPathNodeIterator ni2 = (XPathNodeIterator) ni.Current.Evaluate ("optionalCalendars/calendar");
                                int opt_cal_count = 0;
				while (ni2.MoveNext ()) {
                                        int type;
                                        string greg_type_str;
                                        XPathNavigator df_nav = ni2.Current;
                                        switch (df_nav.GetAttribute ("type", String.Empty)) {
                                        case "Gregorian":
                                                type = 0;
                                                break;
                                        case "Hijri":
                                                type = 0x01;
                                                break;
                                        case "ThaiBuddhist":
                                                type = 0x02;
                                                break;
                                        default:
                                                Console.WriteLine ("unknown calendar type:  " +
                                                                df_nav.GetAttribute ("type", String.Empty));
                                                continue;
                                        }
                                        type <<= 24;
                                        greg_type_str = df_nav.GetAttribute ("greg_type", String.Empty);
                                        if (greg_type_str != null && greg_type_str != String.Empty) {
                                                GregorianCalendarTypes greg_type = (GregorianCalendarTypes)
                                                        Enum.Parse (typeof (GregorianCalendarTypes), greg_type_str);
                                                int greg_type_int = (int) greg_type;
                                                type |= greg_type_int;
                                                
                                        }
                                        Console.WriteLine ("Setting cal type: {0:X}  for   {1}", type, ci.Name);
					ci.CalendarData [opt_cal_count++] = type;
				}
                                
				ni2 = (XPathNodeIterator) ni.Current.Evaluate ("monthNames/month");
				while (ni2.MoveNext ()) {
					if (ni2.CurrentPosition == 1)
						df.MonthNames.Clear ();
					df.MonthNames.Add (ni2.Current.Value);
				}

				ni2 = (XPathNodeIterator) ni.Current.Evaluate ("dayNames/day");
				while (ni2.MoveNext ()) {
					if (ni2.CurrentPosition == 1)
						df.DayNames.Clear ();
					df.DayNames.Add (ni2.Current.Value);
				}

				ni2 = (XPathNodeIterator) ni.Current.Evaluate ("dayAbbr/day");
				while (ni2.MoveNext ()) {
					if (ni2.CurrentPosition == 1)
						df.AbbreviatedDayNames.Clear ();
					df.AbbreviatedDayNames.Add (ni2.Current.Value);
				}

				ni2 = (XPathNodeIterator) ni.Current.Evaluate ("monthAbbr/month");
				while (ni2.MoveNext ()) {
					if (ni2.CurrentPosition == 1)
						df.AbbreviatedMonthNames.Clear ();
					df.AbbreviatedMonthNames.Add (ni2.Current.Value);
				}

				ni2 = (XPathNodeIterator) ni.Current.Evaluate ("dateFormats/dateFormatLength");
				while (ni2.MoveNext ()) {
					XPathNavigator df_nav = ni2.Current;
					XPathNodeIterator p = df_nav.Select ("dateFormat/pattern");
					string value = null;
					if (p.MoveNext ())
						value = p.Current.Value;
					XPathNodeIterator ext = null;
					switch (df_nav.GetAttribute ("type", String.Empty)) {
					case "full":
						if (value != null)
							ParseFullDateFormat (df, value);
						break;
					case "long":
						if (value != null)
							df.LongDatePattern = value;
						ext = df_nav.Select ("extraPatterns/pattern");
						if (ext.MoveNext ()) {
							df.LongDatePatterns.Clear ();
							do {
								df.LongDatePatterns.Add (ext.Current.Value);
							} while (ext.MoveNext ());
						}
						break;
					case "short":
						if (value != null)
							df.ShortDatePattern = value;
						ext = df_nav.Select ("extraPatterns/pattern");
						if (ext.MoveNext ()) {
							df.ShortDatePatterns.Clear ();
							do {
								df.ShortDatePatterns.Add (ext.Current.Value);
							} while (ext.MoveNext ());
						}
						break;
					case "year_month":
						if (value != null)
							df.YearMonthPattern = value;
						break;
					case "month_day":
						if (value != null)
							df.MonthDayPattern = value;
						break;
					}
				}

				ni2 = (XPathNodeIterator) ni.Current.Evaluate ("timeFormats/timeFormatLength");
				while (ni2.MoveNext ()) {
					XPathNavigator df_nav = ni2.Current;
					XPathNodeIterator p = df_nav.Select ("timeFormat/pattern");
					string value = null;
					if (p.MoveNext ())
						value = p.Current.Value;
					XPathNodeIterator ext = null;
					switch (df_nav.GetAttribute ("type", String.Empty)) {
					case "long":
						if (value != null)
							df.LongTimePattern = value.Replace ('a', 't');
						ext = df_nav.Select ("extraPatterns/pattern");
						if (ext.MoveNext ()) {
							df.LongTimePatterns.Clear ();
							do {
								df.LongTimePatterns.Add (ext.Current.Value);
							} while (ext.MoveNext ());
						}
						break;
					case "short":
						if (value != null)
							df.ShortTimePattern = value.Replace ('a', 't');
						ext = df_nav.Select ("extraPatterns/pattern");
						if (ext.MoveNext ()) {
							df.ShortTimePatterns.Clear ();
							do {
								df.ShortTimePatterns.Add (ext.Current.Value);
							} while (ext.MoveNext ());
						}
						break;
					}
				}

				ni2 = (XPathNodeIterator) ni.Current.Evaluate ("dateTimeFormats/dateTimeFormatLength/dateTimeFormat/pattern");
				if (ni2.MoveNext ())
					df.FullDateTimePattern = String.Format (ni2.Current.ToString (),
							df.LongTimePattern, df.LongDatePattern);

				XPathNodeIterator am = ni.Current.SelectChildren ("am", "");
				if (am.MoveNext ())
					df.AMDesignator = am.Current.Value;
				XPathNodeIterator pm = ni.Current.SelectChildren ("pm", "");
				if (pm.MoveNext ())
					df.PMDesignator = pm.Current.Value;
/*
                                string am = (string) ni.Current.Evaluate ("string(am)");
                                string pm = (string) ni.Current.Evaluate ("string(pm)");

                                if (am != String.Empty)
                                        df.AMDesignator = am;
                                if (pm != String.Empty)
                                        df.PMDesignator = pm;
*/
			}

                        string date_sep = (string) nav.Evaluate ("string(ldml/dates/symbols/dateSeparator)");
                        string time_sep = (string) nav.Evaluate ("string(ldml/dates/symbols/timeSeparator)");

                        if (date_sep != String.Empty)
                                ci.DateTimeFormatEntry.DateSeparator = date_sep;
                        if (time_sep != String.Empty)
                                ci.DateTimeFormatEntry.TimeSeparator = time_sep;
		}

		private void LookupNumberInfo (XPathNavigator nav, CultureInfoEntry ci)
		{
			XPathNodeIterator ni =(XPathNodeIterator) nav.Evaluate ("ldml/numbers");

			while (ni.MoveNext ()) {
                                LookupNumberSymbols (ni.Current, ci);
				LookupDecimalFormat (ni.Current, ci);
				LookupPercentFormat (ni.Current, ci);
				LookupCurrencyFormat (ni.Current, ci);
                                LookupCurrencySymbol (ni.Current, ci);
			}
		}

		private void LookupDecimalFormat (XPathNavigator nav, CultureInfoEntry ci)
		{
			string format = (string) nav.Evaluate ("string(decimalFormats/" +
					"decimalFormatLength/decimalFormat/pattern)");

			if (format == String.Empty)
				return;

			string [] part_one, part_two;
			string [] pos_neg = format.Split (new char [1] {';'}, 2);

			// Most of the patterns are common in positive and negative
			if (pos_neg.Length == 1)
				pos_neg = new string [] {pos_neg [0], pos_neg [0]};

			if (pos_neg.Length == 2) {
				
				part_one = pos_neg [0].Split (new char [1] {'.'}, 2);
				if (part_one.Length == 1)
					part_one = new string [] {part_one [0], String.Empty};

				if (part_one.Length == 2) {
					// assumed same for both positive and negative
					// decimal digit side
					ci.NumberFormatEntry.NumberDecimalDigits = 0;					
					for (int i = 0; i < part_one [1].Length; i++) {
						if (part_one [1][i] == '#') {
							ci.NumberFormatEntry.NumberDecimalDigits ++;
						} else
							break;								}
					// FIXME: This should be actually done by modifying culture xml files, but too many files to be modified.
					if (ci.NumberFormatEntry.NumberDecimalDigits > 0)
						ci.NumberFormatEntry.NumberDecimalDigits --;

					// decimal grouping side
					part_two = part_one [0].Split (',');
					if (part_two.Length > 1) {
						int len = part_two.Length - 1;
						ci.NumberFormatEntry.NumberGroupSizes = new int [len];
						for (int i = 0; i < len; i++) {
							string pat = part_two [i + 1];
							ci.NumberFormatEntry.NumberGroupSizes [i] = pat.Length;
						}
					} else {
						ci.NumberFormatEntry.NumberGroupSizes = new int [1] { 3 };
					}

					if (pos_neg [1].StartsWith ("(") && pos_neg [1].EndsWith (")")) {
						ci.NumberFormatEntry.NumberNegativePattern = 0;
					} else if (pos_neg [1].StartsWith ("- ")) {
						ci.NumberFormatEntry.NumberNegativePattern = 2;
					} else if (pos_neg [1].StartsWith ("-")) {
						ci.NumberFormatEntry.NumberNegativePattern = 1;
					} else if (pos_neg [1].EndsWith (" -")) {
						ci.NumberFormatEntry.NumberNegativePattern = 4;
					} else if (pos_neg [1].EndsWith ("-")) {
						ci.NumberFormatEntry.NumberNegativePattern = 3;
					} else {
						ci.NumberFormatEntry.NumberNegativePattern = 1;
					}
				}
			}
		}

		private void LookupPercentFormat (XPathNavigator nav, CultureInfoEntry ci)
		{
			string format = (string) nav.Evaluate ("string(percentFormats/" +
					"percentFormatLength/percentFormat/pattern)");

			if (format == String.Empty)
				return;

			string [] part_one, part_two;

			// we don't have percentNegativePattern in CLDR so 
			// the percentNegativePattern are just guesses
			if (format.StartsWith ("%")) {
				ci.NumberFormatEntry.PercentPositivePattern = 2;
				ci.NumberFormatEntry.PercentNegativePattern = 2;
				format = format.Substring (1);
			} else if (format.EndsWith (" %")) {
				ci.NumberFormatEntry.PercentPositivePattern = 0;
				ci.NumberFormatEntry.PercentNegativePattern = 0;
				format = format.Substring (0, format.Length - 2);
			} else if (format.EndsWith ("%")) {
				ci.NumberFormatEntry.PercentPositivePattern = 1;
				ci.NumberFormatEntry.PercentNegativePattern = 1;
				format = format.Substring (0, format.Length - 1);
			} else {
				ci.NumberFormatEntry.PercentPositivePattern = 0;
				ci.NumberFormatEntry.PercentNegativePattern = 0;
			}

			part_one = format.Split (new char [1] {'.'}, 2);
			if (part_one.Length == 2) {
				// assumed same for both positive and negative
				// decimal digit side
				ci.NumberFormatEntry.PercentDecimalDigits = 0;
				for (int i = 0; i < part_one [1].Length; i++) {
					if (part_one [1][i] == '#')
						ci.NumberFormatEntry.PercentDecimalDigits++;
					else
						break;
				}
			}

			if (part_one.Length > 0) {
				// percent grouping side
				part_two = part_one [0].Split (',');
				if (part_two.Length > 1) {
					int len = part_two.Length - 1;
					ci.NumberFormatEntry.PercentGroupSizes = new int [len];
					for (int i = 0; i < len; i++) {
						string pat = part_two [i + 1];
						if (pat [pat.Length -1] == '0')
							ci.NumberFormatEntry.PercentDecimalDigits = pat.Length - 1;
						ci.NumberFormatEntry.PercentGroupSizes [i] = pat.Length;
					}
				} else {
					ci.NumberFormatEntry.PercentGroupSizes = new int [1] { 3 };
					ci.NumberFormatEntry.PercentDecimalDigits = 2;
				}
			}
		}

		private void LookupCurrencyFormat (XPathNavigator nav, CultureInfoEntry ci)
		{
			string format = (string) nav.Evaluate ("string(currencyFormats/" +
					"currencyFormatLength/currencyFormat/pattern)");

			if (format == String.Empty)
				return;

			string [] part_one, part_two;
			string [] pos_neg = format.Split (new char [1] {';'}, 2);
	
			// Most of the patterns are common in positive and negative
			if (pos_neg.Length == 1)
				pos_neg = new string [] {pos_neg [0], pos_neg [0]};

			if (pos_neg.Length == 2) {
				part_one = pos_neg [0].Split (new char [1] {'.'}, 2);
				if (part_one.Length == 1)
					part_one = new string [] {part_one [0], String.Empty};
				if (part_one.Length == 2) {
					// assumed same for both positive and negative
					// decimal digit side
					ci.NumberFormatEntry.CurrencyDecimalDigits = 0;
					for (int i = 0; i < part_one [1].Length; i++) {
						if (part_one [1][i] == '0')
							ci.NumberFormatEntry.CurrencyDecimalDigits++;
						else
							break;
					}

					// decimal grouping side
					part_two = part_one [0].Split (',');
					if (part_two.Length > 1) {
						int len = part_two.Length - 1;
						ci.NumberFormatEntry.CurrencyGroupSizes = new int [len];
						for (int i = 0; i < len; i++) {
							string pat = part_two [i + 1];
							ci.NumberFormatEntry.CurrencyGroupSizes [i] = pat.Length;
						}
					} else {
						ci.NumberFormatEntry.CurrencyGroupSizes = new int [1] { 3 };
					}

					if (pos_neg [1].StartsWith ("(\u00a4 ") && pos_neg [1].EndsWith (")")) {
						ci.NumberFormatEntry.CurrencyNegativePattern = 14;
					} else if (pos_neg [1].StartsWith ("(\u00a4") && pos_neg [1].EndsWith (")")) {
						ci.NumberFormatEntry.CurrencyNegativePattern = 0;
					} else if (pos_neg [1].StartsWith ("\u00a4 ") && pos_neg [1].EndsWith ("-")) {
						ci.NumberFormatEntry.CurrencyNegativePattern = 11;
					} else if (pos_neg [1].StartsWith ("\u00a4") && pos_neg [1].EndsWith ("-")) {
						ci.NumberFormatEntry.CurrencyNegativePattern = 3;
					} else if (pos_neg [1].StartsWith ("(") && pos_neg [1].EndsWith (" \u00a4")) {
						ci.NumberFormatEntry.CurrencyNegativePattern = 15;
					} else if (pos_neg [1].StartsWith ("(") && pos_neg [1].EndsWith ("\u00a4")) {
						ci.NumberFormatEntry.CurrencyNegativePattern = 4;
					} else if (pos_neg [1].StartsWith ("-") && pos_neg [1].EndsWith (" \u00a4")) {
						ci.NumberFormatEntry.CurrencyNegativePattern = 8;
					} else if (pos_neg [1].StartsWith ("-") && pos_neg [1].EndsWith ("\u00a4")) {
						ci.NumberFormatEntry.CurrencyNegativePattern = 5;
					} else if (pos_neg [1].StartsWith ("-\u00a4 ")) {
						ci.NumberFormatEntry.CurrencyNegativePattern = 9;
					} else if (pos_neg [1].StartsWith ("-\u00a4")) {
						ci.NumberFormatEntry.CurrencyNegativePattern = 1;
					} else if (pos_neg [1].StartsWith ("\u00a4 -")) {
						ci.NumberFormatEntry.CurrencyNegativePattern = 12;
					} else if (pos_neg [1].StartsWith ("\u00a4-")) {
						ci.NumberFormatEntry.CurrencyNegativePattern = 2;
					} else if (pos_neg [1].EndsWith (" \u00a4-")) {
						ci.NumberFormatEntry.CurrencyNegativePattern = 10;
					} else if (pos_neg [1].EndsWith ("\u00a4-")) {
						ci.NumberFormatEntry.CurrencyNegativePattern = 7;
					} else if (pos_neg [1].EndsWith ("- \u00a4")) {
						ci.NumberFormatEntry.CurrencyNegativePattern = 13;
					} else if (pos_neg [1].EndsWith ("-\u00a4")) {
						ci.NumberFormatEntry.CurrencyNegativePattern = 6;
					} else {
						ci.NumberFormatEntry.CurrencyNegativePattern = 0;
					}
					
					if (pos_neg [0].StartsWith ("\u00a4 ")) {
						ci.NumberFormatEntry.CurrencyPositivePattern = 2;
					} else if (pos_neg [0].StartsWith ("\u00a4")) {
						ci.NumberFormatEntry.CurrencyPositivePattern = 0;
					} else if (pos_neg [0].EndsWith (" \u00a4")) {
						ci.NumberFormatEntry.CurrencyPositivePattern = 3;
					} else if (pos_neg [0].EndsWith ("\u00a4")) {
						ci.NumberFormatEntry.CurrencyPositivePattern = 1; 
					} else {
						ci.NumberFormatEntry.CurrencyPositivePattern = 0;
					}
				}
			}
		}

                private void LookupNumberSymbols (XPathNavigator nav, CultureInfoEntry ci)
                {
                        string dec = (string) nav.Evaluate ("string(symbols/decimal)");
                        string group = (string) nav.Evaluate ("string(symbols/group)");
                        string percent = (string) nav.Evaluate ("string(symbols/percentSign)");
                        string positive = (string) nav.Evaluate ("string(symbols/plusSign)");
                        string negative = (string) nav.Evaluate ("string(symbols/minusSign)");
                        string per_mille = (string) nav.Evaluate ("string(symbols/perMille)");
                        string infinity = (string) nav.Evaluate ("string(symbols/infinity)");
                        string nan = (string) nav.Evaluate ("string(symbols/nan)");

                        if (dec != String.Empty) {
                                ci.NumberFormatEntry.NumberDecimalSeparator = dec;
                                ci.NumberFormatEntry.PercentDecimalSeparator = dec;
                                ci.NumberFormatEntry.CurrencyDecimalSeparator = dec;
                        }

                        if (group != String.Empty) {
                                ci.NumberFormatEntry.NumberGroupSeparator = group;
                                ci.NumberFormatEntry.PercentGroupSeparator = group;
                                ci.NumberFormatEntry.CurrencyGroupSeparator = group;
                        }

                        if (percent != String.Empty)
                                ci.NumberFormatEntry.PercentSymbol = percent;
                        if (positive != String.Empty)
                                ci.NumberFormatEntry.PositiveSign = positive;
                        if (negative != String.Empty)
                                ci.NumberFormatEntry.NegativeSign = negative;
                        if (per_mille != String.Empty)
                                ci.NumberFormatEntry.PerMilleSymbol = per_mille;
                        if (infinity != String.Empty)
                                ci.NumberFormatEntry.PositiveInfinitySymbol = infinity;
                        if (nan != String.Empty)
                                ci.NumberFormatEntry.NaNSymbol = nan;
                }

                private void LookupCurrencySymbol (XPathNavigator nav, CultureInfoEntry ci)
                {
                        string type = currency_types [ci.Territory] as string;

                        if (type == null) {
                                Console.WriteLine ("no currency type for:  " + ci.Territory);
                                return;
                        }
                        
                        string cur = (string) nav.Evaluate ("string(currencies/currency [@type='" +
                                        type + "']/symbol)");

                        if (cur != String.Empty)
                                ci.NumberFormatEntry.CurrencySymbol = cur;
                }

		private bool LookupLcids (CultureInfoEntry ci)
		{
			XPathDocument doc = GetXPathDocument ("lcids.xml");
			XPathNavigator nav = doc.CreateNavigator ();
			string name = ci.Language;

                        if (ci.Territory != null)
                                name += "-" + ci.Territory;

			XPathNodeIterator ni =(XPathNodeIterator) nav.Evaluate ("lcids/lcid[@name='"
					+ name + "']");
			if (!ni.MoveNext ()) {
                                string file;
                                if (ci.Territory != null) {
                                        file = Path.Combine ("locales", ci.Language + "_" + ci.Territory + ".xml");
                                        File.Delete (file);
                                        Console.WriteLine ("deleting file:  " + file);
                                }
				Console.WriteLine ("no lcid found for:	" + name);
				return false;
			}

			string id = ni.Current.GetAttribute ("id", String.Empty);
			string parent = ni.Current.GetAttribute ("parent", String.Empty);
                        string specific = ni.Current.GetAttribute ("specific", String.Empty);
                        string iso2 = ni.Current.GetAttribute ("iso2", String.Empty);
                        string iso3 = ni.Current.GetAttribute ("iso3", String.Empty);
                        string win = ni.Current.GetAttribute ("win", String.Empty);
                        string icu = ni.Current.GetAttribute ("icu_name", String.Empty);

			// lcids are in 0x<hex> format
			ci.Lcid = id;
			ci.ParentLcid = parent;
                        ci.SpecificLcid = specific;
                        ci.ISO2Lang = iso2;
                        ci.ISO3Lang = iso3;
                        ci.Win3Lang = win;
                        ci.IcuName = icu;
			
			ci.TextInfoEntry = new TextInfoEntry (int.Parse (id.Substring (2), NumberStyles.HexNumber), GetXPathDocument ("textinfo.xml"));

                        return true;
		}
		
		private string LookupFullName (CultureInfoEntry ci, XPathNavigator nav)
		{
			string pre = "ldml/localeDisplayNames/";
			string ret;

			ret = (string) nav.Evaluate ("string("+
					pre + "languages/language[@type='" + ci.Language + "'])");

			if (ci.Territory == null)
				return ret;
			ret += " (" + (string) nav.Evaluate ("string("+
					pre + "territories/territory[@type='" + ci.Territory + "'])") + ")";

			return ret;
		}

                private void LookupCurrencyTypes ()
                {
                        XPathDocument doc = GetXPathDocument ("supplementalData.xml");
			XPathNavigator nav = doc.CreateNavigator ();

                        currency_types = new Hashtable ();

			XPathNodeIterator ni =(XPathNodeIterator) nav.Evaluate ("supplementalData/currencyData/region");
			while (ni.MoveNext ()) {
				string territory = (string) ni.Current.GetAttribute ("iso3166", String.Empty);
                                string currency = (string) ni.Current.Evaluate ("string(currency/@iso4217)");
                                currency_types [territory] = currency;
			}
                }

                static string control_chars = "ghmsftz";

                // HACK: We are trying to build year_month and month_day patterns from the full pattern.
                private void ParseFullDateFormat (DateTimeFormatEntry df, string full)
                {
                        
                        string month_day = String.Empty;
                        string year_month = String.Empty;
                        bool in_month_data = false;
                        bool in_year_data = false;
                        int month_end = 0;
                        int year_end = 0;
			bool inquote = false;
                        
                        for (int i = 0; i < full.Length; i++) {
                                char c = full [i];
				if (!inquote && c == 'M') {
                                        month_day += c;
                                        year_month += c;
                                        in_year_data = true;
                                        in_month_data = true;
                                        month_end = month_day.Length;
                                        year_end = year_month.Length;
                                } else if (!inquote && Char.ToLower (c) == 'd') {
                                        month_day += c;
                                        in_month_data = true;
                                        in_year_data = false;
                                        month_end = month_day.Length;
                                } else if (!inquote && Char.ToLower (c) == 'y') {
                                        year_month += c;
                                        in_year_data = true;
                                        in_month_data = false;
                                        year_end = year_month.Length;
                                } else if (!inquote && control_chars.IndexOf (Char.ToLower (c)) >= 0) {
                                        in_year_data = false;
                                        in_month_data = false;
                                } else if (in_year_data || in_month_data) {
                                        if (in_month_data)
                                                month_day += c;
                                        if (in_year_data)
                                                year_month += c;
                                }

				if (c == '\'') {
					inquote = !inquote;
                                }
                        }

                        if (month_day != String.Empty) {
                                month_day = month_day.Substring (0, month_end);
                                df.MonthDayPattern = month_day;
                        }
                        if (year_month != String.Empty) {
                                year_month = year_month.Substring (0, year_end);
                                df.YearMonthPattern = year_month;
                        }
                }

                private class LcidComparer : IComparer {

                        public int Compare (object a, object b)
                        {
                                CultureInfoEntry aa = (CultureInfoEntry) a;
                                CultureInfoEntry bb = (CultureInfoEntry) b;
                        
                                return aa.Lcid.CompareTo (bb.Lcid);
                        }                
                }

                private class NameComparer : IComparer {

                        public int Compare (object a, object b)
                        {
                                CultureInfoEntry aa = (CultureInfoEntry) a;
                                CultureInfoEntry bb = (CultureInfoEntry) b;

                                return aa.Name.ToLower ().CompareTo (bb.Name.ToLower ());
                        }
                }
        }
}


