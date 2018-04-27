//
// UnsafeNativeMethods.cs
//
// Author:
//   Tautvydas Žilys <zilys@unity3d.com>
//
// Copyright (c) 2016 Unity Technologies (https://www.unity3d.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices.WindowsRuntime
{
	internal unsafe static class UnsafeNativeMethods
	{
#if !DISABLE_COM
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern int WindowsCreateString(string sourceString, int length, IntPtr* hstring);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern int WindowsDeleteString(IntPtr hstring);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern char* WindowsGetStringRawBuffer(IntPtr hstring, uint* length);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern bool RoOriginateLanguageException(int error, string message, IntPtr languageException);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void RoReportUnhandledError(IRestrictedErrorInfo error);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern IRestrictedErrorInfo GetRestrictedErrorInfo();
#else
		public static int WindowsCreateString(string sourceString, int length, IntPtr* hstring) {
			throw new NotImplementedException ();
		}

		public static int WindowsDeleteString(IntPtr hstring) {
			throw new NotImplementedException ();
		}

		public static char* WindowsGetStringRawBuffer(IntPtr hstring, uint* length) {
			throw new NotImplementedException ();
		}

		public static bool RoOriginateLanguageException(int error, string message, IntPtr languageException) {
			throw new NotImplementedException ();
		}

		public static void RoReportUnhandledError(IRestrictedErrorInfo error) {
			throw new NotImplementedException ();
		}

		public static IRestrictedErrorInfo GetRestrictedErrorInfo() {
			throw new NotImplementedException ();
		}
#endif
	}
}
