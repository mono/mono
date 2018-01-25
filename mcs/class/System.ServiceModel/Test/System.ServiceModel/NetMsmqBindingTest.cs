//
// NetMsmqBindingTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class NetMsmqBindingTest
	{
		[Test]
		public void DefaultValues ()
		{
			NetMsmqBinding b = new NetMsmqBinding ();
			Assert.AreEqual (EnvelopeVersion.Soap12, b.EnvelopeVersion, "#1");
			Assert.AreEqual (0x80000, b.MaxBufferPoolSize, "#2");
			Assert.AreEqual (QueueTransferProtocol.Native, b.QueueTransferProtocol, "#2");
			Assert.IsNotNull (b.ReaderQuotas, "#3");
			Assert.IsFalse (b.UseActiveDirectory, "#4");

			Assert.IsNull (b.CustomDeadLetterQueue, "#5");
			Assert.AreEqual (DeadLetterQueue.System, b.DeadLetterQueue, "#6");
			Assert.IsTrue (b.Durable, "#7");
			Assert.IsTrue (b.ExactlyOnce, "#8");
			Assert.AreEqual (0x10000, b.MaxReceivedMessageSize, "#9");
			Assert.AreEqual (2, b.MaxRetryCycles, "#10");
			Assert.AreEqual (ReceiveErrorHandling.Fault, b.ReceiveErrorHandling, "#11");
			Assert.AreEqual (5, b.ReceiveRetryCount, "#12");
			// hmm, it is documented as 10 minutes but ...
			Assert.AreEqual (TimeSpan.FromMinutes (30), b.RetryCycleDelay, "#13");
			Assert.AreEqual ("net.msmq", b.Scheme, "#14");
			Assert.AreEqual (TimeSpan.FromDays (1), b.TimeToLive, "#15");
			Assert.IsFalse (b.UseMsmqTracing, "#16");
			Assert.IsFalse (b.UseSourceJournal, "#17");
		}

/*
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DefaultValueSecurityModeMessageError ()
		{
			NetMsmqBinding b = new NetMsmqBinding (BasicHttpSecurityMode.Message);
			// "BasicHttp binding requires that NetMsmqBinding.Security.Message.ClientCredentialType be equivalent to the BasicHttpMessageCredentialType.Certificate credential type for secure messages. Select Transport or TransportWithMessageCredential security for UserName credentials."
			b.CreateBindingElements ();
		}
*/

		[Test]
		public void CreateBindingElements ()
		{
			BindingElementCollection bl = new NetMsmqBinding ().CreateBindingElements ();
			Assert.AreEqual (2, bl.Count, "#1");
			Assert.IsTrue (bl [0] is BinaryMessageEncodingBindingElement, "#2");
			Assert.IsTrue (bl [1] is MsmqTransportBindingElement, "#3");
		}

/*
		[Test]
		public void MessageEncoding ()
		{
			NetMsmqBinding b = new NetMsmqBinding ();
			foreach (BindingElement be in b.CreateBindingElements ()) {
				MessageEncodingBindingElement mbe =
					be as MessageEncodingBindingElement;
				if (mbe != null) {
					MessageEncoderFactory f = mbe.CreateMessageEncoderFactory ();
					MessageEncoder e = f.Encoder;

					Assert.AreEqual (typeof (TextMessageEncodingBindingElement), mbe.GetType (), "#1-1");
					Assert.AreEqual (MessageVersion.Soap11, f.MessageVersion, "#2-1");
					Assert.AreEqual ("text/xml; charset=utf-8", e.ContentType, "#3-1");
					Assert.AreEqual ("text/xml", e.MediaType, "#3-2");
					return;
				}
			}
			Assert.Fail ("No message encodiing binding element.");
		}
*/
	}
}
#endif