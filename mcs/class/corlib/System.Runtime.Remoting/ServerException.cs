//
// System.Runtime.Remoting.ServerException.cs
//
// AUthor: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting {

	[Serializable]
	public class ServerException : SystemException
	{
		public ServerException ()
			: base ()
		{
		}

		public ServerException (string message)
			: base (message)
		{
		}

		public ServerException (string message, Exception ex)
			: base (message, ex)
		{
		}
	}
}
