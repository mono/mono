//------------------------------------------------------------------------------
// <copyright file="ClientCultureInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Globalization {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Text;
    using System.Web.Util;
    using System.Web.Script.Serialization;

    internal class ClientCultureInfo {

        private static Hashtable cultureScriptBlockCache = Hashtable.Synchronized(new Hashtable());
        private static readonly CultureInfo enUS = CultureInfo.GetCultureInfo(0x409);
        private static int eraNumber = 0;
        private static int eraName = 1;
        private static int eraStart = 2;
        private static int eraYearOffset = 3;

        public string name;
        public NumberFormatInfo numberFormat;
        public DateTimeFormatInfo dateTimeFormat;
        public object[] eras;
        private string _convertScript;
        private int _adjustment;

        private ClientCultureInfo(CultureInfo cultureInfo) {
            name = cultureInfo.Name;
            numberFormat = cultureInfo.NumberFormat;
            dateTimeFormat = cultureInfo.DateTimeFormat;
            var calendar = dateTimeFormat == null ? null : dateTimeFormat.Calendar;
            if (calendar != null) {
                // Dev10 425049: Support Eras for gregorian based calendars
                // with a simple year offset, and non-gregorian calendars.
                // Era data is stored in binary resource "culture.nlp" in mscorlib,
                // hard coded here for simplicity.
                // era array has the following structure:
                // [eraNumber1, eraName1, eraStartInTicks1, eraGregorianYearOffset1, eraNumber2, ...]
                eras = new object[calendar.Eras.Length * 4];
                int i = 0;
                foreach (int era in calendar.Eras) {
                    // era number
                    eras[i + eraNumber] = era;
                    // era name
                    eras[i + eraName] = dateTimeFormat.GetEraName(era);
                    // calendars with only one era will have a null tick count
                    // signifying that the era starts from the lowest datetime
                    // era begining in ticks (null = the oldest era)
                    // eras[i + eraStart] = null;
                    // era year offset from normal gregorian year
                    // some calendars dont have an offset, just a different name
                    // for the A.D. era (B.C. is not supported by normal calendar,
                    // so most calendars only have 1 era)
                    eras[i + eraYearOffset] = 0;
                    i += 4;
                }
                var calendarType = calendar.GetType();
                if (calendarType != typeof(GregorianCalendar)) {
                    if (calendarType == typeof(TaiwanCalendar)) {
                        // Only the current era is supported, so no tick count is needed
                        //eras[eraStart] = -1830384000000;
                        eras[eraYearOffset] = 1911;
                    }
                    else if (calendarType == typeof(KoreanCalendar)) {
                        // only one era to speak of, so no tick count is needed
                        //eras[eraStart] = -62135596800000;
                        eras[eraYearOffset] = -2333;
                    }
                    else if (calendarType == typeof(ThaiBuddhistCalendar)) {
                        // only one era to speak of, so no tick count is needed
                        //eras[eraStart] = -62135596800000;
                        eras[eraYearOffset] = -543;
                    }
                    else if (calendarType == typeof(JapaneseCalendar)) {
                        // there are multiple eras
                        eras[0 + eraStart] = 60022080000;
                        eras[0 + eraYearOffset] = 1988;
                        eras[4 + eraStart] = -1357603200000;
                        eras[4 + eraYearOffset] = 1925;
                        eras[8 + eraStart] = -1812153600000;
                        eras[8 + eraYearOffset] = 1911;
                        // oldest era is technically from this offset, but for simplicity
                        // it is counted from the lowest date time, so no tick count needed.
                        //eras[12 + eraStart] = -3218832000000;
                        eras[12 + eraYearOffset] = 1867;
                    }
                    else if (calendarType == typeof(HijriCalendar)) {
                        _convertScript = "Date.HijriCalendar.js";
                        _adjustment = ((HijriCalendar)calendar).HijriAdjustment;
                    }
                    else if (calendarType == typeof(UmAlQuraCalendar)) {
                        _convertScript = "Date.UmAlQuraCalendar.js";
                    }
                    // else { other calendars arent supported or have no era offsets just different names for A.D.
                }
            }
            
        }

        internal Tuple<String, String> GetClientCultureScriptBlock() {
            return GetClientCultureScriptBlock(CultureInfo.CurrentCulture);
        }

        internal static Tuple<String, String> GetClientCultureScriptBlock(CultureInfo cultureInfo) {
            if (cultureInfo == null) {
                return null;
            }

            // note: DateTimeFormat could be null since it is a virtual property, but DateTimeFormat.Calendar cannot be
            Type calendarType = cultureInfo.DateTimeFormat == null ? null : cultureInfo.DateTimeFormat.Calendar.GetType();
            if (cultureInfo.Equals(enUS) && (calendarType == typeof(GregorianCalendar))) {
                return null;
            }

            var key = new Tuple<CultureInfo, Type>(cultureInfo, calendarType);
            Tuple<String, String> cached = cultureScriptBlockCache[key] as Tuple<String, String>;
            if (cached == null) {
                ClientCultureInfo clientCultureInfo = new ClientCultureInfo(cultureInfo);
                string json = JavaScriptSerializer.SerializeInternal(BuildSerializeableCultureInfo(clientCultureInfo));
                if (json.Length > 0) {
                    string script = "var __cultureInfo = " + json + ";";
                    if (clientCultureInfo._adjustment != 0) {
                        script += "\r\n__cultureInfo.dateTimeFormat.Calendar._adjustment = " + clientCultureInfo._adjustment.ToString(CultureInfo.InvariantCulture) + ";";
                    }
                    cached = new Tuple<String, String>(script, clientCultureInfo._convertScript);
                }
                cultureScriptBlockCache[key] = cached;
            }
            return cached;
        }

        private static OrderedDictionary BuildSerializeableCultureInfo(ClientCultureInfo clientCultureInfo) {
            // It's safe to serialize the values set in the dictionary
            //  name is a string
            //  numberFormat is NumberFormatInfo which is a public type
            //  dateTimeFormat is a DateFormatInfo which is a public type
            //  eras is an object[] array that only contains strings or numbers
            var dictionary = new OrderedDictionary();
            dictionary["name"] = clientCultureInfo.name;
            dictionary["numberFormat"] = clientCultureInfo.numberFormat;
            dictionary["dateTimeFormat"] = clientCultureInfo.dateTimeFormat;
            dictionary["eras"] = clientCultureInfo.eras;
            return dictionary;
        }
    }
}
