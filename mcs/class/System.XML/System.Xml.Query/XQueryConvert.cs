//
// System.Xml.Query.XQueryConvert
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2004 Novell Inc.
//

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
#if NET_2_0

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace System.Xml
{
	public class XQueryConvert
	{
		private XQueryConvert ()
		{
		}

		[MonoTODO]
		public static bool ShouldCheckValueFacets (XmlSchemaType schemaTypeDest)
		{
			throw new NotImplementedException ();
		}

		public static XmlTypeCode GetFallbackType (XmlTypeCode type)
		{
			switch (type) {
			case XmlTypeCode.AnyAtomicType:
				return XmlTypeCode.Item;
			case XmlTypeCode.UntypedAtomic:
				return XmlTypeCode.String;
			case XmlTypeCode.Notation:
				return XmlTypeCode.QName;
			case XmlTypeCode.NormalizedString:
			case XmlTypeCode.Token:
			case XmlTypeCode.Language:
			case XmlTypeCode.NmToken:
			case XmlTypeCode.Name:
			case XmlTypeCode.NCName:
			case XmlTypeCode.Id:
			case XmlTypeCode.Idref:
			case XmlTypeCode.Entity:
				return XmlTypeCode.String;
			case XmlTypeCode.NonPositiveInteger:
				return XmlTypeCode.Decimal;
			case XmlTypeCode.NegativeInteger:
				return XmlTypeCode.NonPositiveInteger;
			case XmlTypeCode.Long:
				return XmlTypeCode.Integer;
			case XmlTypeCode.Short:
				return XmlTypeCode.Int;
			case XmlTypeCode.Byte:
				return XmlTypeCode.Int;
			case XmlTypeCode.NonNegativeInteger:
				return XmlTypeCode.Decimal;
			case XmlTypeCode.UnsignedLong:
				return XmlTypeCode.NonNegativeInteger;
			case XmlTypeCode.UnsignedInt:
				return XmlTypeCode.Integer;
			case XmlTypeCode.UnsignedShort:
				return XmlTypeCode.Int;
			case XmlTypeCode.UnsignedByte:
				return XmlTypeCode.UnsignedShort;
			case XmlTypeCode.PositiveInteger:
				return XmlTypeCode.NonNegativeInteger;
			default:
				return XmlTypeCode.None;
			}
		}

		[MonoTODO]
		// See XQuery & XPath 2.0 functions & operators section 17.
		public static bool CanConvert (XPathItem item, XmlSchemaType schemaTypeDest)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
			if (schemaTypeDest == null)
				throw new ArgumentNullException ("schemaTypeDest");
			XmlTypeCode src = item.XmlType.TypeCode;
			XmlTypeCode dst = schemaTypeDest.TypeCode;

			// Notation cannot be converted from other than Notation
			if (src == XmlTypeCode.Notation && dst != XmlTypeCode.Notation)
				return false;

			// untypedAtomic and string are convertable unless source type is QName.
			switch (dst) {
			case XmlTypeCode.UntypedAtomic:
			case XmlTypeCode.String:
				return src != XmlTypeCode.QName;
			}

			switch (src) {
			case XmlTypeCode.None:
			case XmlTypeCode.Item:
			case XmlTypeCode.Node:
			case XmlTypeCode.Document:
			case XmlTypeCode.Element:
			case XmlTypeCode.Attribute:
			case XmlTypeCode.Namespace:
			case XmlTypeCode.ProcessingInstruction:
			case XmlTypeCode.Comment:
			case XmlTypeCode.Text:
				throw new NotImplementedException (); // FIXME: check what happens

			case XmlTypeCode.AnyAtomicType:
				throw new NotImplementedException (); // FIXME: check what happens
			case XmlTypeCode.UntypedAtomic:
			case XmlTypeCode.String:
				// 'M'
				throw new NotImplementedException (); // FIXME: check what happens

			case XmlTypeCode.Boolean:
			case XmlTypeCode.Decimal:
				switch (dst) {
				case XmlTypeCode.Float:
				case XmlTypeCode.Double:
				case XmlTypeCode.Decimal:
				case XmlTypeCode.Boolean:
					return true;
				}
				return false;

			case XmlTypeCode.Float:
			case XmlTypeCode.Double:
				if (dst == XmlTypeCode.Decimal)
					// 'M'
					throw new NotImplementedException (); // FIXME: check what happens
				goto case XmlTypeCode.Decimal;

			case XmlTypeCode.Duration:
				switch (dst) {
				case XmlTypeCode.Duration:
				case XmlTypeCode.YearMonthDuration:
				case XmlTypeCode.DayTimeDuration:
					return true;
				}
				return false;

			case XmlTypeCode.DateTime:
				switch (dst) {
				case XmlTypeCode.DateTime:
				case XmlTypeCode.Time:
				case XmlTypeCode.Date:
				case XmlTypeCode.GYearMonth:
				case XmlTypeCode.GYear:
				case XmlTypeCode.GMonthDay:
				case XmlTypeCode.GDay:
				case XmlTypeCode.GMonth:
					return true;
				}
				return false;

			case XmlTypeCode.Time:
				switch (dst) {
				case XmlTypeCode.Time:
				case XmlTypeCode.Date:
					return true;
				}
				return false;

			case XmlTypeCode.Date:
				if (dst == XmlTypeCode.Time)
					return false;
				goto case XmlTypeCode.DateTime;

			case XmlTypeCode.GYearMonth:
			case XmlTypeCode.GYear:
			case XmlTypeCode.GMonthDay:
			case XmlTypeCode.GDay:
			case XmlTypeCode.GMonth:
				return src == dst;

			case XmlTypeCode.HexBinary:
			case XmlTypeCode.Base64Binary:
				if (src == dst)
					return true;
				switch (dst) {
				case XmlTypeCode.HexBinary:
				case XmlTypeCode.Base64Binary:
					return true;
				}
				return false;

			case XmlTypeCode.AnyUri:
			case XmlTypeCode.QName:
			case XmlTypeCode.Notation:
				return src == dst;

			case XmlTypeCode.NormalizedString:
			case XmlTypeCode.Token:
			case XmlTypeCode.Language:
			case XmlTypeCode.NmToken:
			case XmlTypeCode.Name:
			case XmlTypeCode.NCName:
			case XmlTypeCode.Id:
			case XmlTypeCode.Idref:
			case XmlTypeCode.Entity:
			case XmlTypeCode.Integer:
			case XmlTypeCode.NonPositiveInteger:
			case XmlTypeCode.NegativeInteger:
			case XmlTypeCode.Long:
			case XmlTypeCode.Int:
			case XmlTypeCode.Short:
			case XmlTypeCode.Byte:
			case XmlTypeCode.NonNegativeInteger:
			case XmlTypeCode.UnsignedLong:
			case XmlTypeCode.UnsignedInt:
			case XmlTypeCode.UnsignedShort:
			case XmlTypeCode.UnsignedByte:
			case XmlTypeCode.PositiveInteger:
				throw new NotImplementedException ();

			// xdt:*
			case XmlTypeCode.YearMonthDuration:
				if (dst == XmlTypeCode.DayTimeDuration)
					return false;
				goto case XmlTypeCode.Duration;
			case XmlTypeCode.DayTimeDuration:
				if (dst == XmlTypeCode.YearMonthDuration)
					return false;
				goto case XmlTypeCode.Duration;
			}

			throw new NotImplementedException ();
		}

		// Individual conversion

		public static string AnyUriToString (string value)
		{
			return value;
		}

		public static byte [] Base64BinaryToHexBinary (byte [] value)
		{
			return XmlConvert.FromBinHexString (Convert.ToBase64String (value));
		}

		public static string Base64BinaryToString (byte [] value)
		{
			return Convert.ToBase64String (value);
		}

		public static decimal BooleanToDecimal (bool value)
		{
			return Convert.ToDecimal (value);
		}

		public static double BooleanToDouble (bool value)
		{
			return Convert.ToDouble (value);
		}

		public static float BooleanToFloat (bool value)
		{
			return Convert.ToSingle (value);
		}

		public static int BooleanToInt (bool value)
		{
			return Convert.ToInt32 (value);
		}

		public static long BooleanToInteger (bool value)
		{
			return Convert.ToInt64 (value);
		}

		public static string BooleanToString (bool value)
		{
			// It looks not returning "True"
			return value ? "true" : "false";
		}

		[MonoTODO]
		public static DateTime DateTimeToDate (DateTime value)
		{
			return value.Date;
		}

		[MonoTODO]
		public static DateTime DateTimeToGDay (DateTime value)
		{
			return new DateTime (0, 0, value.Day);
		}

		[MonoTODO]
		public static DateTime DateTimeToGMonth (DateTime value)
		{
			return new DateTime (0, value.Month, 0);
		}

		[MonoTODO]
		public static DateTime DateTimeToGYear (DateTime value)
		{
			return new DateTime (value.Year, 0, 0);
		}

		[MonoTODO]
		public static DateTime DateTimeToGYearMonth (DateTime value)
		{
			return new DateTime (value.Year, value.Month, 0);
		}

		[MonoTODO]
		public static DateTime DateToDateTime (DateTime value)
		{
			return value.Date;
		}

		[MonoTODO]
		public static DateTime DateToGDay (DateTime value)
		{
			return new DateTime (0, 0, value.Day);
		}

		[MonoTODO]
		public static DateTime DateToGMonth (DateTime value)
		{
			return new DateTime (0, value.Month, 0);
		}

		[MonoTODO]
		public static DateTime DateToGYear (DateTime value)
		{
			return new DateTime (value.Year, 0, 0);
		}

		[MonoTODO]
		public static DateTime DateToGYearMonth (DateTime value)
		{
			return new DateTime (value.Year, value.Month, 0);
		}

		[MonoTODO]
		public static string DateToString (DateTime value)
		{
			return XmlConvert.ToString (value);
		}

		[MonoTODO]
		public static string DateTimeToString (DateTime value)
		{
			return XmlConvert.ToString (value);
		}

		[MonoTODO]
		public static string DayTimeDurationToDuration (TimeSpan value)
		{
			return XmlConvert.ToString (value);
		}

		[MonoTODO]
		public static string DayTimeDurationToString (TimeSpan value)
		{
			return DayTimeDurationToDuration (value);
		}

		[MonoTODO]
		public static bool DecimalToBoolean (decimal value)
		{
			return value != 0;
		}

		[MonoTODO]
		public static double DecimalToDouble (decimal value)
		{
			return (double) value;
		}

		[MonoTODO]
		public static float DecimalToFloat (decimal value)
		{
			return (float) value;
		}

		[MonoTODO]
		public static int DecimalToInt (decimal value)
		{
			return (int) value;
		}

		[MonoTODO]
		public static long DecimalToInteger (decimal value)
		{
			return (long) value;
		}

		[MonoTODO] // what if value was negative?
		public static decimal DecimalToNonNegativeInteger (decimal value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO] // what if value was positive?
		public static decimal DecimalToNonPositiveInteger (decimal value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string DecimalToString (decimal value)
		{
			return XmlConvert.ToString (value);
		}

		[MonoTODO]
		public static bool DoubleToBoolean (double value)
		{
			return value != 0;
		}

		[MonoTODO]
		public static decimal DoubleToDecimal (double value)
		{
			return (decimal) value;
		}

		[MonoTODO]
		public static float DoubleToFloat (double value)
		{
			return (float) value;
		}

		[MonoTODO]
		public static int DoubleToInt (double value)
		{
			return (int) value;
		}

		[MonoTODO]
		public static long DoubleToInteger (double value)
		{
			return (long) value;
		}

		[MonoTODO] // what if value was negative?
		public static decimal DoubleToNonNegativeInteger (double value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO] // what if value was positive?
		public static decimal DoubleToNonPositiveInteger (double value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string DoubleToString (double value)
		{
			return XmlConvert.ToString (value);
		}

		[MonoTODO]
		public static TimeSpan DurationToDayTimeDuration (string value)
		{
			return XmlConvert.ToTimeSpan (value);
		}

		[MonoTODO]
		public static string DurationToString (string value)
		{
			return XmlConvert.ToString (XmlConvert.ToTimeSpan (value));
		}

		[MonoTODO]
		public static TimeSpan DurationToYearMonthDuration (string value)
		{
			return XmlConvert.ToTimeSpan (value);
		}


		[MonoTODO]
		public static bool FloatToBoolean (float value)
		{
			return value != 0;
		}

		[MonoTODO]
		public static decimal FloatToDecimal (float value)
		{
			return (decimal) value;
		}

		[MonoTODO]
		public static double FloatToDouble (float value)
		{
			return (double) value;
		}

		[MonoTODO]
		public static int FloatToInt (float value)
		{
			return (int) value;
		}

		[MonoTODO]
		public static long FloatToInteger (float value)
		{
			return (long) value;
		}

		[MonoTODO] // what if value was negative?
		public static decimal FloatToNonNegativeInteger (float value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO] // what if value was positive?
		public static decimal FloatToNonPositiveInteger (float value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string FloatToString (float value)
		{
			return XmlConvert.ToString (value);
		}

		[MonoTODO]
		public static string GDayToString (DateTime value)
		{
			return XmlConvert.ToString (TimeSpan.FromDays (value.Day));
		}

		[MonoTODO]
		public static string GMonthDayToString (DateTime value)
		{
			return XmlConvert.ToString (new TimeSpan (value.Day, value.Hour, value.Minute, value.Second));
		}

		[MonoTODO]
		public static string GMonthToString (DateTime value)
		{
			return XmlConvert.ToString (new TimeSpan (0, value.Month, 0));
		}

		[MonoTODO]
		public static string GYearMonthToString (DateTime value)
		{
			return XmlConvert.ToString (new TimeSpan (value.Year, value.Month, 0));
		}

		[MonoTODO]
		public static string GYearToString (DateTime value)
		{
			return XmlConvert.ToString (new TimeSpan (new DateTime (value.Year, 0, 0).Ticks));
		}

		public static string HexBinaryToString (byte [] data)
		{
			return XmlConvert.ToBinHexString (data);
		}

		[MonoTODO]
		public static byte [] HexBinaryToBase64Binary (byte [] data)
		{
			return data;//XmlConvert.ToBinHexString (data);
		}


		[MonoTODO]
		public static bool IntegerToBoolean (long value)
		{
			return value != 0;
		}

		[MonoTODO]
		public static decimal IntegerToDecimal (long value)
		{
			return (decimal) value;
		}

		[MonoTODO]
		public static double IntegerToDouble (long value)
		{
			return (double) value;
		}

		[MonoTODO]
		public static float IntegerToFloat (long value)
		{
			return (float) value;
		}

		[MonoTODO]
		public static int IntegerToInt (long value)
		{
			return (int) value;
		}

		[MonoTODO]
		public static string IntegerToString (long value)
		{
			return XmlConvert.ToString (value);
		}

		[MonoTODO]
		public static bool IntToBoolean (int value)
		{
			return value != 0;
		}

		[MonoTODO]
		public static decimal IntToDecimal (int value)
		{
			return (decimal) value;
		}

		[MonoTODO]
		public static double IntToDouble (int value)
		{
			return (double) value;
		}

		[MonoTODO]
		public static float IntToFloat (int value)
		{
			return (float) value;
		}

		[MonoTODO]
		public static long IntToInteger (int value)
		{
			return (long) value;
		}

		[MonoTODO]
		public static string IntToString (int value)
		{
			return XmlConvert.ToString (value);
		}

		[MonoTODO]
		public static string NonNegativeIntegerToString (decimal value)
		{
			return XmlConvert.ToString (value);
		}

		[MonoTODO]
		public static string NonPositiveIntegerToString (decimal value)
		{
			return XmlConvert.ToString (value);
		}

		[MonoTODO]
		public static DateTime TimeToDateTime (DateTime value)
		{
			return value;
		}

		[MonoTODO]
		public static string TimeToString (DateTime value)
		{
			return XmlConvert.ToString (value, "HH:mm:ssZ");
		}

		[MonoTODO]
		public static string YearMonthDurationToDuration (TimeSpan value)
		{
			return XmlConvert.ToString (value);
		}

		[MonoTODO]
		public static string YearMonthDurationToString (TimeSpan value)
		{
			return YearMonthDurationToDuration (value);
		}

		[MonoTODO]
		public static string StringToAnyUri (string value)
		{
			return value;
		}

		[MonoTODO]
		public static byte [] StringToBase64Binary (string value)
		{
			return Convert.FromBase64String (value);
		}

		[MonoTODO]
		public static bool StringToBoolean (string value)
		{
			return XmlConvert.ToBoolean (value);
		}

		[MonoTODO]
		public static DateTime StringToDate (string value)
		{
			return XmlConvert.ToDateTime (value);
		}

		[MonoTODO]
		public static DateTime StringToDateTime (string value)
		{
			return XmlConvert.ToDateTime (value);
		}

		[MonoTODO]
		public static TimeSpan StringToDayTimeDuration (string value)
		{
			return XmlConvert.ToTimeSpan (value);
		}

		[MonoTODO]
		public static decimal StringToDecimal (string value)
		{
			return XmlConvert.ToDecimal (value);
		}

		[MonoTODO]
		public static double StringToDouble (string value)
		{
			return XmlConvert.ToDouble (value);
		}

		[MonoTODO]
		public static string StringToDuration (string value)
		{
			return XmlConvert.ToString (XmlConvert.ToTimeSpan (value));
		}

		[MonoTODO]
		public static float StringToFloat (string value)
		{
			return XmlConvert.ToSingle (value);
		}

		[MonoTODO]
		public static DateTime StringToGDay (string value)
		{
			return XmlConvert.ToDateTime (value);
		}

		[MonoTODO]
		public static DateTime StringToGMonth (string value)
		{
			return XmlConvert.ToDateTime (value);
		}

		[MonoTODO]
		public static DateTime StringToGMonthDay (string value)
		{
			return XmlConvert.ToDateTime (value);
		}

		[MonoTODO]
		public static DateTime StringToGYear (string value)
		{
			return XmlConvert.ToDateTime (value);
		}

		[MonoTODO]
		public static DateTime StringToGYearMonth (string value)
		{
			return XmlConvert.ToDateTime (value);
		}

		[MonoTODO]
		public static byte [] StringToHexBinary (string value)
		{
			return XmlConvert.FromBinHexString (value);
		}

		[MonoTODO]
		public static int StringToInt (string value)
		{
			return XmlConvert.ToInt32 (value);
		}

		[MonoTODO]
		public static long StringToInteger (string value)
		{
			return XmlConvert.ToInt64 (value);
		}

		[MonoTODO]
		public static decimal StringToNonNegativeInteger (string value)
		{
			return XmlConvert.ToDecimal (value);
		}

		[MonoTODO]
		public static decimal StringToNonPositiveInteger (string value)
		{
			return XmlConvert.ToDecimal (value);
		}

		[MonoTODO]
		public static DateTime StringToTime (string value)
		{
			return XmlConvert.ToDateTime (value);
		}

		[MonoTODO]
		public static long StringToUnsignedInt (string value)
		{
			return XmlConvert.ToInt32 (value);
		}

		[MonoTODO]
		public static decimal StringToUnsignedLong (string value)
		{
			return XmlConvert.ToInt32 (value);
		}

		[MonoTODO]
		public static int StringToUnsignedShort (string value)
		{
			return XmlConvert.ToInt32 (value);
		}

		[MonoTODO]
		public static TimeSpan StringToYearMonthDuration (string value)
		{
			return XmlConvert.ToTimeSpan (value);
		}

		[MonoTODO]
		public static string ItemToAnyUri (XPathItem value)
		{
			return value.Value;
		}

		[MonoTODO]
		public static byte [] ItemToBase64Binary (XPathItem value)
		{
			return Convert.FromBase64String (value.Value);
		}

		[MonoTODO]
		public static bool ItemToBoolean (XPathItem value)
		{
			return XmlConvert.ToBoolean (value.Value);
		}

		[MonoTODO]
		public static DateTime ItemToDate (XPathItem value)
		{
			return XmlConvert.ToDateTime (value.Value);
		}

		[MonoTODO]
		public static DateTime ItemToDateTime (XPathItem value)
		{
			return XmlConvert.ToDateTime (value.Value);
		}

		[MonoTODO]
		public static TimeSpan ItemToDayTimeDuration (XPathItem value)
		{
			return XmlConvert.ToTimeSpan (value.Value);
		}

		[MonoTODO]
		public static decimal ItemToDecimal (XPathItem value)
		{
			return XmlConvert.ToDecimal (value.Value);
		}

		[MonoTODO]
		public static double ItemToDouble (XPathItem value)
		{
			return XmlConvert.ToDouble (value.Value);
		}

		[MonoTODO]
		public static string ItemToDuration (XPathItem value)
		{
			return XmlConvert.ToString (XmlConvert.ToTimeSpan (value.Value));
		}

		[MonoTODO]
		public static float ItemToFloat (XPathItem value)
		{
			return XmlConvert.ToSingle (value.Value);
		}

		[MonoTODO]
		public static DateTime ItemToGDay (XPathItem value)
		{
			return XmlConvert.ToDateTime (value.Value);
		}

		[MonoTODO]
		public static DateTime ItemToGMonth (XPathItem value)
		{
			return XmlConvert.ToDateTime (value.Value);
		}

		[MonoTODO]
		public static DateTime ItemToGMonthDay (XPathItem value)
		{
			return XmlConvert.ToDateTime (value.Value);
		}

		[MonoTODO]
		public static DateTime ItemToGYear (XPathItem value)
		{
			return XmlConvert.ToDateTime (value.Value);
		}

		[MonoTODO]
		public static DateTime ItemToGYearMonth (XPathItem value)
		{
			return XmlConvert.ToDateTime (value.Value);
		}

		[MonoTODO]
		public static byte [] ItemToHexBinary (XPathItem value)
		{
			return XmlConvert.FromBinHexString (value.Value);
		}

		[MonoTODO]
		public static int ItemToInt (XPathItem value)
		{
			return XmlConvert.ToInt32 (value.Value);
		}

		[MonoTODO]
		public static long ItemToInteger (XPathItem value)
		{
			return XmlConvert.ToInt64 (value.Value);
		}

		[MonoTODO]
		public static XPathItem ItemToItem (XPathItem value, XmlSchemaType schemaTypeDest)
		{
			return new XPathAtomicValue (value.Value, schemaTypeDest);
		}

		[MonoTODO]
		public static decimal ItemToNonNegativeInteger (XPathItem value)
		{
			return XmlConvert.ToDecimal (value.Value);
		}

		[MonoTODO]
		public static decimal ItemToNonPositiveInteger (XPathItem value)
		{
			return XmlConvert.ToDecimal (value.Value);
		}

		[MonoTODO]
		public static XmlQualifiedName ItemToQName (XPathItem value)
		{
			return (XmlQualifiedName) value.TypedValue;
		}

		[MonoTODO]
		public static string ItemToString (XPathItem value)
		{
			if (value.ValueType == typeof (DateTime))
				return XmlConvert.ToString ((DateTime) value.TypedValue);
			if (value.TypedValue is XmlQualifiedName)
				throw new ArgumentException ("Invalid cast from schema QName type to string type.");
			return value.Value;
		}

		[MonoTODO]
		public static DateTime ItemToTime (XPathItem value)
		{
			return XmlConvert.ToDateTime (value.Value);
		}

		[MonoTODO]
		public static long ItemToUnsignedInt (XPathItem value)
		{
			return XmlConvert.ToInt32 (value.Value);
		}

		[MonoTODO]
		public static decimal ItemToUnsignedLong (XPathItem value)
		{
			return XmlConvert.ToInt32 (value.Value);
		}

		[MonoTODO]
		public static int ItemToUnsignedShort (XPathItem value)
		{
			return XmlConvert.ToInt32 (value.Value);
		}

		[MonoTODO]
		public static TimeSpan ItemToYearMonthDuration (XPathItem value)
		{
			return XmlConvert.ToTimeSpan (value.Value);
		}
	}
}

#endif
