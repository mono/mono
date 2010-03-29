//
// FaultConverterTest.cs
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
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class FaultConverterTest
	{
		static readonly FaultConverter s11 = FaultConverter.GetDefaultFaultConverter (MessageVersion.Soap11WSAddressing10);
		static readonly FaultConverter s12 = FaultConverter.GetDefaultFaultConverter (MessageVersion.Soap12WSAddressing10);
		static readonly FaultConverter none = FaultConverter.GetDefaultFaultConverter (MessageVersion.None);

		XmlWriterSettings GetWriterSettings ()
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			s.OmitXmlDeclaration = true;
			return s;
		}

		[Test]
		public void TryCreateFaultMessageDefault ()
		{
			Message msg;
			Assert.IsFalse (s12.TryCreateFaultMessage (new Exception ("error happened"), out msg), "#1-1");

			Assert.IsFalse (s12.TryCreateFaultMessage (new FaultException ("fault happened", FaultCode.CreateSenderFaultCode (new FaultCode ("IAmBroken"))), out msg), "#1-2");

			Assert.IsFalse (s12.TryCreateFaultMessage (new FaultException<string> ("fault happened."), out msg), "#1-3");

			Assert.IsTrue (s12.TryCreateFaultMessage (new ActionNotSupportedException (), out msg), "#1-4");
			Assert.IsTrue (msg.IsFault, "#1-5");
			Assert.AreEqual ("http://www.w3.org/2005/08/addressing/fault", msg.Headers.Action, "#1-6");
			var f = MessageFault.CreateFault (msg, 1000);
			Assert.AreEqual ("Sender", f.Code.Name, "#1-7");
			Assert.AreEqual ("http://www.w3.org/2003/05/soap-envelope", f.Code.Namespace, "#1-8");
			Assert.AreEqual ("ActionNotSupported", f.Code.SubCode.Name, "#1-9");
			Assert.AreEqual ("http://www.w3.org/2005/08/addressing", f.Code.SubCode.Namespace, "#1-10");

			Assert.IsFalse (s11.TryCreateFaultMessage (new Exception ("error happened"), out msg), "#2-1");

			Assert.IsFalse (s11.TryCreateFaultMessage (new FaultException ("fault happened", FaultCode.CreateSenderFaultCode (new FaultCode ("IAmBroken"))), out msg), "#2-2");

			Assert.IsFalse (s11.TryCreateFaultMessage (new FaultException<string> ("fault happened."), out msg), "#2-3");

			Assert.IsTrue (s11.TryCreateFaultMessage (new ActionNotSupportedException (), out msg), "#2-4");
			Assert.IsTrue (msg.IsFault, "#2-5");
			Assert.AreEqual ("http://www.w3.org/2005/08/addressing/fault", msg.Headers.Action, "#2-6");
			f = MessageFault.CreateFault (msg, 1000);
			Assert.AreEqual ("ActionNotSupported", f.Code.Name, "#2-7");
			Assert.AreEqual ("http://www.w3.org/2005/08/addressing", f.Code.Namespace, "#2-8");

			Assert.IsFalse (none.TryCreateFaultMessage (new Exception ("error happened"), out msg), "#3-1");

			Assert.IsFalse (none.TryCreateFaultMessage (new FaultException ("fault happened", FaultCode.CreateSenderFaultCode (new FaultCode ("IAmBroken"))), out msg), "#3-2");

			Assert.IsFalse (none.TryCreateFaultMessage (new FaultException<string> ("fault happened."), out msg), "#3-3");

			Assert.IsFalse (none.TryCreateFaultMessage (new ActionNotSupportedException (), out msg), "#3-4");

			Assert.IsFalse (none.TryCreateFaultMessage (new EndpointNotFoundException (), out msg), "#3-5");
		}

		[Test]
		[Category ("NotWorking")]
		public void TryCreateExceptionDefault ()
		{
			var xml = @"
<s:Envelope xmlns:a='http://www.w3.org/2005/08/addressing' xmlns:s='http://schemas.xmlsoap.org/soap/envelope/'>
  <s:Header>
    <a:Action s:mustUnderstand='1'>http://www.w3.org/2005/08/addressing/fault</a:Action>
  </s:Header>
  <s:Body>
    <s:Fault>
      <faultcode>a:ActionNotSupported</faultcode>
      <faultstring xml:lang='en-US'>some error</faultstring>
    </s:Fault>
  </s:Body>
</s:Envelope>";
			var msg = Message.CreateMessage (XmlReader.Create (new StringReader (xml)), 0x1000, MessageVersion.Soap11WSAddressing10);
			var mf = MessageFault.CreateFault (msg, 1000);
			msg = Message.CreateMessage (XmlReader.Create (new StringReader (xml)), 0x1000, MessageVersion.Soap11WSAddressing10);
			Exception ex;
			Assert.IsTrue (s11.TryCreateException (msg, mf, out ex), "#1");

			// test buffered copy (which used to fail)
			msg = Message.CreateMessage (XmlReader.Create (new StringReader (xml)), 0x1000, MessageVersion.Soap11WSAddressing10);
			var mb = msg.CreateBufferedCopy (1000);
			mf = MessageFault.CreateFault (mb.CreateMessage (), 1000);
			Assert.IsTrue (s11.TryCreateException (mb.CreateMessage (), mf, out ex), "#2");
		}
	}
}
