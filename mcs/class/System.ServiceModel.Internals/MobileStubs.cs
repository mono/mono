// help compiles sources for mobile without having unusable
// Windows p/invoke in the assemblies
//
// Copyright 2015 Xamarin Inc.

#if MOBILE

namespace System.Runtime.Interop {

	using Microsoft.Win32.SafeHandles;
	using System.Runtime.Diagnostics;
	using System.Runtime.InteropServices;
	using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

	sealed class SafeEventLogWriteHandle : SafeHandleZeroOrMinusOneIsInvalid {

		SafeEventLogWriteHandle () : base (true)
		{
		}

		public static SafeEventLogWriteHandle RegisterEventSource (string uncServerName, string sourceName)
		{
			throw new NotImplementedException ();
		}

		static bool DeregisterEventSource (IntPtr hEventLog)
		{
			throw new NotImplementedException ();
		}

		protected override bool ReleaseHandle ()
		{
			throw new NotImplementedException ();
		}
	}

	static class UnsafeNativeMethods {

		public const int ERROR_MORE_DATA = 234;
		public const int ERROR_ARITHMETIC_OVERFLOW = 534;
		public const int ERROR_NOT_ENOUGH_MEMORY = 8;

		[StructLayout (LayoutKind.Explicit, Size = 16)]
		public struct EventData {
			[FieldOffset(0)]
			internal UInt64 DataPointer;
			[FieldOffset(8)]
			internal uint Size;
			[FieldOffset(12)]
			internal int Reserved;
		}

		public static SafeWaitHandle CreateWaitableTimer (IntPtr mustBeZero, bool manualReset, string timerName)
		{
			throw new NotImplementedException ();
		}

		public static bool SetWaitableTimer (SafeWaitHandle handle, ref long dueTime, int period, IntPtr mustBeZero, IntPtr mustBeZeroAlso, bool resume)
		{
			throw new NotImplementedException ();
		}

		public static int QueryPerformanceCounter (out long time)
		{
			throw new NotImplementedException ();
		}

		public static uint GetSystemTimeAdjustment (out int adjustment, out uint increment, out uint adjustmentDisabled)
		{
			throw new NotImplementedException ();
		}

		public static void GetSystemTimeAsFileTime (out long time)
		{
			throw new NotImplementedException ();
		}

		internal static string GetComputerName (ComputerNameFormat nameType)
		{
			throw new NotImplementedException ();
		}

		internal static bool IsDebuggerPresent ()
		{
			throw new NotImplementedException ();
		}

		internal static void DebugBreak ()
		{
			throw new NotImplementedException ();
		}

		internal static void OutputDebugString (string lpOutputString)
		{
			throw new NotImplementedException ();
		}

		internal unsafe delegate void EtwEnableCallback (ref Guid sourceId, int isEnabled, byte level, long matchAnyKeywords, long matchAllKeywords, void* filterData, void* callbackContext);

		internal static unsafe uint EventRegister (ref Guid providerId, EtwEnableCallback enableCallback, void* callbackContext, ref long registrationHandle)
		{
			throw new NotImplementedException ();
		}

		internal static uint EventUnregister (long registrationHandle)
		{
			throw new NotImplementedException ();
		}

		internal static bool EventEnabled (long registrationHandle, ref EventDescriptor eventDescriptor)
		{
			throw new NotImplementedException ();
		}

		internal static unsafe uint EventWrite (long registrationHandle, ref EventDescriptor eventDescriptor, uint userDataCount, EventData* userData)
		{
			throw new NotImplementedException ();
		}

		internal static unsafe uint EventWriteTransfer (long registrationHandle, ref EventDescriptor eventDescriptor, ref Guid activityId, ref Guid relatedActivityId, uint userDataCount, EventData* userData)
		{
			throw new NotImplementedException ();
		}

		internal static unsafe uint EventWriteString (long registrationHandle, byte level, long keywords, char* message)
		{
			throw new NotImplementedException ();
		}

		internal static unsafe uint EventActivityIdControl (int ControlCode, ref Guid ActivityId)
		{
			throw new NotImplementedException ();
		}

		internal static bool ReportEvent (SafeHandle hEventLog, ushort type, ushort category, uint eventID, byte[] userSID, ushort numStrings, uint dataLen, HandleRef strings, byte[] rawData)
		{
			throw new NotImplementedException ();
		}

		internal static SafeEventLogWriteHandle RegisterEventSource (string uncServerName, string sourceName)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
