//
// System.UnhandledExceptionEventArgs.cs 
//
// Author:
//   Chris Hynes (chrish@assistedsolutions.com)
//
// (C) 2001 Chris Hynes
//

using System;
using System.Reflection;

namespace System 
{
	public class UnhandledExceptionEventArgs: EventArgs
	{
		protected object exception;
		protected bool isTerminating;

		public UnhandledExceptionEventArgs(object exception, bool isTerminating)
		{
			this.exception = exception;
			this.isTerminating = isTerminating;
		}

		public object ExceptionObject
		{
			get 
			{
				return exception;
			}
		}

		public bool IsTerminating
		{
			get
			{
				return isTerminating;
			}
		}
	}
}
