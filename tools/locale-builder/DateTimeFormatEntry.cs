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
                public string FullDateTimePattern;
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
                
                public int Row;

                public void AppendTableRow (StringBuilder builder)
                {
                        builder.Append ("\t{");
                        builder.Append ("\"" + EncodeString (FullDateTimePattern) + "\", ");
                        builder.Append ("\"" + EncodeString (LongDatePattern) + "\", ");
                        builder.Append ("\"" + EncodeString (ShortDatePattern) + "\", ");

                        builder.Append ("\"" + EncodeString (LongTimePattern) + "\", ");
                        builder.Append ("\"" + EncodeString (ShortTimePattern) + "\", ");

                        builder.Append ("\"" + EncodeString (YearMonthPattern) + "\", ");
                        builder.Append ("\"" + EncodeString (MonthDayPattern) + "\", ");

                        builder.Append ("\"" + EncodeString (AMDesignator) + "\", ");
                        builder.Append ("\"" + EncodeString (PMDesignator) + "\", ");

                        AppendNames (builder, DayNames);
                        builder.Append (", ");
                        AppendNames (builder, AbbreviatedDayNames);
                        builder.Append (", ");

                        AppendNames (builder, MonthNames);
                        builder.Append (", ");
                        AbbreviatedMonthNames.Add (String.Empty); /* ALLWAYS ?? */
                        AppendNames (builder, AbbreviatedMonthNames);
                        builder.Append (", ");

                        builder.Append (CalendarWeekRule + ", ");
                        builder.Append (FirstDayOfWeek + ", ");
                        
                        builder.Append ("\"" + EncodeString (DateSeparator) + "\", ");
                        builder.Append ("\"" + EncodeString (TimeSeparator) + "\"");

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
                                builder.Append ("\"" + EncodeString (names [i].ToString ()) + "\"");
                                if (i+1 < names.Count)
                                        builder.Append (", ");
                        }
                        builder.Append ("}");
                }
        }
}


