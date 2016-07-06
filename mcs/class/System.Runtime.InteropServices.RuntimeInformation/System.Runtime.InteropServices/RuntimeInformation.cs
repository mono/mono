//
// RuntimeInformation.cs
//
// Author:
//   Alexander Köplinger (alexander.koeplinger@xamarin.com)
//
// (C) 2016 Xamarin, Inc.
//

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

using System.IO;
using System.Reflection;

namespace System.Runtime.InteropServices
{
	public static class RuntimeInformation
	{
		[DllImport ("__Internal")]
		extern static string mono_get_runtime_build_info ();

		public static string FrameworkDescription
		{
			get
			{
				return mono_get_runtime_build_info ();
			}
		}

		public static bool IsOSPlatform (OSPlatform osPlatform)
		{
			// TODO: very barebones implementation

			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				return osPlatform == OSPlatform.Windows;

			if (Environment.OSVersion.Platform == PlatformID.Unix && File.Exists ("/usr/lib/libc.dylib"))
				return osPlatform == OSPlatform.OSX;

			if (Environment.OSVersion.Platform == PlatformID.Unix)
				return osPlatform == OSPlatform.Linux;

			return false;
		}

		public static string OSDescription
		{
			get
			{
				return Environment.OSVersion.VersionString;
			}
		}

		public static Architecture OSArchitecture
		{
			get
			{
				// TODO: very barebones implementation, doesn't respect ARM
				return Environment.Is64BitOperatingSystem ? Architecture.X64 : Architecture.X86;
			}
		}

		public static Architecture ProcessArchitecture
		{
			get
			{
				// TODO: very barebones implementation, doesn't respect ARM			
				return Environment.Is64BitProcess ? Architecture.X64 : Architecture.X86;
			}
		}
	}
}
