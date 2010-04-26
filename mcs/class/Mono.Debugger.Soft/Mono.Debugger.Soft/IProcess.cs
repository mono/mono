using System;
using System.Diagnostics;
using System.IO;

namespace Mono.Debugger.Soft
{
	public interface IProcess
	{
		event System.EventHandler Exited;
		StreamReader StandardOutput { get; }
		StreamReader StandardError { get; }
		bool HasExited { get; }
		void Kill ();
		int Id { get; }
		string ProcessName { get; }
	}
	
	internal class ProcessWrapper: IProcess
	{
		Process process;

		public ProcessWrapper (Process process)
		{
			this.process = process;
		}
		
		public event System.EventHandler Exited {
			add { process.Exited += value; }
			remove { process.Exited -= value; }
		}
		
		public StreamReader StandardOutput {
			get {
				return process.StandardOutput;
			}
		}
		
		public StreamReader StandardError {
			get {
				return process.StandardError;
			}
		}
		
		public bool HasExited {
			get {
				return process.HasExited;
			}
		}
		
		public void Kill ()
		{
			process.Kill ();
		}

		public int Id {
			get {
				return process.Id;
			}
		}
		
		public string ProcessName {
			get {
				return process.ProcessName;
			}
		}
		
		
		
	}
}
