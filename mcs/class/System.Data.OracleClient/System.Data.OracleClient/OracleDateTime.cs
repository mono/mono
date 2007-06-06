//
// OracleDateTime.cs 
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Authors: Tim Coleman <tim@timcoleman.com>
//          Daniel Morgan <danielmorgan@verizon.net>
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Daniel Morgan, 2005
//
// Licensed under the MIT/X11 License.
//

using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.Text;

namespace System.Data.OracleClient {
	public struct OracleDateTime : IComparable, INullable
	{
		#region Fields

		public static readonly OracleDateTime MaxValue = new OracleDateTime (4712, 12, 31);
		public static readonly OracleDateTime MinValue = new OracleDateTime (1, 1, 1);
		public static readonly OracleDateTime Null = new OracleDateTime ();

		DateTime value;
		bool notNull;

		#endregion // Fields

		#region Constructors

		public OracleDateTime (DateTime dt)
		{
			value = dt; 
			notNull = true;
		}

		public OracleDateTime (long ticks)
			: this (new DateTime (ticks))
		{
		}

		public OracleDateTime (OracleDateTime from)
			: this (from.Value)
		{
		}

		public OracleDateTime (int year, int month, int day)
			: this (new DateTime (year, month, day))
		{
		}

		public OracleDateTime (int year, int month, int day, Calendar calendar)
			: this (new DateTime (year, month, day, calendar))
		{
		}

		public OracleDateTime (int year, int month, int day, int hour, int minute, int second)
			: this (new DateTime (year, month, day, hour, minute, second))
		{
		}

		public OracleDateTime (int year, int month, int day, int hour, int minute, int second, Calendar calendar)
			: this (new DateTime (year, month, day, hour, minute, second, calendar))
		{
		}

		public OracleDateTime (int year, int month, int day, int hour, int minute, int second, int millisecond)
			: this (new DateTime (year, month, day, hour, minute, second, millisecond))
		{
		}

		public OracleDateTime (int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar)
			: this (new DateTime (year, month, day, hour, minute, second, millisecond, calendar))
		{
		}

		#endregion // Constructors

		#region Properties

		public int Day {
			get { return value.Day; }
		}

		public int Hour {
			get { return value.Hour; }
		}

		public bool IsNull {
			get { return !notNull; }
		}

		public int Millisecond {
			get { return value.Millisecond; }
		}

		public int Minute {
			get { return value.Minute; }
		}

		public int Month {
			get { return value.Month; }
		}

		public int Second {
			get { return value.Second; }
		}

		public DateTime Value {
			get { return value; }
		}

		public int Year {
			get { return value.Year; }
		}

		#endregion // Properties

		#region Methods

		public int CompareTo (object obj)
		{
			OracleDateTime o = (OracleDateTime) obj;
			if (obj == null)
				throw new NullReferenceException ("Object reference not set to an instance of an object");
			else if (!(obj is OracleDateTime))
				throw new ArgumentException ("Value is not a System.Data.OracleClient.OracleDateTime", obj.ToString ());
			else if (o.IsNull && this.IsNull)
				return 0;
			else if (o.IsNull && !(this.IsNull))
				return 1;
			else
				return value.CompareTo (o.Value);
		}

		public override bool Equals (object value)
		{
			if (value is OracleDateTime)
			{
				OracleDateTime d = (OracleDateTime) value;
				if (!(this.IsNull) && !(d.IsNull))
					return this.value == d.value;
				else
					throw new InvalidOperationException ("The value is null");
			}
			return false;
		}

		public static OracleBoolean Equals (OracleDateTime x, OracleDateTime y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value == y.Value);
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		public static OracleBoolean GreaterThan (OracleDateTime x, OracleDateTime y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value > y.Value);
		}

