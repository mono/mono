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
		private MessageQueueErrorCode _messageQueueErrorCode;

		internal MessageQueueException(MessageQueueErrorCode messageQueueErrorCode)
		{
			_messageQueueErrorCode = messageQueueErrorCode;
		}

		protected MessageQueueException (SerializationInfo info, StreamingContext context) : base(info, context)
		{
			_messageQueueErrorCode = (MessageQueueErrorCode) info.GetInt32 ("NativeErrorCode");
		}

		[MonoTODO]
		private string TranslateCodeToDescription()
		{
			return "UnknownError";
		}

		[MonoTODO]
		public override string Message 
		{
			get { return TranslateCodeToDescription(); }
		}
		
		public MessageQueueErrorCode MessageQueueErrorCode 
		{
			get { return _messageQueueErrorCode; }
		}
		
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException();

			info.AddValue ("NativeErrorCode", (int) _messageQueueErrorCode);
			base.GetObjectData(info, context);
		}
	}
}
