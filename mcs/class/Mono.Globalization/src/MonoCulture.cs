//
// MonoCulture.cs - A Mono-specific skeleton of
//                  System.Globalization.CultureInfo.
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Copyright 2003 Ximian Inc.
//

namespace Mono.Globalization {
        public abstract class MonoCulture {
//              public System.Globalization.CultureInfo currentCulture;
//              public System.Globalization.CultureInfo currentUICulture;
//              public System.Globalization.CultureInfo installedUICulture;
//              public System.Globalization.CultureInfo invariantCulture;
                public static System.Globalization.CultureInfo Parent;
                public static System.Int32 LCID;
                public static System.String Name;
                public static System.String DisplayName;
                public static System.String NativeName;
                public static System.String EnglishName;
                public static System.String TwoLetterISOLanguageName;
                public static System.String ThreeLetterISOLanguageName;
                public static System.Globalization.CompareInfo CompareInfo;
                public static System.Globalization.TextInfo TextInfo;
                public static System.Globalization.NumberFormatInfo NumberFormat;
                public static System.Globalization.DateTimeFormatInfo DateTimeFormat;
                public static System.Globalization.Calendar Calendar;
                public static System.Globalization.Calendar[] OptionalCalendars;
        }
}
