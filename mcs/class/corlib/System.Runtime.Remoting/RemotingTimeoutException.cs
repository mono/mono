//
// System.Runtime.Remoting.RemotingTimeoutException.cs
//
// AUthor: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting {

	[Serializable]
	public class RemotingTimeoutException : RemotingException
	{
		public RemotingTimeoutException ()
			: base ()
		{
		}

		public RemotingTimeoutException (string message)
			: base (message)
		{
		}

		public RemotingTimeoutException (string message, Exception ex)
			: base (message, ex)
		{
		}
	}
}
