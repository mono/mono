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
	public abstract class Enum : ValueType, IComparable {

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
		public static string GetName( Type enumType, object value) {
			MonoEnumInfo info;
			int i;
			MonoEnumInfo.GetInfo (enumType, out info);
			for (i = 0; i < info.values.Length; ++i) {
				if (info.values.GetValue (i) == value)
					return info.names [i];
			}
			return null;
		}
		public static bool IsDefined( Type enumType, object value) {
			return GetName (enumType, value) != null;
		}
		public static Type GetUnderlyingType( Type enumType) {
			MonoEnumInfo info;
			MonoEnumInfo.GetInfo (enumType, out info);
			return info.utype;
		}

		/// <summary>
		///   Compares the enum value with another enum value of the same type.
		/// </summary>
		///
		/// <remarks>
		///   
		int IComparable.CompareTo (object obj)
		{
			if (obj == null)
				return 1;

			if (obj.GetType () != GetType ())
				throw new ArgumentException (
					Locale.GetText ("Enumeration and object must be of the same type"));

			throw new NotImplementedException ();
		}
		
		public override string ToString ()
		{
			//throw new NotImplementedException ();
			return "Enum::ToString()";
		}

		public string ToString (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		public string ToString (String format)
		{
			throw new NotImplementedException ();
		}

		public string ToString (String format, IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		public static object ToObject(Type enumType, byte value)
		{
			throw new NotImplementedException ();
		}
		public static object ToObject(Type enumType, short value)
		{
			throw new NotImplementedException ();
		}
		public static object ToObject(Type enumType, int value)
		{
			throw new NotImplementedException ();
		}
		public static object ToObject(Type enumType, long value)
		{
			throw new NotImplementedException ();
		}
		public static object ToObject(Type enumType, object value)
		{
			// needed by mcs
			throw new NotImplementedException ();
		}
		[CLSCompliant(false)]
		public static object ToObject(Type enumType, sbyte value)
		{
			throw new NotImplementedException ();
		}
		[CLSCompliant(false)]
		public static object ToObject(Type enumType, ushort value)
		{
			throw new NotImplementedException ();
		}
		[CLSCompliant(false)]
		public static object ToObject(Type enumType, uint value)
		{
			throw new NotImplementedException ();
		}
		[CLSCompliant(false)]
		public static object ToObject(Type enumType, ulong value)
		{
			throw new NotImplementedException ();
		}
	}
}
