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

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void GetDisplayName_icall (ref string result);

		// Do not inline, to ensure icall handles are references to locals and native code can omit barriers.
		//
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		static string GetDisplayName_internal ()
		{
			// Initialize results to reduce unverified requirement on native code.
			// i.e. Favor "ref" over "out".
			//
			string result = "";
			GetDisplayName_icall (ref result);
			return result;
		}

		// Should not be removed intended for external use
		// Safe to be called using reflection
		// Format is undefined only for use as a string for reporting
#if MOBILE || XAMMAC_4_5
		public
#else
		internal
#endif
		static string GetDisplayName ()
		{
			return GetDisplayName_internal ();
		}

		// This is not used, but perhaps via reflection (via GetNativeStackTrace).
		//
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void GetNativeStackTrace_icall (ref string result, ref Exception exception, ref object temp1);

		// This is not used, but perhaps via reflection.
		//
		// Do not inline, to ensure icall handles are references to locals and native code can omit barriers.
		//
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		static string GetNativeStackTrace_internal (Exception exception)
		{
			// Initialize results to reduce unverified requirement on native code.
			// i.e. Favor "ref" over "out".
			//
			string result = "";
			object temp1 = null;
			GetNativeStackTrace_icall (ref result, ref exception, ref temp1);
			return result;
		}

		// This is not used, but perhaps via reflection.
		//
		static string GetNativeStackTrace (Exception exception)
		{
			return GetNativeStackTrace_internal (exception);
		}

		public static bool SetGCAllowSynchronousMajor (bool flag)
		{
			// No longer used
			return true;
		}

		static object exception_capture = new object ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void ExceptionToState_icall (ref string result, ref Exception exc, ref ulong portable_hash, ref ulong unportable_hash);

		// Do not inline, to ensure icall handles are references to locals and native code can omit barriers.
		//
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		static string ExceptionToState_internal (Exception exc, out ulong portable_hash, out ulong unportable_hash)
		{
			// Initialize results to reduce unverified requirement on native code.
			// i.e. Favor "ref" over "out".
			//
			portable_hash = 0;
			unportable_hash = 0;
			string payload_str = "";
			ExceptionToState_icall (ref payload_str, ref exc, ref portable_hash, ref unportable_hash);
			return payload_str;
		}

		static Tuple<String, ulong, ulong>
		ExceptionToState (Exception exc)
		{
			ulong portable_hash;
			ulong unportable_hash;
			string payload_str = ExceptionToState_internal (exc, out portable_hash, out unportable_hash);

			// FIXME use a named type instead of tuple.
			return new Tuple<String, ulong, ulong> (payload_str, portable_hash, unportable_hash);
		}

#if !MOBILE 
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void DisableMicrosoftTelemetry ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void EnableMicrosoftTelemetry_internal (IntPtr appBundleID, IntPtr appSignature, IntPtr appVersion, IntPtr merpGUIPath, IntPtr appPath, IntPtr configDir);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void SendMicrosoftTelemetry_internal (IntPtr payload, ulong portable_hash, ulong unportable_hash);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void WriteStateToFile_internal (IntPtr payload, ulong portable_hash, ulong unportable_hash);

		static void
		WriteStateToFile (Exception exc)
		{
			ulong portable_hash;
			ulong unportable_hash;
			// FIXME One icall instead of two.
			string payload_str = ExceptionToState_internal (exc, out portable_hash, out unportable_hash);
			using (var payload_chars = RuntimeMarshal.MarshalString (payload_str))
			{
				WriteStateToFile_internal (payload_chars.Value, portable_hash, unportable_hash);
			}
		}

		static void SendMicrosoftTelemetry (string payload_str, ulong portable_hash, ulong unportable_hash)
		{
			if (RuntimeInformation.IsOSPlatform (OSPlatform.OSX)) {
				using (var payload_chars = RuntimeMarshal.MarshalString (payload_str))
				{
					SendMicrosoftTelemetry_internal (payload_chars.Value, portable_hash, unportable_hash);
				}
			} else {
				throw new PlatformNotSupportedException("Merp support is currently only supported on OSX.");
			}
		}

		// Usage: 
		//
		// catch (Exception exc) {
		//   var monoType = Type.GetType ("Mono.Runtime", false);
		//   var m = monoType.GetMethod("SendExceptionToTelemetry", BindingFlags.NonPublic | BindingFlags.Static);
		//   m.Invoke(null, new object[] { exc });
		// }
		static void SendExceptionToTelemetry (Exception exc)
		{
			ulong portable_hash;
			ulong unportable_hash;

			lock (exception_capture)
			{
				// FIXME One icall instead of two.
				string payload_str = ExceptionToState_internal (exc, out portable_hash, out unportable_hash);
				SendMicrosoftTelemetry (payload_str, portable_hash, unportable_hash);
			}
		}

		// All must be set except for configDir_str
		static void EnableMicrosoftTelemetry (string appBundleID_str, string appSignature_str, string appVersion_str, string merpGUIPath_str, string unused /* eventType_str */, string appPath_str, string configDir_str)
		{
			if (RuntimeInformation.IsOSPlatform (OSPlatform.OSX)) {
				using (var appBundleID_chars = RuntimeMarshal.MarshalString (appBundleID_str))
				using (var appSignature_chars = RuntimeMarshal.MarshalString (appSignature_str))
				using (var appVersion_chars = RuntimeMarshal.MarshalString (appVersion_str))
				using (var merpGUIPath_chars = RuntimeMarshal.MarshalString (merpGUIPath_str))
				using (var appPath_chars = RuntimeMarshal.MarshalString (appPath_str))
				using (var configDir_chars = RuntimeMarshal.MarshalString (configDir_str))
				{
					EnableMicrosoftTelemetry_internal (appBundleID_chars.Value, appSignature_chars.Value, appVersion_chars.Value, merpGUIPath_chars.Value, appPath_chars.Value, configDir_chars.Value);
				}
			} else {
				throw new PlatformNotSupportedException("Merp support is currently only supported on OSX.");
			}
		}
