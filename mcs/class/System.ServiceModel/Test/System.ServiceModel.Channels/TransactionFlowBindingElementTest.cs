//
// TransactionFlowBindingElementTest.cs
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
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class TransactionFlowBindingElementTest
	{
		[Test]
		//[Ignore ("Mono never supports OleTx, thus it won't work forever.")]
		public void DefaultValues ()
		{
			TransactionFlowBindingElement be =
				new TransactionFlowBindingElement ();
			Assert.AreEqual (TransactionProtocol.Default,
					 be.TransactionProtocol, "#1");
		}

		public void CanBuildChannelFactory ()
		{
			TransactionFlowBindingElement be =
				new TransactionFlowBindingElement ();
			BindingContext ctx = new BindingContext (
				new CustomBinding (),
				new BindingParameterCollection ());
			Assert.IsTrue (be.CanBuildChannelFactory<IRequestChannel> (ctx), "#1");
			Assert.IsTrue (be.CanBuildChannelFactory<IOutputChannel> (ctx), "#2");
			Assert.IsTrue (be.CanBuildChannelFactory<IRequestSessionChannel> (ctx), "#3");
			Assert.IsTrue (be.CanBuildChannelFactory<IOutputSessionChannel> (ctx), "#4");
		}

		public void CanBuildChannelListener ()
		{
			TransactionFlowBindingElement be =
				new TransactionFlowBindingElement ();
			BindingContext ctx = new BindingContext (
				new CustomBinding (),
				new BindingParameterCollection ());
			Assert.IsTrue (be.CanBuildChannelListener<IReplyChannel> (ctx), "#1");
			Assert.IsTrue (be.CanBuildChannelListener<IInputChannel> (ctx), "#2");
			Assert.IsTrue (be.CanBuildChannelListener<IRequestSessionChannel> (ctx), "#3");
			Assert.IsTrue (be.CanBuildChannelListener<IOutputSessionChannel> (ctx), "#4");
		}
	}
}
#endif