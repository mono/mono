//
// Mono Runtime gateway functions
//
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mono {

#if MOBILE || XAMMAC_4_5
	public
#endif
	static class Runtime
	{

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern void mono_runtime_install_handlers ();

#if MOBILE || XAMMAC_4_5
		public
#else
		internal
#endif
		static void InstallSignalHandlers ()
		{
			mono_runtime_install_handlers ();
		}

#if MOBILE || XAMMAC_4_5
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void mono_runtime_cleanup_handlers ();

		public static void RemoveSignalHandlers ()
		{
			mono_runtime_cleanup_handlers ();
		}
#endif

		// Should not be removed intended for external use
		// Safe to be called using reflection
		// Format is undefined only for use as a string for reporting
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
#if MOBILE || XAMMAC_4_5
		public
#else
		internal
#endif
		static extern string GetDisplayName ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern string GetNativeStackTrace (Exception exception);

		public static bool SetGCAllowSynchronousMajor (bool flag)
		{
			// No longer used
			return true;
		}

#if !MOBILE 
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void DisableMicrosoftTelemetry (IntPtr appBundleID, IntPtr appSignature, IntPtr appVersion, IntPtr merpGUIPath);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void EnableMicrosoftTelemetry_internal (IntPtr appBundleID, IntPtr appSignature, IntPtr appVersion, IntPtr merpGUIPath, IntPtr eventType);

		static void EnableMicrosoftTelemetry (string appBundleID_str, string appSignature_str, string appVersion_str, string merpGUIPath_str, string eventType_str)
		{
			if (RuntimeInformation.IsOSPlatform (OSPlatform.OSX)) {
				using (var appBundleID_chars = RuntimeMarshal.MarshalString (appBundleID_str))
				using (var appSignature_chars = RuntimeMarshal.MarshalString (appSignature_str))
				using (var appVersion_chars = RuntimeMarshal.MarshalString (appVersion_str))
				using (var merpGUIPath_chars = RuntimeMarshal.MarshalString (merpGUIPath_str))
				using (var eventType_chars = RuntimeMarshal.MarshalString (eventType_str))
				{
					EnableMicrosoftTelemetry_internal (appBundleID_chars.Value, appSignature_chars.Value, appVersion_chars.Value, merpGUIPath_chars.Value, eventType_chars.Value);
				}
			} else {
				throw new PlatformNotSupportedException("Merp support is currently only supported on OSX.");
			}
		}
#endif

	}
}
