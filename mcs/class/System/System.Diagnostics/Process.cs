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

namespace System.Diagnostics {
	public class Process : Component {
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

		[MonoTODO]
		public int ExitCode {
			get {
				return(0);
			}
		}

		[MonoTODO]
		public DateTime ExitTime {
			get {
				return(new DateTime(0));
			}
		}

		[MonoTODO]
		public IntPtr Handle {
			get {
				return((IntPtr)0);
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

		[MonoTODO]
		public int Id {
			get {
				return(0);
			}
		}

		[MonoTODO]
		public string MachineName {
			get {
				return("localhost");
			}
		}

		[MonoTODO]
		public ProcessModule MainModule {
			get {
				return(null);
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

		[MonoTODO]
		public ProcessModuleCollection Modules {
			get {
				return(null);
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

		[MonoTODO]
		public ProcessStartInfo StartInfo {
			get {
				return(null);
			}
			set {
			}
		}

		[MonoTODO]
		public DateTime StartTime {
			get {
				return(new DateTime(0));
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

		[MonoTODO]
		public static Process GetCurrentProcess() {
			return(null);
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

		[MonoTODO]
		public bool Start() {
			return(false);
		}

		[MonoTODO]
		public static Process Start(ProcessStartInfo startInfo) {
			return(null);
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

		[MonoTODO]
		public void WaitForExit() {
		}

		[MonoTODO]
		public bool WaitForExit(int milliseconds) {
			return(false);
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

		[MonoTODO]
		protected override void Dispose(bool disposing) {
		}

		[MonoTODO]
		public override void Dispose() {
		}

		[MonoTODO]
		protected void OnExited() {
		}
	}
}

