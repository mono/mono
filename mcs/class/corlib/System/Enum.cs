//
// System.Enum.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Nick Drochak (ndrochak@gol.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO: Mucho left to implement.
//

using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System {
	internal struct MonoEnumInfo {
		internal Type utype;
		internal Array values;
		internal string[] names;
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void get_enum_info (Type enumType, out MonoEnumInfo info);
		
		internal static void GetInfo (Type enumType, out MonoEnumInfo info) {
			get_enum_info (enumType, out info);
			Array.Sort (info.values, info.names);
		}
	};

	[MonoTODO]
	public abstract class Enum : ValueType, IComparable, IConvertible, IFormattable {

		// IConvertible methods Start -->

		[CLSCompliant(false)]
		public TypeCode GetTypeCode () {
			MonoEnumInfo info;
			MonoEnumInfo.GetInfo (this.GetType (), out info);
			return Type.GetTypeCode (info.utype);
		}

		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			return Convert.ToBoolean (get_value (), provider);
		}

		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return Convert.ToByte (get_value (), provider);
		}

		char IConvertible.ToChar (IFormatProvider provider)
		{
			return Convert.ToChar (get_value (), provider);
		}

		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			return Convert.ToDateTime (get_value (), provider);
		}

		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{	
			return Convert.ToDecimal (get_value (), provider);
		}

		double IConvertible.ToDouble (IFormatProvider provider)
		{	
			return Convert.ToDouble (get_value (), provider);
		}

		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			return Convert.ToInt16 (get_value (), provider);
		}

		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			return Convert.ToInt32 (get_value (), provider);
		}

		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			return Convert.ToInt64 (get_value (), provider);
		}

    		[CLSCompliant(false)]
		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return Convert.ToSByte (get_value (), provider);
		}

		float IConvertible.ToSingle (IFormatProvider provider)
		{
			return Convert.ToSingle (get_value (), provider);
		}

		[MonoTODO]
		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}
		
    		[CLSCompliant(false)]
		public ushort ToUInt16 (IFormatProvider provider)
		{
			return Convert.ToUInt16 (get_value (), provider);
		}

    		[CLSCompliant(false)]
		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return Convert.ToUInt32 (get_value (), provider);
		}

    		[CLSCompliant(false)]
		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return Convert.ToUInt64 (get_value (), provider);
		}

		// <-- End IConvertible methods

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern object get_value ();
		
		public static Array GetValues (Type enumType) {
			if (null == enumType)
				throw new ArgumentNullException ("enumType cannot be null.");

			if (!enumType.IsEnum)
				throw new ArgumentException ("enumType is not an Enum type.");

			MonoEnumInfo info;
			MonoEnumInfo.GetInfo (enumType, out info);
			return info.values;
		}
		
		public static string[] GetNames (Type enumType) {
			if (null == enumType)
				throw new ArgumentNullException ("enumType cannot be null.");

			if (!enumType.IsEnum)
				throw new ArgumentException ("enumType is not an Enum type.");

			MonoEnumInfo info;
			MonoEnumInfo.GetInfo (enumType, out info);
			return info.names;
		}
		
		public static string GetName (Type enumType, object value) {
			if (null == enumType)
				throw new ArgumentNullException ("enumType cannot be null.");
			if (null == value)
				throw new ArgumentNullException ("value cannot be null.");

			if (!enumType.IsEnum)
				throw new ArgumentException ("enumType is not an Enum type.");

			MonoEnumInfo info;
			int i;
			value = ToObject (enumType, value);
			MonoEnumInfo.GetInfo (enumType, out info);
			for (i = 0; i < info.values.Length; ++i) {				
				if (value.Equals (info.values.GetValue (i)))
					return info.names [i];
			}
			return null;
		}
		
		public static bool IsDefined (Type enumType, object value) {
			if (null == enumType)
				throw new ArgumentNullException ("enumType cannot be null.");
			if (null == value)
				throw new ArgumentNullException ("value cannot be null.");

			if (!enumType.IsEnum)
				throw new ArgumentException ("enumType is not an Enum type.");

			MonoEnumInfo info;
			MonoEnumInfo.GetInfo (enumType, out info);

			Type vType = value.GetType ();
			if (vType == typeof(String)) {
				return ((IList)(info.names)).Contains (value);
			} else if ((vType == info.utype) || (vType == enumType)) {
				int i;
				value = ToObject (enumType, value);
				MonoEnumInfo.GetInfo (enumType, out info);
				for (i = 0; i < info.values.Length; ++i) {				
					if (value.Equals (info.values.GetValue (i)))
						return true;
				}
				return false;
			} else {
				throw new ArgumentException("The value parameter is not the correct type."
					+ "It must be type String or the same type as the underlying type"
					+ "of the Enum.");
			}
			

		}
		
		public static Type GetUnderlyingType (Type enumType) {
			if (null == enumType)
				throw new ArgumentNullException ("enumType cannot be null.");

			if (!enumType.IsEnum)
				throw new ArgumentException ("enumType is not an Enum type.");

			MonoEnumInfo info;
			MonoEnumInfo.GetInfo (enumType, out info);
			return info.utype;
		}

		public static object Parse (Type enumType, string value)
		{
			// Note: Parameters are checked in the other overload
			return Parse (enumType, value, false);
		}

		public static object Parse (Type enumType, string value, bool ignoreCase)
		{
			if (null == enumType)
				throw new ArgumentNullException ("enumType cannot be null.");

			if (null == value)
				throw new ArgumentNullException ("value cannot be null.");

			if (!enumType.IsEnum)
				throw new ArgumentException ("enumType is not an Enum type.");

			if (String.Empty == value.Trim())
				throw new ArgumentException ("value cannot be empty string.");

			MonoEnumInfo info;
			int i;
			MonoEnumInfo.GetInfo (enumType, out info);

			long retVal = 0;
			string[] names = value.Split(new char[] {','});
			for (i = 0; i < names.Length; ++i)
				names [i] = names [i].Trim ();
			TypeCode typeCode = ((Enum) info.values.GetValue (0)).GetTypeCode ();
			foreach (string name in names) {
				bool found = false;
				for (i = 0; i < info.values.Length; ++i) {				
					if (String.Compare (name, info.names [i], ignoreCase) == 0) {
						switch (typeCode) {
							case TypeCode.Byte:
								retVal |= (long)((byte)info.values.GetValue (i));
								break;
							case TypeCode.SByte:
								// use the unsigned version in the cast to avoid 
								// compiler warning
								retVal |= (long)((byte)(SByte)info.values.GetValue (i));
								break;
							case TypeCode.Int16:
								// use the unsigned version in the cast to avoid 
								// compiler warning
								retVal |= (long)((ushort)(short)info.values.GetValue (i));
								break;
							case TypeCode.Int32:
								// use the unsigned version in the cast to avoid 
								// compiler warning
								retVal |= (long)((uint)(int)info.values.GetValue (i));
								break;
							case TypeCode.Int64:
								retVal |= (long)info.values.GetValue (i);
								break;
							case TypeCode.UInt16:
								retVal |= (long)((UInt16)info.values.GetValue (i));
								break;
							case TypeCode.UInt32:
								retVal |= (long)((UInt32)info.values.GetValue (i));
								break;
							case TypeCode.UInt64:
								retVal |= (long)((UInt64)info.values.GetValue (i));
								break;
						}
						found = true;
						break;
					}
				}
				if (!found)
					throw new ArgumentException ("The requested value was not found");
				
			}
			return ToObject(enumType, retVal);
		}

		/// <summary>
		///   Compares the enum value with another enum value of the same type.
		/// </summary>
		///
		/// <remarks>
		///   
		public int CompareTo (object obj)
		{
			Type thisType;

			if (obj == null)
				return 1;

			thisType = this.GetType();
			if (obj.GetType() != thisType){
				throw new ArgumentException(
					"Object must be the same type as the "
					+ "enum. The type passed in was " 
					+ obj.GetType().ToString()
					+ "; the enum type was " 
					+ thisType.ToString() + ".");
			}

			object value1, value2;

			value1 = this.get_value ();
			value2 = ((Enum)obj).get_value();

			return ((IComparable)value1).CompareTo (value2);
		}
		
		public override string ToString ()
		{
			return ToString ("G", null);
		}

		public string ToString (IFormatProvider provider)
		{
			return ToString ("G", provider);
		}

		public string ToString (String format)
		{
			return ToString (format, null);
		}

		[MonoTODO]
		public string ToString (String format, IFormatProvider provider)
		{
			// provider is not used for Enums

			if (format == String.Empty || format == null){
				format = "G";
			}
			return Format (this.GetType(), this.get_value (), format);
		}

		public static object ToObject(Type enumType, byte value)
		{
			return ToObject (enumType, (object)value);
		}
		
		public static object ToObject(Type enumType, short value)
		{
			return ToObject (enumType, (object)value);
		}
		public static object ToObject(Type enumType, int value)
		{
			return ToObject (enumType, (object)value);
		}
		public static object ToObject(Type enumType, long value)
		{
			return ToObject (enumType, (object)value);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern object ToObject(Type enumType, object value);

		[CLSCompliant(false)]
		public static object ToObject(Type enumType, sbyte value)
		{
			return ToObject (enumType, (object)value);
		}
		[CLSCompliant(false)]
		public static object ToObject(Type enumType, ushort value)
		{
			return ToObject (enumType, (object)value);
		}
		[CLSCompliant(false)]
		public static object ToObject(Type enumType, uint value)
		{
			return ToObject (enumType, (object)value);
		}
		[CLSCompliant(false)]
		public static object ToObject(Type enumType, ulong value)
		{
			return ToObject (enumType, (object)value);
		}

		public override bool Equals (object obj)
		{
			if (null == obj)
				return false;

			if (obj.GetType() != this.GetType())
				return false;

			object v1 = this.get_value ();
			object v2 = ((Enum)obj).get_value ();

			return v1.Equals (v2);
		}

		public override int GetHashCode ()
		{
			object v = this.get_value ();
			return v.GetHashCode ();
		}

		private static string FormatSpecifier_X (Type enumType, object value)
		{
			// FIXME: Not sure if padding should always be with precision
			// 8, if it's culture specific, or what.  This works for me.
			const string format = "x8";

			switch (Type.GetTypeCode(enumType)) {
				case TypeCode.Char:
					// Char doesn't support ToString(format), so convert to an int and
					// use that...
					char v = (char) value;
					return Convert.ToInt32(v).ToString(format);
				case TypeCode.SByte:
					return ((sbyte)value).ToString(format);
				case TypeCode.Byte:
					return ((byte)value).ToString(format);
				case TypeCode.Int16:
					return ((short)value).ToString(format);
				case TypeCode.UInt16:
					return ((ushort)value).ToString(format);
				case TypeCode.Int32:
					return ((int)value).ToString(format);
				case TypeCode.UInt32:
					return ((uint)value).ToString(format);
				case TypeCode.Int64:
					return ((long)value).ToString(format);
				case TypeCode.UInt64:
					return ((ulong)value).ToString(format);
				default:
					throw new Exception ("invalid type code for enumeration");
			}
		}

		[MonoTODO]
		public static string Format (Type enumType, object value, string format)
		{
			if (null == enumType)
				throw new ArgumentNullException("enumType cannot be null");
			if (null == value)
				throw new ArgumentNullException("value cannot be null");
			if (null == format)
				throw new ArgumentNullException("format cannot be null");

			if (!enumType.IsEnum)
				throw new ArgumentException("enumType is not an Enum Type");
			
			Type vType = value.GetType();
			if (vType != enumType && vType != Enum.GetUnderlyingType(enumType))
				throw new ArgumentException();


			if (format.Length != 1)
				throw new FormatException ("Format String can be only \"G\",\"g\",\"X\"," + 
							  "\"x\",\"F\",\"f\",\"D\" or \"d\".");

			char formatChar = format [0];
			if ((formatChar == 'G' || formatChar == 'g') 
				&& Attribute.IsDefined(enumType, typeof(FlagsAttribute)))
				formatChar = 'F';

			string retVal = "";
			switch (formatChar) {
			    case 'G':
			    case 'g':
				retVal = GetName (enumType, value);
				if (retVal == null)
					retVal = value.ToString();
				break;
			    case 'X':
			    case 'x':
				retVal = FormatSpecifier_X (enumType, value);
				break;
			    case 'D':
			    case 'd':
				if (Enum.GetUnderlyingType (enumType) == typeof (ulong)) {
					ulong ulongValue = Convert.ToUInt64 (value);
					retVal = ulongValue.ToString ();
				} else {
					long longValue = Convert.ToInt64 (value);
					retVal = longValue.ToString ();
				}
				break;
			    case 'F':
			    case 'f':
				MonoEnumInfo info;
				MonoEnumInfo.GetInfo (enumType, out info);
				// This is ugly, yes.  We need to handle the different integer
				// types for enums.  If someone else has a better idea, be my guest.
				switch (((Enum)info.values.GetValue (0)).GetTypeCode()) {
					case TypeCode.Byte:
						byte byteFlag = (byte)value;
						byte byteenumValue;
						for (int i = info.values.Length-1; i>=0 && byteFlag != 0; i--) {
							byteenumValue = (byte)info.values.GetValue (i);
							if ((byteenumValue & byteFlag) == byteenumValue){
								retVal = info.names[i] + (retVal == String.Empty ? "" : ", ") + retVal;
								byteFlag -= byteenumValue;
							}
						}
						break;
					case TypeCode.SByte:
						SByte sbyteFlag = (SByte)value;
						SByte sbyteenumValue;
						for (int i = info.values.Length-1; i>=0 && sbyteFlag != 0; i--) {
							sbyteenumValue = (SByte)info.values.GetValue (i);
							if ((sbyteenumValue & sbyteFlag) == sbyteenumValue){
								retVal = info.names[i] + (retVal == String.Empty ? "" : ", ") + retVal;
								sbyteFlag -= sbyteenumValue;
							}
						}
						break;
					case TypeCode.Int16:
						short Int16Flag = (short)value;
						short Int16enumValue;
						for (int i = info.values.Length-1; i>=0 && Int16Flag != 0; i--) {
							Int16enumValue = (short)info.values.GetValue (i);
							if ((Int16enumValue & Int16Flag) == Int16enumValue){
								retVal = info.names[i] + (retVal == String.Empty ? "" : ", ") + retVal;
								Int16Flag -= Int16enumValue;
							}
						}
						break;
					case TypeCode.Int32:
						int Int32Flag = (int)value;
						int Int32enumValue;
						for (int i = info.values.Length-1; i>=0 && Int32Flag != 0; i--) {
							Int32enumValue = (int)info.values.GetValue (i);
							if ((Int32enumValue & Int32Flag) == Int32enumValue){
								retVal = info.names[i] + (retVal == String.Empty ? "" : ", ") + retVal;
								Int32Flag -= Int32enumValue;
							}
						}
						break;
					case TypeCode.Int64:
						long Int64Flag = (long)value;
						long Int64enumValue;
						for (int i = info.values.Length-1; i>=0 && Int64Flag != 0; i--) {
							Int64enumValue = (long)info.values.GetValue (i);
							if ((Int64enumValue & Int64Flag) == Int64enumValue){
								retVal = info.names[i] + (retVal == String.Empty ? "" : ", ") + retVal;
								Int64Flag -= Int64enumValue;
							}
						}
						break;
					case TypeCode.UInt16:
						UInt16 UInt16Flag = (UInt16)value;
						UInt16 UInt16enumValue;
						for (int i = info.values.Length-1; i>=0 && UInt16Flag != 0; i--) {
							UInt16enumValue = (UInt16)info.values.GetValue (i);
							if ((UInt16enumValue & UInt16Flag) == UInt16enumValue){
								retVal = info.names[i] + (retVal == String.Empty ? "" : ", ") + retVal;
								UInt16Flag -= UInt16enumValue;
							}
						}
						break;
					case TypeCode.UInt32:
						UInt32 UInt32Flag = (UInt32)value;
						UInt32 UInt32enumValue;
						for (int i = info.values.Length-1; i>=0 && UInt32Flag != 0; i--) {
							UInt32enumValue = (UInt32)info.values.GetValue (i);
							if ((UInt32enumValue & UInt32Flag) == UInt32enumValue){
								retVal = info.names[i] + (retVal == String.Empty ? "" : ", ") + retVal;
								UInt32Flag -= UInt32enumValue;
							}
						}
						break;
					case TypeCode.UInt64:
						UInt64 UInt64Flag = (UInt64)value;
						UInt64 UInt64enumValue;
						for (int i = info.values.Length-1; i>=0 && UInt64Flag != 0; i--) {
							UInt64enumValue = (UInt64)info.values.GetValue (i);
							if ((UInt64enumValue & UInt64Flag) == UInt64enumValue){
								retVal = info.names[i] + (retVal == String.Empty ? "" : ", ") + retVal;
								UInt64Flag -= UInt64enumValue;
							}
						}
						break;
				}
				break;
			    default:
				throw new FormatException ("Format String can be only \"G\",\"g\",\"X\"," + 
							  "\"x\",\"F\",\"f\",\"D\" or \"d\".");
			}

			return retVal;
		}
	}
}
