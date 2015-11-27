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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace System.Diagnostics {

	[DefaultEvent ("Exited"), DefaultProperty ("StartInfo")]
	[Designer ("System.Diagnostics.Design.ProcessDesigner, " + Consts.AssemblySystem_Design)]
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	[PermissionSet (SecurityAction.InheritanceDemand, Unrestricted = true)]
	[MonitoringDescription ("Represents a system process")]
	public class Process : Component 
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
			public IntPtr thread_handle;
			public int pid; // Contains -GetLastError () on failure.
			public int tid;
			public string [] envKeys;
			public string [] envValues;
			public string UserName;
			public string Domain;
			public IntPtr Password;
			public bool LoadUserProfile;
		};

		IntPtr process_handle;
		int pid;
		int enable_raising_events;
		Thread background_wait_for_exit_thread;
		ISynchronizeInvoke synchronizingObject;
		EventHandler exited_event;

		/* Private constructor called from other methods */
		private Process(IntPtr handle, int id) {
			process_handle = handle;
			pid=id;
		}

		public Process ()
		{
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("Base process priority.")]
		public int BasePriority {
			get { return 0; }
		}

		[DefaultValue (false), Browsable (false)]
		[MonitoringDescription ("Check for exiting of the process to raise the apropriate event.")]
		public bool EnableRaisingEvents {
			get {
				return enable_raising_events == 1;
			}
			set {
				if (value && Interlocked.Exchange (ref enable_raising_events, 1) == 0)
					StartBackgroundWaitForExit ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int ExitCode_internal(IntPtr handle);

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The exit code of the process.")]
		public int ExitCode {
			get {
				if (process_handle == IntPtr.Zero)
					throw new InvalidOperationException ("Process has not been started.");

				int code = ExitCode_internal (process_handle);
				if (code == 259)
					throw new InvalidOperationException ("The process must exit before getting the requested information.");

				return code;
			}
		}

		/* Returns the process start time in Windows file
		 * times (ticks from DateTime(1/1/1601 00:00 GMT))
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static long ExitTime_internal(IntPtr handle);
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The exit time of the process.")]
		public DateTime ExitTime {
			get {
				if (process_handle == IntPtr.Zero)
					throw new InvalidOperationException ("Process has not been started.");

				if (!HasExited)
					throw new InvalidOperationException ("The process must exit before " +
									"getting the requested information.");

				return(DateTime.FromFileTime(ExitTime_internal(process_handle)));
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("Handle for this process.")]
		public IntPtr Handle {
			get {
				if (process_handle == IntPtr.Zero)
					throw new InvalidOperationException ("No process is associated with this object.");
				return(process_handle);
			}
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
		[MonitoringDescription ("Determines if the process is still running.")]
		public bool HasExited {
			get {
				if (process_handle == IntPtr.Zero)
					throw new InvalidOperationException ("Process has not been started.");
					
				int exitcode = ExitCode_internal (process_handle);

				if(exitcode==259) {
					/* STILL_ACTIVE */
					return(false);
				} else {
					return(true);
				}
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("Process identifier.")]
		public int Id {
			get {
				if (pid == 0)
					throw new InvalidOperationException ("Process ID has not been set.");

				return(pid);
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The name of the computer running the process.")]
		public string MachineName {
			get {
				return("localhost");
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The main module of the process.")]
		public ProcessModule MainModule {
			get {
				return(this.Modules[0]);
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

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool GetWorkingSet_internal(IntPtr handle, out int min, out int max);
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool SetWorkingSet_internal(IntPtr handle, int min, int max, bool use_min);

		/* LAMESPEC: why is this an IntPtr not a plain int? */
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The maximum working set for this process.")]
		public IntPtr MaxWorkingSet {
			get {
				if(HasExited)
					throw new InvalidOperationException(
						"The process " + ProcessName +
						" (ID " + Id + ") has exited");
				
				int min;
				int max;
				bool ok=GetWorkingSet_internal(process_handle, out min, out max);
				if(ok==false) {
					throw new Win32Exception();
				}
				
				return((IntPtr)max);
			}
			set {
				if(HasExited) {
					throw new InvalidOperationException("The process " + ProcessName + " (ID " + Id + ") has exited");
				}
				
				bool ok=SetWorkingSet_internal(process_handle, 0, value.ToInt32(), false);
				if(ok==false) {
					throw new Win32Exception();
				}
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The minimum working set for this process.")]
		public IntPtr MinWorkingSet {
			get {
				if(HasExited)
					throw new InvalidOperationException(
						"The process " + ProcessName +
						" (ID " + Id + ") has exited");
				
				int min;
				int max;
				bool ok= GetWorkingSet_internal (process_handle, out min, out max);
				if(!ok)
					throw new Win32Exception();
				return ((IntPtr) min);
			}
			set {
				if(HasExited)
					throw new InvalidOperationException(
						"The process " + ProcessName +
						" (ID " + Id + ") has exited");
				
				bool ok = SetWorkingSet_internal (process_handle, value.ToInt32(), 0, true);
				if (!ok)
					throw new Win32Exception();
			}
		}

		/* Returns the list of process modules.  The main module is
		 * element 0.
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern ProcessModule[] GetModules_internal(IntPtr handle);

		private ProcessModuleCollection module_collection;
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The modules that are loaded as part of this process.")]
		public ProcessModuleCollection Modules {
			get {
				if (module_collection == null)
					module_collection = new ProcessModuleCollection(
						GetModules_internal (process_handle));
				return(module_collection);
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
				return (int)GetProcessData (pid, 8, out error);
			}
		}

		[Obsolete ("Use PeakWorkingSet64")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The maximum amount of system memory used by this process.")]
		public int PeakWorkingSet {
			get {
				int error;
				return (int)GetProcessData (pid, 5, out error);
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
				return GetProcessData (pid, 12, out error);
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
				return GetProcessData (pid, 8, out error);
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The maximum amount of system memory used by this process.")]
		[ComVisible (false)]
		public long PeakWorkingSet64 {
			get {
				int error;
				return GetProcessData (pid, 5, out error);
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

		[MonoLimitation ("Under Unix, only root is allowed to raise the priority.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The relative process priority.")]
		public ProcessPriorityClass PriorityClass {
			get {
				if (process_handle == IntPtr.Zero)
					throw new InvalidOperationException ("Process has not been started.");
				
				int error;
				int prio = GetPriorityClass (process_handle, out error);
				if (prio == 0)
					throw new Win32Exception (error);
				return (ProcessPriorityClass) prio;
			}
			set {
				if (!Enum.IsDefined (typeof (ProcessPriorityClass), value))
					throw new InvalidEnumArgumentException (
						"value", (int) value,
						typeof (ProcessPriorityClass));

				if (process_handle == IntPtr.Zero)
					throw new InvalidOperationException ("Process has not been started.");
				
				int error;
				if (!SetPriorityClass (process_handle, (int) value, out error)) {
					CheckExited ();
					throw new Win32Exception (error);
				}
			}
		}

		void CheckExited () {
			if (HasExited)
				throw new InvalidOperationException (String.Format ("Cannot process request because the process ({0}) has exited.", Id));
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern int GetPriorityClass (IntPtr handle, out int error);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern bool SetPriorityClass (IntPtr handle, int priority, out int error);

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of memory exclusively used by this process.")]
		[Obsolete ("Use PrivateMemorySize64")]
		public int PrivateMemorySize {
			get {
				int error;
				return (int)GetProcessData (pid, 6, out error);
			}
		}

		[MonoNotSupported ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The session ID for this process.")]
		public int SessionId {
			get { throw new NotImplementedException (); }
		}

		/* the meaning of type is as follows: 0: user, 1: system, 2: total */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static long Times (IntPtr handle, int type);

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of processing time spent in the OS core for this process.")]
		public TimeSpan PrivilegedProcessorTime {
			get {
				return new TimeSpan (Times (process_handle, 1));
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static string ProcessName_internal(IntPtr handle);
		
		private string process_name=null;
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The name of this process.")]
		public string ProcessName {
			get {
				if(process_name==null) {
					
					if (process_handle == IntPtr.Zero)
						throw new InvalidOperationException ("No process is associated with this object.");
					
					process_name=ProcessName_internal(process_handle);
					/* If process_name is _still_
					 * null, assume the process
					 * has exited
					 */
					if (process_name == null)
						throw new InvalidOperationException ("Process has exited, so the requested information is not available.");
					
					/* Strip the suffix (if it
					 * exists) simplistically
					 * instead of removing any
					 * trailing \.???, so we dont
					 * get stupid results on sane
					 * systems
					 */
					if(process_name.EndsWith(".exe") ||
					   process_name.EndsWith(".bat") ||
					   process_name.EndsWith(".com")) {
						process_name=process_name.Substring(0, process_name.Length-4);
					}
				}
				return(process_name);
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

#if MONO_FEATURE_PROCESS_START
		private StreamReader error_stream=null;
		bool error_stream_exposed;

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The standard error stream of this process.")]
		public StreamReader StandardError {
			get {
				if (error_stream == null)
					throw new InvalidOperationException("Standard error has not been redirected");

				if ((async_mode & AsyncModes.AsyncError) != 0)
					throw new InvalidOperationException ("Cannot mix asynchronous and synchonous reads.");

				async_mode |= AsyncModes.SyncError;

				error_stream_exposed = true;
				return(error_stream);
			}
		}

		private StreamWriter input_stream=null;
		bool input_stream_exposed;
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The standard input stream of this process.")]
		public StreamWriter StandardInput {
			get {
				if (input_stream == null)
					throw new InvalidOperationException("Standard input has not been redirected");

				input_stream_exposed = true;
				return(input_stream);
			}
		}

		private StreamReader output_stream=null;
		bool output_stream_exposed;
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The standard output stream of this process.")]
		public StreamReader StandardOutput {
			get {
				if (output_stream == null)
					throw new InvalidOperationException("Standard output has not been redirected");

				if ((async_mode & AsyncModes.AsyncOutput) != 0)
					throw new InvalidOperationException ("Cannot mix asynchronous and synchonous reads.");

				async_mode |= AsyncModes.SyncOutput;

				output_stream_exposed = true;
				return(output_stream);
			}
		}

		private ProcessStartInfo start_info=null;
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content), Browsable (false)]
		[MonitoringDescription ("Information for the start of this process.")]
		public ProcessStartInfo StartInfo {
			get {
				if (start_info == null)
					start_info = new ProcessStartInfo();
				return start_info;
			}
			set {
				if (value == null)
					throw new ArgumentNullException("value");
				start_info = value;
			}
		}
#else
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

		[Obsolete ("Process.StartInfo is not supported on the current platform.", true)]
		public ProcessStartInfo StartInfo {
			get { throw new PlatformNotSupportedException ("Process.StartInfo is not supported on the current platform."); }
			set { throw new PlatformNotSupportedException ("Process.StartInfo is not supported on the current platform."); }
		}
#endif // MONO_FEATURE_PROCESS_START

		/* Returns the process start time in Windows file
		 * times (ticks from DateTime(1/1/1601 00:00 GMT))
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static long StartTime_internal(IntPtr handle);
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The time this process started.")]
		public DateTime StartTime {
			get {
				return(DateTime.FromFileTime(StartTime_internal(process_handle)));
			}
		}

		[DefaultValue (null), Browsable (false)]
		[MonitoringDescription ("The object that is used to synchronize event handler calls for this process.")]
		public ISynchronizeInvoke SynchronizingObject {
			get { return synchronizingObject; }
			set { synchronizingObject = value; }
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The number of threads of this process.")]
		public ProcessThreadCollection Threads {
			get {
				// This'll return a correctly-sized array of empty ProcessThreads for now.
				int error;
				return new ProcessThreadCollection(new ProcessThread[GetProcessData (pid, 0, out error)]);
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The total CPU time spent for this process.")]
		public TimeSpan TotalProcessorTime {
			get {
				return new TimeSpan (Times (process_handle, 2));
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The CPU time spent for this process in user mode.")]
		public TimeSpan UserProcessorTime {
			get {
				return new TimeSpan (Times (process_handle, 0));
			}
		}

		[Obsolete ("Use VirtualMemorySize64")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of virtual memory currently used for this process.")]
		public int VirtualMemorySize {
			get {
				int error;
				return (int)GetProcessData (pid, 7, out error);
			}
		}

		[Obsolete ("Use WorkingSet64")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of physical memory currently used for this process.")]
		public int WorkingSet {
			get {
				int error;
				return (int)GetProcessData (pid, 4, out error);
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of memory exclusively used by this process.")]
		[ComVisible (false)]
		public long PrivateMemorySize64 {
			get {
				int error;
				return GetProcessData (pid, 6, out error);
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of virtual memory currently used for this process.")]
		[ComVisible (false)]
		public long VirtualMemorySize64 {
			get {
				int error;
				return GetProcessData (pid, 7, out error);
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of physical memory currently used for this process.")]
		[ComVisible (false)]
		public long WorkingSet64 {
			get {
				int error;
				return GetProcessData (pid, 4, out error);
			}
		}

		public void Close()
		{
			Dispose (true);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static bool Kill_internal (IntPtr handle, int signo);

		/* int kill -> 1 KILL, 2 CloseMainWindow */
		bool Close (int signo)
		{
			if (process_handle == IntPtr.Zero)
				throw new SystemException ("No process to kill.");

			int exitcode = ExitCode_internal (process_handle);
			if (exitcode != 259)
				throw new InvalidOperationException ("The process already finished.");

			return Kill_internal (process_handle, signo);
		}

		public bool CloseMainWindow ()
		{
			return Close (2);
		}

		[MonoTODO]
		public static void EnterDebugMode() {
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static IntPtr GetProcess_internal(int pid);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int GetPid_internal();

		public static Process GetCurrentProcess()
		{
			int pid = GetPid_internal();
			IntPtr proc = GetProcess_internal(pid);
			
			if (proc == IntPtr.Zero)
				throw new SystemException("Can't find current process");

			return (new Process (proc, pid));
		}

		public static Process GetProcessById(int processId)
		{
			IntPtr proc = GetProcess_internal(processId);
			
			if (proc == IntPtr.Zero)
				throw new ArgumentException ("Can't find process with ID " + processId.ToString ());

			return (new Process (proc, processId));
		}

		[MonoTODO ("There is no support for retrieving process information from a remote machine")]
		public static Process GetProcessById(int processId, string machineName) {
			if (machineName == null)
				throw new ArgumentNullException ("machineName");

			if (!IsLocalMachine (machineName))
				throw new NotImplementedException ();

			return GetProcessById (processId);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int[] GetProcesses_internal();

		public static Process[] GetProcesses ()
		{
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

		[MonoTODO ("There is no support for retrieving process information from a remote machine")]
		public static Process[] GetProcesses(string machineName) {
			if (machineName == null)
				throw new ArgumentNullException ("machineName");

			if (!IsLocalMachine (machineName))
				throw new NotImplementedException ();

			return GetProcesses ();
		}

		public static Process[] GetProcessesByName(string processName)
		{
			int [] pids = GetProcesses_internal ();
			if (pids == null)
				return new Process [0];
			
			var proclist = new List<Process> (pids.Length);
			for (int i = 0; i < pids.Length; i++) {
				try {
					Process p = GetProcessById (pids [i]);
					if (String.Compare (processName, p.ProcessName, true) == 0)
						proclist.Add (p);
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

		[MonoTODO]
		public static Process[] GetProcessesByName(string processName, string machineName) {
			throw new NotImplementedException();
		}

		public void Kill ()
		{
			Close (1);
		}

		[MonoTODO]
		public static void LeaveDebugMode() {
		}

		public void Refresh ()
		{
			// FIXME: should refresh any cached data we might have about
			// the process (currently we have none).
		}

#if MONO_FEATURE_PROCESS_START
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool ShellExecuteEx_internal(ProcessStartInfo startInfo,
								   ref ProcInfo proc_info);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool CreateProcess_internal(ProcessStartInfo startInfo,
								  IntPtr stdin,
								  IntPtr stdout,
								  IntPtr stderr,
								  ref ProcInfo proc_info);

		private static bool Start_shell (ProcessStartInfo startInfo, Process process)
		{
			ProcInfo proc_info=new ProcInfo();
			bool ret;

			if (startInfo.RedirectStandardInput ||
			    startInfo.RedirectStandardOutput ||
			    startInfo.RedirectStandardError) {
				throw new InvalidOperationException ("UseShellExecute must be false when redirecting I/O.");
			}

			if (startInfo.HaveEnvVars)
				throw new InvalidOperationException ("UseShellExecute must be false in order to use environment variables.");

			FillUserInfo (startInfo, ref proc_info);
			try {
				ret = ShellExecuteEx_internal (startInfo,
							       ref proc_info);
			} finally {
				if (proc_info.Password != IntPtr.Zero)
					Marshal.ZeroFreeBSTR (proc_info.Password);
				proc_info.Password = IntPtr.Zero;
			}
			if (!ret) {
				throw new Win32Exception (-proc_info.pid);
			}

			process.process_handle = proc_info.process_handle;
			process.pid = proc_info.pid;
			process.StartBackgroundWaitForExit ();
			return(ret);
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

		static bool Start_noshell (ProcessStartInfo startInfo, Process process)
		{
			var proc_info = new ProcInfo ();

			if (startInfo.HaveEnvVars) {
				string [] strs = new string [startInfo.EnvironmentVariables.Count];
				startInfo.EnvironmentVariables.Keys.CopyTo (strs, 0);
				proc_info.envKeys = strs;

				strs = new string [startInfo.EnvironmentVariables.Count];
				startInfo.EnvironmentVariables.Values.CopyTo (strs, 0);
				proc_info.envValues = strs;
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

				FillUserInfo (startInfo, ref proc_info);

				//
				// FIXME: For redirected pipes we need to send descriptors of
				// stdin_write, stdout_read, stderr_read to child process and
				// close them there (fork makes exact copy of parent's descriptors)
				//
				if (!CreateProcess_internal (startInfo, stdin_read, stdout_write, stderr_write, ref proc_info)) {
					throw new Win32Exception (-proc_info.pid, 
					"ApplicationName='" + startInfo.FileName +
					"', CommandLine='" + startInfo.Arguments +
					"', CurrentDirectory='" + startInfo.WorkingDirectory +
					"', Native error= " + Win32Exception.W32ErrorMessage (-proc_info.pid));
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
				if (proc_info.Password != IntPtr.Zero) {
					Marshal.ZeroFreeBSTR (proc_info.Password);
					proc_info.Password = IntPtr.Zero;
				}
			}

			process.process_handle = proc_info.process_handle;
			process.pid = proc_info.pid;
			
			if (startInfo.RedirectStandardInput) {
				//
				// FIXME: The descriptor needs to be closed but due to wapi io-layer
				// not coping with duplicated descriptors any StandardInput write fails
				//
				// MonoIO.Close (stdin_read, out error);

#if MOBILE
				var stdinEncoding = Encoding.Default;
#else
				var stdinEncoding = Console.InputEncoding;
#endif
				process.input_stream = new StreamWriter (new FileStream (stdin_write, FileAccess.Write, true, 8192), stdinEncoding) {
					AutoFlush = true
				};
			}

			if (startInfo.RedirectStandardOutput) {
				MonoIO.Close (stdout_write, out error);

				Encoding stdoutEncoding = startInfo.StandardOutputEncoding ?? Console.Out.Encoding;

				process.output_stream = new StreamReader (new FileStream (stdout_read, FileAccess.Read, true, 8192), stdoutEncoding, true);
			}

			if (startInfo.RedirectStandardError) {
				MonoIO.Close (stderr_write, out error);

				Encoding stderrEncoding = startInfo.StandardErrorEncoding ?? Console.Out.Encoding;

				process.error_stream = new StreamReader (new FileStream (stderr_read, FileAccess.Read, true, 8192), stderrEncoding, true);
			}

			process.StartBackgroundWaitForExit ();

			return true;
		}

		// Note that ProcInfo.Password must be freed.
		private static void FillUserInfo (ProcessStartInfo startInfo, ref ProcInfo proc_info)
		{
			if (startInfo.UserName.Length != 0) {
				proc_info.UserName = startInfo.UserName;
				proc_info.Domain = startInfo.Domain;
				if (startInfo.Password != null)
					proc_info.Password = Marshal.SecureStringToBSTR (startInfo.Password);
				else
					proc_info.Password = IntPtr.Zero;
				proc_info.LoadUserProfile = startInfo.LoadUserProfile;
			}
		}

		private static bool Start_common (ProcessStartInfo startInfo,
						  Process process)
		{
			if (startInfo.FileName.Length == 0)
				throw new InvalidOperationException("File name has not been set");
			
			if (startInfo.StandardErrorEncoding != null && !startInfo.RedirectStandardError)
				throw new InvalidOperationException ("StandardErrorEncoding is only supported when standard error is redirected");
			if (startInfo.StandardOutputEncoding != null && !startInfo.RedirectStandardOutput)
				throw new InvalidOperationException ("StandardOutputEncoding is only supported when standard output is redirected");
			
			if (startInfo.UseShellExecute) {
				if (startInfo.UserName.Length != 0)
					throw new InvalidOperationException ("UseShellExecute must be false if an explicit UserName is specified when starting a process");
				return (Start_shell (startInfo, process));
			} else {
				return (Start_noshell (startInfo, process));
			}
		}
		
		public bool Start ()
		{
			if (process_handle != IntPtr.Zero) {
				Process_free_internal (process_handle);
				process_handle = IntPtr.Zero;
			}
			return Start_common(start_info, this);
		}

		public static Process Start (ProcessStartInfo startInfo)
		{
			if (startInfo == null)
				throw new ArgumentNullException ("startInfo");

			Process process = new Process();
			process.StartInfo = startInfo;
			if (Start_common(startInfo, process) && process.process_handle != IntPtr.Zero)
				return process;
			return null;
		}

		public static Process Start (string fileName)
		{
			return Start (new ProcessStartInfo (fileName));
		}

		public static Process Start(string fileName, string arguments)
		{
			return Start (new ProcessStartInfo (fileName, arguments));
		}

		public static Process Start(string fileName, string username, SecureString password, string domain) {
			return Start(fileName, null, username, password, domain);
		}

		public static Process Start(string fileName, string arguments, string username, SecureString password, string domain) {
			ProcessStartInfo psi = new ProcessStartInfo(fileName, arguments);
			psi.UserName = username;
			psi.Password = password;
			psi.Domain = domain;
			psi.UseShellExecute = false;
			return Start(psi);
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
		public static Process Start(string fileName, string username, SecureString password, string domain)
		{
			throw new PlatformNotSupportedException ("Process.Start is not supported on the current platform.");
		}

		[Obsolete ("Process.Start is not supported on the current platform.", true)]
		public static Process Start(string fileName, string arguments, string username, SecureString password, string domain)
		{
			throw new PlatformNotSupportedException ("Process.Start is not supported on the current platform.");
		}
#endif // MONO_FEATURE_PROCESS_START

		public override string ToString()
		{
			return(base.ToString() + " (" + this.ProcessName + ")");
		}

		/* Waits up to ms milliseconds for process 'handle' to
		 * exit.  ms can be <0 to mean wait forever.
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern bool WaitForExit_internal(IntPtr handle, int ms);

		public void WaitForExit ()
		{
			WaitForExit (-1);
		}

		public bool WaitForExit(int milliseconds) {
			int ms = milliseconds;
			if (ms == int.MaxValue)
				ms = -1;

			if (process_handle == IntPtr.Zero)
				throw new InvalidOperationException ("No process is associated with this object.");

			if (!WaitForExit_internal (process_handle, ms))
				return false;

#if MONO_FEATURE_PROCESS_START
			if (async_output != null && !async_output.IsCompleted)
				async_output.AsyncWaitHandle.WaitOne ();

			if (async_error != null && !async_error.IsCompleted)
				async_error.AsyncWaitHandle.WaitOne ();
#endif // MONO_FEATURE_PROCESS_START

			OnExited ();

			return true;
		}

		/* Waits up to ms milliseconds for process 'handle' to 
		 * wait for input.  ms can be <0 to mean wait forever.
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern bool WaitForInputIdle_internal(IntPtr handle, int ms);

		// The internal call is only implemented properly on Windows.
		[MonoTODO]
		public bool WaitForInputIdle() {
			return WaitForInputIdle (-1);
		}

		// The internal call is only implemented properly on Windows.
		[MonoTODO]
		public bool WaitForInputIdle(int milliseconds) {
			return WaitForInputIdle_internal (process_handle, milliseconds);
		}

		private static bool IsLocalMachine (string machineName)
		{
			if (machineName == "." || machineName.Length == 0)
				return true;

			return (string.Compare (machineName, Environment.MachineName, true) == 0);
		}

		[Browsable (true)]
		[MonitoringDescription ("Raised when it receives output data")]
		public event DataReceivedEventHandler OutputDataReceived;
		[Browsable (true)]
		[MonitoringDescription ("Raised when it receives error data")]
		public event DataReceivedEventHandler ErrorDataReceived;

		void OnOutputDataReceived (string str)
		{
			DataReceivedEventHandler cb = OutputDataReceived;
			if (cb != null)
				cb (this, new DataReceivedEventArgs (str));
		}

		void OnErrorDataReceived (string str)
		{
			DataReceivedEventHandler cb = ErrorDataReceived;
			if (cb != null)
				cb (this, new DataReceivedEventArgs (str));
		}

#if MONO_FEATURE_PROCESS_START
		[Flags]
		enum AsyncModes {
			NoneYet = 0,
			SyncOutput = 1,
			SyncError = 1 << 1,
			AsyncOutput = 1 << 2,
			AsyncError = 1 << 3
		}

		[StructLayout (LayoutKind.Sequential)]
		sealed class ProcessAsyncReader : IOAsyncResult
		{
			Process process;
			IntPtr handle;
			Stream stream;
			bool err_out;

			StringBuilder sb = new StringBuilder ();
			byte[] buffer = new byte [4096];

			const int ERROR_INVALID_HANDLE = 6;

			public ProcessAsyncReader (Process process, FileStream stream, bool err_out)
				: base (null, null)
			{
				this.process = process;
				this.handle = stream.SafeFileHandle.DangerousGetHandle ();
				this.stream = stream;
				this.err_out = err_out;
			}

			public void BeginReadLine ()
			{
				IOSelector.Add (this.handle, new IOSelectorJob (IOOperation.Read, _ => Read (), null));
			}

			void Read ()
			{
				int nread = 0;

				try {
					nread = stream.Read (buffer, 0, buffer.Length);
				} catch (ObjectDisposedException) {
				} catch (IOException ex) {
					if (ex.HResult != (unchecked((int) 0x80070000) | (int) ERROR_INVALID_HANDLE))
						throw;
				} catch (NotSupportedException) {
					if (stream.CanRead)
						throw;
				}

				if (nread == 0) {
					Flush (true);

					if (err_out)
						process.OnOutputDataReceived (null);
					else
						process.OnErrorDataReceived (null);

					IsCompleted = true;

					return;
				}

				try {
					sb.Append (Encoding.Default.GetString (buffer, 0, nread));
				} catch {
					// Just in case the encoding fails...
					for (int i = 0; i < nread; i++) {
						sb.Append ((char) buffer [i]);
					}
				}

				Flush (false);

				IOSelector.Add (this.handle, new IOSelectorJob (IOOperation.Read, _ => Read (), null));
			}

			void Flush (bool last)
			{
				if (sb.Length == 0 || (err_out && process.output_canceled) || (!err_out && process.error_canceled))
					return;

				string[] strs = sb.ToString ().Split ('\n');

				sb.Length = 0;

				if (strs.Length == 0)
					return;

				for (int i = 0; i < strs.Length - 1; i++) {
					if (err_out)
						process.OnOutputDataReceived (strs [i]);
					else
						process.OnErrorDataReceived (strs [i]);
				}

				string end = strs [strs.Length - 1];
				if (last || (strs.Length == 1 && end == "")) {
					if (err_out)
						process.OnOutputDataReceived (end);
					else
						process.OnErrorDataReceived (end);
				} else {
					sb.Append (end);
				}
			}

			public void Close ()
			{
				IOSelector.Remove (handle);
			}

			internal override void CompleteDisposed ()
			{
				throw new NotSupportedException ();
			}
		}

		AsyncModes async_mode;
		bool output_canceled;
		bool error_canceled;
		ProcessAsyncReader async_output;
		ProcessAsyncReader async_error;

		[ComVisibleAttribute(false)] 
		public void BeginOutputReadLine ()
		{
			if (process_handle == IntPtr.Zero || output_stream == null || StartInfo.RedirectStandardOutput == false)
				throw new InvalidOperationException ("Standard output has not been redirected or process has not been started.");

			if ((async_mode & AsyncModes.SyncOutput) != 0)
				throw new InvalidOperationException ("Cannot mix asynchronous and synchonous reads.");

			async_mode |= AsyncModes.AsyncOutput;
			output_canceled = false;
			if (async_output == null) {
				async_output = new ProcessAsyncReader (this, (FileStream) output_stream.BaseStream, true);
				async_output.BeginReadLine ();
			}
		}

		[ComVisibleAttribute(false)] 
		public void CancelOutputRead ()
		{
			if (process_handle == IntPtr.Zero || output_stream == null || StartInfo.RedirectStandardOutput == false)
				throw new InvalidOperationException ("Standard output has not been redirected or process has not been started.");

			if ((async_mode & AsyncModes.SyncOutput) != 0)
				throw new InvalidOperationException ("OutputStream is not enabled for asynchronous read operations.");

			if (async_output == null)
				throw new InvalidOperationException ("No async operation in progress.");

			output_canceled = true;
		}

		[ComVisibleAttribute(false)] 
		public void BeginErrorReadLine ()
		{
			if (process_handle == IntPtr.Zero || error_stream == null || StartInfo.RedirectStandardError == false)
				throw new InvalidOperationException ("Standard error has not been redirected or process has not been started.");

			if ((async_mode & AsyncModes.SyncError) != 0)
				throw new InvalidOperationException ("Cannot mix asynchronous and synchonous reads.");

			async_mode |= AsyncModes.AsyncError;
			error_canceled = false;
			if (async_error == null) {
				async_error = new ProcessAsyncReader (this, (FileStream) error_stream.BaseStream, false);
				async_error.BeginReadLine ();
			}
		}

		[ComVisibleAttribute(false)] 
		public void CancelErrorRead ()
		{
			if (process_handle == IntPtr.Zero || error_stream == null || StartInfo.RedirectStandardError == false)
				throw new InvalidOperationException ("Standard error has not been redirected or process has not been started.");

			if ((async_mode & AsyncModes.SyncOutput) != 0)
				throw new InvalidOperationException ("OutputStream is not enabled for asynchronous read operations.");

			if (async_error == null)
				throw new InvalidOperationException ("No async operation in progress.");

			error_canceled = true;
		}
#else
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
#endif // MONO_FEATURE_PROCESS_START

		[Category ("Behavior")]
		[MonitoringDescription ("Raised when this process exits.")]
		public event EventHandler Exited {
			add {
				if (process_handle != IntPtr.Zero && HasExited) {
					value.BeginInvoke (null, null, null, null);
				} else {
					exited_event += value;
					if (exited_event != null)
						StartBackgroundWaitForExit ();
				}
			}
			remove {
				exited_event -= value;
			}
		}

		// Closes the system process handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void Process_free_internal(IntPtr handle);

		int disposed;

		protected override void Dispose(bool disposing) {
			// Check to see if Dispose has already been called.
			if (disposed != 0 || Interlocked.CompareExchange (ref disposed, 1, 0) != 0)
				return;

			// If this is a call to Dispose,
			// dispose all managed resources.
			if (disposing) {
#if MONO_FEATURE_PROCESS_START
				/* These have open FileStreams on the pipes we are about to close */
				if (async_output != null)
					async_output.Close ();
				if (async_error != null)
					async_error.Close ();

				if (input_stream != null) {
					if (!input_stream_exposed)
						input_stream.Close ();
					input_stream = null;
				}
				if (output_stream != null) {
					if (!output_stream_exposed)
						output_stream.Close ();
					output_stream = null;
				}
				if (error_stream != null) {
					if (!error_stream_exposed)
						error_stream.Close ();
					error_stream = null;
				}
#endif // MONO_FEATURE_PROCESS_START
			}

			// Release unmanaged resources

			if (process_handle!=IntPtr.Zero) {
				Process_free_internal (process_handle);
				process_handle = IntPtr.Zero;
			}

			base.Dispose (disposing);
		}

		~Process ()
		{
			Dispose (false);
		}

		int on_exited_called = 0;

		protected void OnExited()
		{
			if (on_exited_called != 0 || Interlocked.CompareExchange (ref on_exited_called, 1, 0) != 0)
				return;

			var cb = exited_event;
			if (cb == null)
				return;

			if (synchronizingObject != null) {
				synchronizingObject.BeginInvoke (cb, new object [] { this, EventArgs.Empty });
			} else {
				foreach (EventHandler d in cb.GetInvocationList ()) {
					try {
						d (this, EventArgs.Empty);
					} catch {
					}
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

		void StartBackgroundWaitForExit ()
		{
			if (enable_raising_events == 0)
				return;
			if (exited_event == null)
				return;
			if (process_handle == IntPtr.Zero)
				return;
			if (background_wait_for_exit_thread != null)
				return;

			Thread t = new Thread (_ => WaitForExit ()) { IsBackground = true };

			if (Interlocked.CompareExchange (ref background_wait_for_exit_thread, t, null) == null)
				t.Start ();
		}

		class ProcessWaitHandle : WaitHandle
		{
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			private extern static IntPtr ProcessHandle_duplicate (IntPtr handle);

			public ProcessWaitHandle (IntPtr handle)
			{
				// Need to keep a reference to this handle,
				// in case the Process object is collected
				Handle = ProcessHandle_duplicate (handle);

				// When the wait handle is disposed, the duplicated handle will be
				// closed, so no need to override dispose (bug #464628).
			}
		}
	}
}

