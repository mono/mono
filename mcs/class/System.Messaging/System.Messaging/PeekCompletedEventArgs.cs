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
	public class PeekCompletedEventArgs : EventArgs 
	{
		private MessageQueue _sender;
		private IAsyncResult _result;
		private Message _message;

		internal PeekCompletedEventArgs(MessageQueue sender, IAsyncResult result)
		{
			_sender = sender;
			_result = result;
		}

		public IAsyncResult AsyncResult
		{
			get { return _result; }
			set { _result = value; }
		}

		public Message Message
		{
			get
			{
				if (_message == null)
				{
					_message = _sender.EndPeek (_result);
				}
				return _message;
			}
		}
	}
}
