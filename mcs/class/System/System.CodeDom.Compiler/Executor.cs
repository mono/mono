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
			ExecWaitWithCapture (IntPtr.Zero, cmd, Environment.CurrentDirectory, tempFiles, string.Empty, string.Empty);
		}

		[MonoTODO]
		public static Int32 ExecWaitWithCapture (IntPtr userToken, string cmd, string currentDir, TempFileCollection tempFiles, string outputName, string errorName)
		{
			throw new NotImplementedException();
		}

		public static Int32 ExecWaitWithCapture (IntPtr userToken, string cmd, TempFileCollection tempFiles, string outputName, string errorName)
		{
			return ExecWaitWithCapture (userToken, cmd, Environment.CurrentDirectory, tempFiles, outputName, errorName);
		}

		public static Int32 ExecWaitWithCapture (string cmd, string currentDir, TempFileCollection tempFiles, string outputName, string errorName )
		{
			return ExecWaitWithCapture (IntPtr.Zero, cmd, currentDir, tempFiles, outputName, errorName);
		}

		public static Int32 ExecWaitWithCapture (string cmd, TempFileCollection tempFiles, string outputName, string errorName)
		{
			return ExecWaitWithCapture (IntPtr.Zero, cmd, Environment.CurrentDirectory, tempFiles, outputName, errorName);
		}
	}
}
