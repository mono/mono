//
// System.CodeDom.Compiler.Executor.cs
//
// Author(s):
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace System.CodeDom.Compiler
{

	public sealed class Executor
	{
	
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

		private Executor ()
		{
		}

		public static void ExecWait (string cmd, TempFileCollection tempFiles)
		{
			string outputName = null;
			string errorName = null;
			ExecWaitWithCapture (IntPtr.Zero, cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName);
		}

		[MonoTODO ("Do something with userToken")]
		public static Int32 ExecWaitWithCapture (IntPtr userToken, string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName)
		{
			if (outputName == null)
				outputName = tempFiles.AddExtension ("out");
			
			if (errorName == null)
				errorName = tempFiles.AddExtension ("err");
				
			Process proc = new Process();
			proc.StartInfo.FileName = cmd;
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.StartInfo.RedirectStandardError = true;
			proc.StartInfo.WorkingDirectory = currentDir;
			
			try 
			{
				proc.Start();
				
				ProcessResultReader outReader = new ProcessResultReader (proc.StandardOutput, outputName);
				ProcessResultReader errReader = new ProcessResultReader (proc.StandardError, errorName);
				
				Thread t = new Thread (new ThreadStart (errReader.Read));
				t.Start ();
				
				outReader.Read ();
				t.Join ();
				
				proc.WaitForExit();
			} 
			finally 
			{
				proc.Close();
			}
			return proc.ExitCode;
		}
		
		public static Int32 ExecWaitWithCapture (IntPtr userToken, string cmd, TempFileCollection tempFiles, ref string outputName, ref string errorName)
		{
			return ExecWaitWithCapture (userToken, cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName);
		}

		public static Int32 ExecWaitWithCapture (string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName )
		{
			return ExecWaitWithCapture (IntPtr.Zero, cmd, currentDir, tempFiles, ref outputName, ref errorName);
		}

		public static Int32 ExecWaitWithCapture (string cmd, TempFileCollection tempFiles, ref string outputName, ref string errorName)
		{
			return ExecWaitWithCapture (IntPtr.Zero, cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName);
		}
	}
}
