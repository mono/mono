//
// System.Runtime.Serialization.Formatters.ServerFault.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.Serialization.Formatters {

	[Serializable]
	public sealed class ServerFault
	{
		string ex_type;
		string ex_message;
		string stacktrace;
		ServerFault serverFault;

		public ServerFault (string exceptionType, string message,
				  string stackTrace)
		{
			ex_type = exceptionType;
			ex_message = message;
			stacktrace = stackTrace;
		}

		public string ExceptionType {
			get { return ex_type; }
			set { ex_type = value; }
		}

		public string ExceptionMessage {
			get { return ex_message; }
			set { ex_message = value; }
		}

		public string StackTrace {
			get { return stacktrace; }
			set { stacktrace = value; }
		}
	}
}
