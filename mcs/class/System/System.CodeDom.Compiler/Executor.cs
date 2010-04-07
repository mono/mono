//
// System.CodeDom.Compiler.Executor.cs
//
// Authors:
//	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Andreas Nahr
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;

namespace System.CodeDom.Compiler {

	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	public static class Executor {

		class ProcessResultReader
		{
			StreamReader reader;
			string file;
			
			public ProcessResultReader (StreamReader reader, string file)
			{
				this.reader = reader;
				this.file = file;
			}
			
			public void Read ()
			{
				StreamWriter sw = new StreamWriter (file);
				
				try
				{
					string line;
					while ((line = reader.ReadLine()) != null)
						sw.WriteLine (line);
				}
				finally 
				{
					sw.Close ();
				}
			}
		}

		public static void ExecWait (string cmd, TempFileCollection tempFiles)
		{
			string outputName = null;
			string errorName = null;
			ExecWaitWithCapture (cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName);
		}

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		[SecurityPermission (SecurityAction.Assert, ControlPrincipal = true)] // UnmanagedCode "covers" more than ControlPrincipal
		public static Int32 ExecWaitWithCapture (IntPtr userToken, string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName)
		{
#if NET_2_0
			// WindowsImpersonationContext implements IDisposable only in 2.0
			using (WindowsImpersonationContext context = WindowsIdentity.Impersonate (userToken)) {
				return InternalExecWaitWithCapture (cmd, currentDir, tempFiles, ref outputName, ref errorName);
			}
#else
			int result = -1;
			WindowsImpersonationContext context = WindowsIdentity.Impersonate (userToken);
			try {
				result = InternalExecWaitWithCapture (cmd, currentDir, tempFiles, ref outputName, ref errorName);
			}
			finally {
				context.Undo ();
				context = null;
			}
			return result;
#endif
		}
		
		public static Int32 ExecWaitWithCapture (IntPtr userToken, string cmd, TempFileCollection tempFiles, ref string outputName, ref string errorName)
		{
			return ExecWaitWithCapture (userToken, cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName);
		}

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public static Int32 ExecWaitWithCapture (string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName )
		{
			return InternalExecWaitWithCapture (cmd, currentDir, tempFiles, ref outputName, ref errorName);
		}

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public static Int32 ExecWaitWithCapture (string cmd, TempFileCollection tempFiles, ref string outputName, ref string errorName)
		{
			return InternalExecWaitWithCapture (cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName);
		}

		private static int InternalExecWaitWithCapture (string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName)
		{
			if ((cmd == null) || (cmd.Length == 0))
				throw new ExternalException (Locale.GetText ("No command provided for execution."));

			if (outputName == null)
				outputName = tempFiles.AddExtension ("out");
			
			if (errorName == null)
				errorName = tempFiles.AddExtension ("err");

			int exit_code = -1;
			Process proc = new Process ();
			proc.StartInfo.FileName = cmd;
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.StartInfo.RedirectStandardError = true;
			proc.StartInfo.WorkingDirectory = currentDir;
			
			try {
				proc.Start();
			
				ProcessResultReader outReader = new ProcessResultReader (proc.StandardOutput, outputName);
				ProcessResultReader errReader = new ProcessResultReader (proc.StandardError, errorName);
				
				Thread t = new Thread (new ThreadStart (errReader.Read));
				t.Start ();
			
				outReader.Read ();
				t.Join ();
				
				proc.WaitForExit();
			} 
			finally  {
				exit_code = proc.ExitCode;
				// the handle is cleared in Close (so no ExitCode)
				proc.Close ();
			}
			return exit_code;
		}
	}
}
