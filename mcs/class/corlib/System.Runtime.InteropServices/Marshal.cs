// System.Runtime.InteropServices.Marshal
//
// Sean MacIsaac (macisaac@ximian.com)
//
// (C) 2001 Ximian, Inc.

using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices
{
	public sealed class Marshal
	{
		private Marshal () {}

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern string PtrToStringAuto (IntPtr ptr);	

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern IntPtr ReadIntPtr (IntPtr ptr);		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern int GetLastWin32Error();

		[MonoTODO]
		public static IntPtr AllocHGlobal (IntPtr cb) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IntPtr AllocHGlobal (int cb) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void FreeHGlobal (IntPtr hglobal) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void StructureToPtr (object structure, IntPtr ptr, bool fDeleteOld) {
			throw new NotImplementedException ();
		}
	}
}
