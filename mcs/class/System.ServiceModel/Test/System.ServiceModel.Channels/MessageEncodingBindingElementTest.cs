//
// TextMessageEncodingBindingElementTest.cs
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
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using NUnit.Framework;

using Element = System.ServiceModel.Channels.TextMessageEncodingBindingElement;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class MessageEncodingBindingElementTest
	{
		class MyEncodingBindingElement : MessageEncodingBindingElement
		{
			public override BindingElement Clone ()
			{
				throw new Exception ();
			}

			public override MessageEncoderFactory CreateMessageEncoderFactory ()
			{
				throw new Exception ();
			}

			public override MessageVersion MessageVersion {
				get { return MessageVersion.None; }
				set { throw new Exception (); }
			}
		}

		[Test]
		public void BuildChannelFactory1 ()
		{
			MessageEncodingBindingElement be =
				new TextMessageEncodingBindingElement ();
			BindingContext ctx = new BindingContext (
				new CustomBinding (
					new HttpTransportBindingElement ()),
				new BindingParameterCollection ());
			// hmm, it returns HttpChannelFactory, not sure
			// if TextMessageEncodingBindingElement is considered.
			be.BuildChannelFactory<IRequestChannel> (ctx);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void BuildChannelFactoryFail1 ()
		{
			MessageEncodingBindingElement be =
				new TextMessageEncodingBindingElement ();
			BindingContext ctx = new BindingContext (
				// no transport -> fail
				new CustomBinding (),
				new BindingParameterCollection ());
			be.BuildChannelFactory<IRequestChannel> (ctx);
		}

		[Test]
		public void BuildChannelFactory2 ()
		{
			MessageEncodingBindingElement be =
				new MyEncodingBindingElement ();
			BindingContext ctx = new BindingContext (
				new CustomBinding (
					new HttpTransportBindingElement ()),
				new BindingParameterCollection ());
			// hmm, it returns HttpChannelFactory, not sure
			// if TextMessageEncodingBindingElement is considered.
			be.BuildChannelFactory<IRequestChannel> (ctx);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void BuildChannelFactoryFail2 ()
		{
			MessageEncodingBindingElement be =
				new MyEncodingBindingElement ();
			BindingContext ctx = new BindingContext (
				// no transport -> fail
				new CustomBinding (
					new TextMessageEncodingBindingElement ()),
				new BindingParameterCollection ());
			be.BuildChannelFactory<IRequestChannel> (ctx);
		}

		[Test]
		public void GetProperty ()
		{
			var ctx = new BindingContext (new CustomBinding (new HttpTransportBindingElement ()), new BindingParameterCollection ());
			Assert.AreEqual (MessageVersion.None, new MyEncodingBindingElement ().GetProperty<MessageVersion> (ctx), "#1");
		}
	}
}
