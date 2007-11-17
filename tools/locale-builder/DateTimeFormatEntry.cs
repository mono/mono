//
// Mono.Tools.LocaleBuilder.DateTimeFormatEntry
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2004, Novell, Inc (http://www.novell.com)
//


using System;
using System.Text;
using System.Collections;

namespace Mono.Tools.LocaleBuilder {

        public class DateTimeFormatEntry : Entry {

                public string CalendarType;
                public ArrayList AbbreviatedDayNames = new ArrayList ();
                public ArrayList AbbreviatedMonthNames = new ArrayList ();
                public string AMDesignator;
                public int CalendarWeekRule;
                public string DateSeparator;
                public ArrayList DayNames = new ArrayList ();
                public int FirstDayOfWeek;
                public string RawFullDateTimePattern;
                public string LongDatePattern;
                public string LongTimePattern;
                public string MonthDayPattern;
                public ArrayList MonthNames = new ArrayList ();
                public string PMDesignator;
                public string ShortDatePattern;
                public string ShortTimePattern;
                public string TimeSeparator;
                public string YearMonthPattern;
                public int [] OptionalCalendars = new int [5];
                public ArrayList ShortDatePatterns = new ArrayList (14);
                public ArrayList LongDatePatterns = new ArrayList (8);
                public ArrayList ShortTimePatterns = new ArrayList (5);
                public ArrayList LongTimePatterns = new ArrayList (6);

                public string FullDateTimePattern {
                        get { return String.Format (RawFullDateTimePattern, LongTimePattern, LongDatePattern); }
                }

                public int Row;

                public void AppendTableRow (StringBuilder builder)
                {
                        builder.Append ("\t{");
                        builder.Append (EncodeStringIdx (FullDateTimePattern) + ", ");
                        builder.Append (EncodeStringIdx (LongDatePattern) + ", ");
                        builder.Append (EncodeStringIdx (ShortDatePattern) + ", ");

                        builder.Append (EncodeStringIdx (LongTimePattern) + ", ");
                        builder.Append (EncodeStringIdx (ShortTimePattern) + ", ");

                        builder.Append (EncodeStringIdx (YearMonthPattern) + ", ");
                        builder.Append (EncodeStringIdx (MonthDayPattern) + ", ");

                        builder.Append (EncodeStringIdx (AMDesignator) + ", ");
                        builder.Append (EncodeStringIdx (PMDesignator) + ", ");

                        AppendNames (builder, DayNames);
                        builder.Append (", ");
                        AppendNames (builder, AbbreviatedDayNames);
                        builder.Append (", ");

                        AppendNames (builder, MonthNames);
                        builder.Append (", ");
                        AppendNames (builder, AbbreviatedMonthNames);
                        builder.Append (", ");

                        builder.Append (CalendarWeekRule + ", ");
                        builder.Append (FirstDayOfWeek + ", ");
                        
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

                private void AppendPatterns (StringBuilder builder, ArrayList al)
                {
                        string [] patterns = al.ToArray (typeof (string)) as string [];
                        builder.Append ('{');
                        for (int i = 0; i < patterns.Length; i++) {
                                string s = EncodeStringIdx (patterns [i]);
                                builder.Append (s);
                                if (i + 1 < patterns.Length)
                                        builder.Append (',');
                        }
                        if (patterns.Length == 0)
                                builder.Append ('0');
                        builder.Append ('}');
                }

                public override string ToString ()
                {
                        StringBuilder builder = new StringBuilder ();
                        AppendTableRow (builder);
                        return builder.ToString ();
                }

                private void AppendNames (StringBuilder builder, ArrayList names)
                {
                        builder.Append ('{');
                        for (int i=0; i<names.Count; i++) {
                                builder.Append (EncodeStringIdx (names [i].ToString ()));
                                if (i+1 < names.Count)
                                        builder.Append (", ");
                        }
                        builder.Append ("}");
                }
        }
}


