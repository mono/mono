// System.Runtime.CompilerServices.RuntimeHelpers
//
// Sean MacIsaac (macisaac@ximian.com)
// Paolo Molaro (lupus@ximian.com)
//
// (C) Ximian, Inc. 2001

namespace System.Runtime.CompilerServices
{
	public sealed class RuntimeHelpers
	{
		private RuntimeHelpers () {}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void InitializeArray (Array array, RuntimeFieldHandle fldHandle);

		public static int OffsetToStringData {
			get {
				// FIXME: this requires the reimplementation of String
				// as of my mail a few months ago, dum de dum
				throw new NotImplementedException ();
			}
		}
	}
}
