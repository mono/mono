// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
// XmlConvert.cs: Xml data type conversion
// 
using System;
using System.Globalization;

namespace System.Xml {

	public class XmlConvert {

		public XmlConvert()
		{}

		// Methods
		[MonoTODO]
		public static string DecodeName(string name)
		{
			return null;
		}
		[MonoTODO]
		public static string EncodeLocalName(string name)
		{
			return null;
		}
		[MonoTODO]
		public static string EncodeName(string name)
		{
			return null;
		}
		[MonoTODO]
		public static string EncodeNmToken(string name)
		{
			return null;
		}

		// {true, false, 1, 0}
		public static bool ToBoolean(string s)
		{
			s = s.Trim();
			switch(s)
			{
				case "1":
					return true;
				case "true":
					return true;
				case "0":
					return false;
				case "false":
					return false;
				default:
					throw new FormatException(s + " is not a valid boolean value");
			}
		}

		public static byte ToByte(string s)
		{
			return Byte.Parse(s, CultureInfo.InvariantCulture);
		}

		public static char ToChar(string s)
		{
			return char.Parse(s);
		}

		[MonoTODO]
		public static DateTime ToDateTime(string s)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static DateTime ToDateTime(string s, string format)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public static DateTime ToDateTime(string s, string[] formats)
		{
			throw new NotImplementedException();
		}
		
		public static Decimal ToDecimal(string s)
		{
			return Decimal.Parse(s, CultureInfo.InvariantCulture);
		}
		
		public static double ToDouble(string s)
		{
			return Double.Parse(s, CultureInfo.InvariantCulture);
		}

		public static Guid ToGuid(string s)
		{
			return new Guid(s);
		}

		public static short ToInt16(string s)
		{
			return Int16.Parse(s, CultureInfo.InvariantCulture);
		}

		public static int ToInt32(string s)
		{
			return Int32.Parse(s, CultureInfo.InvariantCulture);
		}

		public static long ToInt64(string s)
		{
			return Int64.Parse(s, CultureInfo.InvariantCulture);
		}

		[CLSCompliant (false)]
		public static SByte ToSByte(string s)
		{
			return SByte.Parse(s, CultureInfo.InvariantCulture);
		}

		public static float ToSingle(string s)
		{
			return Single.Parse(s, CultureInfo.InvariantCulture);
		}

		public static string ToString(Guid value)
		{
			return value.ToString("D",CultureInfo.InvariantCulture);
		}

		public static string ToString(int value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString(short value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString(byte value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString(long value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString(char value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString(bool value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		[CLSCompliant (false)]
		public static string ToString(SByte value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}
		public static string ToString(Decimal value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		[CLSCompliant (false)]
		public static string ToString(UInt64 value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}
		public static string ToString(TimeSpan value)
		{
			return value.ToString();
		}
		public static string ToString(double value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}
		public static string ToString(float value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		[CLSCompliant (false)]
		public static string ToString(UInt32 value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		[CLSCompliant (false)]
		public static string ToString(UInt16 value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}
		public static string ToString(DateTime value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}
		public static string ToString(DateTime value, string format)
		{
			return value.ToString(format, CultureInfo.InvariantCulture);
		}
		public static TimeSpan ToTimeSpan(string s)
		{
			return TimeSpan.Parse(s);
		}

		[CLSCompliant (false)]
		public static UInt16 ToUInt16(string s)
		{
			return UInt16.Parse(s, CultureInfo.InvariantCulture);
		}

		[CLSCompliant (false)]
		public static UInt32 ToUInt32(string s)
		{
			return UInt32.Parse(s, CultureInfo.InvariantCulture);
		}

		[CLSCompliant (false)]
		public static UInt64 ToUInt64(string s)
		{
			return UInt64.Parse(s, CultureInfo.InvariantCulture);
		}

		public static string VerifyName(string name)
		{
			Exception innerEx;
			if(name == null)
				throw new ArgumentNullException("name");

			if(XmlConstructs.IsValidName(name, out innerEx))
				return name;
			
			throw new XmlException("'"+name+"' is not a valid XML Name",null);
		}

		public static string VerifyNCName(string ncname)
		{
			Exception innerEx;
			if(ncname == null)
				throw new ArgumentNullException("ncname");

			if(XmlConstructs.IsValidName(ncname, out innerEx))
				return ncname;
			
			throw new XmlException("'"+ncname+"' is not a valid XML NCName",innerEx);
		}
	}
}
