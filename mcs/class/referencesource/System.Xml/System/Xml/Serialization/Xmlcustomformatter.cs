//------------------------------------------------------------------------------
// <copyright file="XmlCustomFormatter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------
namespace System.Xml.Serialization {

    using System;
    using System.Xml;
    using System.Globalization;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Collections;
    using System.Configuration;
    using System.Xml.Serialization.Configuration;

    /// <summary>
    ///   The <see cref="XmlCustomFormatter"/> class provides a set of static methods for converting
    ///   primitive type values to and from their XML string representations.</summary>
    internal class XmlCustomFormatter {
#if CONFIGURATION_DEP
        private static DateTimeSerializationSection.DateTimeSerializationMode mode;

        static DateTimeSerializationSection.DateTimeSerializationMode Mode {
            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread-safety")]
            get {
                if (mode == DateTimeSerializationSection.DateTimeSerializationMode.Default) {
                    DateTimeSerializationSection section = PrivilegedConfigurationManager.GetSection(ConfigurationStrings.DateTimeSerializationSectionPath) as DateTimeSerializationSection;
                    if (section != null) {
                        mode = section.Mode;
                    }
                    else {
                        mode = DateTimeSerializationSection.DateTimeSerializationMode.Roundtrip;
                    }
                }
                return mode;
            }
        }
#endif
        private XmlCustomFormatter() {}
        internal static string FromDefaultValue(object value, string formatter) {
            if (value == null) return null;
            Type type = value.GetType();
            if (type == typeof(DateTime)) {
                if (formatter == "DateTime") {
                    return FromDateTime((DateTime)value);
                }
                if (formatter == "Date") {
                    return FromDate((DateTime)value);
                }
                if (formatter == "Time") {
                    return FromTime((DateTime)value);
                }
            }
            else if (type == typeof(string)) {
                if (formatter == "XmlName") {
                    return FromXmlName((string)value);
                }
                if (formatter == "XmlNCName") {
                    return FromXmlNCName((string)value);
                }
                if (formatter == "XmlNmToken") {
                    return FromXmlNmToken((string)value);
                }
                if (formatter == "XmlNmTokens") {
                    return FromXmlNmTokens((string)value);
                }
            }
            throw new Exception(Res.GetString(Res.XmlUnsupportedDefaultType, type.FullName));
        }

        internal static string FromDate(DateTime value) {
            return XmlConvert.ToString(value, "yyyy-MM-dd");
        }

        internal static string FromTime(DateTime value) {
            return XmlConvert.ToString(DateTime.MinValue + value.TimeOfDay, "HH:mm:ss.fffffffzzzzzz");
        }

        internal static string FromDateTime(DateTime value) {
#if CONFIGURATION_DEP
            if (Mode == DateTimeSerializationSection.DateTimeSerializationMode.Local) {
                return XmlConvert.ToString(value, "yyyy-MM-ddTHH:mm:ss.fffffffzzzzzz");
            }
#endif
                // for mode DateTimeSerializationMode.Roundtrip and DateTimeSerializationMode.Default
                return XmlConvert.ToString(value, XmlDateTimeSerializationMode.RoundtripKind);
        }

        internal static string FromChar(char value) {
            return XmlConvert.ToString((UInt16)value);
        }

        internal static string FromXmlName(string name) {
            return XmlConvert.EncodeName(name);
        }

        internal static string FromXmlNCName(string ncName) {
            return XmlConvert.EncodeLocalName(ncName);
        }

        internal static string FromXmlNmToken(string nmToken) {
            return XmlConvert.EncodeNmToken(nmToken);
        }

        internal static string FromXmlNmTokens(string nmTokens) {
            if (nmTokens == null)
                return null;
            if (nmTokens.IndexOf(' ') < 0) 
                return FromXmlNmToken(nmTokens);
            else {
                string[] toks = nmTokens.Split(new char[] { ' ' });
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < toks.Length; i++) {
                    if (i > 0) sb.Append(' ');
                    sb.Append(FromXmlNmToken(toks[i]));
                }
                return sb.ToString();
            }
        }

        internal static void WriteArrayBase64(XmlWriter writer, byte[] inData, int start, int count) {
            if (inData == null || count == 0) {
                return;
            }
            writer.WriteBase64(inData, start, count);
        }

        internal static string FromByteArrayHex(byte[] value) {
            if (value == null)
                return null;
            if (value.Length == 0)
                return "";
            return XmlConvert.ToBinHexString(value);
        }

        internal static string FromEnum(long val, string[] vals, long[] ids, string typeName) {
#if DEBUG
                // use exception in the place of Debug.Assert to avoid throwing asserts from a server process such as aspnet_ewp.exe
                if (ids.Length != vals.Length) throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorDetails, "Invalid enum"));
