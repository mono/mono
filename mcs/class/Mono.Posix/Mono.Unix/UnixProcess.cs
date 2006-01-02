//
// Mono.Unix/UnixProcess.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004-2005 Jonathan Pryor
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
using Mono.Unix;

namespace Mono.Unix {

	public sealed class UnixProcess
	{
		private int pid;

		internal UnixProcess (int pid)
		{
			this.pid = pid;
		}

		public int Id {
			get {return pid;}
		}

		public bool HasExited {
			get {
				int status = GetProcessStatus ();
				return Native.Syscall.WIFEXITED (status);
			}
		}

		private int GetProcessStatus ()
		{
			int status;
			int r = Native.Syscall.waitpid (pid, out status, 
					Native.WaitOptions.WNOHANG | Native.WaitOptions.WUNTRACED);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			return r;
		}

		public int ExitCode {
			get {
				if (!HasExited)
					throw new InvalidOperationException (
							Locale.GetText ("Process hasn't exited"));
				int status = GetProcessStatus ();
				return Native.Syscall.WEXITSTATUS (status);
			}
		}

		public bool HasSignaled {
			get {
				int status = GetProcessStatus ();
				return Native.Syscall.WIFSIGNALED (status);
			}
		}

		public Native.Signum TerminationSignal {
			get {
				if (!HasSignaled)
					throw new InvalidOperationException (
							Locale.GetText ("Process wasn't terminated by a signal"));
				int status = GetProcessStatus ();
				return Native.Syscall.WTERMSIG (status);
			}
		}

		public bool HasStopped {
			get {
				int status = GetProcessStatus ();
				return Native.Syscall.WIFSTOPPED (status);
			}
		}

		public Native.Signum StopSignal {
			get {
				if (!HasStopped)
					throw new InvalidOperationException (
							Locale.GetText ("Process isn't stopped"));
				int status = GetProcessStatus ();
				return Native.Syscall.WSTOPSIG (status);
			}
		}

		public int ProcessGroupId {
			get {return Native.Syscall.getpgid (pid);}
			set {
				int r = Native.Syscall.setpgid (pid, value);
				UnixMarshal.ThrowExceptionForLastErrorIf (r);
			}
		}

		public int SessionId {
			get {
				int r = Native.Syscall.getsid (pid);
				UnixMarshal.ThrowExceptionForLastErrorIf (r);
				return r;
			}
		}

		public static UnixProcess GetCurrentProcess ()
		{
			return new UnixProcess (GetCurrentProcessId ());
		}

		public static int GetCurrentProcessId ()
		{
			return Native.Syscall.getpid ();
		}

		public void Kill ()
		{
			Signal (Native.Signum.SIGKILL);
		}

		[CLSCompliant (false)]
		public void Signal (Native.Signum signal)
		{
			int r = Native.Syscall.kill (pid, signal);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public void WaitForExit ()
		{
			int status;
			int r;
			do {
				r = Native.Syscall.waitpid (pid, out status, (Native.WaitOptions) 0);
			} while (UnixMarshal.ShouldRetrySyscall (r));
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}
	}
}

// vim: noexpandtab
