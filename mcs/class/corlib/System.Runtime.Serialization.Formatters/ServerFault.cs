//
// System.Runtime.Serialization.Formatters.ServerFault.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
// 	   Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.Serialization.Formatters {

	[Serializable]
	public sealed class ServerFault
	{
		string exceptionType;
		string message;
		string stackTrace;
		Exception exception;

		public ServerFault (string exceptionType, string message,
				  string stackTrace)
		{
			this.exceptionType = exceptionType;
			this.message = message;
			this.stackTrace = stackTrace;
		}

		public string ExceptionType {
			get { return exceptionType; }
			set { exceptionType = value; }
		}

		public string ExceptionMessage {
			get { return message; }
			set { message = value; }
		}

		public string StackTrace {
			get { return stackTrace; }
			set { stackTrace = value; }
		}
	}
}