#endif

            long originalValue = val;
            StringBuilder sb = new StringBuilder();
            int iZero = -1;

            for (int i = 0; i < ids.Length; i++) {
                if (ids[i] == 0) {
                    iZero = i;
                    continue;
                }
                if (val == 0) {
                    break;
                }
                if ((ids[i] & originalValue) == ids[i]) {
                    if (sb.Length != 0)
                        sb.Append(" ");
                    sb.Append(vals[i]);
                    val &= ~ids[i];
                }
            }
            if (val != 0) {
                // failed to parse the enum value
                throw new InvalidOperationException(Res.GetString(Res.XmlUnknownConstant, originalValue, typeName == null ? "enum" : typeName));
            }
            if (sb.Length == 0 && iZero >= 0) {
                sb.Append(vals[iZero]);
            }
            return sb.ToString();
        }

        internal static object ToDefaultValue(string value, string formatter) {
            if (formatter == "DateTime") {
                return ToDateTime(value);
            }
            if (formatter == "Date") {
                return ToDate(value);
            }
            if (formatter == "Time") {
                return ToTime(value);
            }
            if (formatter == "XmlName") {
                return ToXmlName(value);
            }
            if (formatter == "XmlNCName") {
                return ToXmlNCName(value);
            }
            if (formatter == "XmlNmToken") {
                return ToXmlNmToken(value);
            }
            if (formatter == "XmlNmTokens") {
                return ToXmlNmTokens(value);
            }
            throw new Exception(Res.GetString(Res.XmlUnsupportedDefaultValue, formatter));
            //            Debug.WriteLineIf(CompModSwitches.XmlSerialization.TraceVerbose, "XmlSerialization::Unhandled default value " + value + " formatter " + formatter);
            //            return DBNull.Value;
        }

        static string[] allDateTimeFormats = new string[] {
            "yyyy-MM-ddTHH:mm:ss.fffffffzzzzzz",
            "yyyy",
            "---dd",
            "---ddZ",
            "---ddzzzzzz",
            "--MM-dd",
            "--MM-ddZ",
            "--MM-ddzzzzzz",
            "--MM--",
            "--MM--Z",
            "--MM--zzzzzz",
            "yyyy-MM",
            "yyyy-MMZ",
            "yyyy-MMzzzzzz",
            "yyyyzzzzzz",
            "yyyy-MM-dd",
            "yyyy-MM-ddZ",
            "yyyy-MM-ddzzzzzz",

            "HH:mm:ss",
            "HH:mm:ss.f",
            "HH:mm:ss.ff",
            "HH:mm:ss.fff",
            "HH:mm:ss.ffff",
            "HH:mm:ss.fffff",
            "HH:mm:ss.ffffff",
            "HH:mm:ss.fffffff",
            "HH:mm:ssZ",
            "HH:mm:ss.fZ",
            "HH:mm:ss.ffZ",
            "HH:mm:ss.fffZ",
            "HH:mm:ss.ffffZ",
            "HH:mm:ss.fffffZ",
            "HH:mm:ss.ffffffZ",
            "HH:mm:ss.fffffffZ",
            "HH:mm:sszzzzzz",
            "HH:mm:ss.fzzzzzz",
            "HH:mm:ss.ffzzzzzz",
            "HH:mm:ss.fffzzzzzz",
            "HH:mm:ss.ffffzzzzzz",
            "HH:mm:ss.fffffzzzzzz",
            "HH:mm:ss.ffffffzzzzzz",
            "HH:mm:ss.fffffffzzzzzz",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss.f",
            "yyyy-MM-ddTHH:mm:ss.ff",
            "yyyy-MM-ddTHH:mm:ss.fff",
            "yyyy-MM-ddTHH:mm:ss.ffff",
            "yyyy-MM-ddTHH:mm:ss.fffff",
            "yyyy-MM-ddTHH:mm:ss.ffffff",
            "yyyy-MM-ddTHH:mm:ss.fffffff",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss.fZ",
            "yyyy-MM-ddTHH:mm:ss.ffZ",
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "yyyy-MM-ddTHH:mm:ss.ffffZ",
            "yyyy-MM-ddTHH:mm:ss.fffffZ",
            "yyyy-MM-ddTHH:mm:ss.ffffffZ",
            "yyyy-MM-ddTHH:mm:ss.fffffffZ",
            "yyyy-MM-ddTHH:mm:sszzzzzz",
            "yyyy-MM-ddTHH:mm:ss.fzzzzzz",
            "yyyy-MM-ddTHH:mm:ss.ffzzzzzz",
            "yyyy-MM-ddTHH:mm:ss.fffzzzzzz",
            "yyyy-MM-ddTHH:mm:ss.ffffzzzzzz",
            "yyyy-MM-ddTHH:mm:ss.fffffzzzzzz",
            "yyyy-MM-ddTHH:mm:ss.ffffffzzzzzz",
        };

        static string[] allDateFormats = new string[] {
            "yyyy-MM-ddzzzzzz",
            "yyyy-MM-dd",
            "yyyy-MM-ddZ",
            "yyyy",
            "---dd",
            "---ddZ",
            "---ddzzzzzz",
            "--MM-dd",
            "--MM-ddZ",
            "--MM-ddzzzzzz",
            "--MM--",
            "--MM--Z",
            "--MM--zzzzzz",
            "yyyy-MM",
            "yyyy-MMZ",
            "yyyy-MMzzzzzz",
            "yyyyzzzzzz",
        };

        static string[] allTimeFormats = new string[] {
            "HH:mm:ss.fffffffzzzzzz",
            "HH:mm:ss",
            "HH:mm:ss.f",
            "HH:mm:ss.ff",
            "HH:mm:ss.fff",
            "HH:mm:ss.ffff",
            "HH:mm:ss.fffff",
            "HH:mm:ss.ffffff",
            "HH:mm:ss.fffffff",
            "HH:mm:ssZ",
            "HH:mm:ss.fZ",
            "HH:mm:ss.ffZ",
            "HH:mm:ss.fffZ",
            "HH:mm:ss.ffffZ",
            "HH:mm:ss.fffffZ",
            "HH:mm:ss.ffffffZ",
            "HH:mm:ss.fffffffZ",
            "HH:mm:sszzzzzz",
            "HH:mm:ss.fzzzzzz",
            "HH:mm:ss.ffzzzzzz",
            "HH:mm:ss.fffzzzzzz",
            "HH:mm:ss.ffffzzzzzz",
            "HH:mm:ss.fffffzzzzzz",
            "HH:mm:ss.ffffffzzzzzz",
        };

        internal static DateTime ToDateTime(string value) {
#if CONFIGURATION_DEP
            if (Mode == DateTimeSerializationSection.DateTimeSerializationMode.Local) {
                return ToDateTime(value, allDateTimeFormats);
            }
            else
#endif
            {
                // for mode DateTimeSerializationMode.Roundtrip and DateTimeSerializationMode.Default
                return XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.RoundtripKind);
            }
        }

        internal static DateTime ToDateTime(string value, string[] formats) {
            return XmlConvert.ToDateTime(value, formats);
        }

        internal static DateTime ToDate(string value) {
            return ToDateTime(value, allDateFormats);
        }

        internal static DateTime ToTime(string value) {
            return DateTime.ParseExact(value, allTimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowLeadingWhite|DateTimeStyles.AllowTrailingWhite|DateTimeStyles.NoCurrentDateDefault);
        }

        internal static char ToChar(string value) {
            return (char)XmlConvert.ToUInt16(value);
        }

        internal static string ToXmlName(string value) {
            return XmlConvert.DecodeName(CollapseWhitespace(value));
        }

        internal static string ToXmlNCName(string value) {
            return XmlConvert.DecodeName(CollapseWhitespace(value));
        }
        
        internal static string ToXmlNmToken(string value) {
            return XmlConvert.DecodeName(CollapseWhitespace(value));
        }
        
        internal static string ToXmlNmTokens(string value) {
            return XmlConvert.DecodeName(CollapseWhitespace(value));
        }
        
        internal static byte[] ToByteArrayBase64(string value) {
            if (value == null) return null;
            value = value.Trim();
            if (value.Length == 0)
                return new byte[0];           
            return Convert.FromBase64String(value);
        }
        internal static byte[] ToByteArrayHex(string value) {
            if (value == null) return null;
            value = value.Trim();
            return XmlConvert.FromBinHexString(value);
        }

        internal static long ToEnum(string val, Hashtable vals, string typeName, bool validate)     {
            long value = 0;
            string[] parts = val.Split(null);
            for (int i = 0; i < parts.Length; i++) {
                object id = vals[parts[i]];
                if (id != null)
                    value |= (long)id;
                else if (validate && parts[i].Length > 0)
                    throw new InvalidOperationException(Res.GetString(Res.XmlUnknownConstant, parts[i], typeName));
            }
            return value;
        }

        static string CollapseWhitespace(string value) {
            if (value == null)
                return null;
            return value.Trim();
        }
    }
}
