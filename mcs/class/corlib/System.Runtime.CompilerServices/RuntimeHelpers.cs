// System.Runtime.CompilerServices.RuntimeHelpers
//
// Sean MacIsaac (macisaac@ximian.com)
// Paolo Molaro (lupus@ximian.com)
//
// (C) Ximian, Inc. 2001

namespace System.Runtime.CompilerServices
{
	[Serializable]
	public sealed class RuntimeHelpers
	{
		private static int offset_to_string_data;

		static RuntimeHelpers () {
			offset_to_string_data = GetOffsetToStringData();
		}

		private RuntimeHelpers () {}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void InitializeArray (Array array, RuntimeFieldHandle fldHandle);

		public static int OffsetToStringData {
			get {
				return offset_to_string_data;
			}
		}

		public static int GetHashCode (object o) {
			return Object.InternalGetHashCode (o);
		}

		public static new bool Equals (object o1, object o2) {
			// LAMESPEC: According to MSDN, this is equivalent to 
			// Object::Equals (). But the MS version of Object::Equals()
			// includes the functionality of ValueType::Equals(), while
			// our version does not.
			if (o1 == o2)
				return true;
			if ((o1 == null) || (o2 == null))
				return false;
			if (o1 is ValueType)
				return ValueType.InternalEquals (o1, o2);
			else
				return Object.Equals (o1, o2);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern object GetObjectValue (object obj);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void RunClassConstructor (RuntimeTypeHandle type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int GetOffsetToStringData();
	}
}
