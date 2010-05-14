//
// BindingContextTest.cs
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
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class BindingContextTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestCtorBindingNull ()
		{
			new BindingContext (null,
				new BindingParameterCollection ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestCtorParametersNull ()
		{
			new BindingContext (new CustomBinding (), null);
		}

		[Test]
		//[ExpectedException (typeof (ArgumentNullException))]
		public void TestCtorListenUriBaseAddressNull ()
		{
			new BindingContext (new CustomBinding (),
				new BindingParameterCollection (),
				null,
				"http://localhost:8080",
				ListenUriMode.Unique);
		}

		[Test]
		//[ExpectedException (typeof (ArgumentNullException))]
		public void TestCtorListenUriRelativeAddressNull ()
		{
			new BindingContext (new CustomBinding (),
				new BindingParameterCollection (),
				new Uri ("http://localhost:8080"),
				null,
				ListenUriMode.Unique);
		}

		[Test]
		// Now this test is mostly useless ...
		public void ConsumeBindingElements ()
		{
			BindingContext ctx = new BindingContext (
				new CustomBinding (
					new TextMessageEncodingBindingElement (),
					new HttpTransportBindingElement ()),
				new BindingParameterCollection ());

			HttpTransportBindingElement be =
				new HttpTransportBindingElement ();
			be.BuildChannelFactory<IRequestChannel> (ctx);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (ArgumentException))]
		public void DuplicateMesageEncodingBindingElementError ()
		{
			BindingContext ctx = new BindingContext (
				new CustomBinding (
					new TextMessageEncodingBindingElement (),
					new HttpTransportBindingElement ()),
				new BindingParameterCollection ());

			TextMessageEncodingBindingElement te =
				new TextMessageEncodingBindingElement ();
			te.BuildChannelFactory<IRequestChannel> (ctx);
		}

		[Test]
		public void GetInnerPropertyIsNothingToDoWithParameters ()
		{
			BindingParameterCollection pl =
				new BindingParameterCollection ();
			pl.Add (new ClientCredentials ());
			BindingContext ctx =
				new BindingContext (new CustomBinding (), pl);
			Assert.IsNull (ctx.GetInnerProperty<ClientCredentials> ());

			CustomBinding binding = new CustomBinding (new HttpTransportBindingElement ());
			ctx = new BindingContext (binding, pl);
			Assert.IsNull (ctx.GetInnerProperty<ClientCredentials> ());
		}

		[Test]
		public void RemainingBindingElements ()
		{
			var b = new CustomBinding (
				new BinaryMessageEncodingBindingElement (),
				new InterceptorBindingElement2 (),
				new HttpTransportBindingElement ());
			Assert.AreEqual (3, new BindingContext (b, new BindingParameterCollection ()).RemainingBindingElements.Count, "#1");
			Assert.IsTrue (b.CanBuildChannelFactory<IRequestChannel> (), "#2");
			
		}
		
		[Test]
		public void RemainingBindingElements2 ()
		{
			var b = new CustomBinding (
				new BindingElement2 (),
				new InterceptorBindingElement3 (),
				new BindingElement3 ());

			Assert.AreEqual (3, new BindingContext (b, new BindingParameterCollection ()).RemainingBindingElements.Count, "New BindingContext element count");
			Assert.IsTrue (b.CanBuildChannelFactory<IRequestChannel> (), "supports IRequestChannel?");
		}

		public class InterceptorBindingElement2 : BindingElement
		{
			public override bool CanBuildChannelFactory<TChannel> (BindingContext bc)
			{
				Assert.AreEqual (1, bc.Clone ().RemainingBindingElements.Count, "#i1");
				Assert.IsNull (bc.GetInnerProperty<MessageEncodingBindingElement> (), "#i2");
				Assert.IsNull (bc.GetInnerProperty<MessageEncoder> (), "#i3");
				Assert.IsNull (bc.GetInnerProperty<MessageEncoderFactory> (), "#i4");
				Assert.AreEqual (1, bc.RemainingBindingElements.Count, "#i5");
				Assert.IsTrue (bc.RemainingBindingElements [0] is HttpTransportBindingElement, "#i6");
				Assert.AreEqual (3, bc.Binding.CreateBindingElements ().Count, "#i7");
				return base.CanBuildChannelFactory<TChannel> (bc);
			}

			public override BindingElement Clone ()
			{
				return new InterceptorBindingElement2 ();
			}

			public override T GetProperty<T> (BindingContext bc)
			{
				return null;
			}
		}

		public class InterceptorBindingElement3 : BindingElement {
			public override bool CanBuildChannelFactory<TChannel> (BindingContext bc) {
				if (this is BindingElement2) {
					Assert.AreEqual (2, bc.Clone ().RemainingBindingElements.Count, "1- Cloned context element count");
					Assert.AreEqual (3, bc.Binding.CreateBindingElements ().Count, "1- Original binding element count");
					Assert.IsTrue (bc.RemainingBindingElements [0] is InterceptorBindingElement3, "1- binding element 1 type");
					Assert.IsTrue (bc.RemainingBindingElements [1] is BindingElement3, "1- binding element 2 type");
				} else {
					Assert.AreEqual (1, bc.Clone ().RemainingBindingElements.Count, "2- Cloned context element count");
					Assert.AreEqual (3, bc.Binding.CreateBindingElements ().Count, "2- Original binding element count");
					Assert.IsTrue (bc.RemainingBindingElements [0] is BindingElement3, "2- binding element 1 type");
				}
				return base.CanBuildChannelFactory<TChannel> (bc);
			}
			public override BindingElement Clone () {
				return new InterceptorBindingElement3 ();
			}

			public override T GetProperty<T> (BindingContext bc) {
				return null;
			}
		}

		public class BindingElement2 : InterceptorBindingElement3 {

			public override BindingElement Clone () {
				return new BindingElement2 ();
			}
			public override bool CanBuildChannelFactory<TChannel> (BindingContext bc) {
				return base.CanBuildChannelFactory<TChannel> (bc);
			}

		}

		public class BindingElement3 : HttpTransportBindingElement {

			public override BindingElement Clone () {
				return new BindingElement3 ();
			}
			public override bool CanBuildChannelFactory<TChannel> (BindingContext bc) {
				return base.CanBuildChannelFactory<TChannel> (bc);
			}

		}

	}
}

