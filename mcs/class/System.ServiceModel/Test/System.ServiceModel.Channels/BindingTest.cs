//
// BindingTest.cs
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
	public class BindingTest
	{
		class MyBinding : Binding
		{
			public override string Scheme {
				get { return "my"; }
			}

			public override BindingElementCollection CreateBindingElements ()
			{
				return new BindingElementCollection (
					new BindingElement [] { new HttpTransportBindingElement () });
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void BuildChannelFactoryGeneric ()
		{
			// i.e. it should not reuse non-generic version of
			// BuildChannelFactory().
			new MyBinding ().BuildChannelFactory<IRequestChannel> ((BindingParameterCollection) null);
		}

		[Test]
		public void BuildChannelFactoryGeneric2 ()
		{
			new MyBinding ().BuildChannelFactory<IRequestChannel> (
				new BindingParameterCollection ());
		}

		[Test]
		public void MessageVersionProperty ()
		{
			Assert.AreEqual (MessageVersion.Soap11, new BasicHttpBinding ().MessageVersion, "#1");
			Assert.IsNull (new CustomBinding ().MessageVersion, "#2");
		}
	}
}
