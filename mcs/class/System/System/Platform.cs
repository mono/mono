//
// System.Platform
//
// Copyright (C) 2011 Xamarin, Inc. (www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Runtime.InteropServices;

namespace System {
	internal static class Platform {
		static bool checkedIsMacOS;
		static bool isMacOS;
		
		// This is the struct layout for MacOSX. Linux has an additional field,
		// but each field length is only 65 bytes (as opposed to 256 on MacOSX).
		//
		// Since we are only using uname() to detect whether or not the system
		// is MacOSX, we only need .sysname so it doesn't matter if it is too
		// big for other platforms or not.
		[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		struct utsname {
			[MarshalAs (UnmanagedType.ByValTStr, SizeConst = 256)]
			public string sysname;
			[MarshalAs (UnmanagedType.ByValTStr, SizeConst = 256)]
			public string nodename;
			[MarshalAs (UnmanagedType.ByValTStr, SizeConst = 256)]
			public string release;
			[MarshalAs (UnmanagedType.ByValTStr, SizeConst = 256)]
			public string version;
			[MarshalAs (UnmanagedType.ByValTStr, SizeConst = 256)]
			public string machine;
		}
		
		[DllImport ("libc")]
                static extern int uname (ref utsname buf);
		
		public static bool IsMacOS {
			get {
				if (!checkedIsMacOS) {
					utsname buf = new utsname ();
					isMacOS = uname (ref buf) == 0 && buf.sysname == "Darwin";
					checkedIsMacOS = true;
				}
				
				return isMacOS;
			}
		}
	}
}
