//
// BindingElementTest.cs
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class BindingElementTest
	{
		class MyChannelFactory<TChannel>
			: ChannelFactoryBase<TChannel>
		{
			public MyChannelFactory ()
				: base ()
			{
			}

			protected override TChannel OnCreateChannel (EndpointAddress address, Uri via)
			{
				throw new NotSupportedException ();
			}

			protected override IAsyncResult OnBeginOpen (
				TimeSpan timeout, AsyncCallback callback, object state)
			{
				throw new NotImplementedException ();
			}

			protected override void OnEndOpen (IAsyncResult result)
			{
				throw new NotImplementedException ();
			}

			protected override void OnOpen (TimeSpan timeout)
			{
				throw new NotImplementedException ();
			}
		}

		class MyChannelListener<TChannel>
			: ChannelListenerBase<TChannel>
			where TChannel : class, IChannel
		{
			public MyChannelListener ()
				: base ()
			{
			}

			public override Uri Uri {
				get { throw new NotImplementedException (); }
			}

			protected override IAsyncResult OnBeginAcceptChannel (
				TimeSpan timeout, AsyncCallback callback, object state)
			{
				throw new NotImplementedException ();
			}

			protected override TChannel OnAcceptChannel (TimeSpan timeout)
			{
				throw new NotImplementedException ();
			}

			protected override TChannel OnEndAcceptChannel (IAsyncResult result)
			{
				throw new NotImplementedException ();
			}

			protected override IAsyncResult OnBeginWaitForChannel (
				TimeSpan timeout, AsyncCallback callback, object state)
			{
				throw new NotImplementedException ();
			}

			protected override bool OnEndWaitForChannel (IAsyncResult result)
			{
				throw new NotImplementedException ();
			}

			protected override bool OnWaitForChannel (TimeSpan timeout)
			{
				throw new NotImplementedException ();
			}

			protected override void OnAbort ()
			{
				throw new NotImplementedException ();
			}

			protected override IAsyncResult OnBeginClose (
				TimeSpan timeout, AsyncCallback callback, object state)
			{
				throw new NotImplementedException ();
			}

			protected override IAsyncResult OnBeginOpen (
				TimeSpan timeout, AsyncCallback callback, object state)
			{
				throw new NotImplementedException ();
			}

			protected override void OnEndClose (IAsyncResult result)
			{
				throw new NotImplementedException ();
			}

			protected override void OnEndOpen (IAsyncResult result)
			{
				throw new NotImplementedException ();
			}

			protected override void OnClose (TimeSpan timeout)
			{
				throw new NotImplementedException ();
			}

			protected override void OnOpen (TimeSpan timeout)
			{
			}
		}

		class MyBindingElement : BindingElement
		{
			public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (
				BindingContext ctx)
			{
				return new MyChannelFactory<TChannel> ();
			}

			public override IChannelListener<TChannel> BuildChannelListener<TChannel> (
				BindingContext ctx)
			{
				return new MyChannelListener<TChannel> ();
			}

			public override bool CanBuildChannelListener<TChannel> (BindingContext ctx)
			{
				return true;
			}

			public override BindingElement Clone ()
			{
				return new MyBindingElement ();
			}

			public override T GetProperty<T> (BindingContext context)
			{
				return null;
			}
		}

		[Test]
		[Ignore ("It is even not a test yet.")]
		public void BuildChannelFactory ()
		{
			ServiceHost host = new ServiceHost (typeof (Foo));
			host.AddServiceEndpoint (typeof (Foo),
				new CustomBinding (new MyBindingElement (),
					new HttpTransportBindingElement ()),
				"http://localhost:8080");
			host.Open ();
		}

		[ServiceContract]
		class Foo
		{
			[OperationContract]
			public void Whee ()
			{
			}
		}
	}
}
#endif
