//
// Mono.Unix/UnixProcess.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004 Jonathan Pryor
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
				return Syscall.WIFEXITED (status);
			}
		}

		private int GetProcessStatus ()
		{
			int status;
			int r = Syscall.waitpid (pid, out status, 
					WaitOptions.WNOHANG | WaitOptions.WUNTRACED);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			return r;
		}

		public int ExitCode {
			get {
				if (!HasExited)
					throw new InvalidOperationException (
							Locale.GetText ("Process hasn't exited"));
				int status = GetProcessStatus ();
				return Syscall.WEXITSTATUS (status);
			}
		}

		public bool HasSignaled {
			get {
				int status = GetProcessStatus ();
				return Syscall.WIFSIGNALED (status);
			}
		}

		public Signum TerminationSignal {
			get {
				if (!HasSignaled)
					throw new InvalidOperationException (
							Locale.GetText ("Process wasn't terminated by a signal"));
				int status = GetProcessStatus ();
				return Syscall.WTERMSIG (status);
			}
		}

		public bool HasStopped {
			get {
				int status = GetProcessStatus ();
				return Syscall.WIFSTOPPED (status);
			}
		}

		public Signum StopSignal {
			get {
				if (!HasStopped)
					throw new InvalidOperationException (
							Locale.GetText ("Process isn't stopped"));
				int status = GetProcessStatus ();
				return Syscall.WSTOPSIG (status);
			}
		}

		public int ProcessGroupId {
			get {return Syscall.getpgid (pid);}
			set {
				int r = Syscall.setpgid (pid, value);
				UnixMarshal.ThrowExceptionForLastErrorIf (r);
			}
		}

		public int SessionId {
			get {
				int r = Syscall.getsid (pid);
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
			return Syscall.getpid ();
		}

		public void Kill ()
		{
			Signal (Signum.SIGKILL);
		}

		public void Signal (Signum signal)
		{
			int r = Syscall.kill (pid, signal);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public void WaitForExit ()
		{
			int status;
			int r;
			Error e;
			do {
				r = Syscall.waitpid (pid, out status, (WaitOptions) 0);
			} while (r == -1 && (e = Syscall.GetLastError()) == Error.EINTR);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}
	}
}

// vim: noexpandtab
