//
// System.Runtime.Remoting.Messaging.InternalMessageWrapper.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;

namespace System.Runtime.Remoting.Messaging {

	public class InternalMessageWrapper
	{
		public InternalMessageWrapper (IMessage msg)
		{
			WrappedMessage = msg;
		}

		protected IMessage WrappedMessage;
	}
}