#endif

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void DumpStateSingle_icall (ref string result, ref ulong portable_hash, ref ulong unportable_hash);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void DumpStateTotal_icall (ref string result, ref ulong portable_hash, ref ulong unportable_hash);

		// Do not inline, to ensure icall handles are references to locals and native code can omit barriers.
		//
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		static string DumpStateSingle_internal (out ulong portable_hash, out ulong unportable_hash)
		{
			// Initialize in managed to reduce unverified requirements on native code.
			//
			portable_hash = 0;
			unportable_hash = 0;
			string payload_str = "";
			DumpStateSingle_icall (ref payload_str, ref portable_hash, ref unportable_hash);
			return payload_str;
		}

		// Do not inline, to ensure icall handles are references to locals and native code can omit barriers.
		//
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		static string DumpStateTotal_internal (out ulong portable_hash, out ulong unportable_hash)
		{
			// Initialize in managed to reduce unverified requirements on native code.
			//
			portable_hash = 0;
			unportable_hash = 0;
			string payload_str = "";
			DumpStateTotal_icall (ref payload_str, ref portable_hash, ref unportable_hash);
			return payload_str;
		}

		static Tuple<String, ulong, ulong>
		DumpStateSingle ()
		{
			ulong portable_hash;
			ulong unportable_hash;
			string payload_str = DumpStateSingle_internal (out portable_hash, out unportable_hash);

			// FIXME use a named type instead of tuple.
			return new Tuple<String, ulong, ulong> (payload_str, portable_hash, unportable_hash);
		}

		static Tuple<String, ulong, ulong>
		DumpStateTotal ()
		{
			ulong portable_hash;
			ulong unportable_hash;
			string payload_str = DumpStateTotal_internal (out portable_hash, out unportable_hash);

			// FIXME use a named type instead of tuple.
			return new Tuple<String, ulong, ulong> (payload_str, portable_hash, unportable_hash);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void RegisterReportingForAllNativeLibs_internal ();

		static void RegisterReportingForAllNativeLibs ()
		{
			RegisterReportingForAllNativeLibs_internal ();
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void RegisterReportingForNativeLib_internal (IntPtr modulePathSuffix, IntPtr moduleName);

		static void RegisterReportingForNativeLib (string modulePathSuffix_str, string moduleName_str)
		{
			using (var modulePathSuffix_chars = RuntimeMarshal.MarshalString (modulePathSuffix_str))
			using (var moduleName_chars = RuntimeMarshal.MarshalString (moduleName_str))
			{
				RegisterReportingForNativeLib_internal (modulePathSuffix_chars.Value, moduleName_chars.Value);
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void EnableCrashReportLog_internal (IntPtr directory);

		static void EnableCrashReportLog (string directory_str)
		{
			using (var directory_chars = RuntimeMarshal.MarshalString (directory_str))
			{
				EnableCrashReportLog_internal (directory_chars.Value);
			}
		}

		enum CrashReportLogLevel : int {
			MonoSummaryNone = 0,
			MonoSummarySetup,
			MonoSummarySuspendHandshake,
			MonoSummaryUnmanagedStacks,
			MonoSummaryManagedStacks,
			MonoSummaryStateWriter,
			MonoSummaryStateWriterDone,
			MonoSummaryMerpWriter,
			MonoSummaryMerpInvoke,
			MonoSummaryCleanup,
			MonoSummaryDone,

			MonoSummaryDoubleFault
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern int CheckCrashReportLog_internal (IntPtr directory, bool clear);

		static CrashReportLogLevel CheckCrashReportLog (string directory_str, bool clear)
		{
			using (var directory_chars = RuntimeMarshal.MarshalString (directory_str))
			{
				return (CrashReportLogLevel) CheckCrashReportLog_internal (directory_chars.Value, clear);
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void AnnotateMicrosoftTelemetry_internal (IntPtr key, IntPtr val);

		static void AnnotateMicrosoftTelemetry (string key, string val)
		{
			using (var key_chars = RuntimeMarshal.MarshalString (key))
			using (var val_chars = RuntimeMarshal.MarshalString (val))
			{
				AnnotateMicrosoftTelemetry_internal (key_chars.Value, val_chars.Value);
			}
		}
	}
}
