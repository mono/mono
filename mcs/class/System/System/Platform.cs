//
// System.Platform
//
// Copyright (C) 2011-2013 Xamarin Inc. (www.xamarin.com)
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

using System;
using System.Runtime.InteropServices;

namespace System {
	internal static class Platform {
		static bool checkedOS;
		static bool isMacOS;

#if MONOTOUCH || XAMMAC
		const bool isFreeBSD = false;

		private static void CheckOS() {
			isMacOS = true;
			checkedOS = true;
		}

#elif ORBIS
		const bool isFreeBSD = true;

 		private static void CheckOS() {
 			checkedOS = true;
 		}

#else
		static bool isFreeBSD;

		[DllImport ("libc")]
		static extern int uname (IntPtr buf);

		private static void CheckOS() {
			if (Environment.OSVersion.Platform != PlatformID.Unix) {
				checkedOS = true;
				return;
			}

			IntPtr buf = Marshal.AllocHGlobal (8192);
			try {
				if (uname (buf) == 0) {
					string os = Marshal.PtrToStringAnsi (buf);
					switch (os) {
					case "Darwin":
						isMacOS = true;
						break;
					case "FreeBSD":
						isFreeBSD = true;
						break;
					}
				}
			}
			finally {
				Marshal.FreeHGlobal (buf);
				checkedOS = true;
			}
		}
#endif

		// UNITY: runtime replaces this with intrinsic
		public static bool IsMacOS {
			get {
				if (!checkedOS)
#if UNITY
					try {
						CheckOS();
					}
					catch (DllNotFoundException) {
						// libc does not exist, so this is not MacOS
						isMacOS = false;
					}
#else
					CheckOS();
#endif
				return isMacOS;
			}
		}

		// UNITY: runtime replaces this with intrinsic
		public static bool IsFreeBSD {
			get {
				if (!checkedOS)
					CheckOS();
				return isFreeBSD;
			}
		}
	}
}
