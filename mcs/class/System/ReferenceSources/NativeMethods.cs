
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Win32
{
	static class NativeMethods
	{
		public const int E_ABORT = unchecked ((int)0x80004004);        

		public const int PROCESS_TERMINATE = 0x0001;
		public const int PROCESS_CREATE_THREAD = 0x0002;
		public const int PROCESS_SET_SESSIONID = 0x0004;
		public const int PROCESS_VM_OPERATION = 0x0008;
		public const int PROCESS_VM_READ = 0x0010;
		public const int PROCESS_VM_WRITE = 0x0020;
		public const int PROCESS_DUP_HANDLE = 0x0040;
		public const int PROCESS_CREATE_PROCESS = 0x0080;
		public const int PROCESS_SET_QUOTA = 0x0100;
		public const int PROCESS_SET_INFORMATION = 0x0200;
		public const int PROCESS_QUERY_INFORMATION = 0x0400;
		public const int PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
		public const int STANDARD_RIGHTS_REQUIRED = 0x000F0000;
		public const int SYNCHRONIZE = 0x00100000;
		public const int PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF;

		public const int DUPLICATE_CLOSE_SOURCE = 1;
		public const int DUPLICATE_SAME_ACCESS = 2;

		public const int STILL_ACTIVE = 0x00000103;

		public const int WAIT_OBJECT_0    = 0x00000000;
		public const int WAIT_FAILED      = unchecked((int)0xFFFFFFFF);
		public const int WAIT_TIMEOUT     = 0x00000102;
		public const int WAIT_ABANDONED   = 0x00000080;
		public const int WAIT_ABANDONED_0 = WAIT_ABANDONED;

		public const int ERROR_FILE_NOT_FOUND = 2;
		public const int ERROR_PATH_NOT_FOUND = 3;
		public const int ERROR_ACCESS_DENIED = 5;
		public const int ERROR_INVALID_HANDLE = 6;
		public const int ERROR_SHARING_VIOLATION = 32;
		public const int ERROR_INVALID_NAME = 0x7B;
		public const int ERROR_ALREADY_EXISTS = 183;
		public const int ERROR_FILENAME_EXCED_RANGE = 0xCE;

		public static bool DuplicateHandle(HandleRef hSourceProcessHandle, SafeHandle hSourceHandle, HandleRef hTargetProcess,
			out SafeWaitHandle targetHandle, int dwDesiredAccess, bool bInheritHandle, int dwOptions)
		{
			bool release = false;
			try {
				hSourceHandle.DangerousAddRef (ref release);

				MonoIOError error;
				IntPtr nakedTargetHandle;
				bool ret = MonoIO.DuplicateHandle (hSourceProcessHandle.Handle, hSourceHandle.DangerousGetHandle (), hTargetProcess.Handle,
					out nakedTargetHandle, dwDesiredAccess, bInheritHandle ? 1 : 0, dwOptions, out error);

				if (error != MonoIOError.ERROR_SUCCESS)
					throw MonoIO.GetException (error);

				targetHandle = new SafeWaitHandle (nakedTargetHandle, true);
				return ret;
			} finally {
				if (release)
					hSourceHandle.DangerousRelease ();
			}
		}

		public static bool DuplicateHandle(HandleRef hSourceProcessHandle, HandleRef hSourceHandle, HandleRef hTargetProcess,
			out SafeProcessHandle targetHandle, int dwDesiredAccess, bool bInheritHandle, int dwOptions)
		{
				MonoIOError error;
				IntPtr nakedTargetHandle;
				bool ret = MonoIO.DuplicateHandle (hSourceProcessHandle.Handle, hSourceHandle.Handle, hTargetProcess.Handle,
					out nakedTargetHandle, dwDesiredAccess, bInheritHandle ? 1 : 0, dwOptions, out error);

				if (error != MonoIOError.ERROR_SUCCESS)
					throw MonoIO.GetException (error);

				targetHandle = new SafeProcessHandle (nakedTargetHandle, true);
				return ret;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern IntPtr GetCurrentProcess();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern bool GetExitCodeProcess (IntPtr processHandle, out int exitCode);

		public static bool GetExitCodeProcess (SafeProcessHandle processHandle, out int exitCode)
		{
			bool release = false;
			try {
				processHandle.DangerousAddRef (ref release);
				return GetExitCodeProcess (processHandle.DangerousGetHandle (), out exitCode);
			} finally {
				if (release)
					processHandle.DangerousRelease ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern bool TerminateProcess (IntPtr processHandle, int exitCode);

		public static bool TerminateProcess (SafeProcessHandle processHandle, int exitCode)
		{
			bool release = false;
			try {
				processHandle.DangerousAddRef (ref release);
				return TerminateProcess (processHandle.DangerousGetHandle (), exitCode);
			} finally {
				if (release)
					processHandle.DangerousRelease ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern int WaitForInputIdle (IntPtr handle, int milliseconds);

		public static int WaitForInputIdle (SafeProcessHandle handle, int milliseconds)
		{
			bool release = false;
			try {
				handle.DangerousAddRef (ref release);
				return WaitForInputIdle (handle.DangerousGetHandle (), milliseconds);
			} finally {
				if (release)
					handle.DangerousRelease ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern bool GetProcessWorkingSetSize (IntPtr handle, out IntPtr min, out IntPtr max);

		public static bool GetProcessWorkingSetSize (SafeProcessHandle handle, out IntPtr min, out IntPtr max)
		{
			bool release = false;
			try {
				handle.DangerousAddRef (ref release);
				return GetProcessWorkingSetSize (handle.DangerousGetHandle (), out min, out max);
			} finally {
				if (release)
					handle.DangerousRelease ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern bool SetProcessWorkingSetSize (IntPtr handle, IntPtr min, IntPtr max);

		public static bool SetProcessWorkingSetSize (SafeProcessHandle handle, IntPtr min, IntPtr max)
		{
			bool release = false;
			try {
				handle.DangerousAddRef (ref release);
				return SetProcessWorkingSetSize (handle.DangerousGetHandle (), min, max);
			} finally {
				if (release)
					handle.DangerousRelease ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern bool GetProcessTimes (IntPtr handle, out long creation, out long exit, out long kernel, out long user);

		public static bool GetProcessTimes (SafeProcessHandle handle, out long creation, out long exit, out long kernel, out long user)
		{
			bool release = false;
			try {
				handle.DangerousAddRef (ref release);
				return GetProcessTimes (handle.DangerousGetHandle (), out creation, out exit, out kernel, out user);
			} finally {
				if (release)
					handle.DangerousRelease ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern int GetCurrentProcessId ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern int GetPriorityClass (IntPtr handle);

		public static int GetPriorityClass(SafeProcessHandle handle)
		{
			bool release = false;
			try {
				handle.DangerousAddRef (ref release);
				return GetPriorityClass (handle.DangerousGetHandle ());
			} finally {
				if (release)
					handle.DangerousRelease ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern bool SetPriorityClass (IntPtr handle, int priorityClass);

		public static bool SetPriorityClass(SafeProcessHandle handle, int priorityClass)
		{
			bool release = false;
			try {
				handle.DangerousAddRef (ref release);
				return SetPriorityClass (handle.DangerousGetHandle (), priorityClass);
			} finally {
				if (release)
					handle.DangerousRelease ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static bool CloseProcess (IntPtr handle);
	}
}