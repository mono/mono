//
// OneWayBindingElementTest.cs
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class OneWayBindingElementTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BuildRequestChannelFactory ()
		{
			OneWayBindingElement be =
				new OneWayBindingElement ();
			CustomBinding binding = new CustomBinding (
				new HandlerTransportBindingElement (null));
			BindingContext ctx = new BindingContext (
				binding, new BindingParameterCollection ());
			be.BuildChannelFactory<IRequestChannel> (ctx);
		}

		[Test]
		public void BuildOutputChannelFactory ()
		{
			OneWayBindingElement be =
				new OneWayBindingElement ();
			CustomBinding binding = new CustomBinding (
				new HandlerTransportBindingElement (null));
			BindingContext ctx = new BindingContext (
				binding, new BindingParameterCollection ());
			be.BuildChannelFactory<IOutputChannel> (ctx);
		}
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BuildReplyChannelListener ()
		{
			OneWayBindingElement be =
				new OneWayBindingElement ();
			CustomBinding binding = new CustomBinding (
				new HandlerTransportBindingElement (null));
			BindingContext ctx = new BindingContext (
				binding, new BindingParameterCollection ());
			be.BuildChannelListener<IReplyChannel> (ctx);
		}

		[Test]
		public void BuildInputChannelListener ()
		{
			OneWayBindingElement be =
				new OneWayBindingElement ();
			CustomBinding binding = new CustomBinding (
				new HandlerTransportBindingElement (null));
			BindingContext ctx = new BindingContext (
				binding, new BindingParameterCollection ());
			be.BuildChannelListener<IInputChannel> (ctx);
		}
	}
}
#endif
