//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//      Rafael Teixeira   (rafaelteixeirabr@hotmail.com)
//
// (C) 2003 Peter Van Isacker
//
using System;
using System.Runtime.Remoting.Messaging;

namespace System.Messaging 
{
	public class ReceiveCompletedEventArgs: EventArgs 
	{
		public IAsyncResult AsyncResult;
		public Message Message;

		internal ReceiveCompletedEventArgs(AsyncResult AsyncResult, Message Message)
		{
			this.AsyncResult = AsyncResult;
			this.Message = Message;
		}

		[MonoTODO]
		~ReceiveCompletedEventArgs()
		{
		}
	}
}
