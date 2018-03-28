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
	internal delegate /* uLong */ uint ReadFileFunc32 (IntPtr opaque, IntPtr stream, IntPtr buffer, /* uLong */ uint size);

	[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
	internal delegate /* uLong */ uint WriteFileFunc32 (IntPtr opaque, IntPtr stream, IntPtr buffer, /* uLong */ uint size);

	[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
	internal delegate /* long */ int TellFileFunc32 (IntPtr opaque, IntPtr stream);

	[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
	internal delegate /* long */ int SeekFileFunc32 (IntPtr opaque, IntPtr stream, /* uLong */ uint offset, int origin);

	[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
	internal delegate /* uLong */ ulong ReadFileFunc64 (IntPtr opaque, IntPtr stream, IntPtr buffer, /* uLong */ ulong size);

	[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
	internal delegate /* uLong */ ulong WriteFileFunc64 (IntPtr opaque, IntPtr stream, IntPtr buffer, /* uLong */ ulong size);

	[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
	internal delegate /* long */ long TellFileFunc64 (IntPtr opaque, IntPtr stream);

	[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
	internal delegate /* long */ long SeekFileFunc64 (IntPtr opaque, IntPtr stream, /* uLong */ ulong offset, int origin);

	[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
	internal delegate int CloseFileFunc (IntPtr opaque, IntPtr stream);

	[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
	internal delegate int TestErrorFileFunc (IntPtr opaque, IntPtr stream);

	[StructLayout (LayoutKind.Sequential)]
	internal struct ZlibFileFuncDef32
	{
		[MarshalAs (UnmanagedType.FunctionPtr)] public OpenFileFunc      zopen_file;
		[MarshalAs (UnmanagedType.FunctionPtr)] public ReadFileFunc32    zread_file;
		[MarshalAs (UnmanagedType.FunctionPtr)] public WriteFileFunc32   zwrite_file;
		[MarshalAs (UnmanagedType.FunctionPtr)] public TellFileFunc32    ztell_file;
		[MarshalAs (UnmanagedType.FunctionPtr)] public SeekFileFunc32    zseek_file;
		[MarshalAs (UnmanagedType.FunctionPtr)] public CloseFileFunc     zclose_file;
		[MarshalAs (UnmanagedType.FunctionPtr)] public TestErrorFileFunc zerror_file;
		public IntPtr            opaque;
	}

	[StructLayout (LayoutKind.Sequential)]
	internal struct ZlibFileFuncDef64
	{
		[MarshalAs (UnmanagedType.FunctionPtr)] public OpenFileFunc      zopen_file;
		[MarshalAs (UnmanagedType.FunctionPtr)] public ReadFileFunc64    zread_file;
		[MarshalAs (UnmanagedType.FunctionPtr)] public WriteFileFunc64   zwrite_file;
		[MarshalAs (UnmanagedType.FunctionPtr)] public TellFileFunc64    ztell_file;
		[MarshalAs (UnmanagedType.FunctionPtr)] public SeekFileFunc64    zseek_file;
		[MarshalAs (UnmanagedType.FunctionPtr)] public CloseFileFunc     zclose_file;
		[MarshalAs (UnmanagedType.FunctionPtr)] public TestErrorFileFunc zerror_file;
		public IntPtr            opaque;
	}
}
