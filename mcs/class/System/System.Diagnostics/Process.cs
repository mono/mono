//
// System.Diagnostics.Process.cs
//
// Authors:
//   Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.
//

using System;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Diagnostics {
	public class Process : Component {
		[StructLayout(LayoutKind.Sequential)]
		private struct ProcInfo {
			public IntPtr process_handle;
			public IntPtr thread_handle;
			public int pid;
			public int tid;
		};
		
		IntPtr process_handle;
		int pid;
		
		/* Private constructor called from other methods */
		private Process(IntPtr handle, int id) {
			process_handle=handle;
			pid=id;
		}
		
		[MonoTODO]
		public Process() {
		}

		[MonoTODO]
		public int BasePriority {
			get {
				return(0);
			}
		}

		[MonoTODO]
		public bool EnableRaisingEvents {
			get {
				return(false);
			}
			set {
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int ExitCode_internal(IntPtr handle);

		public int ExitCode {
			get {
				return(ExitCode_internal(process_handle));
			}
		}

		/* Returns the process start time in Windows file
		 * times (ticks from DateTime(1/1/1601 00:00 GMT))
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static long ExitTime_internal(IntPtr handle);
		
		public DateTime ExitTime {
			get {
				return(DateTime.FromFileTime(ExitTime_internal(process_handle)));
			}
		}

		public IntPtr Handle {
			get {
				return(process_handle);
			}
		}

		[MonoTODO]
		public int HandleCount {
			get {
				return(0);
			}
		}

		[MonoTODO]
		public bool HasExited {
			get {
				return(false);
			}
		}

		public int Id {
			get {
				return(pid);
			}
		}

		[MonoTODO]
		public string MachineName {
			get {
				return("localhost");
			}
		}

		public ProcessModule MainModule {
			get {
				return(this.Modules[0]);
			}
		}

		[MonoTODO]
		public IntPtr MainWindowHandle {
			get {
				return((IntPtr)0);
			}
		}

		[MonoTODO]
		public string MainWindowTitle {
			get {
				return("null");
			}
		}

		[MonoTODO]
		public IntPtr MaxWorkingSet {
			get {
				return((IntPtr)0);
			}
			set {
			}
		}

		[MonoTODO]
		public IntPtr MinWorkingSet {
			get {
				return((IntPtr)0);
			}
			set {
			}
		}

		/* Returns the list of process modules.  The main module is
		 * element 0.
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern ProcessModule[] GetModules_internal();

		private ProcessModuleCollection module_collection;
		
		public ProcessModuleCollection Modules {
			get {
				if(module_collection==null) {
					module_collection=new ProcessModuleCollection(GetModules_internal());
				}

				return(module_collection);
			}
		}

		[MonoTODO]
		public int NonpagedSystemMemorySize {
			get {
				return(0);
			}
		}

		[MonoTODO]
		public int PagedMemorySize {
			get {
				return(0);
			}
		}

		[MonoTODO]
		public int PagedSystemMemorySize {
			get {
				return(0);
			}
		}

		[MonoTODO]
		public int PeakPagedMemorySize {
			get {
				return(0);
			}
		}

		[MonoTODO]
		public int PeakVirtualMemorySize {
			get {
				return(0);
			}
		}

		[MonoTODO]
		public int PeakWorkingSet {
			get {
				return(0);
			}
		}

		[MonoTODO]
		public bool PriorityBoostEnabled {
			get {
				return(false);
			}
			set {
			}
		}

		[MonoTODO]
		public ProcessPriorityClass PriorityClass {
			get {
				return(ProcessPriorityClass.Normal);
			}
			set {
			}
		}

		[MonoTODO]
		public int PrivateMemorySize {
			get {
				return(0);
			}
		}

		[MonoTODO]
		public TimeSpan PrivilegedProcessorTime {
			get {
				return(new TimeSpan(0));
			}
		}

		[MonoTODO]
		public string ProcessName {
			get {
				return("this-process");
			}
		}

		[MonoTODO]
		public IntPtr ProcessorAffinity {
			get {
				return((IntPtr)0);
			}
			set {
			}
		}

		[MonoTODO]
		public bool Responding {
			get {
				return(false);
			}
		}

		[MonoTODO]
		public StreamReader StandardError {
			get {
				return(null);
			}
		}

		[MonoTODO]
		public StreamWriter StandardInput {
			get {
				return(null);
			}
		}

		[MonoTODO]
		public StreamReader StandardOutput {
			get {
				return(null);
			}
		}

		private ProcessStartInfo start_info=null;
		
		public ProcessStartInfo StartInfo {
			get {
				if(start_info==null) {
					start_info=new ProcessStartInfo();
				}
				
				return(start_info);
			}
			set {
				if(value==null) {
					throw new ArgumentException("value is null");
				}
				
				start_info=value;
			}
		}

		/* Returns the process start time in Windows file
		 * times (ticks from DateTime(1/1/1601 00:00 GMT))
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static long StartTime_internal(IntPtr handle);
		
		public DateTime StartTime {
			get {
				return(DateTime.FromFileTime(StartTime_internal(process_handle)));
			}
		}

		[MonoTODO]
		public ISynchronizeInvoke SynchronizingObject {
			get {
				return(null);
			}
			set {
			}
		}

		[MonoTODO]
		public ProcessThreadCollection Threads {
			get {
				return(null);
			}
		}

		[MonoTODO]
		public TimeSpan TotalProcessorTime {
			get {
				return(new TimeSpan(0));
			}
		}

		[MonoTODO]
		public TimeSpan UserProcessorTime {
			get {
				return(new TimeSpan(0));
			}
		}

		[MonoTODO]
		public int VirtualMemorySize {
			get {
				return(0);
			}
		}

		[MonoTODO]
		public int WorkingSet {
			get {
				return(0);
			}
		}

		[MonoTODO]
		public void Close() {
		}

		[MonoTODO]
		public bool CloseMainWindow() {
			return(false);
		}

		[MonoTODO]
		public static void EnterDebugMode() {
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static IntPtr GetCurrentProcess_internal();
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int GetPid_internal();

		public static Process GetCurrentProcess() {
			return(new Process(GetCurrentProcess_internal(),
					   GetPid_internal()));
		}

		[MonoTODO]
		public static Process GetProcessById(int processId) {
			return(null);
		}

		[MonoTODO]
		public static Process GetProcessById(int processId, string machineName) {
			return(null);
		}

		[MonoTODO]
		public static Process[] GetProcesses() {
			return(null);
		}

		[MonoTODO]
		public static Process[] GetProcesses(string machineName) {
			return(null);
		}

		[MonoTODO]
		public static Process[] GetProcessesByName(string processName) {
			return(null);
		}

		[MonoTODO]
		public static Process[] GetProcessesByName(string processName, string machineName) {
			return(null);
		}

		[MonoTODO]
		public void Kill() {
		}

		[MonoTODO]
		public static void LeaveDebugMode() {
		}

		[MonoTODO]
		public void Refresh() {
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool Start_internal(string file,
							  string args,
							  ref ProcInfo proc_info);

		[MonoTODO("file descriptors and the rest of ProcessStartInfo")]
		public bool Start() {
			bool ret;
			ProcInfo proc_info=new ProcInfo();
			
			ret=Start_internal(start_info.FileName,
					   start_info.Arguments,
					   ref proc_info);

			if(ret==true) {
				process_handle=proc_info.process_handle;
				pid=proc_info.pid;
			}
			
			return(ret);
		}

		[MonoTODO("file descriptors and the rest of ProcessStartInfo")]
		public static Process Start(ProcessStartInfo startInfo) {
			bool ret;
			ProcInfo proc_info=new ProcInfo();
			
			ret=Start_internal(startInfo.FileName,
					   startInfo.Arguments, ref proc_info);
			
			if(ret==true) {
				return(new Process(proc_info.process_handle,
						   proc_info.pid));
			} else {
				return(null);
			}
		}

		[MonoTODO]
		public static Process Start(string fileName) {
			return(null);
		}

		[MonoTODO]
		public static Process Start(string fileName, string arguments) {
			return(null);
		}

		[MonoTODO]
		public override string ToString() {
			return("process name");
		}

		/* Waits up to ms milliseconds for process 'handle' to
		 * exit.  ms can be <0 to mean wait forever.
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern bool WaitForExit_internal(IntPtr handle,
							 int ms);

		public void WaitForExit() {
			WaitForExit_internal(process_handle, -1);
		}

		public bool WaitForExit(int milliseconds) {
			return(WaitForExit_internal(process_handle,
						    milliseconds));
		}

		[MonoTODO]
		public bool WaitForInputIdle() {
			return(false);
		}

		[MonoTODO]
		public bool WaitForInputIdle(int milliseconds) {
			return(false);
		}

		[MonoTODO]
		public event EventHandler Exited;

		// Closes the system process handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void Process_free_internal(IntPtr handle);
		
		private bool disposed = false;
		
		protected override void Dispose(bool disposing) {
			// Check to see if Dispose has already been called.
			if(this.disposed) {
				// If this is a call to Dispose,
				// dispose all managed resources.
				if(disposing) {
					// Do stuff here
				}
				
				// Release unmanaged resources
				this.disposed=true;

				lock(this) {
					if(process_handle!=IntPtr.Zero) {
						
						Process_free_internal(process_handle);
						process_handle=IntPtr.Zero;
					}
				}
			}
		}

		public override void Dispose() {
			Dispose(true);
			// Take yourself off the Finalization queue
			GC.SuppressFinalize(this);
		}

		[MonoTODO]
		protected void OnExited() {
		}
	}
}

