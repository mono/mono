using System;
using System.Collections.Generic;
using System.Text;

namespace TestMonkey
{
	public class FailedTest
	{
		public string Name;
		public string Message;
		public string StackTrace;
		
		public FailedTest ()
		{
		}
		
		public FailedTest (string name, string message, string stackTrace)
		{
			this.Name = name;
			this.Message = message;
			this.StackTrace = stackTrace;
		}
	}
}
