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
		static readonly Architecture _osArchitecture;
		static readonly Architecture _processArchitecture;
		static readonly OSPlatform _osPlatform;

		static RuntimeInformation ()
		{
			// we can use the runtime's compiled config options for DllMaps here
			// process architecure for us is runtime architecture
			// see for values: mono-config.c
			var runtimeArchitecture = GetRuntimeArchitecture ();
			var osName = GetOSName ();

			// check OS/process architecture
			switch (runtimeArchitecture) {
			case "arm":
				_osArchitecture = Environment.Is64BitOperatingSystem ? Architecture.Arm64 : Architecture.Arm;
				_processArchitecture = Architecture.Arm;
				break;
			case "armv8":
				_osArchitecture = Environment.Is64BitOperatingSystem ? Architecture.Arm64 : Architecture.Arm;
				_processArchitecture = Architecture.Arm64;
				break;
			case "x86":
				_osArchitecture = Environment.Is64BitOperatingSystem ? Architecture.X64 : Architecture.X86;
				_processArchitecture = Architecture.X86;
				break;
			case "x86-64":
				_osArchitecture = Environment.Is64BitOperatingSystem ? Architecture.X64 : Architecture.X86;
				_processArchitecture = Architecture.X64;
				break;
			// upstream only has these values; try to pretend we're x86 if nothing matches
			// want more? bug: https://github.com/dotnet/corefx/issues/30706
			default:
				_osArchitecture = Environment.Is64BitOperatingSystem ? Architecture.X64 : Architecture.X86;
				_processArchitecture = Environment.Is64BitProcess ? Architecture.X64 : Architecture.X86;
				break;
			}

			// check OS platform
			switch (osName) {
				case "linux":
					_osPlatform = OSPlatform.Linux;
					break;
				case "osx":
					_osPlatform = OSPlatform.OSX;
					break;
				case "windows":
					_osPlatform = OSPlatform.Windows;
					break;
				case "solaris":
					_osPlatform = OSPlatform.Create ("SOLARIS");
					break;
				case "freebsd":
					_osPlatform = OSPlatform.Create ("FREEBSD");
					break;
				case "netbsd":
					_osPlatform = OSPlatform.Create ("NETBSD");
					break;
				case "openbsd":
					_osPlatform = OSPlatform.Create ("OPENBSD");
					break;
				case "aix":
					_osPlatform = OSPlatform.Create ("AIX");
					break;
				case "hpux":
					_osPlatform = OSPlatform.Create ("HPUX");
					break;
				case "haiku":
					_osPlatform = OSPlatform.Create ("HAIKU");
					break;
				case "wasm":
					_osPlatform = OSPlatform.Create ("WEBASSEMBLY");
					break;
				default:
					_osPlatform = OSPlatform.Create ("UNKNOWN");
					break;
			}
		}

		/* gets the runtime's arch from the value it uses for DllMap */
		[MethodImpl (MethodImplOptions.InternalCall)]
		static extern string GetRuntimeArchitecture ();

		/* gets the runtime's OS from the value it uses for DllMap */
		[MethodImpl (MethodImplOptions.InternalCall)]
		static extern string GetOSName ();

		public static string FrameworkDescription {
			get {
				return "Mono " + Mono.Runtime.GetDisplayName ();
			}
		}

		public static bool IsOSPlatform (OSPlatform osPlatform)
		{
			return _osPlatform == osPlatform;
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
				return _osArchitecture;
			}
		}

		public static Architecture ProcessArchitecture
		{
			get
			{
				return _processArchitecture;
			}
		}
	}
}
