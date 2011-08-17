// IOFunctions.cs created with MonoDevelop
// User: alan at 14:43 20/10/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Runtime.InteropServices;

namespace zipsharp
{
	// this matches a native 'enum', don't modify
	enum Append
	{
		Create = 0,
		CreateAfter = 1,
		AddInZip = 2
	}
	
	[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
	internal delegate IntPtr OpenFileFunc (IntPtr opaque, string filename, int mode);

	[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
	internal delegate /* ulong */ IntPtr ReadFileFunc (IntPtr opaque, IntPtr stream, IntPtr buffer, /* ulong */ IntPtr size);

	[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
	internal delegate /* ulong */ IntPtr WriteFileFunc (IntPtr opaque, IntPtr stream, IntPtr buffer, /* ulong */ IntPtr size);

	[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
	internal delegate /* long */ IntPtr TellFileFunc (IntPtr opaque, IntPtr stream);

	[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
	internal delegate /* long */ IntPtr SeekFileFunc (IntPtr opaque, IntPtr stream, /* ulong */ IntPtr offset, int origin);

	[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
	internal delegate int CloseFileFunc (IntPtr opaque, IntPtr stream);

	[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
	internal delegate int TestErrorFileFunc (IntPtr opaque, IntPtr stream);

	[StructLayout (LayoutKind.Sequential)]
	internal struct ZlibFileFuncDef
	{
		[MarshalAs (UnmanagedType.FunctionPtr)] public OpenFileFunc      zopen_file;
		[MarshalAs (UnmanagedType.FunctionPtr)] public ReadFileFunc      zread_file;
		[MarshalAs (UnmanagedType.FunctionPtr)] public WriteFileFunc     zwrite_file;
		[MarshalAs (UnmanagedType.FunctionPtr)] public TellFileFunc      ztell_file;
		[MarshalAs (UnmanagedType.FunctionPtr)] public SeekFileFunc      zseek_file;
		[MarshalAs (UnmanagedType.FunctionPtr)] public CloseFileFunc     zclose_file;
		[MarshalAs (UnmanagedType.FunctionPtr)] public TestErrorFileFunc zerror_file;
		public IntPtr            opaque;
	}
}
