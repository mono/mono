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
	public abstract class Enum : ValueType, IComparable, IConvertible {

		// IConvertible methods Start -->

		[CLSCompliant(false)]
		public TypeCode GetTypeCode () {
			MonoEnumInfo info;
			MonoEnumInfo.GetInfo (this.GetType (), out info);
			return Type.GetTypeCode (info.utype);
		}

		[MonoTODO]
		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		byte IConvertible.ToByte (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		char IConvertible.ToChar (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		double IConvertible.ToDouble (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
    		[CLSCompliant(false)]
		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		float IConvertible.ToSingle (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
    		[CLSCompliant(false)]
		public ushort ToUInt16 (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
    		[CLSCompliant(false)]
		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
    		[CLSCompliant(false)]
		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			throw new NotImplementedException ();
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
		
		[MonoTODO("Complete parameter validation")]
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
			throw new ArgumentException ("value is an invalid type.");
		}
		
		[MonoTODO("Complete parameter validation")]
		public static bool IsDefined (Type enumType, object value) {
			if (null == enumType)
				throw new ArgumentNullException ("enumType cannot be null.");
			if (null == value)
				throw new ArgumentNullException ("value cannot be null.");

			if (!enumType.IsEnum)
				throw new ArgumentException ("enumType is not an Enum type.");

			// FIXME: Not sure how to ensure Type of value parameter is correct

			Type t = value.GetType ();
			if (!(t == typeof (SByte)
				|| t == typeof (Int16)
				|| t == typeof (Int32)
				|| t == typeof (Int64)
				|| t == typeof (Byte)
				|| t == typeof (UInt16)
				|| t == typeof (UInt32)
				|| t == typeof (UInt64)
				))
				throw new ExecutionEngineException();

			MonoEnumInfo info;
			MonoEnumInfo.GetInfo (enumType, out info);
			return ((IList)(info.values)).Contains (value);
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
			for (i = 0; i < info.values.Length; ++i) {				
				if (String.Compare (value, info.names [i], ignoreCase) == 0)
					return info.values.GetValue (i);
			}
			throw new ArgumentException ("The rquested value was not found");
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
			// fixme: consider format and provider
			return GetName (this.GetType(), this.get_value ());
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

		[MonoTODO]
		public static string Format (Type enumType, object value, string format)
		{
			if (null == enumType)
				throw new ArgumentNullException("enumType cannot be null");
			if (null == value)
				throw new ArgumentNullException("value cannot be null");
			if (null == format)
				throw new ArgumentNullException("format cannot be null");

			if (enumType != typeof(Enum))
				throw new ArgumentException("enumType is not an Enum Type");

			// FIXME: other tests for ArgumentException

			// fixme: consider format, including test and throw FormatException
			return GetName (enumType, value);
		}
	}
}
