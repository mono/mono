//
// DebugBindingElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;
using NUnit.Framework;


// This binding element class is for testing some bindings to intercept
// messages.

namespace MonoTests.System.ServiceModel.Channels
{
	class DebugBindingElement : BindingElement
	{
		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (BindingContext context)
		{
			return new DebugChannelFactory<TChannel> (context.BuildInnerChannelFactory<TChannel> ());
		}

		public override BindingElement Clone ()
		{
			return new DebugBindingElement ();
		}

		public override T GetProperty<T> (BindingContext context)
		{
			return context.GetInnerProperty<T> ();
		}
	}

	class DebugChannelFactory<TChannel> : ChannelFactoryBase<TChannel>
	{
		IChannelFactory<TChannel> inner;

		public DebugChannelFactory (IChannelFactory<TChannel> inner)
		{
			this.inner = inner;
		}

		public override T GetProperty<T> ()
		{
			return inner.GetProperty<T> ();
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			inner.Open (timeout);
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotSupportedException ();
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotSupportedException ();
		}

		protected override TChannel OnCreateChannel (EndpointAddress ep, Uri via)
		{
			if (typeof (TChannel) == typeof (IRequestChannel))
				return (TChannel) (object) new DebugRequestChannel (this, (IRequestChannel) (object) inner.CreateChannel (ep, via));
			throw new Exception ("huh?");
		}
	}

	class DebugRequestChannel : RequestChannelBase
	{
		ChannelFactoryBase source;
		IRequestChannel inner;

		public DebugRequestChannel (ChannelFactoryBase source, IRequestChannel inner)
			 : base (source)
		{
			this.source = source;
			this.inner = inner;
		}

		public override EndpointAddress RemoteAddress {
			get { return inner.RemoteAddress; }
		}

		public override Uri Via {
			get { return inner.Via; }
		}

		protected override void OnAbort ()
		{
			inner.Abort ();
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			inner.Open (timeout);
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotSupportedException ();
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotSupportedException ();
		}

		protected override void OnClose (TimeSpan timeout)
		{
			inner.Close (timeout);
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotSupportedException ();
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			throw new NotSupportedException ();
		}

		public override Message Request (Message req, TimeSpan timeout)
		{
XmlWriterSettings settings = new XmlWriterSettings ();
settings.Indent = true;
MessageBuffer buf = req.CreateBufferedCopy (0x10000);
using (XmlWriter w = XmlWriter.Create (Console.Error, settings)) {
buf.CreateMessage ().WriteMessage (w);
}
Console.Error.WriteLine ("******************** Debug Request() ********************");
Console.Error.Flush ();
			return inner.Request (buf.CreateMessage (), timeout);
		}

		public override IAsyncResult BeginRequest (Message req, TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotSupportedException ();
		}

		public override Message EndRequest (IAsyncResult result)
		{
			throw new NotSupportedException ();
		}
	}
}
