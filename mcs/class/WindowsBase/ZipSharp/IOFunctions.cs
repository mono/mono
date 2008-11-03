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
	
	internal delegate IntPtr OpenFileFunc (IntPtr opaque, string filename, int mode);

	internal delegate /* ulong */ IntPtr ReadFileFunc (IntPtr opaque, IntPtr stream, IntPtr buffer, /* ulong */ IntPtr size);
	internal delegate /* ulong */ IntPtr WriteFileFunc (IntPtr opaque, IntPtr stream, IntPtr buffer, /* ulong */ IntPtr size);

	internal delegate /* long */ IntPtr TellFileFunc (IntPtr opaque, IntPtr stream);
	internal delegate /* long */ IntPtr SeekFileFunc (IntPtr opaque, IntPtr stream, /* ulong */ IntPtr offset, int origin);

	internal delegate int CloseFileFunc (IntPtr opaque, IntPtr stream);
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
		[MarshalAs (UnmanagedType.FunctionPtr)] public IntPtr            opaque;
	}
}