		public static OracleBoolean GreaterThanOrEqual (OracleDateTime x, OracleDateTime y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value >= y.Value);
		}

		public static OracleBoolean LessThan (OracleDateTime x, OracleDateTime y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value < y.Value);
		}

		public static OracleBoolean LessThanOrEqual (OracleDateTime x, OracleDateTime y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value <= y.Value);
		}

		public static OracleBoolean NotEquals (OracleDateTime x, OracleDateTime y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value != y.Value);
		}

		public static OracleDateTime Parse (string s)
		{
			return new OracleDateTime (DateTime.Parse (s));
		}

		public override string ToString ()
		{
			if (IsNull)
				return "Null";
			return Value.ToString ();
		}

		#endregion // Methods

		#region Operators and Type Conversions

		public static OracleBoolean operator == (OracleDateTime x, OracleDateTime y)
		{
			return Equals (x, y);
		}

		public static OracleBoolean operator > (OracleDateTime x, OracleDateTime y)
		{
			return GreaterThan (x, y);
		}

		public static OracleBoolean operator >= (OracleDateTime x, OracleDateTime y)
		{
			return GreaterThanOrEqual (x, y);
		}

		public static OracleBoolean operator != (OracleDateTime x, OracleDateTime y)
		{
			return NotEquals (x, y);
		}

		public static OracleBoolean operator < (OracleDateTime x, OracleDateTime y)
		{
			return LessThan (x, y);
		}

		public static OracleBoolean operator <= (OracleDateTime x, OracleDateTime y)
		{
			return LessThanOrEqual (x, y);
		}

		public static explicit operator DateTime (OracleDateTime x)
		{
			return x.Value;
		}

		public static explicit operator OracleDateTime (string x)
		{
			return new OracleDateTime (DateTime.Parse (x));
		}

		private static string ConvertSystemDatePartToOracleDate (string sysPart) 
		{
			if (sysPart.Length == 0)
				return "";
			else {
				switch (sysPart) {
				case "tt":  
					return "AM";
				case "yy":
					return "YY";
				case "yyyy":
					return "YYYY";
				case "MM":
					return "MM";
				case "MON":
					return "MMM";
				case "MMMM":
					return "MONTH";
				case "dd":
					return "DD";
				case "ddd":
					return "DY";
				case "dddd":
					return "DAY";
				case "hh":
					return "HH";
				case "HH":
					return "HH24";
				case "mm": 
					return "MI";
				case "ss":
					return "SS";
				default:
					// ignore any others?
					return "";
				}
			}
		}

		private static string ConvertOracleDatePartToSystemDatePart (string oraPart) 
		{
			if (oraPart.Length == 0)
				return "";
			else {
				switch (oraPart) {
				case "AM":  
				case "PM":
					// TODO: need to handle "A.M." and "P.M."
					return "tt";
				case "RR": // TODO: handle special year RR.  for now, treat it like yy
					return "yy";
				case "YY":
					return "yy";
				case "YYYY":
					return "yyyy";
				case "MM":
					return "MM";
				case "MON":
					return "MMM";
				case "MONTH":
					return "MMMM";
				case "DD":
					return "dd";
				case "DY":
					return "ddd";
				case "DAY":
					return "dddd";
				case "HH":
				case "HH12":
					return "hh";
				case "HH24":
					return "HH";
				case "MI": 
					return "mm";
				case "SS":
					return "ss";
				default:
					// ignore any others?
					return "";
				}
			}
		}

		internal static string ConvertSystemDateTimeFormatToOracleDate (string sysFormat) 
		{
			if (sysFormat == String.Empty)
				return String.Empty;

			char[] chars = sysFormat.ToCharArray ();

			StringBuilder sb = new StringBuilder ();
			StringBuilder sfinal = new StringBuilder ();

			int i = 0;
			bool inQuote = false;
			char quoteChar = '\"';
			string sPart;
		
			for (i = 0; i < chars.Length; i++) {
				char ch = chars[i];
				
				if (inQuote == true) {
					sfinal.Append (ch);
					if (ch == quoteChar)
						inQuote = false;
				}
				else {
					switch (ch) {
					case ' ':
					case '.':
					case ',':
					case '/':
					case '-':
					case ':':
						if (sb.Length > 0) {
							sPart = ConvertSystemDatePartToOracleDate (sb.ToString ());
							if (sPart.Length > 0)
								sfinal.Append (sPart);
						}
						sb = null;
						sb = new StringBuilder ();
						sfinal.Append (ch);
						break;
					case '\"':
						sfinal.Append (ch);
						inQuote = true;
						quoteChar = '\"';
						break;
					default:
						sb.Append (ch);
						break;
					}
				}
			}

			if (sb.Length > 0) {
				sPart = ConvertSystemDatePartToOracleDate (sb.ToString ());
				if (sPart.Length > 0)
					sfinal.Append (sPart);
				sb = null;
			}
			string returnStr = sfinal.ToString ();
			sfinal = null;
			return returnStr;
		}

		internal static string ConvertOracleDateFormatToSystemDateTime (string oraFormat) 
		{
			if (oraFormat == String.Empty)
				return String.Empty;

			char[] chars = oraFormat.ToCharArray ();

			StringBuilder sb = new StringBuilder ();
			StringBuilder sfinal = new StringBuilder ();

			int i = 0;
			bool inQuote = false;
			char quoteChar = '\"';
			string sPart;
		
			for (i = 0; i < chars.Length; i++) {
				char ch = chars[i];
				
				if (inQuote == true) {
					sfinal.Append (ch);
					if (ch == quoteChar)
						inQuote = false;
				}
				else {
					switch (ch) {
					case ' ':
					case '.':
					case ',':
					case '/':
					case '-':
					case ':':
						if (sb.Length > 0) {
							sPart = ConvertOracleDatePartToSystemDatePart (sb.ToString ());
							if (sPart.Length > 0)
								sfinal.Append (sPart);
						}
						sb = null;
						sb = new StringBuilder ();
						sfinal.Append (ch);
						break;
					case '\"':
						sfinal.Append (ch);
						inQuote = true;
						quoteChar = '\"';
						break;
					default:
						sb.Append (ch);
						break;
					}
				}
			}

			if (sb.Length > 0) {
				sPart = ConvertOracleDatePartToSystemDatePart (sb.ToString ());
				if (sPart.Length > 0)
					sfinal.Append (sPart);
				sb = null;
			}
			string returnStr = sfinal.ToString ();
			sfinal = null;
			return returnStr;
		}

		#endregion // Operators and Type Conversions
	}
}

