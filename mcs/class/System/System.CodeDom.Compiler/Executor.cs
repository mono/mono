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

namespace System.CodeDom.Compiler
{

	public abstract class Executor
	{

		private Executor ()
		{
		}

		public static void ExecWait (string cmd, TempFileCollection tempFiles)
		{
			string outputName = null;
			string errorName = null;
			ExecWaitWithCapture (IntPtr.Zero, cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName);
		}

		[MonoTODO]
		public static Int32 ExecWaitWithCapture (IntPtr userToken, string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName)
		{
			throw new NotImplementedException();
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
