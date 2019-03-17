using System.Reflection;
using System.Runtime.CompilerServices;

namespace System
{
	partial class Enum
	{
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern bool GetEnumValuesAndNames (RuntimeType enumType, out ulong[] values, out string[] names);

		static TypeValuesAndNames GetCachedValuesAndNames (RuntimeType enumType, bool getNames)
		{
			var entry = enumType.GenericCache as TypeValuesAndNames;

			if (entry == null || (getNames && entry.Names == null)) {
				ulong[] values = null;
				String[] names = null;

				if (!GetEnumValuesAndNames (enumType, out values, out names))
					Array.Sort (values, names, System.Collections.Generic.Comparer<ulong>.Default);

				bool isFlags = enumType.IsDefined (typeof(FlagsAttribute), inherit: false);
				entry = new TypeValuesAndNames (isFlags, values, names);
				enumType.GenericCache = entry;
			}

			return entry;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern object InternalBoxEnum (RuntimeType enumType, long value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern int InternalCompareTo (object o1, object o2);

		static CorElementType InternalGetCorElementType () => throw new NotImplementedException ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern RuntimeType InternalGetUnderlyingType (RuntimeType enumType);

		public override bool Equals (object obj)
		{
			return DefaultEquals (this, obj);
		}

		[Intrinsic]
		public bool HasFlag (Enum flag)
		{
			throw new NotImplementedException ();
		}

		public static string GetName (Type enumType, object value)
		{
			if (enumType == null)
				throw new ArgumentNullException (nameof(enumType));

			return enumType.GetEnumName (value);
		}

		public static string[] GetNames (Type enumType)
		{
			if (enumType == null)
				throw new ArgumentNullException (nameof (enumType));

			return enumType.GetEnumNames ();
		}

		public static Type GetUnderlyingType (Type enumType)
		{
			if (enumType == null)
				throw new ArgumentNullException (nameof(enumType));

			return enumType.GetEnumUnderlyingType ();
		}

		public static Array GetValues(Type enumType)
		{
			if (enumType == null)
				throw new ArgumentNullException (nameof(enumType));

			return enumType.GetEnumValues ();
		}

		private static RuntimeType ValidateRuntimeType (Type enumType)
		{
			if (enumType == null)
				throw new ArgumentNullException (nameof (enumType));
			if (!enumType.IsEnum)
				throw new ArgumentException (SR.Arg_MustBeEnum, nameof (enumType));
			if (!(enumType is RuntimeType rtType))
				throw new ArgumentException (SR.Arg_MustBeType, nameof (enumType));
			return rtType;
		}		
	}
}