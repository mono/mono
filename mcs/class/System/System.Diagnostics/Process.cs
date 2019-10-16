//
// System.Diagnostics.Process.cs
//
// Authors:
// 	Dick Porter (dick@ximian.com)
// 	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc.
// (C) 2003 Andreas Nahr
// (c) 2004,2005,2006 Novell, Inc. (http://www.novell.com)
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
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Diagnostics
{
	public partial class Process : Component 
	{
		[StructLayout(LayoutKind.Sequential)]
		private struct ProcInfo 
		{
			public IntPtr process_handle;
			/* If thread_handle is ever needed for
			 * something, take out the CloseHandle() in
			 * the Start_internal icall in
			 * mono/metadata/process.c
			 */
			public int pid; // Contains -GetLastError () on failure.
			public string[] envVariables;
			public string UserName;
			public string Domain;
			public IntPtr Password;
			public bool LoadUserProfile;
		};

		string process_name;

		static ProcessModule current_main_module;

		/* Private constructor called from other methods */
		private Process (SafeProcessHandle handle, int id) {
			SetProcessHandle (handle);
			SetProcessId (id);
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("Base process priority.")]
		public int BasePriority {
			get { return 0; }
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("Handles for this process.")]
		public int HandleCount {
			get {
				return(0);
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The main module of the process.")]
		public ProcessModule MainModule {
			get {
				/* Optimize Process.GetCurrentProcess ().MainModule */
				if (processId == NativeMethods.GetCurrentProcessId ()) {
					if (current_main_module == null)
						current_main_module = this.Modules [0];
					return current_main_module;
				} else {
					return this.Modules [0];
				}
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The handle of the main window of the process.")]
		public IntPtr MainWindowHandle {
			get {
				return((IntPtr)0);
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The title of the main window of the process.")]
		public string MainWindowTitle {
			get {
				return("null");
			}
		}

		private static void AppendArguments (StringBuilder stringBuilder, Collection<string> argumentList)
		{
			if (argumentList.Count > 0) {
				foreach (string argument in argumentList) {
					PasteArguments.AppendArgument (stringBuilder, argument);
				}
			}
		}

		/* Returns the list of process modules.  The main module is
		 * element 0.
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern ProcessModule[] GetModules_icall (IntPtr handle);

		ProcessModule[] GetModules_internal (SafeProcessHandle handle)
		{
			bool release = false;
			try {
				handle.DangerousAddRef (ref release);
				return GetModules_icall (handle.DangerousGetHandle ());
			} finally {
				if (release)
					handle.DangerousRelease ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The modules that are loaded as part of this process.")]
		public ProcessModuleCollection Modules {
			get {
				if (modules == null) {
					SafeProcessHandle handle = null;
					try {
						handle = GetProcessHandle (NativeMethods.PROCESS_QUERY_INFORMATION);
						modules = new ProcessModuleCollection (GetModules_internal (handle));
					} finally {
						ReleaseProcessHandle (handle);
					}
				}

				return modules;
			}
		}

		/* data type is from the MonoProcessData enum in mono-proclib.h in the runtime */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static long GetProcessData (int pid, int data_type, out int error);

		[MonoTODO]
		[Obsolete ("Use NonpagedSystemMemorySize64")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The number of bytes that are not pageable.")]
		public int NonpagedSystemMemorySize {
			get {
				return(0);
			}
		}

		[Obsolete ("Use PagedMemorySize64")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The number of bytes that are paged.")]
		public int PagedMemorySize {
			get {
				return(int)PagedMemorySize64;
			}
		}

		[Obsolete ("Use PagedSystemMemorySize64")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of paged system memory in bytes.")]
		public int PagedSystemMemorySize {
			get {
				return(int)PagedMemorySize64;
			}
		}

		[MonoTODO]
		[Obsolete ("Use PeakPagedMemorySize64")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The maximum amount of paged memory used by this process.")]
		public int PeakPagedMemorySize {
			get {
				return(0);
			}
		}

		[Obsolete ("Use PeakVirtualMemorySize64")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The maximum amount of virtual memory used by this process.")]
		public int PeakVirtualMemorySize {
			get {
				int error;
				return (int)GetProcessData (processId, 8, out error);
			}
		}

		[Obsolete ("Use PeakWorkingSet64")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The maximum amount of system memory used by this process.")]
		public int PeakWorkingSet {
			get {
				int error;
				return (int)GetProcessData (processId, 5, out error);
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The number of bytes that are not pageable.")]
		[ComVisible (false)]
		public long NonpagedSystemMemorySize64 {
			get {
				return(0);
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The number of bytes that are paged.")]
		[ComVisible (false)]
		public long PagedMemorySize64 {
			get {
				int error;
				return GetProcessData (processId, 12, out error);
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of paged system memory in bytes.")]
		[ComVisible (false)]
		public long PagedSystemMemorySize64 {
			get {
				return PagedMemorySize64;
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The maximum amount of paged memory used by this process.")]
		[ComVisible (false)]
		public long PeakPagedMemorySize64 {
			get {
				return(0);
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The maximum amount of virtual memory used by this process.")]
		[ComVisible (false)]
		public long PeakVirtualMemorySize64 {
			get {
				int error;
				return GetProcessData (processId, 8, out error);
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The maximum amount of system memory used by this process.")]
		[ComVisible (false)]
		public long PeakWorkingSet64 {
			get {
				int error;
				return GetProcessData (processId, 5, out error);
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("Process will be of higher priority while it is actively used.")]
		public bool PriorityBoostEnabled {
			get {
				return(false);
			}
			set {
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of memory exclusively used by this process.")]
		[Obsolete ("Use PrivateMemorySize64")]
		public int PrivateMemorySize {
			get {
				int error;
				return (int)GetProcessData (processId, 6, out error);
			}
		}

		[MonoNotSupported ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The session ID for this process.")]
		public int SessionId {
			get { return 0; }
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static string ProcessName_icall (IntPtr handle);

		static string ProcessName_internal(SafeProcessHandle handle)
		{
			bool release = false;
			try {
				handle.DangerousAddRef (ref release);
				return ProcessName_icall (handle.DangerousGetHandle ());
			} finally {
				if (release)
					handle.DangerousRelease ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The name of this process.")]
		public string ProcessName {
			get {
				if (process_name == null) {
					SafeProcessHandle handle = null;
					try {
						handle = GetProcessHandle (NativeMethods.PROCESS_QUERY_INFORMATION);

						process_name = ProcessName_internal (handle);

						/* If process_name is _still_ null, assume the process has exited or is inaccessible */
						if (process_name == null)
							throw new InvalidOperationException ("Process has exited or is inaccessible, so the requested information is not available.");

						/* Strip the suffix (if it exists) simplistically instead of removing
						 * any trailing \.???, so we dont get stupid results on sane systems */
						if(process_name.EndsWith(".exe") || process_name.EndsWith(".bat") || process_name.EndsWith(".com"))
							process_name = process_name.Substring (0, process_name.Length - 4);
					} finally {
						ReleaseProcessHandle (handle);
					}
				}
				return process_name;
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("Allowed processor that can be used by this process.")]
		public IntPtr ProcessorAffinity {
			get {
				return((IntPtr)0);
			}
			set {
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("Is this process responsive.")]
		public bool Responding {
			get {
				return(false);
			}
		}

#if !MONO_FEATURE_PROCESS_START
		[Obsolete ("Process.StartInfo is not supported on the current platform.", true)]
		public ProcessStartInfo StartInfo {
			get { throw new PlatformNotSupportedException ("Process.StartInfo is not supported on the current platform."); }
			set { throw new PlatformNotSupportedException ("Process.StartInfo is not supported on the current platform."); }
		}

		[Obsolete ("Process.StandardError is not supported on the current platform.", true)]
		public StreamReader StandardError {
			get { throw new PlatformNotSupportedException ("Process.StandardError is not supported on the current platform."); }
		}

		[Obsolete ("Process.StandardInput is not supported on the current platform.", true)]
		public StreamWriter StandardInput {
			get { throw new PlatformNotSupportedException ("Process.StandardInput is not supported on the current platform."); }
		}

		[Obsolete ("Process.StandardOutput is not supported on the current platform.", true)]
		public StreamReader StandardOutput {
			get { throw new PlatformNotSupportedException ("Process.StandardOutput is not supported on the current platform."); }
		}
#endif // !MONO_FEATURE_PROCESS_START

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The number of threads of this process.")]
		public ProcessThreadCollection Threads {
			get {
				if (threads == null) {
					int error;
					// This'll return a correctly-sized array of empty ProcessThreads for now.
					threads = new ProcessThreadCollection(new ProcessThread [GetProcessData (processId, 0, out error)]);
				}

				return threads;
			}
		}

		[Obsolete ("Use VirtualMemorySize64")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of virtual memory currently used for this process.")]
		public int VirtualMemorySize {
			get {
				int error;
				return (int)GetProcessData (processId, 7, out error);
			}
		}

		[Obsolete ("Use WorkingSet64")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of physical memory currently used for this process.")]
		public int WorkingSet {
			get {
				int error;
				return (int)GetProcessData (processId, 4, out error);
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of memory exclusively used by this process.")]
		[ComVisible (false)]
		public long PrivateMemorySize64 {
			get {
				int error;
				return GetProcessData (processId, 6, out error);
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of virtual memory currently used for this process.")]
		[ComVisible (false)]
		public long VirtualMemorySize64 {
			get {
				int error;
				return GetProcessData (processId, 7, out error);
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of physical memory currently used for this process.")]
		[ComVisible (false)]
		public long WorkingSet64 {
			get {
				int error;
				return GetProcessData (processId, 4, out error);
			}
		}

		public bool CloseMainWindow ()
		{
			SafeProcessHandle handle = null;
			try {
				handle = GetProcessHandle (NativeMethods.PROCESS_TERMINATE);
				return NativeMethods.TerminateProcess(handle, -2);
			} finally {
				ReleaseProcessHandle(handle);
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static IntPtr GetProcess_internal(int pid);

		[MonoTODO ("There is no support for retrieving process information from a remote machine")]
		public static Process GetProcessById(int processId, string machineName) {
			if (machineName == null)
				throw new ArgumentNullException ("machineName");

			if (!IsLocalMachine (machineName))
				throw new NotImplementedException ();

			IntPtr proc = GetProcess_internal(processId);

			if (proc == IntPtr.Zero)
				throw new ArgumentException ("Can't find process with ID " + processId.ToString ());

			/* The handle returned by GetProcess_internal is owned by its caller, so we must pass true to SafeProcessHandle */
			return (new Process (new SafeProcessHandle (proc, true), processId));
		}

		public static Process[] GetProcessesByName(string processName, string machineName)
		{
			if (machineName == null)
				throw new ArgumentNullException ("machineName");

			if (!IsLocalMachine (machineName))
				throw new NotImplementedException ();

			Process[] processes = GetProcesses ();
			if (processes.Length == 0)
				return processes;

			int size = 0;

			for (int i = 0; i < processes.Length; i++) {
				var process = processes[i];
				try {
					if (String.Compare (processName, process.ProcessName, true) == 0)
						processes [size++] = process;
					else
						process.Dispose();
				} catch (SystemException) {
					/* The process might exit between GetProcesses_internal and GetProcessById */
				}
			}

			Array.Resize<Process> (ref processes, size);

			return processes;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int[] GetProcesses_internal();

		[MonoTODO ("There is no support for retrieving process information from a remote machine")]
		public static Process[] GetProcesses(string machineName) {
			if (machineName == null)
				throw new ArgumentNullException ("machineName");

			if (!IsLocalMachine (machineName))
				throw new NotImplementedException ();

			int [] pids = GetProcesses_internal ();
			if (pids == null)
				return new Process [0];

			var proclist = new List<Process> (pids.Length);
			for (int i = 0; i < pids.Length; i++) {
				try {
					proclist.Add (GetProcessById (pids [i]));
				} catch (SystemException) {
					/* The process might exit
					 * between
					 * GetProcesses_internal and
					 * GetProcessById
					 */
				}
			}

			return proclist.ToArray ();
		}

		private static bool IsLocalMachine (string machineName)
		{
			if (machineName == "." || machineName.Length == 0)
				return true;

			return (string.Compare (machineName, Environment.MachineName, true) == 0);
		}

#if MONO_FEATURE_PROCESS_START
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool ShellExecuteEx_internal(ProcessStartInfo startInfo, ref ProcInfo procInfo);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool CreateProcess_internal(ProcessStartInfo startInfo, IntPtr stdin, IntPtr stdout, IntPtr stderr, ref ProcInfo procInfo);

		bool StartWithShellExecuteEx (ProcessStartInfo startInfo)
		{
			if (this.disposed)
				throw new ObjectDisposedException (GetType ().Name);

			if (!String.IsNullOrEmpty(startInfo.UserName) || (startInfo.Password != null))
				throw new InvalidOperationException(SR.GetString(SR.CantStartAsUser));

			if (startInfo.RedirectStandardInput || startInfo.RedirectStandardOutput || startInfo.RedirectStandardError)
				throw new InvalidOperationException(SR.GetString(SR.CantRedirectStreams));

			if (startInfo.StandardErrorEncoding != null)
				throw new InvalidOperationException(SR.GetString(SR.StandardErrorEncodingNotAllowed));

			if (startInfo.StandardOutputEncoding != null)
				throw new InvalidOperationException(SR.GetString(SR.StandardOutputEncodingNotAllowed));

			// can't set env vars with ShellExecuteEx...
			if (startInfo.environmentVariables != null)
				throw new InvalidOperationException(SR.GetString(SR.CantUseEnvVars));

			ProcInfo procInfo = new ProcInfo();
			bool ret;

			FillUserInfo (startInfo, ref procInfo);
			try {
				ret = ShellExecuteEx_internal (startInfo, ref procInfo);
			} finally {
				if (procInfo.Password != IntPtr.Zero)
					Marshal.ZeroFreeBSTR (procInfo.Password);
				procInfo.Password = IntPtr.Zero;
			}
			if (!ret) {
				throw new Win32Exception (-procInfo.pid);
			}

			SetProcessHandle (new SafeProcessHandle (procInfo.process_handle, true));
			SetProcessId (procInfo.pid);

			return ret;
		}

		//
		// Creates a pipe with read and write descriptors
		//
		static void CreatePipe (out IntPtr read, out IntPtr write, bool writeDirection)
		{
			MonoIOError error;

			//
			// Creates read/write pipe from parent -> child perspective
			// a child process uses same descriptors after fork. That's
			// 4 descriptors in total where only 2. One in child, one in parent
			// should be active and the other 2 closed. Which ones depends on
			// comunication direction
			//
			// parent  -------->  child   (parent can write, child can read)
			//
			// read: closed       read: used
			// write: used        write: closed
			//
			//
			// parent  <--------  child   (parent can read, child can write)
			//
			// read: used         read: closed
			// write: closed      write: used
			//
			// It can still be tricky for predefined descriptiors http://unixwiz.net/techtips/remap-pipe-fds.html
			//
			if (!MonoIO.CreatePipe (out read, out write, out error))
				throw MonoIO.GetException (error);

			if (IsWindows) {
				const int DUPLICATE_SAME_ACCESS = 0x00000002;
				var tmp = writeDirection ? write : read;

				if (!MonoIO.DuplicateHandle (Process.GetCurrentProcess ().Handle, tmp, Process.GetCurrentProcess ().Handle, out tmp, 0, 0, DUPLICATE_SAME_ACCESS, out error))
					throw MonoIO.GetException (error);

				if (writeDirection) {
					if (!MonoIO.Close (write, out error))
						throw MonoIO.GetException (error);
					write = tmp;
				} else {
					if (!MonoIO.Close (read, out error))
						throw MonoIO.GetException (error);
					read = tmp;
				}
			}
		}

		static bool IsWindows
		{
			get
			{
				PlatformID platform = Environment.OSVersion.Platform;
				if (platform == PlatformID.Win32S ||
					platform == PlatformID.Win32Windows ||
					platform == PlatformID.Win32NT ||
					platform == PlatformID.WinCE) {
					return true;
				}
				return false;
			}
		}

		bool StartWithCreateProcess (ProcessStartInfo startInfo)
		{
			if (startInfo.StandardOutputEncoding != null && !startInfo.RedirectStandardOutput)
				throw new InvalidOperationException (SR.GetString(SR.StandardOutputEncodingNotAllowed));

			if (startInfo.StandardErrorEncoding != null && !startInfo.RedirectStandardError)
				throw new InvalidOperationException (SR.GetString(SR.StandardErrorEncodingNotAllowed));

			if (this.disposed)
				throw new ObjectDisposedException (GetType ().Name);

			var procInfo = new ProcInfo ();

			if (startInfo.HaveEnvVars) {
				List<string> envVariables = new List<string> ();

				foreach (DictionaryEntry de in startInfo.EnvironmentVariables) {
					if (de.Value == null)
						continue;

					envVariables.Add (string.Concat (
						(string) de.Key,
						"=",
						(string) de.Value));
				}

				procInfo.envVariables = envVariables.ToArray ();
			}

			MonoIOError error;
			IntPtr stdin_read = IntPtr.Zero, stdin_write = IntPtr.Zero;
			IntPtr stdout_read = IntPtr.Zero, stdout_write = IntPtr.Zero;
			IntPtr stderr_read = IntPtr.Zero, stderr_write = IntPtr.Zero;

			try {
				if (startInfo.RedirectStandardInput) {
					CreatePipe (out stdin_read, out stdin_write, true);
				} else {
					stdin_read = MonoIO.ConsoleInput;
					stdin_write = IntPtr.Zero;
				}

				if (startInfo.RedirectStandardOutput) {
					CreatePipe (out stdout_read, out stdout_write, false);
				} else {
					stdout_read = IntPtr.Zero;
					stdout_write = MonoIO.ConsoleOutput;
				}

				if (startInfo.RedirectStandardError) {
					CreatePipe (out stderr_read, out stderr_write, false);
				} else {
					stderr_read = IntPtr.Zero;
					stderr_write = MonoIO.ConsoleError;
				}

				FillUserInfo (startInfo, ref procInfo);

				//
				// FIXME: For redirected pipes we need to send descriptors of
				// stdin_write, stdout_read, stderr_read to child process and
				// close them there (fork makes exact copy of parent's descriptors)
				//
				if (!CreateProcess_internal (startInfo, stdin_read, stdout_write, stderr_write, ref procInfo)) {
					throw new Win32Exception (-procInfo.pid, "ApplicationName='" + startInfo.FileName + "', CommandLine='" + startInfo.Arguments +
						"', CurrentDirectory='" + startInfo.WorkingDirectory + "', Native error= " + Win32Exception.GetErrorMessage (-procInfo.pid));
				}
			} catch {
				if (startInfo.RedirectStandardInput) {
					if (stdin_read != IntPtr.Zero)
						MonoIO.Close (stdin_read, out error);
					if (stdin_write != IntPtr.Zero)
						MonoIO.Close (stdin_write, out error);
				}

				if (startInfo.RedirectStandardOutput) {
					if (stdout_read != IntPtr.Zero)
						MonoIO.Close (stdout_read, out error);
					if (stdout_write != IntPtr.Zero)
						MonoIO.Close (stdout_write, out error);
				}

				if (startInfo.RedirectStandardError) {
					if (stderr_read != IntPtr.Zero)
						MonoIO.Close (stderr_read, out error);
					if (stderr_write != IntPtr.Zero)
						MonoIO.Close (stderr_write, out error);
				}

				throw;
			} finally {
				if (procInfo.Password != IntPtr.Zero) {
					Marshal.ZeroFreeBSTR (procInfo.Password);
					procInfo.Password = IntPtr.Zero;
				}
			}

			SetProcessHandle (new SafeProcessHandle (procInfo.process_handle, true));
			SetProcessId (procInfo.pid);
			
#pragma warning disable 618

			if (startInfo.RedirectStandardInput) {
				MonoIO.Close (stdin_read, out error);

#if MOBILE
				var stdinEncoding = startInfo.StandardInputEncoding ?? Encoding.Default;
#else
				var stdinEncoding = startInfo.StandardInputEncoding ?? Console.InputEncoding;
#endif
				standardInput = new StreamWriter (new FileStream (stdin_write, FileAccess.Write, true, 8192), stdinEncoding) {
					AutoFlush = true
				};
			}

			if (startInfo.RedirectStandardOutput) {
				MonoIO.Close (stdout_write, out error);

				Encoding stdoutEncoding = startInfo.StandardOutputEncoding ?? Console.OutputEncoding;

				standardOutput = new StreamReader (new FileStream (stdout_read, FileAccess.Read, true, 8192), stdoutEncoding, true);
			}

			if (startInfo.RedirectStandardError) {
				MonoIO.Close (stderr_write, out error);

				Encoding stderrEncoding = startInfo.StandardErrorEncoding ?? Console.OutputEncoding;

				standardError = new StreamReader (new FileStream (stderr_read, FileAccess.Read, true, 8192), stderrEncoding, true);
			}
#pragma warning restore

			return true;
		}

		// Note that ProcInfo.Password must be freed.
		private static void FillUserInfo (ProcessStartInfo startInfo, ref ProcInfo procInfo)
		{
			if (startInfo.UserName.Length != 0) {
				procInfo.UserName = startInfo.UserName;
				procInfo.Domain = startInfo.Domain;
				if (startInfo.Password != null)
					procInfo.Password = Marshal.SecureStringToBSTR (startInfo.Password);
				else
					procInfo.Password = IntPtr.Zero;
				procInfo.LoadUserProfile = startInfo.LoadUserProfile;
			}
		}
#else
		[Obsolete ("Process.Start is not supported on the current platform.", true)]
		public bool Start ()
		{
			throw new PlatformNotSupportedException ("Process.Start is not supported on the current platform.");
		}

		[Obsolete ("Process.Start is not supported on the current platform.", true)]
		public static Process Start (ProcessStartInfo startInfo)
		{
			throw new PlatformNotSupportedException ("Process.Start is not supported on the current platform.");
		}

		[Obsolete ("Process.Start is not supported on the current platform.", true)]
		public static Process Start (string fileName)
		{
			throw new PlatformNotSupportedException ("Process.Start is not supported on the current platform.");
		}

		[Obsolete ("Process.Start is not supported on the current platform.", true)]
		public static Process Start(string fileName, string arguments)
		{
			throw new PlatformNotSupportedException ("Process.Start is not supported on the current platform.");
		}

		[Obsolete ("Process.Start is not supported on the current platform.", true)]
		public static Process Start(string fileName, string userName, SecureString password, string domain)
		{
			throw new PlatformNotSupportedException ("Process.Start is not supported on the current platform.");
		}

		[Obsolete ("Process.Start is not supported on the current platform.", true)]
		public static Process Start(string fileName, string arguments, string userName, SecureString password, string domain)
		{
			throw new PlatformNotSupportedException ("Process.Start is not supported on the current platform.");
		}
#endif // MONO_FEATURE_PROCESS_START

#if !MONO_FEATURE_PROCESS_START
		[Obsolete ("Process.BeginOutputReadLine is not supported on the current platform.", true)]
		public void BeginOutputReadLine ()
		{
			throw new PlatformNotSupportedException ("Process.BeginOutputReadLine is not supported on the current platform.");
		}

		[Obsolete ("Process.BeginOutputReadLine is not supported on the current platform.", true)]
		public void CancelOutputRead ()
		{
			throw new PlatformNotSupportedException ("Process.BeginOutputReadLine is not supported on the current platform.");
		}

		[Obsolete ("Process.BeginOutputReadLine is not supported on the current platform.", true)]
		public void BeginErrorReadLine ()
		{
			throw new PlatformNotSupportedException ("Process.BeginOutputReadLine is not supported on the current platform.");
		}

		[Obsolete ("Process.BeginOutputReadLine is not supported on the current platform.", true)]
		public void CancelErrorRead ()
		{
			throw new PlatformNotSupportedException ("Process.BeginOutputReadLine is not supported on the current platform.");
		}
#endif // !MONO_FEATURE_PROCESS_START

		/// <devdoc>
		///     Raise the Exited event, but make sure we don't do it more than once.
		/// </devdoc>
		/// <internalonly/>
		void RaiseOnExited() {
			if (!watchForExit)
				return;
			if (!raisedOnExited) {
				lock (this) {
					if (!raisedOnExited) {
						raisedOnExited = true;
						OnExited();
					}
				}
			}
		}
	}
}

