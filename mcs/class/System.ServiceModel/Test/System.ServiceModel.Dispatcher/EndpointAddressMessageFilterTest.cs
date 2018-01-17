//
// EndpointAddressMessageFilterTest.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Dispatcher
{
	[TestFixture]
	public class EndpointAddressMessageFilterTest
	{
		const string anonymous_uri = "http://www.w3.org/2005/08/addressing/anonymous";

		static Message CreateMessageWithTo (string to)
		{
			return CreateMessageWithTo (new Uri (to));
		}

		static Message CreateMessageWithTo (Uri to)
		{
			Message msg = Message.CreateMessage (MessageVersion.Default, "urn:myaction");
			msg.Headers.Add (MessageHeader.CreateHeader ("To", "http://www.w3.org/2005/08/addressing", to));
			return msg;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorNoAddress ()
		{
			new EndpointAddressMessageFilter (null, false);
		}

		[Test]
		public void Match ()
		{
			EndpointAddressMessageFilter f =
				new EndpointAddressMessageFilter (new EndpointAddress ("http://localhost:8080"));
			Assert.IsTrue (f.Match (CreateMessageWithTo ("http://localhost:8080")), "#1");
			Assert.IsTrue (f.Match (CreateMessageWithTo (new Uri ("http://localhost:8080"))), "#1-2");
			Assert.IsFalse (f.Match (CreateMessageWithTo (anonymous_uri)), "#2");
			Assert.IsFalse (f.Match (CreateMessageWithTo (EndpointAddress.AnonymousUri)), "#3");
			Assert.IsFalse (f.Match (CreateMessageWithTo (EndpointAddress.NoneUri)), "#4");
			// no To header
			Assert.IsFalse (f.Match (Message.CreateMessage (MessageVersion.Default, "urn:myaction")), "#5");
			
			Assert.IsTrue (f.Match (CreateMessageWithTo ("http://10.1.1.1:8080")), "#6");
			Assert.IsFalse (f.Match (CreateMessageWithTo ("http://10.1.1.1:8081")), "#7");

			f = new EndpointAddressMessageFilter (new EndpointAddress ("http://localhost:8080/abc"), true);
			Assert.IsFalse (f.Match (CreateMessageWithTo ("http://127.0.0.2:8080/abc")), "#8");

			Assert.IsTrue (f.Match (CreateMessageWithTo ("http://localhost:8080/abc?wsdl")), "#9");

			f = new EndpointAddressMessageFilter (new EndpointAddress ("http://localhost:8080/abc?foo"), true);
			Assert.IsTrue (f.Match (CreateMessageWithTo ("http://localhost:8080/abc?wsdl")), "#10");
		}
	}
}
#endif
