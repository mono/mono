//
// System.Runtime.Remoting.RemotingException.cs
//
// AUthor: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting {

	[Serializable]
	public class RemotingException : SystemException
	{
		public RemotingException ()
			: base ()
		{
		}

		public RemotingException (string message)
			: base (message)
		{
		}

		protected RemotingException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public RemotingException (string message, Exception ex)
			: base (message, ex)
		{
		}
	}
}
