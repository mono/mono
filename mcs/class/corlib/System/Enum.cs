//
// System.Enum.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO: Mucho left to implement.
//

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
			MonoEnumInfo info;
			MonoEnumInfo.GetInfo (enumType, out info);
			return info.values;
		}
		
		public static string[] GetNames (Type enumType) {
			MonoEnumInfo info;
			MonoEnumInfo.GetInfo (enumType, out info);
			return info.names;
		}
		
		public static string GetName (Type enumType, object value) {
			MonoEnumInfo info;
			int i;
			MonoEnumInfo.GetInfo (enumType, out info);
			for (i = 0; i < info.values.Length; ++i) {				
				if (value.Equals (info.values.GetValue (i)))
					return info.names [i];
			}
			return null;
		}
		
		public static bool IsDefined (Type enumType, object value) {
			return GetName (enumType, value) != null;
		}
		
		public static Type GetUnderlyingType (Type enumType) {
			MonoEnumInfo info;
			MonoEnumInfo.GetInfo (enumType, out info);
			return info.utype;
		}

		public static object Parse (Type enumType, string value)
		{
			return Parse (enumType, value, false);
		}

		public static object Parse (Type enumType, string value, bool ignoreCase)
		{
			MonoEnumInfo info;
			int i;
			MonoEnumInfo.GetInfo (enumType, out info);
			for (i = 0; i < info.values.Length; ++i) {				
				if (String.Compare (value, info.names [i], ignoreCase) == 0)
					return ToObject (enumType, info.values.GetValue (i));
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
			thisType = this.GetType();
			if (obj.GetType() != thisType){
				throw new ArgumentException(
					"Object must be the same type as the "
					+ "enum. The type passed in was " 
					+ obj.GetType().ToString()
					+ "; the enum type was " 
					+ thisType.ToString() + ".");
			}

			thisType = GetUnderlyingType(this.GetType());
			if (!(thisType == typeof(SByte)
				|| thisType == typeof(Int16)
				|| thisType == typeof(Int32)
				|| thisType == typeof(Int64)
				|| thisType == typeof(Byte)
				|| thisType == typeof(UInt16)
				|| thisType == typeof(UInt32)
				|| thisType == typeof(UInt64)
				)
			)
				throw new InvalidOperationException();

			if (obj == null)
				return 1;

			object value1, value2;

			value1 = this.get_value ();

			if (obj is Enum)
				value2 = ((Enum)obj).get_value();
			else
				value2 = obj;

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
			if (!(obj is Enum))
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
			// fixme: consider format
			return GetName (enumType, value);
		}
	}
}
