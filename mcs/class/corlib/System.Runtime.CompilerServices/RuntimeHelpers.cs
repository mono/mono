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

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern object GetObjectValue (object obj);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void RunClassConstructor (RuntimeTypeHandle type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int GetOffsetToStringData();
	}
}
