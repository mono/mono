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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Messaging 
{	
	public class MessageQueueException: ExternalException 
	{
		private MessageQueueErrorCode messageQueueErrorCode;

		internal MessageQueueException(MessageQueueErrorCode messageQueueErrorCode)
		{
			this.messageQueueErrorCode = messageQueueErrorCode;
		}
		
		[MonoTODO]
		private string TranslateCodeToDescription()
		{
			return "UnknownError";
		}

		public override string Message 
		{
			get { return TranslateCodeToDescription(); }
		}
		
		public MessageQueueErrorCode MessageQueueErrorCode 
		{
			get { return messageQueueErrorCode; }
		}
		
		[MonoTODO]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException();
			base.GetObjectData(info, context);
			// serialize just the messageQueueErrorCode private field;
		}
		
		[MonoTODO]
		public override string ToString()
		{
			return base.ToString();
		}
		
		[MonoTODO]
		~MessageQueueException()
		{
		}
	}
}
