//
// ChannelManagerBase.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;

namespace System.ServiceModel.Channels
{
	public abstract class ChannelManagerBase : CommunicationObject,
		ICommunicationObject, IDefaultCommunicationTimeouts
	{
		protected ChannelManagerBase ()
		{
		}

		protected internal abstract TimeSpan DefaultReceiveTimeout { get; }

		protected internal abstract TimeSpan DefaultSendTimeout { get; }

		TimeSpan IDefaultCommunicationTimeouts.OpenTimeout {
			get { return DefaultOpenTimeout; }
		}

		TimeSpan IDefaultCommunicationTimeouts.CloseTimeout {
			get { return DefaultCloseTimeout; }
		}

		TimeSpan IDefaultCommunicationTimeouts.ReceiveTimeout {
			get { return DefaultReceiveTimeout; }
		}

		TimeSpan IDefaultCommunicationTimeouts.SendTimeout {
			get { return DefaultSendTimeout; }
		}

		internal MessageEncoder CreateEncoder<TChannel> (MessageEncodingBindingElement mbe)
		{
			var f = mbe.CreateMessageEncoderFactory ();
			var t = typeof (TChannel);
			if (t == typeof (IRequestSessionChannel) ||
#if !MOBILE
			    t == typeof (IReplySessionChannel) ||
#endif
			    t == typeof (IInputSessionChannel) ||
			    t == typeof (IOutputSessionChannel) ||
			    t == typeof (IDuplexSessionChannel))
				return f.CreateSessionEncoder ();
			else
				return f.Encoder;
		}
	}
}
