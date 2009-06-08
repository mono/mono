//
// ChannelBase.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Channels
{
	[MonoTODO]
	public abstract class ChannelBase : CommunicationObject,
		ICommunicationObject, IChannel,
		IDefaultCommunicationTimeouts
	{
		ChannelManagerBase manager;

		protected ChannelBase (ChannelManagerBase manager)
		{
			this.manager = manager;
		}

		protected internal override TimeSpan DefaultCloseTimeout {
			get { throw new NotImplementedException ("Actually it should be overriden before being used."); }
		}

		protected internal override TimeSpan DefaultOpenTimeout {
			get { throw new NotImplementedException ("Actually it should be overriden before being used."); }
		}

		protected internal TimeSpan DefaultReceiveTimeout {
			get { return manager.DefaultReceiveTimeout; }
		}

		protected internal TimeSpan DefaultSendTimeout {
			get { return manager.DefaultSendTimeout; }
		}

		protected ChannelManagerBase Manager {
			get { return manager; }
		}

		public virtual T GetProperty<T> () where T : class
		{
			return null;
		}

		protected override void OnClosed ()
		{
			base.OnClosed ();
		}

		TimeSpan IDefaultCommunicationTimeouts.CloseTimeout {
			get { return DefaultCloseTimeout; }
		}

		TimeSpan IDefaultCommunicationTimeouts.OpenTimeout {
			get { return DefaultOpenTimeout; }
		}

		TimeSpan IDefaultCommunicationTimeouts.ReceiveTimeout {
			get { return DefaultReceiveTimeout; }
		}

		TimeSpan IDefaultCommunicationTimeouts.SendTimeout {
			get { return DefaultSendTimeout; }
		}
	}
}
