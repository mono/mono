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
using System.Collections;

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

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool GetWorkingSet_internal(IntPtr handle, out int min, out int max);
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool SetWorkingSet_internal(IntPtr handle, int min, int max, bool use_min);

		/* LAMESPEC: why is this an IntPtr not a plain int? */
		public IntPtr MaxWorkingSet {
			get {
				if(HasExited) {
					throw new InvalidOperationException("The process " + ProcessName + " (ID " + Id + ") has exited");
				}
				
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

		public IntPtr MinWorkingSet {
			get {
				if(HasExited) {
					throw new InvalidOperationException("The process " + ProcessName + " (ID " + Id + ") has exited");
				}
				
				int min;
				int max;
				bool ok=GetWorkingSet_internal(process_handle, out min, out max);
				if(ok==false) {
					throw new Win32Exception();
				}
				
				return((IntPtr)min);
			}
			set {
				if(HasExited) {
					throw new InvalidOperationException("The process " + ProcessName + " (ID " + Id + ") has exited");
				}
				
				bool ok=SetWorkingSet_internal(process_handle, value.ToInt32(), 0, true);
				if(ok==false) {
					throw new Win32Exception();
				}
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

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static string ProcessName_internal(IntPtr handle);
		
		private string process_name=null;
		
		public string ProcessName {
			get {
				if(process_name==null) {
					process_name=ProcessName_internal(process_handle);
					/* If process_name is _still_
					 * null, assume the process
					 * has exited
					 */
					if(process_name==null) {
						throw new SystemException("The process has exited");
					}
					
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

		private StreamReader error_stream=null;
		
		public StreamReader StandardError {
			get {
				return(error_stream);
			}
		}

		private StreamWriter input_stream=null;
		
		public StreamWriter StandardInput {
			get {
				return(input_stream);
			}
		}

		private StreamReader output_stream=null;
		
		public StreamReader StandardOutput {
			get {
				return(output_stream);
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
		private extern static IntPtr GetProcess_internal(int pid);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int GetPid_internal();

		public static Process GetCurrentProcess() {
			int pid=GetPid_internal();
			IntPtr proc=GetProcess_internal(pid);
			
			if(proc==IntPtr.Zero) {
				throw new SystemException("Can't find current process");
			}

			return(new Process(proc, pid));
		}

		public static Process GetProcessById(int processId) {
			IntPtr proc=GetProcess_internal(processId);
			
			if(proc==IntPtr.Zero) {
				throw new ArgumentException("Can't find process with ID " + processId.ToString());
			}

			return(new Process(proc, processId));
		}

		[MonoTODO]
		public static Process GetProcessById(int processId, string machineName) {
			throw new NotImplementedException();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int[] GetProcesses_internal();

		public static Process[] GetProcesses() {
			int[] pids=GetProcesses_internal();
			ArrayList proclist=new ArrayList();
			
			for(int i=0; i<pids.Length; i++) {
				try {
					proclist.Add(GetProcessById(pids[i]));
				} catch (SystemException) {
					/* The process might exit
					 * between
					 * GetProcesses_internal and
					 * GetProcessById
					 */
				}
			}

			return((Process[])proclist.ToArray(typeof(Process)));
		}

		[MonoTODO]
		public static Process[] GetProcesses(string machineName) {
			throw new NotImplementedException();
		}

		public static Process[] GetProcessesByName(string processName) {
			Process[] procs=GetProcesses();
			ArrayList proclist=new ArrayList();
			
			for(int i=0; i<procs.Length; i++) {
				/* Ignore case */
				if(String.Compare(processName,
						  procs[i].ProcessName,
						  true)==0) {
					proclist.Add(procs[i]);
				}
			}

			return((Process[])proclist.ToArray(typeof(Process)));
		}

		[MonoTODO]
		public static Process[] GetProcessesByName(string processName, string machineName) {
			throw new NotImplementedException();
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
		private extern static bool Start_internal(string cmd,
							  string dir,
							  IntPtr stdin,
							  IntPtr stdout,
							  IntPtr stderr,
							  ref ProcInfo proc_info);

		private static bool Start_common(ProcessStartInfo startInfo,
						 Process process) {
			ProcInfo proc_info=new ProcInfo();
			IntPtr stdin_rd, stdin_wr;
			IntPtr stdout_rd, stdout_wr;
			IntPtr stderr_rd, stderr_wr;
			bool ret;
			
			if(startInfo.FileName == "") {
				throw new InvalidOperationException("File name has not been set");
			}
			
			if(startInfo.RedirectStandardInput==true) {
				ret=MonoIO.CreatePipe(out stdin_rd,
						      out stdin_wr);
			} else {
				stdin_rd=MonoIO.ConsoleInput;
				/* This is required to stop the
				 * &$*£ing stupid compiler moaning
				 * that stdin_wr is unassigned, below.
				 */
				stdin_wr=(IntPtr)0;
			}

			if(startInfo.RedirectStandardOutput==true) {
				ret=MonoIO.CreatePipe(out stdout_rd,
						      out stdout_wr);
			} else {
				stdout_rd=(IntPtr)0;
				stdout_wr=MonoIO.ConsoleOutput;
			}

			if(startInfo.RedirectStandardError==true) {
				ret=MonoIO.CreatePipe(out stderr_rd,
						      out stderr_wr);
			} else {
				stderr_rd=(IntPtr)0;
				stderr_wr=MonoIO.ConsoleError;
			}
			
			ret=Start_internal(startInfo.FileName + " " +
					   startInfo.Arguments,
					   startInfo.WorkingDirectory,
					   stdin_rd, stdout_wr, stderr_wr,
					   ref proc_info);

			process.process_handle=proc_info.process_handle;
			process.pid=proc_info.pid;
			
			MonoIOError error;
			
			if(startInfo.RedirectStandardInput==true) {
				MonoIO.Close(stdin_rd, out error);
				process.input_stream=new StreamWriter(new FileStream(stdin_wr, FileAccess.Write, true));
				process.input_stream.AutoFlush=true;
			}

			if(startInfo.RedirectStandardOutput==true) {
				MonoIO.Close(stdout_wr, out error);
				process.output_stream=new StreamReader(new FileStream(stdout_rd, FileAccess.Read, true));
			}

			if(startInfo.RedirectStandardError==true) {
				MonoIO.Close(stderr_wr, out error);
				process.error_stream=new StreamReader(new FileStream(stderr_rd, FileAccess.Read, true));
			}

			return(ret);
		}
		
		public bool Start() {
			bool ret;
			
			ret=Start_common(start_info, this);
			
			return(ret);
		}

		public static Process Start(ProcessStartInfo startInfo) {
			Process process=new Process();
			bool ret;

			ret=Start_common(startInfo, process);
			
			if(ret==true) {
				return(process);
			} else {
				return(null);
			}
		}

		public static Process Start(string fileName) {
                       return Start(new ProcessStartInfo(fileName));
		}

		public static Process Start(string fileName,
					    string arguments) {
                       return Start(new ProcessStartInfo(fileName, arguments));
		}

		public override string ToString() {
			return(base.ToString() +
			       " (" + this.ProcessName + ")");
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
				this.disposed=true;
				// If this is a call to Dispose,
				// dispose all managed resources.
				if(disposing) {
					// Do stuff here
				}
				
				// Release unmanaged resources

				lock(this) {
					if(process_handle!=IntPtr.Zero) {
						
						Process_free_internal(process_handle);
						process_handle=IntPtr.Zero;
					}
				}
			}
			base.Dispose (disposing);
		}

		[MonoTODO]
		protected void OnExited() {
		}
	}
}

