//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//      Rafael Teixeira   (rafaelteixeirabr@hotmail.com)
//
// (C) 2003 Peter Van Isacker
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Messaging 
{	
	[Serializable]
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
