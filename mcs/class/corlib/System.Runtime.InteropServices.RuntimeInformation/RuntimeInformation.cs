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
using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices
{
	public static class RuntimeInformation
	{
		/* gets the runtime's arch from the value it uses for DllMap */
		static extern string RuntimeArchitecture
		{
			[MethodImpl (MethodImplOptions.InternalCall)]
			get;
		}

		public static string FrameworkDescription {
			get {
				return "Mono " + Mono.Runtime.GetDisplayName ();
			}
		}

		public static bool IsOSPlatform (OSPlatform osPlatform)
		{
#if WASM
			return osPlatform == OSPlatform.Create ("WEBASSEMBLY"); 
#else
			switch (Environment.Platform) {
			case PlatformID.Win32NT:
				return osPlatform == OSPlatform.Windows;
			case PlatformID.MacOSX:
				return osPlatform == OSPlatform.OSX;
			case PlatformID.Unix:
				return osPlatform == OSPlatform.Linux;
			default:
				return false;
			}
#endif
		}

		public static string OSDescription
		{
			get
			{
#if WASM
				return "web"; //yes, hardcoded as right now we don't really support other environments
#else
				return Environment.OSVersion.VersionString;
#endif
			}
		}

		public static Architecture OSArchitecture
		{
			get
			{
				switch (RuntimeArchitecture) {
				case "arm":
				case "armv8":
					return Environment.Is64BitOperatingSystem ? Architecture.Arm64 : Architecture.Arm;
				case "x86":
				case "x86-64":
				// upstream only has these values; try to pretend we're x86 if nothing matches
				// want more? bug: https://github.com/dotnet/corefx/issues/30706
				default:
					return Environment.Is64BitOperatingSystem ? Architecture.X64 : Architecture.X86;
				}
			}
		}

		public static Architecture ProcessArchitecture
		{
			get
			{
				// we can use the runtime's compiled config options for DllMaps here
				// process architecure for us is runtime architecture (OS is much harder)
				// see for values: mono-config.c
				switch (RuntimeArchitecture) {
				case "x86":
					return Architecture.X86;
				case "x86-64":
					return Architecture.X64;
				case "arm":
					return Architecture.Arm;
				case "armv8":
					return Architecture.Arm64;
				// see comment in OSArchiteture default case
				default:
					return Environment.Is64BitProcess ? Architecture.X64 : Architecture.X86;
				}
			}
		}
	}
}
