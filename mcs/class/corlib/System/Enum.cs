//
// System.Enum.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Nick Drochak (ndrochak@gol.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
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
		static Hashtable cache;
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void get_enum_info (Type enumType, out MonoEnumInfo info);
		
		private MonoEnumInfo (MonoEnumInfo other)
		{
			utype = other.utype;
			values = other.values;
			names = other.names;
		}
		
		internal static void GetInfo (Type enumType, out MonoEnumInfo info)
		{
			if (cache == null)
				cache = Hashtable.Synchronized (new Hashtable ());
			lock (cache) {
				if (cache.ContainsKey (enumType)) {
					info = (MonoEnumInfo) cache [enumType];
					return;
				}
				get_enum_info (enumType, out info);
				Array.Sort (info.values, info.names);
				cache.Add (enumType, new MonoEnumInfo (info));
			}
		}
	};

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

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			return Convert.ToType (get_value (), conversionType, provider);
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
			return (Array) info.values.Clone ();
		}
		
		public static string[] GetNames (Type enumType) {
			if (null == enumType)
				throw new ArgumentNullException ("enumType cannot be null.");

			if (!enumType.IsEnum)
				throw new ArgumentException ("enumType is not an Enum type.");

			MonoEnumInfo info;
			MonoEnumInfo.GetInfo (enumType, out info);
			return (string []) info.names.Clone ();
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

			TypeCode typeCode = ((Enum) info.values.GetValue (0)).GetTypeCode ();

			try {
				// Attempt to convert to numeric type
				return ToObject(enumType, Convert.ChangeType (value, typeCode) );
			} catch {}

			string[] names = value.Split(new char[] {','});
			for (i = 0; i < names.Length; ++i)
				names [i] = names [i].Trim ();

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
		/// <remarks/>
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

		static string FormatFlags (Type enumType, object value)
		{
			string retVal = "";
			MonoEnumInfo info;
			MonoEnumInfo.GetInfo (enumType, out info);
			string asString = value.ToString ();
			if (asString == "0") {
				retVal = GetName (enumType, value);
				if (retVal == null)
					retVal = asString;

				return retVal;
			}
			// This is ugly, yes.  We need to handle the different integer
			// types for enums.  If someone else has a better idea, be my guest.
			switch (((Enum)info.values.GetValue (0)).GetTypeCode()) {
			case TypeCode.SByte: {
				sbyte flags = (sbyte) value;
				sbyte enumValue;
				for (int i = info.values.Length - 1; i >= 0; i--) {
					enumValue = (sbyte) info.values.GetValue (i);
					if (i == 0 && enumValue == 0)
						continue;

					if ((flags & enumValue) == enumValue){
						retVal = info.names[i] + (retVal == String.Empty ? "" : ", ") + retVal;
						flags -= enumValue;
					}
				}
				}
				break;
			case TypeCode.Byte:{
				byte flags = (byte) value;
				byte enumValue;
				for (int i = info.values.Length - 1; i >= 0; i--) {
					enumValue = (byte) info.values.GetValue (i);
					if (i == 0 && enumValue == 0)
						continue;

					if ((flags & enumValue) == enumValue){
						retVal = info.names[i] + (retVal == String.Empty ? "" : ", ") + retVal;
						flags -= enumValue;
					}
				}
				}
				break;
			case TypeCode.Int16: {
				short flags = (short) value;
				short enumValue;
				for (int i = info.values.Length - 1; i >= 0; i--) {
					enumValue = (short) info.values.GetValue (i);
					if (i == 0 && enumValue == 0)
						continue;

					if ((flags & enumValue) == enumValue){
						retVal = info.names[i] + (retVal == String.Empty ? "" : ", ") + retVal;
						flags -= enumValue;
					}
				}
				}
				break;
			case TypeCode.Int32: {
				int flags = (int) value;
				int enumValue;
				for (int i = info.values.Length - 1; i >= 0; i--) {
					enumValue = (int) info.values.GetValue (i);
					if (i == 0 && enumValue == 0)
						continue;

					if ((flags & enumValue) == enumValue){
						retVal = info.names[i] + (retVal == String.Empty ? "" : ", ") + retVal;
						flags -= enumValue;
					}
				}
				}
				break;
			case TypeCode.UInt16: {
				ushort flags = (ushort) value;
				ushort enumValue;
				for (int i = info.values.Length - 1; i >= 0; i--) {
					enumValue = (ushort) info.values.GetValue (i);
					if (i == 0 && enumValue == 0)
						continue;

					if ((flags & enumValue) == enumValue){
						retVal = info.names[i] + (retVal == String.Empty ? "" : ", ") + retVal;
						flags -= enumValue;
					}
				}
				}
				break;
			case TypeCode.UInt32: {
				uint flags = (uint) value;
				uint enumValue;
				for (int i = info.values.Length - 1; i >= 0; i--) {
					enumValue = (uint) info.values.GetValue (i);
					if (i == 0 && enumValue == 0)
						continue;

					if ((flags & enumValue) == enumValue){
						retVal = info.names[i] + (retVal == String.Empty ? "" : ", ") + retVal;
						flags -= enumValue;
					}
				}
				}
				break;
			case TypeCode.Int64: {
				long flags = (long) value;
				long enumValue;
				for (int i = info.values.Length - 1; i >= 0; i--) {
					enumValue = (long) info.values.GetValue (i);
					if (i == 0 && enumValue == 0)
						continue;

					if ((flags & enumValue) == enumValue){
						retVal = info.names[i] + (retVal == String.Empty ? "" : ", ") + retVal;
						flags -= enumValue;
					}
				}
				}
				break;
			case TypeCode.UInt64: {
				ulong flags = (ulong) value;
				ulong enumValue;
				for (int i = info.values.Length - 1; i >= 0; i--) {
					enumValue = (ulong) info.values.GetValue (i);
					if (i == 0 && enumValue == 0)
						continue;

					if ((flags & enumValue) == enumValue){
						retVal = info.names[i] + (retVal == String.Empty ? "" : ", ") + retVal;
						flags -= enumValue;
					}
				}
				}
				break;
			}

			if (retVal == "")
				return asString;

			return retVal;
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
			string retVal;
			if ((formatChar == 'G' || formatChar == 'g')) {
				if (!Attribute.IsDefined (enumType, typeof(FlagsAttribute))) {
					retVal = GetName (enumType, value);
					if (retVal == null)
						retVal = value.ToString();

					return retVal;
				}

				formatChar = 'f';
			}
			
			if ((formatChar == 'f' || formatChar == 'F'))
				return FormatFlags (enumType, value);

			retVal = "";
			switch (formatChar) {
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
			    default:
				throw new FormatException ("Format String can be only \"G\",\"g\",\"X\"," + 
							  "\"x\",\"F\",\"f\",\"D\" or \"d\".");
			}

			return retVal;
		}
	}
}
